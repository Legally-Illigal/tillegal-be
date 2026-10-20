---
name: implement-task
description: Implement a bounded software task from its requirements and acceptance criteria through code changes, verification, and handoff.
---

# Implement Task

## Procedure
1. Read the task specification and identify objective, scope, exclusions, dependencies, and acceptance criteria.
2. Inspect the working tree and relevant code, tests, configuration, and project rules.
3. Check prerequisites. If a blocking requirement is missing, ask a focused question; otherwise state reasonable, low-risk assumptions.
4. Write a short implementation plan for non-trivial changes.
5. Implement the smallest coherent change that satisfies the task.
6. Add or update tests where appropriate.
7. Run relevant tests/build/static checks and inspect their actual results.
8. Review the final diff for unrelated changes, security issues, accidental generated files, and documentation drift.
9. Compare the result against every acceptance criterion.
10. Report changes, verification results, limitations, and follow-up work.

## Stop and ask before
- expanding the task into unrelated refactoring;
- making irreversible or destructive changes;
- changing a major architectural decision;
- introducing a new dependency with material impact;
- choosing between materially different product behaviors not specified by the task.

## Completion criteria
Do not call the task complete solely because code was written. State any acceptance criteria that remain unverified.
