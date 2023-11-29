module PushSum

open Akka
open System
open Utility
open Schedulers
open Akka.Actor
open Akka.Actor.Scheduler
open Akka.FSharp
open System.Diagnostics
open System.Collections.Generic
open Akka.Configuration
open CommonConfig


let pushsumConvergenceThreshold = 15
let pushsumRoundsDurationMs = 50.0


let PushsumActor (myActorIndex:int) (mailbox:Actor<_>) =
    let mutable myNeighborsArray = []
    let mutable s = myActorIndex + 1 |> float
    let mutable w = 1 |> float
    let mutable prevRatio = s/w
    let mutable currRatio = s/w
    let mutable convergenceCount = 0
    let mutable isActive = 1
    let mutable incomingSList = []
    let mutable incomingWList = []
    let mutable s_aggregatePrev = 0.0
    let mutable w_aggregatePrev = 0.0
    let mutable count = 0
 
    let rec loop () = actor {
        if isActive = 1 then    
            let! (message) = mailbox.Receive()

            match message with 
            | FindNeighbors(pool, topology) ->
                myNeighborsArray <- discoverNeighbors(pool, topology, myActorIndex)
                primaryActorRef <! DiscoveredNeighbors(myActorIndex)

            | PerformRound(_) ->
                
                // Step 2: s <- Σ(incomingSList) and w <- Σ(incomingWList)
                s <- s_aggregatePrev
                w <- w_aggregatePrev

                // Step 3: Choose a random neighbor
                let randomNeighbor = Random().Next(0, myNeighborsArray.Length)
                let randomActor = myNeighborsArray.[randomNeighbor]

                // Step 4: Send the pair ( ½s , ½w ) to randomNeighbor and self
                randomActor <! PushsumMessage(myActorIndex , (s/2.0) , (w/2.0))
                mailbox.Self <! PushsumMessage(myActorIndex , (s/2.0) , (w/2.0))

                // Check for convergence
                currRatio <- s / w
                if (abs(currRatio - prevRatio)) < (pown 10.0 -10) then 
                    convergenceCount <- convergenceCount + 1
                else 
                    convergenceCount <- 0
                
                if convergenceCount = pushsumConvergenceThreshold then 
                    primaryActorRef <! ConvergedPushsumActor(myActorIndex, s,w)
                    isActive <- 0

                prevRatio <- currRatio

                // Reset the aggregate back to 0 after each round
                s_aggregatePrev <- 0.0
                w_aggregatePrev <- 0.0
                count <- 0
                       
            | PushsumMessage (fromIndex, incomingS, incomingW) ->
                count <- count + 1
                s_aggregatePrev <- s_aggregatePrev + incomingS
                w_aggregatePrev <- w_aggregatePrev + incomingW

            | InitiatePushsum ->
                mailbox.Self <! PushsumMessage(myActorIndex , s , w)
                communicationSystem.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0.0),TimeSpan.FromMilliseconds(pushsumRoundsDurationMs), mailbox.Self, PerformRound)
 
            | _ -> ()

            return! loop()
        }
    loop()