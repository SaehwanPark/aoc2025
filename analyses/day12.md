# Day 12: Christmas Tree Farm

## 1\. Problem Overview

The objective was to determine how many **Regions** (rectangular grids of varying sizes) could successfully accommodate a specific multiset of **Presents** (polyomino shapes).
This is a classic **2D Packing Problem** (a subset of Constraint Satisfaction Problems). The core task is a boolean check: *"Can these $N$ items tile into this $W \times H$ grid without overlapping?"*

## 2\. Challenges

  * **Combinatorial Explosion:** The search space is effectively $O(Positions^{Items})$. With input regions requiring up to \~180 items (summing the counts in the input file), a naive recursive search is impossible.
  * **Geometric Complexity:** Shapes can be rotated (0°, 90°, 180°, 270°) and flipped, resulting in up to 8 variations per shape.
  * **"Impossible" Instances:** The most computationally expensive scenarios are those where the total area of presents is *less* than the grid area (passing the trivial check), but the shapes geometrically *cannot* fit. The solver must exhaustively explore the entire search tree to prove impossibility.
  * **Grid Size:** Input grids (e.g., `50x50`) exceed 64 cells, preventing the use of standard `uint64` bitmasks, forcing the use of slower `BigInteger`.

## 3\. Approach & Strategy

We modeled the problem as an **Exact Cover** style backtracking search optimized with heuristics.

  * **Bitmask Representation:** The grid state was flattened into a single `BigInteger`.
      * Collision detection becomes a single bitwise operation: `(GridState &&& ShapeMask) == 0`.
      * State updates are simple ORs: `GridState ||| ShapeMask`.
  * **Preprocessing (Cache):** Before recursing, we pre-calculated every valid `BigInteger` mask for every orientation of every shape on the specific grid size. This moved the expensive geometry math out of the hot loop.
  * **Heuristic (Fail-Fast):**
    1.  **Area Pruning:** If `Sum(PresentAreas) > GridArea`, return `False` immediately.
    2.  **Sort Descending:** We placed the largest/most complex shapes first. Large shapes have fewer valid positions and are more likely to cause collisions early, pruning bad branches near the root of the recursion tree.

## 4\. Domain Modeling

  * **`ShapeDef`**: The raw visual definition of a present.
  * **`Variant`**: A pre-computed orientation containing the `Width`, `Height`, and relative `Points` normalized to (0,0).
  * **`Region`**: The target grid dimensions and the flattened list of `ShapeId`s to pack.
  * **`Bitmask`**: A `System.Numerics.BigInteger` where the bit at index `r * width + c` represents the cell at `(r, c)`.

## 5\. Algorithm Sketch

```text
Function Solve(Region):
   1. Calculate TotalPresentArea. If > Region.Area, return False.
   2. Sort Presents by Area (Descending).
   3. Build MoveCache: Map<ShapeID, List<BitMasks>> 
      (All valid placements for each shape on this grid).
   4. If any Shape has 0 valid moves, return False.
   5. Recursive DFS(RemainingPresents, CurrentGridMask):
      a. If Empty(RemainingPresents) -> Return True
      b. CurrentShape = Head(RemainingPresents)
      c. For each Mask in MoveCache[CurrentShape]:
         i. If (CurrentGridMask AND Mask) == 0:  // No Collision
            If DFS(Tail(RemainingPresents), CurrentGridMask OR Mask) -> Return True
      d. Return False (Backtrack)
```

## 6\. Complexity Analysis

  * **Time Complexity:** $O(R \cdot 8 \cdot W \cdot H \cdot B^N)$
      * $R$: Number of regions.
      * $8 \cdot W \cdot H$: Pre-computation of variants.
      * $B$: Branching factor (average valid positions per piece).
      * $N$: Number of presents.
      * *Note:* In the worst case (unsatisfiable tight packing), this is exponential.
  * **Space Complexity:** $O(N \cdot W \cdot H)$ to store the pre-computed bitmasks for the recursion.

## 7\. Performance Analysis (Why 25s?)

I noted the execution took **25+ seconds**, significantly longer than previous days. This may be expected for this class of problem.

1.  **The "Unsatisfiable" Trap:**
      * The solver is fast when a solution exists (it returns `True` as soon as it finds *one*).
      * The solver is extremely slow when a solution **does not** exist but the Area Check passes. It must try *every possible combination* of 180+ items to prove there is no way to fit them. The pruning heuristics help, but the tree remains massive.
2.  **Identical Item Symmetry:**
      * The input contains high counts (e.g., "30 of Shape 0"). My solution expands this into a list: `[Shape0; Shape0; ... Shape0]`.
      * The solver treats these as distinct items. It tries placing "Instance 1" at (0,0) and "Instance 2" at (0,1). Then it backtracks and tries "Instance 1" at (0,1) and "Instance 2" at (0,0).
      * This is redundant work ($30!$ permutations). A specialized solver would enforce an ordering (e.g., "Instance 2 must be placed at a higher index than Instance 1"), drastically cutting the search space.
3.  **BigInteger Overhead:**
      * With grid sizes like `50x50`, we are manipulating 2500-bit integers. While faster than 2D array lookups, `BigInteger` bitwise ops are significantly slower than native `uint64` CPU instructions.

**Summary:** The solution is correct and robust, but the 25s runtime is the cost of using a general-purpose backtracking algorithm on "Unsatisfiable" packing instances without advanced symmetry-breaking optimizations.