---
name: debug-and-fix
description: Investigate a bug, identify its root cause, implement a focused fix, and verify against regression.
---

# Debug and Fix

## Procedure
1. Capture the observed behavior, expected behavior, error output, and reproduction steps.
2. Reproduce the issue when possible before changing code.
3. Trace the failing path and inspect relevant logs, callers, state, and tests.
4. Identify the root cause and supporting evidence; distinguish it from symptoms.
5. Propose the smallest safe fix.
6. Add a regression test when practical.
7. Run the regression test and relevant surrounding checks.
8. Review for side effects and report any remaining uncertainty.

## Constraints
- Do not make unrelated changes while debugging.
- Do not suppress errors, loosen validation, or skip tests merely to hide the symptom.
- If the issue cannot be reproduced, explain what evidence was available and how confidence is limited.
