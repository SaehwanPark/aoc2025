# Advent of Code 2025 Solutions

F# solutions for [Advent of Code 2025](https://adventofcode.com/2025), featuring my own approaches and algorithms.

### My AoC Workflow

1.  **Abstract & Read Carefully:** Read the prompt *twice*. Highlighting specific rules (e.g., "ties are broken by reading order").
2.  **Input Analysis & Parsing:** Look at the raw input file immediately. Is it a grid? A list of instructions? Write a parser to convert `input.txt` $\rightarrow$ `YourDataStructure`.
3.  **Algorithm Sketch & Big O:**
    * Determine if the search space is small (Simulation) or massive (Math/Dynamic Programming).
    * *Check:* Can I brute force this in under 15 seconds?
4.  **Modular Design (Part 2 Preparation):**
    * Instead of writing one big function, write helpers: `get_neighbors()`, `parse_line()`, `calculate_score()`.
    * *Crucial:* Keep your logic for Part 1 separate from the shared utilities so you can copy-paste or extend for Part 2.
5.  **Implementation & Visualization:**
    * Implement. If working with grids/mazes, add a `print_grid()` function immediately. Visual debugging is faster than stepping through a debugger in AoC.
6.  **The "Sample" Test (Integration):**
    * Run against the provided example.
    * **Stop:** If the sample passes but the answer is wrong, re-read the prompt for edge cases.
7.  **Run Real Input:** Submit Step 1.
8.  **Refactor for Part 2:**
    * Read Part 2.
    * *Decision point:* Do I extend Part 1's code, or is the complexity shift so high (e.g., changing from simulation to modular arithmetic) that I start a fresh file?

### A Note on Unit Tests

For Earlier days, writing formal unit tests for every function might be overkill and slow us down.

* **Keep it:** For complex utility functions (e.g., "Rotate a 3D coordinate 90 degrees").
* **Skip it:** For simple logic where the Integration Test (Step 8) covers the behavior.

### Puzzles and Solutions

|Day|Puzzle Link|Solution Link|
|---|---|---|
|1|[Link](https://adventofcode.com/2025/day/1)|[Link](https://github.com/SaehwanPark/aoc2025/blob/main/scripts/day01.fsx)|
|2|[Link](https://adventofcode.com/2025/day/2)|[Link](https://github.com/SaehwanPark/aoc2025/blob/main/scripts/day02.fsx)|
