# Day 5: Cafeteria

## Problem Overview

The core of Day 5 describes a classic **1D Interval** problem.

* **Part 1** asks for a **Point-in-Interval** check: Given a list of specific coordinates (IDs) and a set of overlapping intervals (Ranges), determine how many coordinates lie within the union of those intervals.
* **Part 2** asks for the **Measure of the Union**: Determine the total integer count covered by the union of all intervals.

## Challenges

1.  **Overlapping Intervals:** The input ranges (e.g., `10-14`, `12-18`) overlap. Treating them independently leads to:
  * **Inefficiency** in Part 1 (checking the same integers multiple times).
  * **Incorrectness** in Part 2 (double-counting the overlap `12-14`).
2.  **Coordinate Scale:** The inputs represent potentially massive IDs, requiring 64-bit integers (`int64`) to avoid `System.OverflowException`.
3.  **Sparse Data:** Creating a boolean array or `Set` of all valid IDs is memory-prohibitive given the large coordinates.

## Algorithm: Merge Intervals

The optimal strategy for both parts is to **normalize** the input data by merging overlapping intervals into a set of disjoint, sorted intervals.

**Logic :**

1.  **Sort** all intervals by their `Start` coordinate.
2.  **Iterate** through the sorted list, maintaining a `current` merged interval.
3.  **Compare** the `next` interval with `current`:
      * If `next.Start <= current.End + 1`: They overlap or are adjacent. Extend `current.End` to `max(current.End, next.End)`.
      * Else: The `current` interval is complete. Push it to results and start a new interval with `next`.

## Complexity Analysis

Let $N$ be the number of ranges and $M$ be the number of available IDs (Part 1).

* **Time Complexity:**
    * **Preprocessing (Merge):** $O(N \log N)$ due to sorting. The merge pass is $O(N)$.
    * **Part 1 Query:** With disjoint ranges, checking an ID is $O(K)$, where $K$ is the number of merged ranges. Total: $O(N \log N + M \cdot K)$.
        * *Optimization Note:* Since $K$ is sorted, we *could* use Binary Search for $O(M \log K)$, but given $K \le N$, linear scan is sufficient here.
    * **Part 2 Calculation:** Summing lengths is linear over the merged ranges. Total: $O(N \log N + K)$.
* **Space Complexity:** $O(N)$ to store the intervals.
