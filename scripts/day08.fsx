// Day 8: Playground
// Run with: dotnet fsi day08.fsx

open System
open System.IO
open System.Diagnostics

// ==========================================
// 1. Domain Modeling
// ==========================================

module Domain =
  type Point3D = { 
    Id: int
    X: int; Y: int; Z: int 
  }

  type Edge = {
    P1: int
    P2: int
    DistSq: int64
  }

  /// Immutable Disjoint Set Union State
  type CircuitState = {
    Parents: Map<int, int>
    Sizes: Map<int, int>
    ComponentCount: int
  }

// ==========================================
// 2. Core Algorithms (DSU & Geometry)
// ==========================================

module Algorithms =
  open Domain

  let distSq (p1: Point3D) (p2: Point3D) : int64 =
    let dx = int64 (p1.X - p2.X)
    let dy = int64 (p1.Y - p2.Y)
    let dz = int64 (p1.Z - p2.Z)
    dx*dx + dy*dy + dz*dz

  /// Initialize DSU state where every point is its own root
  let initCircuitState (points: Point3D[]) =
    let initialSizes = points |> Seq.map (fun p -> p.Id, 1) |> Map.ofSeq
    { Parents = Map.empty
      Sizes = initialSizes
      ComponentCount = points.Length }

  /// Find root recursive lookup (Depth is low enough for recursion here)
  let rec findRoot (parents: Map<int, int>) (i: int) =
    match Map.tryFind i parents with
    | Some p when p <> i -> findRoot parents p
    | _ -> i

  /// Attempts to merge sets. Returns struct(NewState, DidMergeOccur).
  let tryUnion (state: CircuitState) (edge: Edge) =
    let root1 = findRoot state.Parents edge.P1
    let root2 = findRoot state.Parents edge.P2

    if root1 = root2 then
      struct(state, false)
    else
      // Union by Size: Attach smaller tree to larger tree
      let size1 = state.Sizes.[root1]
      let size2 = state.Sizes.[root2]
      let (child, parent) = if size1 < size2 then (root1, root2) else (root2, root1)
      
      let newParents = state.Parents |> Map.add child parent
      let newSizes = 
        state.Sizes 
        |> Map.remove child 
        |> Map.add parent (size1 + size2)

      let newState = { 
          Parents = newParents
          Sizes = newSizes
          ComponentCount = state.ComponentCount - 1 
      }
      struct(newState, true)

// ==========================================
// 3. Parsing & Preprocessing
// ==========================================

module Parsing =
  open Domain
  open Algorithms

  let parsePoint (id: int) (line: string) : Point3D =
    let parts = line.Split(',') |> Array.map int
    { Id = id; X = parts.[0]; Y = parts.[1]; Z = parts.[2] }

  /// Loads points and pre-calculates the sorted edge list
  let processInput (filePath: string) =
    if not (File.Exists filePath) then failwithf "File not found: %s" filePath
    
    // 1. Parse Points
    let points = 
      File.ReadAllLines(filePath)
      |> Seq.filter (fun s -> not (String.IsNullOrWhiteSpace s))
      |> Seq.indexed
      |> Seq.map (fun (i, line) -> parsePoint i line)
      |> Seq.toArray

    // 2. Generate and Sort Edges (O(N^2 log N))
    let n = points.Length
    let sortedEdges = 
      [| 
        for i in 0 .. n - 1 do
          for j in i + 1 .. n - 1 do
            yield { 
              P1 = points.[i].Id
              P2 = points.[j].Id
              DistSq = distSq points.[i] points.[j] 
            }
      |]
      |> Array.sortBy (fun e -> e.DistSq)

    (points, sortedEdges)

// ==========================================
// 4. Solvers
// ==========================================

module Solvers =
  open Domain
  open Algorithms

  let solvePart1 (points: Point3D[]) (edges: Edge[]) =
    let limit = 1000
    let initialState = initCircuitState points

    // Take first 1000 edges, fold union over them
    let finalState = 
      edges 
      |> Seq.truncate limit
      |> Seq.fold (fun state edge -> 
          let struct(newState, _) = tryUnion state edge
          newState
      ) initialState

    // Calculate result: Product of top 3 largest circuit sizes
    finalState.Sizes
    |> Map.values
    |> Seq.sortDescending
    |> Seq.truncate 3
    |> Seq.fold ( * ) 1

  let solvePart2 (points: Point3D[]) (edges: Edge[]) =
    let initialState = initCircuitState points
    // Fast lookup for X coordinate
    let xCoords = points |> Array.map (fun p -> p.Id, int64 p.X) |> Map.ofArray

    let rec findMstEdge edges state =
      match edges with
      | [] -> failwith "All edges exhausted without forming a single component."
      | edge :: rest ->
          let struct(newState, merged) = tryUnion state edge
          
          if merged then
            if newState.ComponentCount = 1 then
              // STOP: This is the edge that completed the MST
              let x1 = xCoords.[edge.P1]
              let x2 = xCoords.[edge.P2]
              x1 * x2
            else
              findMstEdge rest newState
          else
            // Edge redundant
            findMstEdge rest state

    findMstEdge (Array.toList edges) initialState

// ==========================================
// 5. Execution & Timing
// ==========================================

let measure label f =
  let sw = Stopwatch.StartNew()
  let result = f ()
  sw.Stop()
  // Only print time here, not the result
  printfn "[%s] Time: %0.4f ms" label sw.Elapsed.TotalMilliseconds
  result

let run () =
  printfn "=== Day 8: Playground ==="
  let inputPath = Path.Combine(__SOURCE_DIRECTORY__, "../inputs/day08.txt")

  // 1. Parse & Preprocess
  let (points, sortedEdges) = 
    measure "Parsing & Sort" (fun () -> Parsing.processInput inputPath)
  
  printfn "Stats: %d nodes, %d edges generated." points.Length sortedEdges.Length

  // 2. Part 1
  let part1Ans = measure "Part 1" (fun () -> Solvers.solvePart1 points sortedEdges)
  printfn "Part 1 Answer: %d" part1Ans

  // 3. Part 2
  let part2Ans = measure "Part 2" (fun () -> Solvers.solvePart2 points sortedEdges)
  printfn "Part 2 Answer: %d" part2Ans

run ()