# Advent of Code 2025 Solutions

- Personal Leaderboard: [link](https://adventofcode.com/2025/leaderboard/private/view/4062839?view_key=68c3d332)
- You can join the leaderboard by using the code `4062839-80e6de7e`.

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

### Puzzles and Solutions

|Day|Puzzle Link|Solution Link|
|---|---|---|
|1|[Link](https://adventofcode.com/2025/day/1)|[Link](https://github.com/SaehwanPark/aoc2025/blob/main/scripts/day01.fsx)|
|2|[Link](https://adventofcode.com/2025/day/2)|[Link](https://github.com/SaehwanPark/aoc2025/blob/main/scripts/day02.fsx)|
|3|[Link](https://adventofcode.com/2025/day/3)|[Link](https://github.com/SaehwanPark/aoc2025/blob/main/scripts/day03.fsx)|
|4|[Link](https://adventofcode.com/2025/day/4)|[Link](https://github.com/SaehwanPark/aoc2025/blob/main/scripts/day04.fsx)|
|5|[Link](https://adventofcode.com/2025/day/5)||
|6|[Link](https://adventofcode.com/2025/day/6)||
|7|[Link](https://adventofcode.com/2025/day/7)||
|8|[Link](https://adventofcode.com/2025/day/8)||
|9|[Link](https://adventofcode.com/2025/day/9)||
|10|[Link](https://adventofcode.com/2025/day/10)||
|11|[Link](https://adventofcode.com/2025/day/11)||
|12|[Link](https://adventofcode.com/2025/day/12)||
