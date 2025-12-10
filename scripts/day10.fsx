(* File: day10.fsx *)
open System
open System.Collections.Generic
open System.Text.RegularExpressions
open System.Diagnostics

// ==========================================
// 1. Domain Modeling
// ==========================================

module Domain =
  type LightState = uint64

  type Machine = {
    // Part 1: Light Configuration
    LightsTarget: LightState
    LightsButtons: LightState list
    
    // Part 2: Joltage Configuration
    JoltageTarget: float[]   // Vector b
    JoltageButtons: int[][]  // Raw button effects (indices)
    
    // Meta
    OriginalLine: string
  }

// ==========================================
// 2. Parsing Strategy
// ==========================================

module Parsing =
  
  // Helper: Convert string indices "0,2" to int array
  let parseIndices (s: string) =
    s.Split(',', StringSplitOptions.RemoveEmptyEntries) 
    |> Array.map int

  // Helper: Convert string "##.." to bitmask
  let parseDiagram (s: string) : Domain.LightState =
    s |> Seq.mapi (fun i c -> if c = '#' then (1UL <<< i) else 0UL)
    |> Seq.fold (|||) 0UL

  // Helper: Convert int array [0; 2] to bitmask
  let indicesToMask (indices: int[]) : Domain.LightState =
    indices 
    |> Array.map (fun shift -> 1UL <<< shift) 
    |> Array.fold (|||) 0UL

  let parseLine (line: string): Domain.Machine =
    // Regex patterns
    let diagramMatch = Regex.Match(line, @"\[([.#]+)\]")
    let targetMatch = Regex.Match(line, @"\{([\d,]+)\}")
    let buttonMatches = Regex.Matches(line, @"\(([\d,]+)\)")

    if not (diagramMatch.Success && targetMatch.Success) then
      failwithf "Invalid line format: %s" line

    // 1. Parse Light Data (Part 1)
    let diagramStr = diagramMatch.Groups.[1].Value
    let lightsTarget = parseDiagram diagramStr
    
    // 2. Parse Joltage Data (Part 2)
    let joltageTarget = 
      targetMatch.Groups.[1].Value.Split(',', StringSplitOptions.RemoveEmptyEntries)
      |> Array.map float

    // 3. Parse Buttons (Shared)
    let rawButtons = 
      [| for m in buttonMatches -> parseIndices m.Groups.[1].Value |]
    
    let lightsButtons = 
      rawButtons 
      |> Array.map indicesToMask 
      |> Array.toList

    { 
      LightsTarget = lightsTarget
      LightsButtons = lightsButtons
      JoltageTarget = joltageTarget
      JoltageButtons = rawButtons
      OriginalLine = line
    }

  let loadInput (filePath: string) =
    if not (System.IO.File.Exists filePath) then failwithf "File not found: %s" filePath
    
    System.IO.File.ReadAllLines(filePath)
    // Filter out empty lines or metadata lines starting with 
    |> Array.filter (fun s -> 
      not (String.IsNullOrWhiteSpace s) && 
      not (s.Trim().StartsWith("[source"))
    )
    |> Array.map parseLine

// ==========================================
// 3. Algorithms & Solvers
// ==========================================

module Solvers =
  
  // --- Part 1: BFS ---
  let solvePart1 (machine: Domain.Machine) : int =
    if machine.LightsTarget = 0UL then 0
    else
      let q = Queue<Domain.LightState * int>()
      q.Enqueue(0UL, 0)
      let visited = HashSet<Domain.LightState>()
      visited.Add(0UL) |> ignore
      
      let mutable result = None
      
      while q.Count > 0 && result.IsNone do
        let (curr, presses) = q.Dequeue()
        for btn in machine.LightsButtons do
          let next = curr ^^^ btn
          if next = machine.LightsTarget then result <- Some (presses + 1)
          elif visited.Add(next) then q.Enqueue(next, presses + 1)
      
      // Return 0 if unsolvable (though puzzle implies solvability)
      defaultArg result 0

  // --- Part 2: Linear Algebra ---
  
  module LinAlg =
    let epsilon = 1e-4
    let isInt (v: float) = abs(v - round(v)) < epsilon
    let isNonNegative (v: float) = v > -epsilon

    // Solves Ax = b using Gaussian Elimination + Search
    let solveSystem (m: Domain.Machine) : int64 option =
      let rows = m.JoltageTarget.Length
      let cols = m.JoltageButtons.Length
      
      // Build Matrix [A|b]
      // A[r][c] = 1.0 if Button c affects Counter r
      let augmented = Array.init rows (fun r -> 
        let rowData = Array.zeroCreate<float> (cols + 1)
        for c in 0 .. cols - 1 do
          if Array.contains r m.JoltageButtons.[c] then
            rowData.[c] <- 1.0
        rowData.[cols] <- m.JoltageTarget.[r]
        rowData
      )

      // 1. Gaussian Elimination (RREF)
      let mutable pivotRow = 0
      let pivotCols = Array.create rows -1

      for col = 0 to cols - 1 do
        if pivotRow < rows then
          let mutable searchRow = pivotRow
          while searchRow < rows && abs(augmented.[searchRow].[col]) < epsilon do
            searchRow <- searchRow + 1
          
          if searchRow < rows then
            // Swap
            let tmp = augmented.[pivotRow]
            augmented.[pivotRow] <- augmented.[searchRow]
            augmented.[searchRow] <- tmp
            
            // Normalize
            let pivotVal = augmented.[pivotRow].[col]
            for j = col to cols do augmented.[pivotRow].[j] <- augmented.[pivotRow].[j] / pivotVal
            
            // Eliminate
            for r = 0 to rows - 1 do
              if r <> pivotRow then
                let f = augmented.[r].[col]
                for j = col to cols do augmented.[r].[j] <- augmented.[r].[j] - f * augmented.[pivotRow].[j]
            
            pivotCols.[pivotRow] <- col
            pivotRow <- pivotRow + 1

      // 2. Consistency Check
      let mutable consistent = true
      for r = pivotRow to rows - 1 do
        if abs(augmented.[r].[cols]) > epsilon then consistent <- false
      
      if not consistent then None
      else
        // 3. Parametric Search (Free Variables)
        let isPivot = Array.zeroCreate cols
        for r = 0 to pivotRow - 1 do 
          if pivotCols.[r] >= 0 then isPivot.[pivotCols.[r]] <- true
        
        let freeCols = [| 0 .. cols - 1 |] |> Array.filter (fun c -> not isPivot.[c])

        // Evaluates a specific assignment of free variables
        let evaluate (freeVals: int[]) =
          let solution = Array.zeroCreate<float> cols
          for i = 0 to freeCols.Length - 1 do solution.[freeCols.[i]] <- float freeVals.[i]
          
          let mutable valid = true
          for r = pivotRow - 1 downto 0 do
            let pCol = pivotCols.[r]
            let constant = augmented.[r].[cols]
            let mutable sumFree = 0.0
            for fc in freeCols do
              if fc > pCol then 
                sumFree <- sumFree + (augmented.[r].[fc] * solution.[fc])
            
            let valPivot = constant - sumFree
            if not (isInt valPivot && isNonNegative valPivot) then valid <- false
            solution.[pCol] <- valPivot
          
          if valid then Some (solution |> Array.sumBy (round >> int64))
          else None

        // Search Range: Max target value + buffer
        let maxTarget = (m.JoltageTarget |> Array.max |> int) + 5
        let mutable minTotal = Int64.MaxValue
        let mutable found = false

        let rec search idx current =
          if idx = freeCols.Length then
            match evaluate (List.toArray current) with
            | Some t -> if t < minTotal then minTotal <- t; found <- true
            | None -> ()
          else
            // Optimization: if freeCols is empty, loop runs once
            for v = 0 to maxTarget do search (idx + 1) (current @ [v])
        
        search 0 []
        if found then Some minTotal else None

  let solvePart2 (machine: Domain.Machine) : int64 =
    match LinAlg.solveSystem machine with
    | Some presses -> presses
    | None -> 0L

// ==========================================
// 4. Execution & Utilities
// ==========================================

let measureTime (label: string) (action: unit -> 'a) : 'a =
  let sw = Stopwatch.StartNew()
  let result = action()
  sw.Stop()
  printfn "[%s] Time: %0.4f ms" label sw.Elapsed.TotalMilliseconds
  result

// Main Workflow
let inputPath = __SOURCE_DIRECTORY__ + "/../inputs/day10.txt"

printfn "=== Day 10: Factory ==="

if System.IO.File.Exists inputPath then
  
  // 1. Parsing
  let machines = 
    measureTime "Parsing" (fun () -> 
      Parsing.loadInput inputPath
    )
  printfn "Loaded %d machines." machines.Length
  printfn ""

  // 2. Part 1
  let part1Result = 
    measureTime "Part 1 Logic" (fun () -> 
      machines 
      |> Array.map Solvers.solvePart1 
      |> Array.sum
    )
  printfn "Part 1 Answer: %d" part1Result
  printfn ""

  // 3. Part 2
  let part2Result = 
    measureTime "Part 2 Logic" (fun () -> 
      machines 
      |> Array.map Solvers.solvePart2 
      |> Array.sum
    )
  printfn "Part 2 Answer: %d" part2Result

else
  printfn "Error: Input file not found at %s" inputPath