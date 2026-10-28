# Backend Feature Backlog

## Purpose and status rules

This document is the system-level feature inventory for the backend. The entries below define feature scope and ownership boundaries; they are **not decomposed implementation tasks** and do not assert that a feature is implemented.

Initial status for every feature is **Chưa đánh giá** because this inventory has not yet been reconciled against all current source code and tests. Before changing a status, inspect the relevant implementation and tests and record the evidence. Use these status values consistently:

- **Chưa đánh giá** — implementation status has not been verified.
- **Chưa triển khai** — no implementation exists for the feature.
- **Đang triển khai** — verified work is underway; summarize it in the work log.
- **Một phần** — some scope exists, but meaningful scope remains.
- **Hoàn thành** — the agreed scope is implemented and verified; link or summarize validation evidence.
- **Blocked** — work cannot proceed; record the blocker and required decision.

Do not split these feature entries into child implementation tasks unless the user requests task decomposition. Do not infer feature completion from infrastructure scaffolding, database entities, or documentation alone.

## Backend feature inventory

| ID | Tính năng | Phạm vi chính | Trạng thái |
|---|---|---|---|
| BE-F01 | Quản lý tài khoản | Đăng ký, đăng nhập, đăng xuất và quản lý phiên xác thực người dùng. | Một phần |
| BE-F02 | Xác thực và phân quyền | Xác thực danh tính, kiểm soát quyền truy cập theo vai trò và quyền sở hữu dữ liệu. | Chưa đánh giá |
| BE-F03 | Quản lý hội thoại | Tạo, xem, cập nhật và quản lý các cuộc hội thoại của người dùng. | Hoàn thành |
| BE-F04 | Quản lý tin nhắn và lịch sử | Lưu trữ, truy xuất tin nhắn và cung cấp ngữ cảnh hội thoại cho các lượt trao đổi tiếp theo. | Chưa đánh giá |
| BE-F05 | Tiếp nhận và xử lý yêu cầu pháp lý | Cung cấp API tiếp nhận yêu cầu từ Frontend và điều phối yêu cầu đến AI Service. | Chưa đánh giá |
| BE-F06 | Tích hợp AI Service | Giao tiếp với AI Service để thực hiện các chức năng hỏi đáp, tư vấn pháp lý và kiểm chứng trích dẫn. | Chưa đánh giá |
| BE-F07 | Quản lý trạng thái và kết quả xử lý | Tiếp nhận, xử lý và trả kết quả từ AI Service theo hợp đồng API; xử lý lỗi hoặc kết quả không hợp lệ. | Chưa đánh giá |
| BE-F08 | Chức năng quản trị | Cung cấp API quản trị tài khoản và thông tin trạng thái vận hành cần thiết cho quản trị viên. | Chưa đánh giá |
| BE-F09 | Kiểm soát bảo mật và nhật ký | Kiểm soát truy cập dữ liệu, bảo vệ thông tin nhạy cảm và ghi nhận các sự kiện cần thiết phục vụ kiểm tra. | Chưa đánh giá |

## Backend / AI Service responsibility boundary

| Chức năng | Thành phần chịu trách nhiệm chính |
|---|---|
| Quản lý tài khoản, xác thực, phân quyền | Backend |
| Quản lý hội thoại, tin nhắn và lịch sử | Backend |
| Tiếp nhận yêu cầu từ Frontend và giao tiếp với AI | Backend |
| Tìm kiếm, truy xuất tri thức pháp luật (RAG) | AI Service |
| Phân tích câu hỏi và tình huống pháp lý | AI Service |
| Sinh câu trả lời, lập luận pháp lý | AI Service |
| Kiểm chứng căn cứ và trích dẫn pháp luật | AI Service |
| Quản trị tài khoản và trạng thái vận hành | Backend cung cấp API; AI Service chịu trách nhiệm về trạng thái nội bộ của mình |

## Explicitly out of scope until confirmed

The current feature inventory does not include file uploads, payments, or streaming. Google account registration and login are implemented following explicit user requests; other OAuth providers remain out of scope.

## Agent workflow

- Start with this inventory and `docs/development/status-and-roadmap.md` for product scope and the current baseline. Read only the source files, tests, and architecture documents relevant to the active feature; do not reread the whole repository by default.
- Keep feature status truthful and evidence-based. When work changes implementation status, update the relevant row and add a concise entry to `docs/development/agent-worklog.md` with the code/tests/docs examined.
- For every task that changes persistent repository content, update the work log before handoff: what was completed, what remains or is blocked, files changed, and checks actually run. Clearly mark unfinished work; never report a task as complete if acceptance criteria remain unverified.
- When a task is in progress across turns, maintain its current state and next action in the work log. Do not create a child task breakdown unless requested.
- Keep entries concise and append new entries; do not erase prior history or overwrite unrelated user work.
