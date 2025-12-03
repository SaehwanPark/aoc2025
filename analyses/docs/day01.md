### Abstract

The problem asks us to simulate a circular dial (numbered 0-99) reacting to a sequence of rotational instructions (`L` or `R` followed by an integer magnitude).

  * **Part 1:** Calculate how many times the dial lands **exactly** on `0` at the *end* of a rotation.
  * **Part 2:** Calculate how many times the dial visits or passes through `0` during **any** point of the rotation (including full loops).

### Domain Modeling

We avoid primitive obsession by encoding the movement direction and state explicitly.

```fsharp
type TurnDirection = Left | Right

type Instruction = {
  Direction: TurnDirection
  Amount: int
}

// Used for accumulating state in the Fold operation
type DialState = {
  CurrentNumber: int
  Score: int // Represents "ZeroHitCount" for Part 1 or "TotalClicks" for Part 2
}
```

### Algorithmic Strategy

#### Core Math: Canonical Modulo

F\#'s native `%` operator returns the remainder, which carries the sign of the dividend (e.g., `-5 % 100 = -5`). For circular buffers, we need a canonical modulus to ensure negative numbers wrap correctly to the end of the range (e.g., `-5 -> 95`).
$$a \pmod n = ((a \% n) + n) \% n$$

#### Part 1: End-State Simulation

  * **Complexity:** $O(N)$ Time, $O(1)$ Space.
  * **Logic:** Apply the rotation, normalize the position, and check `if pos = 0`.

#### Part 2: Traversal Analysis

  * **Complexity:** $O(N)$ Time, $O(1)$ Space.
  * **Challenge:** Large rotations (e.g., `R1000`) make iterative simulation inefficient.
  * **Logic:** We calculate hits mathematically in $O(1)$ per instruction:
    1.  **Full Loops:** `Amount / 100`. Every 360° turn hits zero once.
    2.  **Partial Turn:** `Amount % 100`.
          * **Right (CW):** A hit occurs if `Current + Remainder >= 100`.
          * **Left (CCW):** A hit occurs if `Current > 0` AND `Current - Remainder <= 0`.
    <!-- end list -->
      * *Note:* The check `Current > 0` for Left turns prevents a "wrap-around" (0 -\> 99) from being falsely counted as a zero-crossing.

-----

### Comments

This solution is **Tail Recursive** (via `Seq.fold`), **Immutable**, and **Type-Safe**. The shift from Part 1 to Part 2 highlights a common Advent of Code pattern: Part 1 usually allows for naive simulation, while Part 2 introduces scale (larger numbers or more steps) that requires O(1) mathematical insight or optimized data structures.
