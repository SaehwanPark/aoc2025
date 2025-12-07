# Day 7: Laboratories

## Problem Overview & Abstraction

The problem asks us to simulate the propagation of "tachyon beams" through a 2D grid containing empty space (`.`) and splitters (`^`).

* **Directionality:** The flow is strictly monotonic on the Y-axis (downward). Beams/Particles never move up or stay on the same row.
* **Graph Structure:** Because movement is strictly downward, the grid forms an implicit **Directed Acyclic Graph (DAG)**. Cycle detection is unnecessary.
* **Part 1 (Geometric Coverage):** We treat the entity as a continuous "beam." If two beams converge, they merge into one. The goal is to count the unique interaction points (splitters hit). This is a **Reachability** problem.
* **Part 2 (Combinatorial Paths):** We treat the entity as a "quantum particle" in a Many-Worlds interpretation. Splits create distinct timelines; convergences sum the number of timelines. The goal is to count total distinct paths from start to finish. This is a **Path Counting** problem.

## Challenges

* **State Explosion (Part 2):** In Part 2, a single particle passing through $N$ splitters creates $2^N$ paths. A naive recursive search (DFS) without memoization would result in $O(2^N)$ time complexity, leading to timeouts on large inputs.
* **Integer Overflow:** Due to the exponential growth of paths in Part 2, the result exceeds the capacity of a standard 32-bit integer.
* **Beam Merging:**
  * In **Part 1**, we must ensure we don't double-count splitters if multiple beams hit them simultaneously.
  * In **Part 2**, when timelines converge (e.g., Left-then-Right meets Right-then-Left), we must accurately sum their history counts, requiring a sparse accumulation strategy.
* **Boundary Checking:** Splitters eject beams to $x-1$ and $x+1$. These indices often fall outside the grid (negative or $\ge$ Width), requiring strict boundary guards to prevent `IndexOutOfRangeException`.

## Domain Modeling

We utilized a functional, immutable state approach. Instead of mutating a 2D array, we transformed the state of "active columns" row by row.

### Static Data

```fsharp
type Manifold = {
  Grid: string array
  Width: int; Height: int
  StartX: int; StartY: int
}
```

### Dynamic State

**Part 1 (Set-based):**
We used a `Set<int>` to represent the X-coordinates of active beams.

  * *Why?* Sets automatically handle deduplication (merging beams) and provide efficient lookups.

**Part 2 (Map-based):**
We used a `Map<int, int64>` to represent `Column Index -> Timeline Count`.

  * *Why?* Maps allow us to store the "weight" (number of paths) at a specific column. It also handles sparse data efficiently (we don't need an array of size $W$ if only 3 beams are active).

## Algorithm Sketch

Since the flow is strictly vertical, we employed an iterative **Dynamic Programming** approach, processing the grid one row at a time (`Row $N$ -> Row $N+1$`).

### Part 1: Beam Propagation

1.  **Init:** Start with `Set { StartX }`.
2.  **Transition:** For every column $x$ in the current Set:
      * If cell is `^`: Add $x-1$ and $x+1$ to the *Next Row's Set*. Increment global split counter.
      * If cell is `.` or `S`: Add $x$ to the *Next Row's Set*.
3.  **Merge:** The `Set` structure implicitly merges beams falling into the same column.
4.  **Repeat:** Until $y = Height$.

### Part 2: Timeline Summation

1.  **Init:** Start with `Map { StartX -> 1L }`.
2.  **Transition:** For every `(x, count)` in the current Map:
      * If cell is `^`: Add `count` to $x-1$ AND $x+1$ in the *Next Row's Map*.
      * If cell is `.` or `S`: Add `count` to $x$ in the *Next Row's Map*.
3.  **Accumulate:** When adding to the Next Map, if the key exists, **sum** the new count with the existing count (`new + old`).
4.  **Result:** Sum all values in the final Map at the bottom of the grid.

## Complexity Analysis

Let $H$ be the grid height and $W$ be the grid width.

### Time Complexity

* **Part 1 & 2:** $O(H \cdot W \cdot \log W)$
  * We iterate through $H$ rows.
  * In the worst case (a full pyramid), we process up to $W$ active columns per row.
  * Map/Set insertions and lookups in F\# are tree-based, taking $O(\log W)$.
  * *Note:* On sparse grids, this performs significantly faster than $O(H \cdot W)$.

### Space Complexity

* **Part 1 & 2:** $O(W)$
  * We only ever hold the state of the *current* row and the *next* row in memory.
  * The storage requirement is proportional to the grid width (maximum number of concurrent beams).

### Data Types

* **Part 2:** Required `int64` (System.Int64) to accommodate path counts exceeding 2 billion.