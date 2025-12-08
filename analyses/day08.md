# Day 8: Playground

## Problem Overview & Abstraction

The problem presents a set of entities (junction boxes) in 3D space that need to be electrically connected via wires.

* **Abstraction:** The problem is a classic undirected weighted graph problem.
    * **Nodes:** 3D coordinates $(x, y, z)$.
    * **Edges:** Implicitly defined between *every* pair of nodes.
    * **Weights:** Euclidean distance between nodes.
* **Part 1 Goal:** Simulate the addition of the $1,000$ shortest edges. Analyze the resulting Disjoint Sets (connected components) to find the product of the sizes of the three largest clusters.
* **Part 2 Goal:** Continue adding edges in ascending order of weight until the graph becomes fully connected (a single component). This is effectively constructing a **Minimum Spanning Tree (MST)**. The answer is derived from the specific edge that merges the last two remaining components.

## Challenges

* **Combinatorial Explosion:**
    With $N$ nodes, a fully connected graph has $\frac{N(N-1)}{2}$ edges.
    * For $N=2,000$, $E \approx 2,000,000$. This is manageable but requires efficiency.
    * For $N=20,000$, $E \approx 200,000,000$, which would OOM (Out of Memory) a naive approach.
* **Floating Point Precision:**
    Calculating Euclidean distance requires $\sqrt{dx^2 + dy^2 + dz^2}$. Comparing floats is risky due to precision errors.
    * *Solution:* We used **Squared Euclidean Distance** (`int64`) for all comparisons. Since $d_1 < d_2 \iff d_1^2 < d_2^2$, this preserves strict ordering without floating-point overhead or inaccuracy.
* **State Management:**
    Tracking connectivity requires merging sets efficiently. A naive "flood fill" or BFS after every edge addition would differ from $O(N^2)$ to $O(N^3)$ or worse.

## Domain Modeling

We prioritized functional immutability and type safety.

* **`Point3D` (Record):** Stores `X, Y, Z` and a crucial `Id` (integer index). The `Id` allows us to use simple `int` keys in our data structures rather than complex object hashing.
* **`Edge` (Record):** Stores `P1` (ID), `P2` (ID), and `DistSq` (int64).
* **`CircuitState` (Record):** The immutable state for the Union-Find data structure.
    * `Parents: Map<int, int>`: Tracks the parent of each node.
    * `Sizes: Map<int, int>`: Tracks the size of the tree rooted at a specific node (essential for weighted union and Part 1 scoring).
    * `ComponentCount: int`: Tracks how many disjoint sets remain (essential for Part 2 termination).

## Algorithm Sketch

The solution relies on a variation of **Kruskal’s Algorithm** utilizing a **Disjoint Set Union (DSU)** data structure.

### Phase A: Preprocessing
1.  **Parse Points:** Assign ID $0 \dots N-1$.
2.  **Generate Edges:** Create edge objects for all unique pairs $(i, j)$ where $i < j$.
3.  **Sort:** Sort all edges by `DistSq` ascending.

### Phase B: Disjoint Set Union (DSU) Logic
We implemented a functional DSU with **Union by Size**:
1.  **`findRoot`:** Recursively traverses the `Parents` map to find the representative ID of a set.
2.  **`tryUnion`:**
    * Find roots of both endpoints.
    * If roots are identical, the edge is redundant (cycle). Return state unchanged.
    * If roots are different, attach the root of the smaller tree to the root of the larger tree. Update `Sizes` map. Decrement `ComponentCount`.

### Phase C: Execution
* **Part 1:** Take the first 1,000 sorted edges. `Seq.fold` the `tryUnion` function over them. Inspect the `Sizes` map of the final state.
* **Part 2:** Recursively process the sorted edge list. After every successful union, check `ComponentCount`. If it hits `1`, the current edge bridges the final gap. Calculate result using this edge's endpoints.

## Complexity Analysis

Let $N$ be the number of junction boxes (nodes) and $E$ be the number of potential edges ($E \approx N^2$).

### Time Complexity: $O(N^2 \log N)$
1.  **Edge Generation:** $O(N^2)$. We visit every pair once.
2.  **Sorting:** $O(E \log E) \equiv O(N^2 \log(N^2)) \equiv O(N^2 \log N)$. This is the dominant operation.
3.  **DSU Operations:**
    * `findRoot` and `union` take approximately $O(\alpha(N))$ (inverse Ackermann function), which is nearly constant ($O(1)$) in practice.
    * We perform this at most $E$ times.
    * Total DSU time: $O(N^2)$.
4.  **Total:** The sorting step dominates, making the algorithm **$O(N^2 \log N)$**.

### Space Complexity: $O(N^2)$
1.  **Edge List:** We must store all $\frac{N(N-1)}{2}$ edges to sort them.
    * For $N=2000$, this is $\approx 2 \times 10^6$ objects (approx 64MB - 128MB RAM).
2.  **DSU Maps:** The `Parents` and `Sizes` maps store $O(N)$ entries.

### Scalability Note
This solution is optimal for dense graphs where we must consider all edges, up to $N \approx 5,000$. If $N$ were significantly larger (e.g., $100,000$), the $O(N^2)$ edge generation would fail. We would then need a spatial partitioning structure (like a **k-d tree** or **Delaunay Triangulation**) to generate only the $O(N)$ edges likely to be in the MST.