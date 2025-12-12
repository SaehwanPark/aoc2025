open System
open System.IO

// ========================================================
// 1. CONFIGURATION & INFRASTRUCTURE
// ========================================================

let INPUT_FILE = Path.Combine(__SOURCE_DIRECTORY__, "../inputs/day01.txt")

// ========================================================
// 2. DOMAIN TYPES
// ========================================================

type TurnDirection = Left | Right

type Instruction = {
  Direction: TurnDirection
  Amount: int
}

type DialState = {
  CurrentNumber: int
  Score: int 
}

// ========================================================
// 3. PARSING & MATH HELPERS
// ========================================================

/// Canonical modulo to handle negative wrapping correctly.
/// Formula: ((a % b) + b) % b
let inline (%!) a b = ((a % b) + b) % b

let parseInstruction (line: string) =
  if String.IsNullOrWhiteSpace(line) then failwith "Invalid empty line"
  let dir = if line.[0] = 'L' then Left else Right
  let amt = line.Substring(1) |> int
  { Direction = dir; Amount = amt }

// ========================================================
// 4. ALGORITHMS
// ========================================================

/// Logic for Part 1: Only counts if we land on 0 at the END of a turn.
let applyMovePart1 (state: DialState) (move: Instruction) =
  let nextPos = 
    match move.Direction with
    | Right -> (state.CurrentNumber + move.Amount) %! 100
    | Left  -> (state.CurrentNumber - move.Amount) %! 100
  
  let hit = if nextPos = 0 then 1 else 0
  { CurrentNumber = nextPos; Score = state.Score + hit }

/// Logic for Part 2: Counts EVERY time we pass through or land on 0.
let applyMovePart2 (state: DialState) (move: Instruction) =
  // 1. Count full rotations (guaranteed hits)
  let fullLoops = move.Amount / 100
  let remainder = move.Amount % 100
  
  // 2. Check partial rotation for crossing 0
  // Note: We perform check BEFORE normalizing the new position
  let crossingHit = 
    match move.Direction with
    | Right -> 
        // Logic: Did we overflow 99?
        if state.CurrentNumber + remainder >= 100 then 1 else 0
    | Left ->
        // Logic: Did we underflow 0? 
        // Guard: If we are already at 0, moving left goes to 99 (no hit)
        if state.CurrentNumber > 0 && (state.CurrentNumber - remainder) <= 0 then 1 else 0

  // 3. Update Position
  let rawNewPos = 
    match move.Direction with
    | Right -> state.CurrentNumber + remainder
    | Left  -> state.CurrentNumber - remainder
  
  { CurrentNumber = rawNewPos %! 100
    Score = state.Score + fullLoops + crossingHit }

// ========================================================
// 5. SOLVERS
// ========================================================

let solve strategy inputs =
  let initialState = { CurrentNumber = 50; Score = 0 }
  
  inputs
  |> Seq.map parseInstruction
  |> Seq.fold strategy initialState
  |> fun s -> s.Score

// ========================================================
// 6. EXECUTION
// ========================================================

if File.Exists(INPUT_FILE) then
  // printfn "Reading input from: %s" INPUT_FILE
  let lines = 
    File.ReadLines(INPUT_FILE) 
    |> Seq.filter (fun s -> not (String.IsNullOrWhiteSpace s))
    |> Seq.cache // Cache sequence to iterate twice

  printfn "=== Day 1: Secret Entrance ==="

  // Measure Part 1
  let p1Start = DateTime.Now
  let result1 = solve applyMovePart1 lines
  printfn "[Part 1] Answer: %d (Time: %.1f ms)" result1 (DateTime.Now - p1Start).TotalMilliseconds

  // Measure Part 2
  let p2Start = DateTime.Now
  let result2 = solve applyMovePart2 lines
  printfn "[Part 2] Answer: %d (Time: %.1f ms)" result2 (DateTime.Now - p2Start).TotalMilliseconds

else
  printfn "Error: Input file not found."