(*
  Day 11: Reactor - Refactored Solution
  
  Architecture:
  1. Domain: Type definitions for the Graph.
  2. Parsing: Raw text to Adjacency Map.
  3. GraphOps: Core path-finding algorithms (DFS + Memoization).
  4. Diagnostics: Timing and performance measurement utilities.
  5. Solvers: Specific business logic for Part 1 and Part 2.
*)

open System
open System.IO
open System.Collections.Generic
open System.Diagnostics

// --- 1. Domain Modeling ---
module Domain =
  type DeviceId = string
  
  // A Network is a Directed Acyclic Graph (DAG)
  // Key: Source Device, Value: List of Destination Devices
  type Network = Map<DeviceId, DeviceId list>

// --- 2. Parsing Logic ---
module Parsing =
  open Domain

  let private parseLine (line: string) =
    let parts = line.Split([|':'|], StringSplitOptions.RemoveEmptyEntries)
    let source = parts.[0].Trim()
    let destinations = 
      if parts.Length > 1 then
        parts.[1].Trim().Split([|' '|], StringSplitOptions.RemoveEmptyEntries)
        |> Array.toList
      else 
        []
    source, destinations

  let parseInput (lines: string seq) : Network =
    lines
    |> Seq.filter (fun x -> not (String.IsNullOrWhiteSpace x))
    |> Seq.map parseLine
    |> Map.ofSeq

// --- 3. Algorithms ---
module GraphOps =
  open Domain

  /// Counts all distinct paths from startNode to endNode using DFS with Memoization.
  /// Returns 0L if endNode is unreachable.
  let countPaths (network: Network) (startNode: DeviceId) (endNode: DeviceId) : int64 =
    let memo = Dictionary<DeviceId, int64>()

    let rec search current =
      match memo.TryGetValue current with
      | true, count -> count
      | false, _ ->
        let result =
          if current = endNode then
            1L
          else
            match Map.tryFind current network with
            | None -> 0L // Dead end
            | Some neighbors ->
              neighbors |> List.sumBy search
        
        memo.[current] <- result
        result

    search startNode

// --- 4. Diagnostics ---
module Diagnostics =
  let measureTime (label: string) (action: unit -> 'T) : 'T =
    let sw = Stopwatch.StartNew()
    let result = action()
    sw.Stop()
    printfn "[%s] Time Elapsed: %0.1f ms" label sw.Elapsed.TotalMilliseconds
    result

// --- 5. Solvers ---
module Solvers =
  open Domain
  open GraphOps

  let solvePart1 (network: Network) =
    // Find all paths from 'you' to 'out'
    countPaths network "you" "out"

  let solvePart2 (network: Network) =
    // Find paths from 'svr' to 'out' passing through BOTH 'dac' and 'fft'.
    // Since it's a DAG, the path must follow a strict linear order.
    // It's either: svr -> dac -> fft -> out
    // OR:      svr -> fft -> dac -> out
    
    let start, destination = "svr", "out"
    let wp1, wp2 = "dac", "fft"

    // Sequence A: svr -> dac -> fft -> out
    let seqA = 
      (countPaths network start wp1) * (countPaths network wp1 wp2) * (countPaths network wp2 destination)

    // Sequence B: svr -> fft -> dac -> out
    let seqB = 
      (countPaths network start wp2) * (countPaths network wp2 wp1) * (countPaths network wp1 destination)

    seqA + seqB

// --- Main Execution ---
open Parsing
open Diagnostics
open Solvers

let main () =
  let inputPath = Path.Combine(__SOURCE_DIRECTORY__, "../inputs/day11.txt")
  printfn "=== Day 11: Reactor ==="

  // 1. Parsing Phase
  let network = 
    measureTime "Parsing" (fun () -> 
      File.ReadLines(inputPath) |> parseInput
    )
  
  printfn "Graph Loaded: %d nodes" network.Count
  printfn "---"

  // 2. Part 1 Phase
  let part1Result = 
    measureTime "Part 1" (fun () -> 
      solvePart1 network
    )
  printfn "[Part 1] Answer: %d" part1Result
  printfn "---"

  // 3. Part 2 Phase
  let part2Result = 
    measureTime "Part 2" (fun () -> 
      solvePart2 network
    )
  printfn "[Part 2] Answer: %d" part2Result

main ()