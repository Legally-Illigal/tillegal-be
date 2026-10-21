---
trigger: model_decision
description: Apply when a task changes architecture, module boundaries, dependencies, or cross-layer code.
---

# Architecture Rule

- Inspect the existing architecture and relevant project files before proposing structural changes.
- Respect dependency direction and module boundaries documented by the repository.
- Do not move responsibilities across layers just to make an implementation convenient.
- Prefer existing abstractions and patterns unless they demonstrably fail the requirement.
- If architecture documentation and implementation disagree, explain the discrepancy and ask for a decision when it changes the intended design.
- Avoid broad refactors in a task with a narrow scope.
