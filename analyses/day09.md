# Day 9: Movie Theater

## 1\. Problem Overview

The objective was to identify the largest rectangular area formable by selecting two "Red Tiles" (coordinates) from a discrete grid to serve as opposite corners.

  * **Part 1:** Required finding the maximum inclusive area defined by *any* two coordinates in the input set.
  * **Part 2:** Introduced a geometric constraint. The input coordinates formed a loop (a polygon). Valid rectangles were restricted to those contained entirely within (or on the boundary of) this polygon.

## 2\. Challenges

  * **Integer Overflow (The "Silent Failure"):**
      * The coordinate values were large enough ($\approx 50,000$) that calculating the area ($w \times h$) exceeded the bounds of a signed 32-bit integer (`2.14B`).
      * *Impact:* Initial submissions were incorrect because `List.max` prioritized small positive numbers over large values that had wrapped around to negatives.
  * **Geometric Containment (Part 2):**
      * **Non-Convexity:** The polygon was not guaranteed to be convex. A rectangle could share corners with the polygon but cross "empty space" (the background).
      * **The "Splitting" Edge Case:** A naive check (ensuring no *vertices* are inside the rectangle) failed because a long polygon edge could slice completely through a rectangle without depositing a vertex inside it.
  * **Inclusive Geometry:**
      * Area calculations had to be discrete (inclusive of endpoints), requiring the formula $(|\Delta x| + 1) \times (|\Delta y| + 1)$ rather than standard Cartesian area.

## 3\. Domain Modeling

We avoided primitive obsession by using F\# Records to model the grid.

```fsharp
type Tile = { X: int; Y: int }
type Edge = Tile * Tile
```

  * **Rationale:** Using a Record instead of `int * int` tuples clarified the code, specifically preventing confusion between `x, y` coordinate pairs and `width, height` dimensions.
  * **Input Data:** Modeled as `Tile list`. Since the input order mattered for the polygon definition in Part 2, a `List` (ordered sequence) was preferable to a `Set` or `Map`.

## 4\. Algorithm Sketch

### Part 1: Combinatorial Area

1.  **Parse:** Convert input lines to `Tile` records.
2.  **Generate Pairs:** Create all unique combinations of two tiles $(O(N^2))$.
3.  **Compute:** Calculate Area using `int64` arithmetic.
4.  **Reduce:** Find the maximum.

### Part 2: Polygon Containment Filter

We filtered the pairs from Part 1 using a three-stage validity check:

1.  **Intrusion Check:** Iterate all vertices. If any vertex $V$ lies strictly inside the candidate rectangle boundaries, the rectangle is **Invalid**. (The polygon folds inside the rectangle).
2.  **Splitting Check:** Iterate all polygon edges. If any vertical edge sits strictly between the rectangle's X-bounds and spans the full Y-height (or vice versa for horizontal), the rectangle is **Invalid**. (The polygon slices the rectangle).
3.  **Center Containment:** If the above pass, check the rectangle's geometric center.
      * Use **Ray Casting (Even-Odd Rule)**.
      * Cast a ray from the center to $x = \infty$. Count intersections with polygon edges. Odd = Inside.
      * *Note:* Used floating-point math (`double`) for slope calculations here to handle midpoints accurately.

## 5\. Complexity Analysis

Given $N$ is the number of tiles (vertices).

  * **Time Complexity:**
      * **Part 1:** $O(N^2)$. We iterate all pairs once.
      * **Part 2:** $O(N^3)$.
          * We generate $N^2$ pairs.
          * For each pair, we iterate through $N$ vertices (intrusion) and $N$ edges (splitting/raycast).
          * With $N \approx 500$, operations $\approx 1.25 \times 10^8$. This runs in $\approx 2.5$ seconds on modern CPUs.
  * **Space Complexity:** $O(N)$. We only store the list of tiles and edges. The pair generation is lazy (or transient on the stack), preventing memory blowup.

## 6\. Comments & Notes

  * **Type Safety saved time:** Explicitly defining `Tile` prevented mixing up `X` and `Y` in the complex logic of Part 2.
  * **The Overflow Trap:** This is a recurring theme in algorithmic puzzles. Standard practice should be to default to `int64` (or `BigInt`) whenever multiplying coordinates, even if the coordinates themselves fit in `int32`.
  * **Alternative for Part 2:** If $N$ were significantly larger (e.g., $10,000$), the $O(N^3)$ approach would fail. We would need a **Sweep Line Algorithm** or a **QuadTree** to perform range queries and collision detection in $O(N \log N)$ or $O(N^2 \log N)$. For $N=500$, the brute-force geometry check was acceptable and easier to implement correctly.