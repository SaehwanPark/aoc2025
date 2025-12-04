open System
open System.IO
open System.Diagnostics

// ==========================================
// 1. Domain Modeling
// ==========================================
type Position = { 
  Row: int 
  Col: int 
}

// Sparse representation of the grid using an immutable Set
type PaperGrid = Set<Position>

// ==========================================
// 2. Parsing Logic
// ==========================================
module Parser =
  let parseInput (lines: string seq) : PaperGrid =
    lines
    |> Seq.indexed
    |> Seq.collect (fun (r, line) ->
      line
      |> Seq.indexed
      |> Seq.choose (fun (c, char) ->
        match char with
        | '@' -> Some { Row = r; Col = c }
        | _ -> None
      )
    )
    |> Set.ofSeq

// ==========================================
// 3. Core Logic
// ==========================================
module Logic =
  
  // Pre-calculated offsets for the 8 neighbors
  let private neighborOffsets = 
    [ (-1, -1); (-1, 0); (-1, 1)
      ( 0, -1);          ( 0, 1)
      ( 1, -1); ( 1, 0); ( 1, 1) ]

  // Generates 8 neighbor coordinates
  let private getNeighbors (pos: Position) : Position list =
    neighborOffsets
    |> List.map (fun (dr, dc) -> 
      { Row = pos.Row + dr; Col = pos.Col + dc }
    )

  // Determines if a roll is accessible (fewer than 4 neighbors)
  let private isAccessible (grid: PaperGrid) (pos: Position) : bool =
    let neighborCount =
      getNeighbors pos
      |> List.filter (fun n -> Set.contains n grid)
      |> List.length
    
    neighborCount < 4

  // Part 1: Count accessible rolls in the initial state
  let solvePart1 (grid: PaperGrid) : int =
    grid
    |> Set.filter (isAccessible grid)
    |> Set.count

  // Part 2: Recursively remove accessible rolls until stable
  let solvePart2 (initialGrid: PaperGrid) : int =
    
    let rec simulateGeneration (currentGrid: PaperGrid) (totalRemoved: int) =
      // Identify all rolls to be removed in this generation simultaneously
      let toRemove = 
        currentGrid 
        |> Set.filter (isAccessible currentGrid)

      if Set.isEmpty toRemove then
        totalRemoved // Base case: stability reached
      else
        let nextGrid = Set.difference currentGrid toRemove
        simulateGeneration nextGrid (totalRemoved + Set.count toRemove)

    simulateGeneration initialGrid 0

// ==========================================
// 4. Execution & Benchmarking
// ==========================================
let measure label action =
  let sw = Stopwatch.StartNew()
  let result = action()
  sw.Stop()
  printfn "%s: %A (Time: %d ms)" label result sw.ElapsedMilliseconds
  result

let getFilePath fileName =
  Path.Combine(__SOURCE_DIRECTORY__, "../inputs", fileName)

let main () =
  let inputPath = getFilePath "day04.txt"
  
  match File.Exists inputPath with
  | false -> 
      printfn "Error: Input file not found at %s" inputPath
  | true ->
      // Parse silently to avoid cluttering output
      let lines = File.ReadAllLines inputPath
      let grid = Parser.parseInput lines

      printfn "=== Day 4: Printing Department ==="

      // Measure Part 1
      let _ = measure "Part 1 Result" (fun () -> Logic.solvePart1 grid)

      // Measure Part 2
      let _ = measure "Part 2 Result" (fun () -> Logic.solvePart2 grid)
      
      ()

// Execute
main ()