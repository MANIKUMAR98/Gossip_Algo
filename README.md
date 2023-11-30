The objective of this project is to assess the convergence of Gossip-type algorithms using a simulator built on the Actor model and implemented in F# with the Akka.NET framework. Given the fully asynchronous nature of F# actors, the specific Gossip algorithm employed is Asynchronous Gossip. The implementation covers both Gossip and PushSum algorithms, with the flexibility to choose from various topologies through a configurable parameter during simulation. For more detailed information, refer to the report attached.

Project Members:
    1. Manikumar Honnenahalli Lakshminarayana Swamy (UFID: 24369711)
    2. Nischith Bairannanavara Omprakash (UFID: 67462832)
    3. Anvesh Gupta (UFID: 89277005)
    4. Swapnanil Gupta (UFID: 46875775)

Steps to run the prgram:
    cd Gossip_Algo
    dotnet run {nodecount} {topology} {algorithm} (eg: dotnet run 200 line gossip / dotnet run 10 line pushsum)

The maximum number of nodes for which we executed the Gossip algorithm
    Line - 6000
    Full - 6000
    3D - 6000
    Imp3D - 6000

The maximum number of nodes for which we executed the Gossip algorithm
    Line - 60
    Full - 60
    3D - 60
    Imp3D - 60