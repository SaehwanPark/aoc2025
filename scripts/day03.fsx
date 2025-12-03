open System
open System.IO
open System.Diagnostics

// Set execution context to the script's directory
Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

// ==========================================
// 1. Domain Modeling
// ==========================================

type Bank = {
  Digits : int list
}

// ==========================================
// 2. Core Logic
// ==========================================

module BatteryLogic =

  /// Part 1: Select 2 digits to make the largest number.
  let solvePart1 (bank: Bank) : int =
    let arr = List.toArray bank.Digits
    let len = arr.Length
    
    if len < 2 then 0
    else
      // 1. Find max tens in range [0 .. len-2]
      let maxTens = 
        arr 
        |> Array.take (len - 1) 
        |> Array.max
      
      // 2. Find first index of that max tens
      let tensIdx = 
        arr 
        |> Array.findIndex (fun x -> x = maxTens)
      
      // 3. Find max ones in range [tensIdx+1 .. end]
      let maxOnes = 
        arr 
        |> Array.skip (tensIdx + 1) 
        |> Array.max
        
      (maxTens * 10) + maxOnes

  /// Part 2: Select 12 digits to make the largest number.
  /// Strategy: Monotonic Decreasing Stack (Greedy).
  let solvePart2 (bank: Bank) : int64 =
    let targetLength = 12
    let n = bank.Digits.Length
    let totalDropsAllowed = n - targetLength

    let rec buildStack input stack dropsLeft =
      match input with
      | [] -> stack
      | d :: rest ->
          // Pop while top < current and drops > 0
          let rec pop st dr =
            match st with
            | top :: tail when top < d && dr > 0 -> 
                pop tail (dr - 1)
            | _ -> (st, dr)
            
          let (newStack, newDrops) = pop stack dropsLeft
          buildStack rest (d :: newStack) newDrops

    let rawStack = buildStack bank.Digits [] totalDropsAllowed
    
    let resultString =
      rawStack
      |> List.rev
      |> List.take targetLength
      |> List.map string
      |> String.concat ""
      
    int64 resultString

// ==========================================
// 3. Input Parsing
// ==========================================

module InputParser =
  
  let parseLine (line: string) : Bank =
    let digits =
      line.Trim()
      |> Seq.map (fun c -> int c - int '0')
      |> Seq.toList
    { Digits = digits }

  let loadBanks (filePath: string) : Bank list =
    File.ReadAllLines(filePath)
    |> Array.filter (fun line -> not (String.IsNullOrWhiteSpace(line)))
    |> Array.map parseLine
    |> Array.toList

// ==========================================
// 4. Execution & Timing
// ==========================================

/// Helper to measure execution time of a specific function
let measure label action =
  let sw = Stopwatch.StartNew()
  let result = action()
  sw.Stop()
  printfn "[%s] Result: %A (Time: %0.4f ms)" label result sw.Elapsed.TotalMilliseconds
  result

let run () =
  let inputPath = Path.Combine("..", "inputs", "day03.txt")
  
  if File.Exists(inputPath) then
    printfn "--- Day 3: Lobby ---"

    // Load data silently
    let banks = InputParser.loadBanks inputPath

    // Measure Part 1
    measure "Part 1" (fun () ->
      banks 
      |> List.sumBy (fun b -> int64 (BatteryLogic.solvePart1 b))
    ) |> ignore

    // Measure Part 2
    measure "Part 2" (fun () ->
      banks 
      |> List.sumBy BatteryLogic.solvePart2
    ) |> ignore
    
  else
    printfn "Error: Input file not found at %s" inputPath

run()