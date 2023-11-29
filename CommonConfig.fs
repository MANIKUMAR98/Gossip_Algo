module CommonConfig

open Akka
open System
open Schedulers
open Akka.Actor
open Akka.Actor.Scheduler
open Akka.FSharp
open System.Diagnostics
open System.Collections.Generic
open Akka.Configuration
open Utility

let config = 
    ConfigurationFactory.ParseString(
        @"akka {            
            stdout-loglevel : ERROR
            loglevel : ERROR
           ad-letters = 0
            log-dead-letters-during-shutdown = off
        }")

type Commands =
    | FindNeighbors of list<IActorRef> * string
    | SetupTopology of string
    | GossipTransmissionComplete of string
    | LaunchAlgorithmExecution of string
    | GossipMessage of int * string
    | PerformRound
    | ReceivedRumor of int
    | DiscoveredNeighbors of int
    | PushsumMessage of int * float * float
    | InitiatePushsum
    | ConvergedPushsumActor of int * float * float



let mutable nodeCount = 27000

let mutable isIrregular = false

let mutable primaryActorRef: IActorRef = null
let communicationSystem = ActorSystem.Create("GossipSystem", config)

// discoverNeighbors function takes a list of actor references 'pool', a 'topology' string,
// and the index of the current actor 'myActorIndex' to determine its neighbors.
let discoverNeighbors (actors: list<IActorRef>, topology: string, actorIndex: int) =
    let neighbors =
        match topology with
        | "line" ->
            Utility.findLinearNeighborsFor(actors, actorIndex, nodeCount)
        | "2D" | "2d" | "imp2D" | "imp2d" ->
            let side = nodeCount |> float |> sqrt |> int
            Utility.find2DGridNeighbors(actors, actorIndex, side, nodeCount, isIrregular)
        | "3D" | "3d" | "imp3D" | "imp3d" ->
            let side = nodeCount |> float |> Math.Cbrt |> int
            Utility.find3DGridNeighbors(actors, actorIndex, side, (side * side), nodeCount, isIrregular)
        | "full" ->
            Utility.findCompleteNeighborsFor(actors, actorIndex, nodeCount)
        | _ ->
            [] // Return an empty list for unrecognized topologies
    neighbors

// Rounds up the node count to the nearest perfect cube.
let roundToNearestCube (nodeCount:int) =
    let cubicSides = nodeCount |> float |> Math.Cbrt |> ceil |> int
    cubicSides * cubicSides * cubicSides

// Rounds up the node count to the nearest perfect square.
let roundToNearestSquare (nodeCount:int) =
    let squareSides = nodeCount |> float |> sqrt |> ceil |> int
    squareSides * squareSides
