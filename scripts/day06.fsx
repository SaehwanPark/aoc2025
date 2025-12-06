(* Day 6: Trash Compactor - Consolidated Solution
   Run with: dotnet fsi Day06.fsx
*)

open System
open System.IO
open System.Diagnostics

// ==========================================
// 1. Domain Modeling
// ==========================================
module Domain =
  type Operation =
    | Add
    | Multiply

  type MathProblem = {
    Numbers : int64 list
    Operator : Operation
  }

// ==========================================
// 2. Grid & Parsing Utilities
// ==========================================
module GridTools =
  /// Converts raw lines into a rectangular character grid (padded with spaces).
  let toCharGrid (lines: string array) : char array array =
    if lines.Length = 0 then [||]
    else
      let width = lines |> Array.map _.Length |> Array.max
      lines |> Array.map (fun line -> line.PadRight(width).ToCharArray())

  /// Checks if a specific column index contains only whitespace.
  let isColumnEmpty (grid: char array array) (colIndex: int) : bool =
    grid |> Array.forall (fun row -> row[colIndex] = ' ')

  /// Helper to extract the operator symbol from the bottom row of a specific column range.
  let extractOperator (grid: char array array) (startCol: int) (endCol: int) =
    let lastRowIdx = grid.Length - 1
    let slice = String(grid.[lastRowIdx].[startCol .. endCol])
    if slice.Contains("+") then Domain.Add
    elif slice.Contains("*") then Domain.Multiply
    else failwithf "No operator found in columns %d-%d" startCol endCol

// ==========================================
// 3. Parsing Strategies
// ==========================================
module Parser =
  open Domain
  open GridTools

  /// Part 1 Logic: Numbers are horizontal rows within the block.
  let extractBlockRowWise (grid: char array array) (startCol: int) (endCol: int) : MathProblem =
    let op = extractOperator grid startCol endCol
    
    // Rows 0 to N-2 are numbers
    let numbers = 
      [| 0 .. grid.Length - 2 |]
      |> Array.choose (fun r ->
        let raw = String(grid.[r].[startCol .. endCol]).Trim()
        match Int64.TryParse(raw) with
        | true, v -> Some v
        | _ -> None 
      )
      |> Array.toList

    { Numbers = numbers; Operator = op }

  /// Part 2 Logic: Numbers are vertical columns, read Right-to-Left.
  let extractBlockColWise (grid: char array array) (startCol: int) (endCol: int) : MathProblem =
    let op = extractOperator grid startCol endCol
    
    // Iterate columns Right -> Left
    let numbers = 
      [| endCol .. -1 .. startCol |]
      |> Array.choose (fun col ->
        let verticalDigits = 
          [| 0 .. grid.Length - 2 |] // Skip operator row
          |> Array.map (fun row -> grid.[row].[col])
          |> Array.filter Char.IsDigit
        
        if verticalDigits.Length = 0 then None
        else
          let numStr = String(verticalDigits)
          match Int64.TryParse(numStr) with
          | true, v -> Some v
          | _ -> None
      )
      |> Array.toList

    { Numbers = numbers; Operator = op }

  /// Generic function to scan the grid and apply an extraction strategy
  let parseInput (lines: string array) (extractor: char[][] -> int -> int -> MathProblem) : MathProblem list =
    let grid = toCharGrid lines
    if grid.Length = 0 then []
    else
      let width = grid.[0].Length
      
      // Recursive tail-call loop to find blocks separated by empty columns
      let rec scan col startCol acc =
        if col >= width then
          // End of grid
          if col > startCol then (extractor grid startCol (col - 1)) :: acc
          else acc
        elif isColumnEmpty grid col then
          // Separator found
          if col > startCol then
            let problem = extractor grid startCol (col - 1)
            scan (col + 1) (col + 1) (problem :: acc)
          else
            // Moving through whitespace
            scan (col + 1) (col + 1) acc
        else
          // Inside a block
          scan (col + 1) startCol acc

      scan 0 0 [] |> List.rev

// ==========================================
// 4. Solver Core
// ==========================================
module Solver =
  open Domain

  let solveProblem (problem: MathProblem) : int64 =
    match problem.Numbers with
    | [] -> 0L
    | head :: tail ->
        match problem.Operator with
        | Add -> List.fold (+) head tail
        | Multiply -> List.fold (*) head tail

  let calculateGrandTotal (problems: MathProblem list) : int64 =
    problems |> List.map solveProblem |> List.sum

// ==========================================
// 5. Execution & Benchmarking
// ==========================================

let measureTime (label: string) (action: unit -> 'a) : 'a =
  let sw = Stopwatch.StartNew()
  let result = action()
  sw.Stop()
  printfn "[%s] Time: %0.4f ms" label (sw.Elapsed.TotalMilliseconds)
  result

let inputPath = Path.Combine(__SOURCE_DIRECTORY__, "../inputs/day06.txt")

let run () =
  try
    let lines = File.ReadAllLines(inputPath)
    printfn "=== Day 6: Trash Compactor ==="
    // printfn "Loaded input: %d lines." lines.Length

    // --- Part 1 ---
    let part1Result = measureTime "Part 1" (fun () ->
      let problems = Parser.parseInput lines Parser.extractBlockRowWise
      Solver.calculateGrandTotal problems
    )
    printfn "[Part 1] Answer: %d" part1Result
    printfn ""

    // --- Part 2 ---
    let part2Result = measureTime "Part 2" (fun () ->
      let problems = Parser.parseInput lines Parser.extractBlockColWise
      Solver.calculateGrandTotal problems
    )
    printfn "[Part 2] Answer: %d" part2Result

  with
  | :? FileNotFoundException -> printfn "Error: Input file not found at %s" inputPath
  | ex -> printfn "Error: %s" ex.Message

run ()