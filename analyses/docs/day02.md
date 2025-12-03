### Abstract

The core challenge involves identifying "invalid" product IDs within large numeric ranges. The definition of "invalid" changes between parts, but both rely on detecting repeated digit patterns.

**The Engineering Approach:**
Instead of converting numbers to strings—which incurs significant memory allocation and garbage collection overhead—we utilize **modular arithmetic**.

1.  **Logarithms:** We determine the number of digits using $\lfloor \log_{10}(N) \rfloor + 1$.
2.  **Part 1 Math:** A number $N$ composed of two identical halves $X$ (e.g., $1212$) can be expressed as $X \cdot 10^k + X$, which simplifies to $X \cdot (10^k + 1)$. We simply check if $N \pmod{10^k+1} \equiv 0$.
3.  **Part 2 Math:** A number $N$ composed of a pattern $X$ of length $L$ repeated $k$ times is divisible by the "Repunit" polynomial $\sum_{i=0}^{k-1} 10^{i \cdot L}$. We check divisibility by this sum for valid sub-lengths.

This $O(1)$ space complexity approach allows us to process millions of IDs efficiently.

---

### Key Logic & Mathematical Proofs

#### Part 1: exact dual repetition

If a number $N$ consists of a pattern $P$ repeated exactly twice, and $P$ has length $k$, then:
$$N = P \cdot 10^k + P$$
$$N = P \cdot (10^k + 1)$$
Therefore, any such number must be perfectly divisible by $(10^k + 1)$. This check is sufficient because $P < 10^k$, ensuring the "halves" don't overlap or carry over in a way that breaks the pattern.

#### Part 2: $n$-times repetition

If a number $N$ consists of a pattern $P$ of length $L$ repeated $m$ times, the total length of $N$ is $D = m \cdot L$.
The number can be written as a sum of the pattern shifted by multiples of $L$:
$$N = \sum_{j=0}^{m-1} (P \cdot 10^{j \cdot L}) = P \cdot \sum_{j=0}^{m-1} 10^{j \cdot L}$$
This is a geometric series sum:
$$N = P \cdot \frac{10^{m \cdot L} - 1}{10^L - 1} = P \cdot \frac{10^D - 1}{10^L - 1}$$
Thus, $N$ must be divisible by $\frac{10^D - 1}{10^L - 1}$.

---

### Performance Note

  * **Time Complexity:** $O(N)$ where $N$ is the total count of numbers in all ranges. The validation itself is effectively $O(1)$ constant time because the number of digits fits within a 64-bit integer (max \~19 digits), meaning the inner loop runs at most $\approx 9$ times.
  * **Space Complexity:** $O(R)$ where $R$ is the number of range definitions (to store the list). We rely on `Seq` (lazy evaluation) to iterate through the billions of potential IDs without storing them in memory.