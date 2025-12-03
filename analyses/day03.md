# Day 3: Lobby

### Overview

The objective was to calculate the maximum output "joltage" from banks of batteries (represented as strings of digits).

  - **Part 1:** Select exactly **2 digits** (preserving order) to form the largest possible 2-digit number.
  - **Part 2:** Select exactly **12 digits** (preserving order) to form the largest possible 12-digit number.

### Part 1: The Greedy Pairs

**Challenge:** Given a sequence, find $d_i$ and $d_j$ ($i < j$) such that $10 \cdot d_i + d_j$ is maximized.
**Algorithm:** Since the "tens" place contributes significantly more value than the "ones" place, we use a greedy approach:

1.  Find the maximum value in the sequence (excluding the very last digit, which cannot be a "tens" digit).
2.  Select the **first occurrence** of this maximum value. This is critical because picking an earlier index leaves a larger suffix of the array to search for the "ones" digit.
3.  Find the maximum value in the remaining suffix.

### Part 2: The Monotonic Stack

**Challenge:** Given a sequence of length $N$, find a subsequence of length $K=12$ that forms the lexicographically largest integer.
**Algorithm:** This is a variation of the "Remove $K$ Digits" problem. We need to remove exactly $N - 12$ digits. We use a **Monotonic Decreasing Stack**.

1.  Iterate through the digits.
2.  For each digit, check the stack: if the current digit is **larger** than the top of the stack, and we still have "drops" allowed, **pop** the smaller digit off the stack.
3.  Push the current digit.
4.  This ensures that larger digits migrate as far to the left (highest magnitude positions) as possible.

### Solution Architecture

  * **Type Strategy:** Used a `Record` type `Bank` to wrap the data.
  * **Data Structures:** \* Part 1 used `Array` for fast indexing ($O(1)$) to jump between search ranges.
      * Part 2 used `List` and recursion to implement the stack, ensuring immutability.
  * **Safety:** Switched to `int64` for Part 2 to prevent overflow, as 12-digit numbers exceed `Int32.MaxValue`.

