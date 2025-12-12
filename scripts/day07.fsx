(* File: day07.fsx *)
open System
open System.IO
open System.Diagnostics

// --- Domain Models ---
module Domain =
  
  type Manifold = {
    Grid: string array
    Width: int
    Height: int
    StartX: int
    StartY: int
  }

  let isValidColumn (m: Manifold) (x: int) =
    x >= 0 && x < m.Width

// --- Infrastructure ---
module Diagnostics =
  
  /// Executes 'f', prints only the elapsed time, and returns the result.
  /// Use this for steps with large objects (like Parsing).
  let measureTime (name: string) (f: unit -> 'T) : 'T =
    let sw = Stopwatch.StartNew()
    let result = f ()
    sw.Stop()
    printfn "%s: Done (Time: %.1fms)" name sw.Elapsed.TotalMilliseconds
    result

  /// Executes 'f', prints the result value and elapsed time, and returns the result.
  /// Use this for Part 1/2 answers.
  let measureResult (name: string) (f: unit -> 'T) : 'T =
    let sw = Stopwatch.StartNew()
    let result = f ()
    sw.Stop()
    printfn "%s: %A (Time: %.1fms)" name result sw.Elapsed.TotalMilliseconds
    result

// --- Parsing ---
module Parsing =
  open Domain

  let parseInput (filePath: string) : Manifold =
    let lines = File.ReadAllLines(filePath)
    let height = lines.Length
    let width = if height > 0 then lines.[0].Length else 0
    
    let rec findStart y =
      if y >= height then failwith "Start point 'S' not found."
      else
        match lines.[y].IndexOf('S') with
        | -1 -> findStart (y + 1)
        | x -> (x, y)

    let startX, startY = findStart 0

    { Grid = lines
      Width = width
      Height = height
      StartX = startX
      StartY = startY }

// --- Core Logic ---
module Solution =
  open Domain

  /// Part 1: Count total split events (Geometric Coverage)
  let solvePart1 (manifold: Manifold) : int =
    let rec processRow (rowIdx: int) (activeCols: Set<int>) (splitCount: int) =
      if rowIdx >= manifold.Height then
        splitCount
      else
        let nextRowCols, splitsInRow =
          activeCols
          |> Set.fold (fun (nextCols, splits) x ->
            if not (isValidColumn manifold x) then
              (nextCols, splits)
            else
              match manifold.Grid.[rowIdx].[x] with
              | '^' ->
                let newSet = nextCols |> Set.add (x - 1) |> Set.add (x + 1)
                (newSet, splits + 1)
              | '.' | 'S' ->
                (Set.add x nextCols, splits)
              | _ -> 
                (Set.add x nextCols, splits)
          ) (Set.empty, 0)

        if Set.isEmpty nextRowCols then
          splitCount + splitsInRow
        else
          processRow (rowIdx + 1) nextRowCols (splitCount + splitsInRow)

    processRow manifold.StartY (Set.singleton manifold.StartX) 0

  /// Part 2: Count total resulting timelines (Combinatorial Paths)
  let solvePart2 (manifold: Manifold) : int64 =
    
    let addPaths (col: int) (count: int64) (accMap: Map<int, int64>) =
      if not (isValidColumn manifold col) then accMap
      else
        match Map.tryFind col accMap with
        | Some existing -> Map.add col (existing + count) accMap
        | None -> Map.add col count accMap

    let rec processRow (rowIdx: int) (activeTimelines: Map<int, int64>) =
      if rowIdx >= manifold.Height then
        activeTimelines |> Map.values |> Seq.sum
      else
        let nextRowTimelines =
          activeTimelines
          |> Map.fold (fun accMap x count ->
            match manifold.Grid.[rowIdx].[x] with
            | '^' ->
              accMap 
              |> addPaths (x - 1) count
              |> addPaths (x + 1) count
            | '.' | 'S' ->
              accMap |> addPaths x count
            | _ -> 
              accMap |> addPaths x count
          ) Map.empty

        if Map.isEmpty nextRowTimelines then
          0L
        else
          processRow (rowIdx + 1) nextRowTimelines

    let initialTimelines = Map.empty.Add(manifold.StartX, 1L)
    processRow manifold.StartY initialTimelines

// --- Main Execution ---
let main () =
  let inputPath = Path.Combine(__SOURCE_DIRECTORY__, "../inputs/day07.txt")
  
  printfn "=== Day 7: Laboratories ==="

  if not (File.Exists inputPath) then
    printfn "Error: Input file not found at %s" inputPath
  else
    // 1. Parsing Phase (No result print, just timing)
    let manifold = 
      Diagnostics.measureTime "Parsing Input" (fun () -> 
        Parsing.parseInput inputPath)
    
    printfn "Grid Dimensions: %dx%d" manifold.Width manifold.Height
    
    // 2. Part 1 Phase (Prints Result + Time)
    Diagnostics.measureResult "[Part 1] Answer" (fun () -> 
      Solution.solvePart1 manifold) 
    |> ignore
      
    // 3. Part 2 Phase (Prints Result + Time)
    Diagnostics.measureResult "[Part 2] Answer" (fun () -> 
      Solution.solvePart2 manifold)
    |> ignore

main ()