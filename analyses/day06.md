# Day 6: Trash Compactor

## Problem Overview & Abstraction

The goal of this puzzle is to calculate a global checksum ("Grand Total") derived from a series of arithmetic problems embedded in a large text file. Unlike standard input formats where data is line-delimited, this input treats text as a **2D Cartesian grid**.

  * **The Entities:** "Problems" are rectangular blocks of text containing numbers and an operator symbol (`+` or `*`).
  * **The Delimiter:** Problems are separated not by newlines, but by vertical columns consisting entirely of whitespace.
  * **The Transformation:**
      * **Part 1:** Treats rows within a block as numbers.
      * **Part 2:** Treats columns within a block as numbers (read vertically), ordered right-to-left.

## Challenges

  * **Columnar Parsing:** The standard file reading approach (`ReadLine()`) is insufficient because a single line of text contains fragments of multiple independent problems. We must parse "vertically."
  * **Ragged Input Data:** The raw input file has lines of varying lengths. Attempting to access `line[x]` where `x` exceeds the length of a specific line would cause runtime exceptions.
  * **Orientation Shift:** Part 2 flips the reading direction. We must conceptualize strings as vertical slices of characters rather than horizontal sequences.
  * **Integer Overflow:** The multiplication of several inputs results in values exceeding the bounds of a standard 32-bit integer.

## Domain Modeling

To maintain type safety and separate the "shape" of the data from the "logic" of the solution, we define the following F\# types:

```fsharp
type Operation = 
  | Add 
  | Multiply

type MathProblem = {
  // A generic list of operands. The parser determines 
  // if these came from rows (Part 1) or columns (Part 2).
  Numbers : int64 list 
  Operator : Operation
}
```

We also implicitly model the raw input as a **Normalized Grid**:
`type CharGrid = char array array` (A rectangular matrix where all rows have equal length).

## Algorithm Sketch

The solution follows a pipeline architecture: **Normalize $\rightarrow$ Scan $\rightarrow$ Extract $\rightarrow$ Compute**.

### Phase 1: Grid Normalization

To solve the "Ragged Input" challenge, we read all lines, determine the maximum width ($W_{max}$), and pad every line with spaces on the right. This allows us to access `grid[row][col]` safely for any coordinate.

```text
Raw Input:      Normalized Grid:
"123 5"     ->  ['1','2','3',' ','5',' ']
"4 6"       ->  ['4',' ','6',' ',' ',' ']
```

### Phase 2: The Column Scan

We iterate through the grid columns ($x = 0$ to $W_{max}$). We track the state of the current "block":

1.  Check if column $x$ is a **Separator** (all spaces).
2.  If it is a separator and we were inside a block, we define the block boundaries as `[startCol, x-1]` and trigger **Extraction**.
3.  If it is not a separator, we continue accumulating the block width.

### Phase 3: Extraction Strategy

This is where the logic diverges for Part 1 and Part 2. We pass a "Strategy Function" to the parser.

  * **Operator Extraction:** Always found by scanning the **last row** of the block between `startCol` and `endCol`.
  * **Part 1 (Row-Wise):** \* Iterate rows $0$ to $N-2$.
      * Slice the row string horizontally: `row[startCol..endCol]`.
      * Parse as Int64.
  * **Part 2 (Col-Wise):**
      * Iterate columns from `endCol` down to `startCol` (Right-to-Left).
      * Slice the column vertically: `grid[0..N-2][col]`.
      * Concatenate digits, ignoring spaces.
      * Parse as Int64.

### Phase 4: Computation

We map the list of parsed `MathProblem` objects to their results using a simple fold:

  * `Add` $\rightarrow$ `List.fold (+) 0L`
  * `Multiply` $\rightarrow$ `List.fold (*) 1L`

Finally, we sum all results for the Grand Total.

## Complexity Analysis

Let $H$ be the number of lines (height) and $W$ be the maximum line length (width).

  * **Time Complexity: $O(H \times W)$**

      * **Normalization:** We iterate over every character once to create the padded grid.
      * **Scanning:** We iterate over every column ($W$), checking every row ($H$) to detect separators.
      * **Parsing:** Inside the blocks, we iterate over the characters again to form strings. Since blocks do not overlap, this remains linear relative to the total number of characters.
      * **Total:** The solution is strictly linear with respect to the input size (total characters).

  * **Space Complexity: $O(H \times W)$**

      * We allocate a new 2D array to store the normalized grid.
      * The storage for the list of `MathProblem` objects is negligible compared to the grid itself.