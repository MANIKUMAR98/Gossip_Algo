module Gossip

open System
open Akka.FSharp
open CommonConfig

// GossipActor is an actor that participates in a gossip-based communication protocol.
let GossipActor (id:int) (mailbox: Actor<_>) =
    // Mutable state variables
    let mutable myNeighborsArray = []
    let mutable myRumorCount = 0
    let myActorIndex = id
    let mutable isActive = 1
 
    // The main loop of the actor
    let rec loop () = actor {
         // Check if the actor is active
        if isActive = 1 then
            // Receive a message from the mailbox
            let! (message) = mailbox.Receive()
            // Match the received message against different patterns
            match message with 
            | FindNeighbors(pool, topology) ->
                myNeighborsArray <- discoverNeighbors(pool, topology, myActorIndex)
                primaryActorRef <! DiscoveredNeighbors(myActorIndex)

            | GossipMessage(fromNode, message) ->
                if myRumorCount = 0 then
                    primaryActorRef <! ReceivedRumor(myActorIndex)
                    mailbox.Self <! PerformRound

                // Increment the rumor count for this actor
                myRumorCount <- myRumorCount + 1

                // If the actor has transmitted the rumor more than 10 times, deactivate the actor
                if myRumorCount > 10 then
                    isActive <- 0
                    // Notify the primary actor that this actor has finished transmitting rumors.
                    primaryActorRef <! GossipTransmissionComplete(mailbox.Self.Path.Name)

            | PerformRound(_) ->
                // If the message is a Round message
                // Select a random neighbor and send a gossip message to that neighbor
                let randomNeighbor = Random().Next(myNeighborsArray.Length)
                let randomActor = myNeighborsArray.[randomNeighbor]
                randomActor <! GossipMessage(myActorIndex, "rumor")
                // Schedule a Round message for itself after a delay
                communicationSystem.Scheduler.ScheduleTellOnce(TimeSpan.FromMilliseconds(10.0), mailbox.Self, PerformRound, mailbox.Self)
                
            | _ -> ()

            return! loop()
        }
    loop()