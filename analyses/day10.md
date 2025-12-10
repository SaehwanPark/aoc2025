# Day 10: Factory

## Problem Overview / Abstraction

The puzzle presents two distinct configuration problems based on a set of "Machines." Each machine has a starting state (zero) and a target state, manipulated by a set of buttons.

  * **Part 1 (Discrete State):** We must toggle binary flags (Lights) to match a pattern. Button presses act as `XOR` operations. The goal is the **shortest path** (fewest presses) in a state graph.
  * **Part 2 (Linear System):** We must increment counters (Joltage) to match specific integer targets. Button presses act as vector addition. The goal is to find a non-negative integer solution vector $x$ to $Ax = b$ that minimizes the $L_1$ norm (sum of elements).

## Challenges

1.  **State Space Explosion (Part 1):** While small inputs allow brute force, a naive DFS could get stuck in cycles (toggling a button on and off).
2.  **Domain Shift:** The transition from Part 1 to Part 2 changes the algebraic structure from a Field GF(2) (Boolean algebra) to the Field of Real Numbers ($\mathbb{R}$), specifically restricted to non-negative integers ($\mathbb{Z}_{\ge 0}$).
3.  **Underdetermined Systems (Part 2):** Many machines have more buttons than counters (columns \> rows). This creates "Free Variables," meaning there are infinite solutions in $\mathbb{R}$. We must identify the specific solution that satisfies integer constraints and minimizes the total presses.
4.  **Search Bounds (Part 2):** A critical failure point in the initial implementation was assuming button press counts would be small (constant cap). Real inputs required press counts proportional to the target values (up to \~300), requiring a dynamic search space.

## Approach / Strategy

### Part 1: Breadth-First Search (BFS)

Since the edges in the state graph are unweighted (every button press costs 1), BFS is the optimal algorithm to find the shortest path. We treat the configuration as a bitmask to allow $O(1)$ transitions via bitwise XOR.

### Part 2: Linear Algebra + Parametric Search

We model the machine as a system of linear equations.

1.  **Gaussian Elimination:** Convert the augmented matrix $[A|b]$ to Reduced Row Echelon Form (RREF). This separates variables into **Pivot Variables** (fixed by equations) and **Free Variables** (degrees of freedom).
2.  **Integer Search:** Since the system is small, we iterate over reasonable integer ranges for the Free Variables.
3.  **Back-Substitution:** For each guess of Free Variables, we calculate the Pivot Variables. If all resulting values are non-negative integers, we calculate the cost and track the minimum.

## Domain Modeling

We utilized a unified `Machine` record to encapsulate both interpretations of the input data.

```fsharp
type LightState = uint64

type Machine = {
    // Part 1: Bit manipulation domain
    LightsTarget: LightState
    LightsButtons: LightState list
    
    // Part 2: Linear algebra domain
    JoltageTarget: float[]     // Vector b
    JoltageButtons: int[][]    // Adjacency list representation of Matrix A
}
```

## Algorithm Sketch

### Part 1: BFS Solver

1.  **Queue**: `(CurrentState, Depth)` initialized to `(0, 0)`.
2.  **Visited Set**: Tracks `uint64` states to prevent cycles.
3.  **Loop**:
      * Dequeue `state`.
      * If `state == target`, return `Depth`.
      * Else, generate neighbors: `state XOR buttonMask`.
      * Enqueue unvisited neighbors.

### Part 2: "Smart" Brute Force

1.  **Matrix Construction**: Map buttons to columns and counters to rows.
2.  **Row Reduction**: Apply Gaussian Elimination to reach RREF.
3.  **Variable Classification**:
      * *Free Vars*: Columns containing no leading 1 (pivot).
      * *Pivot Vars*: Columns containing a leading 1.
4.  **Search Loop**:
      * Iterate combinations of Free Vars from $0$ to $Max(Target)$.
      * Compute dependent Pivot Vars: $x_{pivot} = b'_{row} - \sum (A'_{row, free} \times x_{free})$.
      * **Constraint Check**: Is $x_{pivot}$ an integer $\ge 0$?
      * **Optimization**: Track global minimum sum.

## Complexity Analysis

### Part 1

  * **Time:** $O(B \cdot 2^L)$, where $B$ is the number of buttons and $L$ is the number of lights. In practice, we only visit reachable states, which is significantly smaller than $2^{64}$.
  * **Space:** $O(2^L)$ to store the `Visited` hash set.

### Part 2

  * **Time:** $O(R^2C + T^F)$.
      * $R^2C$: Cost of Gaussian elimination (negligible for $N < 10$).
      * $T$: Search limit (proportional to max target value).
      * $F$: Number of free variables (Rank deficiency).
      * Because $F$ is very small (usually 0, 1, or 2), this exponential term is manageable.
  * **Space:** $O(R \cdot C)$ for the matrix.

## Comments / Notes

  * **F\# Data Structures:** Using `uint64` for Part 1 was a major ease-of-use win. F\# binary literals (`0b...UL`) and operators (`^^^`, `<<<`) make bit-masking idiomatic.
  * **Constraint Programming:** Part 2 is technically an Integer Linear Programming (ILP) problem. While general ILP is NP-hard, the specific constraints here (small dimensions, non-negative) allowed for a custom solver rather than importing a heavy solver like CPLEX or Gurobi.
  * **The "Limit" Bug:** The initial failure in Part 2 was due to a "Magic Number" heuristic (capping loop at 100). This reinforces the engineering maxim: **Avoid magic numbers.** The limit should always be derived from the input data (e.g., `Max(Targets)`).