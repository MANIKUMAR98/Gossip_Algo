module Utility

open Akka.Actor
open Akka.FSharp

// Given a list of actor references 'nodePool', an index 'nodeIndex', and the total number of nodes 'totalNodes',
// this function identifies and returns the complete set of neighbors for the node at the specified index.
// Complete neighbors include all nodes in the node pool except the node at the specified index.
let findCompleteNeighborsFor (nodePool: list<IActorRef>, nodeIndex: int, totalNodes: int) =
    let neighborList =
        nodePool
        |> List.indexed
        |> List.filter (fun (i, _) -> i <> nodeIndex)
        |> List.map snd

    neighborList


// Given a list of actor references 'nodePool', an index 'nodeIndex', and the total number of nodes 'totalNodes',
// this function identifies and returns the linear neighbors of the node at the specified index.
// Linear neighbors include the node at the index before and after the current node in the node pool.
let findLinearNeighborsFor (nodePool: list<IActorRef>, nodeIndex: int, totalNodes: int) =
    let mutable neighborList = []

    if nodeIndex <> 0 then
        neighborList <- nodePool.[nodeIndex - 1] :: neighborList 

    if nodeIndex <> totalNodes - 1 then
        neighborList <- nodePool.[nodeIndex + 1] :: neighborList 

    neighborList

// Given a list of actor references 'nodePool', an index 'nodeIndex', the 'gridSize' of the 2D grid,
// the total number of nodes 'totalNodes', and a flag 'isIrregular' indicating irregular connections,
// this function identifies and returns the 2D grid neighbors for the node at the specified index.
// If 'isIrregular' is true, a random neighbor is added to the list.
let find2DGridNeighbors (nodePool: list<IActorRef>, nodeIndex: int, gridSize: int, totalNodes: int, isIrregular: bool) =
    let mutable neighborList = []

    // Add left neighbor if not on the left edge.
    if nodeIndex % gridSize <> 0 then
        neighborList <- nodePool.[nodeIndex - 1] :: neighborList

    // Add right neighbor if not on the right edge.
    if nodeIndex % gridSize <> gridSize - 1 then
        neighborList <- nodePool.[nodeIndex + 1] :: neighborList

    // Add top neighbor if not on the top edge.
    if nodeIndex - gridSize >= 0 then
        neighborList <- nodePool.[nodeIndex - gridSize] :: neighborList

    // Add bottom neighbor if not on the bottom edge.
    if (nodeIndex + gridSize) <= totalNodes - 1 then
        neighborList <- nodePool.[nodeIndex + gridSize] :: neighborList

    // If irregular connections are allowed, add a random neighbor not already in the list.
    if isIrregular then
        let r = System.Random()
        let randomNeighbor =
            nodePool
            |> List.filter (fun x -> (x <> nodePool.[nodeIndex] && not (List.contains x neighborList)))
            |> fun y -> y.[r.Next(y.Length - 1)]
        neighborList <- randomNeighbor :: neighborList

    neighborList


// Given a list of actor references 'nodePool', an index 'nodeIndex', the 'gridSize' of the 3D grid,
// the 'gridSquare' representing the size of each square in the grid, the total number of nodes 'totalNodes',
// and a flag 'isIrregular' indicating irregular connections,
// this function identifies and returns the 3D grid neighbors for the node at the specified index.
// If 'isIrregular' is true, a random neighbor is added to the list.
let find3DGridNeighbors (nodePool: list<IActorRef>, nodeIndex: int, gridSize: int, gridSquare: int, totalNodes: int, isIrregular: bool) =
    let mutable neighborList = []

    // Add left neighbor if not on the left edge.
    if (nodeIndex % gridSize) <> 0 then
        neighborList <- nodePool.[nodeIndex - 1] :: neighborList 

    // Add right neighbor if not on the right edge.
    if (nodeIndex % gridSize) <> (gridSize - 1) then
        neighborList <- nodePool.[nodeIndex + 1] :: neighborList 

    // Add top neighbor if not on the top edge.
    if nodeIndex % gridSquare >= gridSize then 
        neighborList <- nodePool.[nodeIndex - gridSize] :: neighborList

    // Add bottom neighbor if not on the bottom edge.
    if gridSquare - (nodeIndex % gridSquare) > gridSize then 
        neighborList <- nodePool.[nodeIndex + gridSize] :: neighborList

    // Add front neighbor if not on the front edge.
    if nodeIndex >= gridSquare then 
        neighborList <- nodePool.[nodeIndex - gridSquare] :: neighborList

    // Add back neighbor if not on the back edge.
    if (totalNodes - nodeIndex) > gridSquare then 
        neighborList <- nodePool.[nodeIndex + gridSquare] :: neighborList

    // If irregular connections are allowed, add a random neighbor not already in the list.
    if isIrregular then
        let r = System.Random()
        let randomNeighbor =
            nodePool
            |> List.filter (fun x -> (x <> nodePool.[nodeIndex] && not (List.contains x neighborList)))
            |> fun y -> y.[r.Next(y.Length - 1)]
        neighborList <- randomNeighbor :: neighborList

    neighborList
