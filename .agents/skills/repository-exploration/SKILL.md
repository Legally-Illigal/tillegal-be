---
name: repository-exploration
description: Map an unfamiliar repository before implementation. Use when starting work in a new codebase, investigating architecture, or locating the correct files and tests.
---

# Repository Exploration

## Procedure
1. Inspect the top-level directory and relevant project manifests.
2. Identify application entry points, major modules, dependency direction, configuration, and test projects.
3. Search for the existing implementation of the behavior being changed before proposing new code.
4. Read the closest relevant tests and project documentation.
5. Verify important claims against actual file paths and code; do not infer implementation from filenames alone.
6. Produce a concise repository map containing:
   - relevant paths and responsibilities;
   - important call/data flows;
   - existing conventions to follow;
   - build/test commands discovered in the repository;
   - uncertainties and contradictions.

## Constraints
- Do not modify files during exploration unless the user explicitly asks for changes.
- Do not dump the entire repository into the response.
- Separate verified findings from hypotheses.
