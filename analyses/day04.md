# Day 4: Printing Department

## Problem Overview

The Elves need to clear a path through a warehouse filled with large rolls of paper (`@`) arranged in a grid. Forklifts can only move a specific roll if it is "accessible."

* **The Grid:** A 2D map where `@` represents a paper roll and `.` represents empty space.
* **Accessibility Rule:** A roll is accessible if it has **strictly fewer than 4** adjacent paper rolls in its immediate 8 neighbors (Horizontal, Vertical, and Diagonal).
* **Part 1 Goal:** Calculate how many paper rolls are accessible in the initial configuration.
* **Part 2 Goal:** Simulate a removal process. When rolls are removed, they leave empty space, potentially making *other* rolls accessible. The process repeats until no more rolls can be removed. Calculate the total number of rolls removed.

## Domain Modeling

* **`Position`**: `{ Row: int; Col: int }` representing coordinates.
* **`PaperGrid`**: `Set<Position>` containing **only** the coordinates of existing paper rolls. This sparse structure optimizes memory and enables $O(\log N)$ lookups, automatically handling boundary checks (neighbors not in the set are empty).

## Algorithms

* **Part 1 (Static Analysis):**
    * We iterate through the $N$ rolls in the grid exactly once.
    * For each roll, we perform 8 lookups in the Set to count neighbors.
    * **Complexity:** $O(N \log N)$.
* **Part 2 (Iterative Simulation):**
    * We perform a simulation loop where each iteration represents a "generation" of removals.
    * In each generation, we scan the remaining rolls to find removable ones.
    * We simultaneously remove them and update the grid.
    * This repeats $k$ times (where $k$ is the number of generations until stability).
    * **Complexity:** $O(k \cdot N \log N)$.

## Complexity Summary

  * **Space:** $O(N)$ (to store the set).
  * **Time:** Part 1 is $O(N \log N)$; Part 2 is $O(k \cdot N \log N)$.
