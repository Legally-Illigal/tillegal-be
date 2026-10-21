---
name: code-review
description: Review a change for correctness, security, architecture, maintainability, and regression risk. Use before handing off or merging code.
---

# Code Review

## Review order
1. Understand the task and acceptance criteria.
2. Inspect the diff, then read surrounding code to understand its context.
3. Check correctness, edge cases, error handling, data integrity, and backward compatibility.
4. Check security and privacy implications where relevant.
5. Check architecture boundaries, dependency direction, and consistency with established patterns.
6. Check tests: whether they cover meaningful behavior and failure cases.
7. Check documentation and configuration consistency.

## Findings
- Lead with actionable findings, ordered by severity.
- For each finding, include file/line or a precise location, the scenario, impact, and a suggested correction.
- Separate definite bugs from risks or questions.
- Do not invent findings to fill a checklist.
- If no issues are found, state the scope reviewed and what was not verified.
