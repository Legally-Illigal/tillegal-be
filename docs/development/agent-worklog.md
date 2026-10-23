# Agent Work Log

Use this file as a concise handoff record, not as a transcript. Append an entry for each task that changes repository content. Keep current work, blockers, next action, changed paths, and checks explicit so the next agent can continue without rereading the entire repository.

## 2026-10-23 — Clarify architecture and establish feature backlog

- **Status:** Completed for this documentation task. Backend feature implementation status remains **Chưa đánh giá** for all BE-F01–BE-F09; this task did not audit or implement those features.
- **Completed:** Recorded the nine requested backend feature scopes and Backend/AI Service ownership boundaries in `docs/development/backend-feature-backlog.md`; documented excluded scope and evidence-based status rules. Added agent handoff expectations to `AGENTS.md` and linked the backlog/log from the documentation index and roadmap.
- **Architecture context:** Clarified that domain repository contracts belong in `Tillegal.Domain`, their EF Core implementations in `Tillegal.Persistence`, Application handlers call Domain services, and Application-owned technical ports (such as password hashing/JWT generation) are implemented by Infrastructure. Documented the intended login flow while explicitly noting the login endpoint/service/repository/JWT generation are not present in the inspected baseline and that Bearer-token input is not automatically cookie authentication.
- **Changed paths:** `AGENTS.md`; `docs/README.md`; `docs/development/status-and-roadmap.md`; `docs/development/backend-feature-backlog.md`; `docs/development/agent-worklog.md`; `docs/architecture/boundaries.md`; `docs/architecture/data-flow.md`; `docs/architecture/overview.md`; `docs/development/conventions.md`.
- **Checks:** `git diff --check` passed for tracked changes. The two new Markdown files were manually reviewed. No build or tests were run because this is documentation-only.
- **Remaining / next action:** None for this documentation task.
