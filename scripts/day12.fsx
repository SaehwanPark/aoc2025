(*
  Day 12: Christmas Tree Farm (Final with Instrumentation)
  Solution by Senior F# Engineer
  
  Algorithm: 
  - Constraint Satisfaction Problem (CSP) using Bitmask Backtracking.
  - Metrics: Separate timing for parsing/prep vs. constraint solving.
*)

open System
open System.IO
open System.Numerics

// ==========================================
// 1. Domain Modeling
// ==========================================

type Coord = int * int

type ShapeDef = {
  Id: int
  Points: Set<Coord>
}

// A specific, normalized orientation of a shape (rotated/flipped)
type Variant = {
  OriginalId: int
  Width: int
  Height: int
  Points: Coord list
}

type Region = {
  Width: int
  Height: int
  PresentsToFit: int list // Flattened list of Shape IDs required
}

// ==========================================
// 2. Functional Parsing
// ==========================================

module Parser =
  
  let private parseShapeBlock (shapeId: int) (lines: string list) =
    let points = 
      lines 
      |> List.mapi (fun r line -> 
        line 
        |> Seq.mapi (fun c char -> if char = '#' then Some(r, c) else None) 
        |> Seq.choose id)
      |> Seq.concat
      |> Set.ofSeq
    { Id = shapeId; Points = points }

  let parseInput (lines: string[]) =
    // 1. Separate Shape lines from Region lines
    let shapeLines, regionLines =
      lines
      |> Array.filter (fun s -> not (String.IsNullOrWhiteSpace s))
      |> Array.partition (fun s -> not (s.Contains("x") && s.Contains(":")))

    // 2. Parse Shapes
    let shapes = 
      let blocks = 
        shapeLines 
        |> Seq.fold (fun (acc: (int * string list) list) line ->
          if line.EndsWith(":") then
            let newId = line.TrimEnd(':') |> int
            (newId, []) :: acc
          else
            match acc with
            | (currId, lines) :: rest -> (currId, line :: lines) :: rest
            | [] -> acc 
        ) []
      
      blocks 
      |> List.map (fun (id, lineBlock) -> parseShapeBlock id (List.rev lineBlock))
      |> List.rev

    // 3. Parse Regions
    let regions = 
      regionLines
      |> Array.map (fun line ->
        // Format: "12x5: 1 0 1 0 2 2"
        let parts = line.Split(':')
        let dimParts = parts.[0].Split('x')
        let w, h = int dimParts.[0], int dimParts.[1]
        
        let counts = 
          parts.[1].Trim().Split([|' '|], StringSplitOptions.RemoveEmptyEntries)
          |> Array.map int
        
        let presentList = 
          counts
          |> Array.mapi (fun shapeId count -> List.replicate count shapeId)
          |> List.concat
        
        { Width = w; Height = h; PresentsToFit = presentList }
      )
      |> Array.toList

    shapes, regions

// ==========================================
// 3. Geometry & Bitmasks
// ==========================================

module Geometry =
  
  let normalize (points: Set<Coord>) =
    if Set.isEmpty points then [], 0, 0
    else
      let minR = points |> Seq.map fst |> Seq.min
      let minC = points |> Seq.map snd |> Seq.min
      let newPoints = points |> Set.map (fun (r,c) -> r - minR, c - minC)
      let maxR = newPoints |> Seq.map fst |> Seq.max
      let maxC = newPoints |> Seq.map snd |> Seq.max
      (Set.toList newPoints), (maxC + 1), (maxR + 1)

  let rotate90 (points: Set<Coord>) = points |> Set.map (fun (r,c) -> (c, -r))
  let flip (points: Set<Coord>) = points |> Set.map (fun (r,c) -> (r, -c))

  let generateVariants (shape: ShapeDef) : Variant list =
    let basePoints = shape.Points
    let rotations p = [
      p; p |> rotate90; p |> rotate90 |> rotate90; p |> rotate90 |> rotate90 |> rotate90
    ]
    
    (rotations basePoints) @ (rotations (flip basePoints))
    |> List.map normalize
    |> List.distinct 
    |> List.map (fun (pts, w, h) -> 
      { OriginalId = shape.Id; Width = w; Height = h; Points = pts })

  let inline pointToBit (gridW: int) (r: int, c: int) =
    let index = r * gridW + c
    BigInteger.One <<< index

  let getPlacementMask (gridW: int) (gridH: int) (variant: Variant) (r: int, c: int) =
    if r + variant.Height > gridH || c + variant.Width > gridW then
      None
    else
      let mutable mask = BigInteger.Zero
      for (pr, pc) in variant.Points do
        mask <- mask ||| (pointToBit gridW (r + pr, c + pc))
      Some mask

// ==========================================
// 4. Constraint Solver
// ==========================================

module Solver = 
  
  let prepareVariants (shapes: ShapeDef list) =
    shapes 
    |> List.map (fun s -> s.Id, Geometry.generateVariants s)
    |> Map.ofList

  // Precompute valid moves (bitmasks) for every shape on the specific grid dimensions
  let private buildMoveCache (variantsMap: Map<int, Variant list>) (presentIds: int list) (w: int) (h: int) =
    let distinctIds = presentIds |> List.distinct
    
    distinctIds
    |> List.map (fun id ->
      let vars = variantsMap.[id]
      let validMasks = 
        [ for v in vars do
          for r in 0 .. (h - v.Height) do
            for c in 0 .. (w - v.Width) do
              match Geometry.getPlacementMask w h v (r, c) with
              | Some m -> yield m
              | None -> () 
        ]
      id, validMasks
    )
    |> Map.ofList

  let solveRegion (variantsMap: Map<int, Variant list>) (region: Region) : bool =
    let gridArea = region.Width * region.Height
    
    let getArea (v: Variant) = v.Points.Length
    let shapeAreas = variantsMap |> Map.map (fun _ vars -> getArea vars.Head)
    let requiredArea = region.PresentsToFit |> List.sumBy (fun id -> shapeAreas.[id])
    
    if requiredArea > gridArea then 
      false
    else
      // Sort: Largest first to fail fast
      let sortedPresents = 
        region.PresentsToFit
        |> List.sortByDescending (fun id -> shapeAreas.[id])

      let moveCache = buildMoveCache variantsMap sortedPresents region.Width region.Height
      
      if sortedPresents |> List.exists (fun id -> moveCache.[id].IsEmpty) then
        false
      else
        let rec search (remaining: int list) (occupied: BigInteger) =
          match remaining with
          | [] -> true 
          | currentId :: rest ->
            let possibleMoves = moveCache.[currentId]
            
            let rec tryMoves moves =
              match moves with
              | [] -> false
              | mask :: tail ->
                if (occupied &&& mask).IsZero then
                  if search rest (occupied ||| mask) then true
                  else tryMoves tail
                else
                  tryMoves tail
            
            tryMoves possibleMoves

        search sortedPresents BigInteger.Zero

// ==========================================
// 5. Execution & Instrumentation
// ==========================================

let inputPath = Path.Combine(__SOURCE_DIRECTORY__, "../inputs/day12.txt")

printfn "=== Day 12: Christmas Tree Farm ==="

if File.Exists inputPath then
  printfn "Reading input from: %s" inputPath
  let lines = File.ReadAllLines inputPath
  
  let sw = System.Diagnostics.Stopwatch.StartNew()
  
  // --- Phase 1: Parsing & Prep ---
  let shapes, regions = Parser.parseInput lines
  let variantsMap = Solver.prepareVariants shapes
  
  sw.Stop()
  let prepTime = sw.Elapsed.TotalMilliseconds
  printfn "Parsing & Prep Time: %0.4f ms" prepTime
  
  // --- Phase 2: Solving ---
  sw.Restart()
  
  // To ensure accuracy, we force evaluation with Seq.length
  let result = 
    regions 
    |> Seq.filter (fun r -> Solver.solveRegion variantsMap r)
    |> Seq.length
    
  sw.Stop()
  let solveTime = sw.Elapsed.TotalMilliseconds
  
  printfn "Solving Time     : %0.4f ms" solveTime
  printfn "------------------------------------------------"
  printfn "Total Time     : %0.4f ms" (prepTime + solveTime)
  printfn "Final Result     : %d" result
else
  printfn "Error: Input file not found at %s" inputPath