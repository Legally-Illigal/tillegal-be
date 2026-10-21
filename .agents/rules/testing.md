---
trigger: model_decision
description: Apply when implementing or changing behavior, tests, build configuration, or test infrastructure.
---

# Testing Rule

- Identify the behavior and acceptance criteria that need verification before editing.
- Add or update tests for meaningful behavior changes where the repository has a test framework.
- Prefer deterministic tests and cover relevant failure paths and boundary cases.
- Run the narrowest relevant test first, then broader checks when practical.
- Report exact commands and outcomes. Never invent test results.
- If tests cannot run, state the blocker and provide the unverified risks.
