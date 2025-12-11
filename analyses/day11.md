# Day 11: Reactor

## Problem Overview

The task was to analyze a network of devices connected by unidirectional cables (a directed graph) to troubleshoot communication issues.

* **Part 1:** Calculate the total number of distinct paths from a starting node (`you`) to an ending node (`out`).
* **Part 2:** Calculate the number of paths from a server node (`svr`) to `out` that are constrained to pass through two specific intermediate nodes (`dac` and `fft`).

## Challenges

* **Combinatorial Explosion:** In a dense graph, the number of unique paths grows exponentially with graph size. A naive recursive search (without memory) would re-evaluate identical sub-paths millions of times, leading to unacceptable execution time.
* **Integer Overflow:** The sheer volume of paths exceeded the capacity of a standard 32-bit integer, necessitating the use of 64-bit integers (`int64`).
* **Constraint Logic (Part 2):** Filtering paths that must visit *multiple* specific nodes adds complexity. Naively tracking visited nodes in the recursion state prevents efficient caching (memoization keys would become too specific: `(CurrentNode, VisitedSet)` vs just `(CurrentNode)`).

## High-Level Approach

We recognized that the problem description ("data... can't flow backwards") defined the system as a **Directed Acyclic Graph (DAG)**.

* **Dynamic Programming (Memoization):** Instead of traversing every single path instance, we calculated "how many paths lead from *here* to the end" for each node exactly once and cached the result.
* **Decomposition (Part 2):** Rather than writing a complex filter, we utilized the Multiplication Principle. Because cycles are impossible, a path visiting $A$ and $B$ must strictly follow one of two orders ($A \to B$ or $B \to A$). We broke the problem into segments (e.g., $Start \to A$, $A \to B$, $B \to End$), calculated paths for each segment, and multiplied them.

## Domain Modeling

We avoided primitive string parsing in the core logic by defining specific types to represent the graph structure.

```fsharp
type DeviceId = string

// Adjacency List: Optimized for fast lookups of outgoing edges.
type Network = Map<DeviceId, DeviceId list>
```

## Algorithm Sketch

### Core Algorithm: DFS with Memoization

1.  Define function `solve(node)`:
2.  **Check Cache:** If `node` is in `memo` dictionary, return value.
3.  **Base Case:** If `node` == `target`, return 1.
4.  **Recursive Step:**
      * Retrieve neighbors of `node`.
      * If no neighbors (dead end), return 0.
      * Sum `solve(neighbor)` for all neighbors.
5.  **Cache & Return:** Store result in `memo` and return.

### Part 2 Application:

Calculated $Count = (Paths_{Start \to DAC} \times Paths_{DAC \to FFT} \times Paths_{FFT \to End}) + (Paths_{Start \to FFT} \times Paths_{FFT \to DAC} \times Paths_{DAC \to End})$.

## Complexity Analysis

* **Time Complexity:** $O(V + E)$
    * $V$: Vertices (Devices), $E$: Edges (Cables).
    * Thanks to memoization, each node and edge is processed exactly once per query.
* **Space Complexity:** $O(V)$
    * Storage required for the Adjacency Map, the Memoization Dictionary, and the recursion stack depth.

## Comments & Notes

* **Graph Theory Insight:** The solution relies heavily on the graph being Acyclic. If cycles existed (e.g., `A -> B -> A`), the "count paths" problem becomes infinite, and the memoization strategy would require cycle detection logic to avoid stack overflows.
* **F\# Strengths:** The immutable `Map` made parsing clean, while the mutable `Dictionary` inside the function allowed for high-performance caching without leaking mutation to the outer scope.
* **Future Proofing:** If Part 2 required visiting 3 or more nodes, the permutation logic would grow factorially ($3! = 6$ path checks). For a large set of required nodes, a topological sort approach would be more scalable than manual permutation checks.