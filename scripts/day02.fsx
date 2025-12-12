open System
open System.IO
open System.Diagnostics

// ==========================================
// 1. Domain Modeling
// ==========================================

type ProductId = int64

type SearchRange = {
    Start: ProductId
    End: ProductId
}

// ==========================================
// 2. Logic & Algorithms (The Validator)
// ==========================================

module Validator =

    /// Helper: Returns the total number of digits in a positive number.
    let inline digitCount (n: int64) =
        if n = 0L then 1
        else (Math.Log10(float n) |> int) + 1

    /// Part 1: ID is valid if it is a sequence repeated EXACTLY TWICE.
    let isValidPart1 (id: ProductId) : bool =
        if id < 10L then false
        else
            let d = digitCount id
            if d % 2 <> 0 then false
            else
                let halfLen = d / 2
                let divisor = (pown 10L halfLen) + 1L
                id % divisor = 0L

    /// Part 2: ID is valid if it is a sequence repeated TWO OR MORE times.
    let isValidPart2 (id: ProductId) : bool =
        if id < 10L then false
        else
            let d = digitCount id
            
            // Generate valid sub-pattern lengths (L)
            let possibleSubLengths = 
                seq { 
                    for l in 1 .. d / 2 do
                        if d % l = 0 then yield l 
                }

            // Check divisibility by Repunit polynomial
            possibleSubLengths
            |> Seq.exists (fun l ->
                let num = pown 10L d - 1L
                let den = pown 10L l - 1L
                let patternDivisor = num / den
                id % patternDivisor = 0L
            )

// ==========================================
// 3. Parsing Strategy
// ==========================================

module Parser =
    let parseInput (input: string) : SearchRange list =
        input.Split([|','|], StringSplitOptions.RemoveEmptyEntries)
        |> Array.map (fun segment ->
            match segment.Trim().Split('-') with
            | [| startStr; endStr |] -> 
                { Start = int64 startStr; End = int64 endStr }
            | _ -> failwithf "Invalid range format: %s" segment
        )
        |> Array.toList

// ==========================================
// 4. Execution Helpers
// ==========================================

let solve (ranges: SearchRange list) (validator: ProductId -> bool) : int64 =
    ranges
    |> Seq.collect (fun r -> seq { r.Start .. r.End })
    |> Seq.filter validator
    |> Seq.sum

/// Helper to measure and print execution time
let measure label action =
    let sw = Stopwatch.StartNew()
    let result = action()
    sw.Stop()
    printfn "[%s] Answer: %d (Elapsed: %.1f ms)" label result sw.Elapsed.TotalMilliseconds
    result

// ==========================================
// 5. Main
// ==========================================

let run () =
    let inputPath = Path.Combine(__SOURCE_DIRECTORY__, "../inputs/day02.txt")
    
    if File.Exists inputPath then
        // printfn "Input: %s" inputPath
        let content = File.ReadAllText(inputPath)
        let ranges = Parser.parseInput content
        
        // printfn "--- Processing %d Ranges ---" ranges.Length

        printfn "=== Day 2: Gift Shop ==="

        // Execute Part 1
        measure "Part 1" (fun () -> solve ranges Validator.isValidPart1) |> ignore

        // Execute Part 2
        measure "Part 2" (fun () -> solve ranges Validator.isValidPart2) |> ignore

    else
        printfn "Error: Input file not found at %s" inputPath

run ()