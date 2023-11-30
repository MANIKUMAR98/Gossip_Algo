module Main

open System
open Akka.FSharp
open System.Diagnostics
open CommonConfig
open PushSum


let mutable topologyType = "3D"
let mutable algoType = "pushsum"
let mutable actorList = []
let stopwatch = Stopwatch()

// Main actor function that handles different messages for topology construction and algorithm execution.
let MainActor (mailbox:Actor<_>) =    
    let mutable actorsCompleted = 0
    let mutable actorsInformed = 0
    let mutable topologyConstruction = 0

     // Recursive loop function to continuously process incoming messages.
    let rec loop () = 
        actor {
            let! (message) = mailbox.Receive()

            match message with 
            | SetupTopology(_) ->
                // Constructs topology based on the algorithm type (gossip or pushsum).
                printfn "Constructing topology for %s algorithm." algoType
                if algoType = "gossip" then    
                    actorList <-
                        [0 .. nodeCount-1]
                        |> List.map(fun id -> spawn communicationSystem (sprintf "GossipActor_%d" id) (Gossip.GossipActor id))
                else
                    actorList <- 
                        [0 .. nodeCount-1]
                        |> List.map(fun id -> spawn communicationSystem (sprintf "PushsumActor_%d" id) (PushsumActor id))

                actorList |> List.iter (fun item -> 
                    item <! FindNeighbors(actorList, topologyType))

            | DiscoveredNeighbors(index) ->
            // Tracks the completion of topology construction.
                topologyConstruction <- topologyConstruction + 1
                if topologyConstruction = nodeCount then 
                    printfn "Topology construction completed for %d nodes.\n" nodeCount
                    stopwatch.Start()
                    primaryActorRef <! LaunchAlgorithmExecution(algoType)

            | LaunchAlgorithmExecution(algorithm) ->
               // Starts the specified algorithm (gossip or pushsum).
                match algorithm with 
                | "gossip" ->
                    let randomNeighbor = Random().Next(actorList.Length)
                    let randomActor = actorList.[randomNeighbor]
                    printfn "Sending the first rumor to neighbor %d\n" randomNeighbor
                    randomActor <! GossipMessage(0, "theRumor")
                | "pushsum" ->
                    actorList |> List.iter (fun item -> 
                        item <! InitiatePushsum)
                | _ -> ()

            | GossipTransmissionComplete(actorName) ->
            // Tracks the number of actors that have completed transmitting gossip.
                actorsCompleted <- actorsCompleted + 1

            | ReceivedRumor(actorIndex) ->
            // Tracks the number of actors informed about the rumor.
                actorsInformed <- actorsInformed + 1
                printfn "Actor %d has been informed! Total actors informed: %d.\n" actorIndex actorsInformed
                if actorsInformed = nodeCount then 
                    stopwatch.Stop()
                    printfn "\nTotal time taken: %d ms | Total actors terminated: %d\n" stopwatch.ElapsedMilliseconds actorsCompleted
                    printfn "\nEveryone knows the rumor! Mission accomplished.\n"
                    Environment.Exit(0)

            | ConvergedPushsumActor (index, s, w) ->
            // Tracks the number of actors that have converged in the pushsum algorithm.
                actorsCompleted <- actorsCompleted + 1
                printfn "id = %d | s = %f | w = %f | s/w = %f | Total terminated = %d"  index s w (s/w) actorsCompleted
                if actorsCompleted = nodeCount then 
                    printfn "\n\n All nodes have converged!!\n\n"
                    stopwatch.Stop()
                    printfn "Total time = %dms" stopwatch.ElapsedMilliseconds
                    Environment.Exit(0)

            | _ -> ()
            // Continues the loop to process the next message
            return! loop()
        }
    loop()


[<EntryPoint>]
let main argv =

    // Check if command-line arguments are provided; if not, start with default values.
    if (argv.Length <> 3) then 
        printfn "Starting with default values" 
    else 
        // Extract values from command-line arguments.
        topologyType <- argv.[1]
        algoType <- argv.[2]

        // Determine the node count based on the provided topology type.
        if topologyType = "2D" || topologyType = "imp2D" || topologyType = "imp2d" || topologyType = "2d" then 
            nodeCount <- argv.[0] |> int |> roundToNearestSquare
        else if topologyType = "3D" || topologyType = "imp3D" || topologyType = "imp3d" || topologyType = "3d" then 
            nodeCount <- argv.[0] |> int |> roundToNearestCube
        else 
            nodeCount <- argv.[0] |> int

        // Check if the topology is irregular and set the flag accordingly.
        if topologyType = "imp2D" || topologyType = "imp2d" || topologyType = "imp3D" || topologyType = "imp3d" then 
            isIrregular <- true

    // Spawn the primary actor in the communication system.
    primaryActorRef <- spawn communicationSystem "MainActor" MainActor
    // Send a message to the primary actor to start constructing the topology.
    primaryActorRef <! SetupTopology("start")

    // Wait for the communication system to terminate.
    communicationSystem.WhenTerminated.Wait()

    0