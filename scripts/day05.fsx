open System
open System.IO
open System.Diagnostics

// ==========================================
// 1. Domain Modeling
// ==========================================

type IngredientRange = {
  Start: int64
  End: int64
}

type InventoryData = {
  FreshRanges: IngredientRange list
  AvailableIds: int64 list
}

// ==========================================
// 2. Parsing Logic
// ==========================================

module Parser =
  let private parseRange (line: string) =
    match line.Split('-') with
    | [| s; e |] -> { Start = int64 s; End = int64 e }
    | _ -> failwithf "Invalid range format: %s" line

  let parse (filePath: string) =
    let content = File.ReadAllText(filePath).Trim()
    let blocks = content.Split([| "\r\n\r\n"; "\n\n" |], StringSplitOptions.None)
    
    let ranges = 
      blocks.[0].Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
      |> Array.map parseRange
      |> Array.toList

    let ids = 
      blocks.[1].Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
      |> Array.map int64
      |> Array.toList

    { FreshRanges = ranges; AvailableIds = ids }

// ==========================================
// 3. Algorithms
// ==========================================

module Algorithms =
  
  /// Sorts and merges overlapping/adjacent intervals into disjoint intervals.
  /// Complexity: O(N log N)
  let mergeRanges (ranges: IngredientRange list) =
    ranges
    |> List.sortBy (fun r -> r.Start)
    |> List.fold (fun acc current ->
        match acc with
        | [] -> [current]
        | last :: rest ->
            // Check for overlap or immediate adjacency (e.g., 5-6 and 7-8)
            if current.Start <= (last.End + 1L) then
              // Merge: Extend the end of the last range
              { last with End = max last.End current.End } :: rest
            else
              // Disjoint: Add as new range
              current :: acc
    ) []
    |> List.rev 

  /// Checks if an ID exists within a list of ranges (Linear Scan)
  let isIdFresh (ranges: IngredientRange list) (id: int64) =
    ranges |> List.exists (fun r -> id >= r.Start && id <= r.End)

  /// Calculates total integer count in a range
  let rangeLength (r: IngredientRange) =
    r.End - r.Start + 1L

// ==========================================
// 4. Solvers
// ==========================================

let solvePart1 (data: InventoryData) (mergedRanges: IngredientRange list) =
  data.AvailableIds
  |> List.filter (Algorithms.isIdFresh mergedRanges)
  |> List.length

let solvePart2 (mergedRanges: IngredientRange list) =
  mergedRanges
  |> List.map Algorithms.rangeLength
  |> List.sum

// ==========================================
// 5. Execution & Benchmarking
// ==========================================

let time (name: string) (f: unit -> 'T) =
  let sw = Stopwatch.StartNew()
  let result = f ()
  sw.Stop()
  printfn "%s: %A (%.1f ms)" name result sw.Elapsed.TotalMilliseconds
  result

let main () =
  printfn "=== Day 5: Cafeteria ==="
  let inputPath = Path.Combine(__SOURCE_DIRECTORY__, "../inputs/day05.txt")
  let data = Parser.parse inputPath
  printfn "Input Loaded: %d ranges, %d IDs" data.FreshRanges.Length data.AvailableIds.Length

  // 1. Measure Preprocessing (Merge)
  // We want to see how much the search space was reduced and how long it took
  let sw = Stopwatch.StartNew()
  let mergedRanges = Algorithms.mergeRanges data.FreshRanges
  sw.Stop()
  
  printfn "Preprocessing (Merge): Reduced %d ranges to %d disjoint ranges (%.1f ms)" 
    data.FreshRanges.Length 
    mergedRanges.Length 
    sw.Elapsed.TotalMilliseconds

  // 2. Measure Part 1 (Query)
  time "[Part 1] Answer" (fun () -> solvePart1 data mergedRanges) |> ignore

  // 3. Measure Part 2 (Calculation)
  time "[Part 2] Answer" (fun () -> solvePart2 mergedRanges) |> ignore

main ()