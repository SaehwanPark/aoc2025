open System
open System.IO
open System.Diagnostics

// ==========================================
// 1. Domain Modeling
// ==========================================

module Domain =
  type Tile = { X: int; Y: int }
  type Edge = Tile * Tile

// ==========================================
// 2. Parsing Logic
// ==========================================

module Parsing =
  open Domain

  let private parseLine (line: string) : Tile option =
    match line.Split(',') with
    | [| xStr; yStr |] -> 
      match Int32.TryParse(xStr.Trim()), Int32.TryParse(yStr.Trim()) with
      | (true, x), (true, y) -> Some { X = x; Y = y }
      | _ -> None
    | _ -> None

  let parseInput (input: string) : Tile list =
    input.Split('\n', StringSplitOptions.RemoveEmptyEntries)
    |> Array.toList
    |> List.choose parseLine

// ==========================================
// 3. Core Algorithms
// ==========================================

module Algorithms =
  open Domain

  /// Generates all unique combinations of size 2 from a list.
  let rec pairs list =
    match list with
    | [] -> []
    | head :: tail -> 
      (tail |> List.map (fun other -> head, other)) 
      @ (pairs tail)

  /// Calculates the inclusive area of a rectangle defined by two corners.
  /// Returns int64 to prevent overflow.
  let calculateArea (t1: Tile) (t2: Tile) : int64 =
    let width = int64 (abs (t1.X - t2.X)) + 1L
    let height = int64 (abs (t1.Y - t2.Y)) + 1L
    width * height

  /// Ray Casting Algorithm to check if a point is inside a polygon.
  /// Coordinates are passed as doubles or scaled integers to handle midpoints.
  /// This implementation uses doubles for slope calculation.
  let isPointInPolygon (px: double) (py: double) (edges: Edge list) : bool =
    let intersections =
      edges
      |> List.filter (fun (v1, v2) ->
        let v1y, v2y = double v1.Y, double v2.Y
        let v1x, v2x = double v1.X, double v2.X
        
        // Check Y-straddle (point's Y is between endpoints)
        // We use strictly greater/lesser to handle vertices exactly on the ray robustly
        ((v1y > py) <> (v2y > py)) &&
        (
          // Calculate intersection X coordinate
          let slope = (v2x - v1x) / (v2y - v1y)
          let intersectX = v1x + slope * (py - v1y)
          // Check if intersection is strictly to the right of point
          intersectX > px
        )
      )
      |> List.length

    // Odd intersections = Inside; Even = Outside
    intersections % 2 <> 0

// ==========================================
// 4. Part Solvers
// ==========================================

module Solvers =
  open Domain
  open Algorithms

  let solvePart1 (tiles: Tile list) : int64 =
    match tiles with
    | [] | [_] -> 0L
    | _ ->
      tiles
      |> pairs
      |> List.map (fun (t1, t2) -> calculateArea t1 t2)
      |> List.max

  let solvePart2 (tiles: Tile list) : int64 =
    // Pre-compute edges once
    let edges = 
      (tiles @ [List.head tiles]) 
      |> List.pairwise

    let isValidRectangle (t1: Tile) (t2: Tile) : bool =
      let xMin = min t1.X t2.X
      let xMax = max t1.X t2.X
      let yMin = min t1.Y t2.Y
      let yMax = max t1.Y t2.Y

      // 1. INTRUSION CHECK: 
      // Reject if any vertex of the polygon is strictly inside the rectangle.
      let hasIntrusion =
        tiles |> List.exists (fun t -> 
          t.X > xMin && t.X < xMax && t.Y > yMin && t.Y < yMax
        )
      
      if hasIntrusion then false
      else
        // 2. SPLITTING CHECK:
        // Reject if an edge of the polygon slices completely across the rectangle.
        let isSplit =
          edges |> List.exists (fun (v1, v2) ->
            if v1.X = v2.X then 
              // Vertical Edge
              let vx = v1.X
              let vyMin, vyMax = min v1.Y v2.Y, max v1.Y v2.Y
              // Sits between X bounds AND spans full Y height
              vx > xMin && vx < xMax && vyMin <= yMin && vyMax >= yMax
            elif v1.Y = v2.Y then
              // Horizontal Edge
              let vy = v1.Y
              let vxMin, vxMax = min v1.X v2.X, max v1.X v2.X
              // Sits between Y bounds AND spans full X width
              vy > yMin && vy < yMax && vxMin <= xMin && vxMax >= xMax
            else 
              false // Diagonal edges ignored for this specific grid problem
          )

        if isSplit then false
        else
          // 3. CENTER CHECK:
          // If no intrusion and no splitting, the center determines containment.
          let centerX = double (t1.X + t2.X) / 2.0
          let centerY = double (t1.Y + t2.Y) / 2.0
          isPointInPolygon centerX centerY edges

    // Execute Logic
    match tiles with
    | [] | [_] -> 0L
    | _ ->
      pairs tiles
      |> List.filter (fun (t1, t2) -> isValidRectangle t1 t2)
      |> List.map (fun (t1, t2) -> calculateArea t1 t2)
      |> List.max

// ==========================================
// 5. Execution & Benchmarking
// ==========================================

// Helper to time a function execution
let time (name: string) (func: unit -> 'a) =
  let sw = Stopwatch.StartNew()
  let result = func()
  sw.Stop()
  printfn "[%s] Time: %0.1f ms" name sw.Elapsed.TotalMilliseconds
  result

// Input Helper
let readInput filename =
  let path = Path.Combine(__SOURCE_DIRECTORY__, "../inputs", filename)
  if File.Exists(path) then File.ReadAllText(path)
  else failwithf "Input file not found at: %s" path

try
  printfn "=== Day 9: Movie Theater ==="
  
  // 1. Read & Parse
  let rawInput = readInput "day09.txt"
  let tiles = time "Parsing" (fun () -> Parsing.parseInput rawInput)
  printfn "Parsed %d tiles." tiles.Length

  // 2. Part 1
  let p1Result = time "Part 1" (fun () -> Solvers.solvePart1 tiles)
  printfn "[Part 1] Answer: %d" p1Result

  // 3. Part 2
  let p2Result = time "Part 2" (fun () -> Solvers.solvePart2 tiles)
  printfn "[Part 2] Answer: %d" p2Result

  printfn "\nDone."

with
| ex -> printfn "Error: %s" ex.Message