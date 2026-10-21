# ADR 0003: Serverless-Friendly External Integrations

## Status
Accepted

## Context
Tillegal Backend integrates with third-party cloud services for caching, email delivery, object storage, and background scheduling. The system requires stateless, cloud-native integrations suitable for containerized and serverless environments.

## Decision
1. **Cache (`ICacheService`)**:
   - Utilize **Upstash Redis** REST API (`UpstashRedisCacheService`) over HTTPS rather than maintaining persistent TCP connection pools.
   - Values are stored as Base64 strings. Expirations use millisecond precision (`PX`).
2. **Transactional Email (`IEmailService`)**:
   - Utilize **Resend** REST API (`ResendEmailService`, `https://api.resend.com/emails`) with Bearer token authentication for transactional email dispatch.
3. **Object Storage (`IStorageService`)**:
   - Utilize **Supabase Storage** REST API (`SupabaseStorageService`, `/storage/v1/object/{bucket}/{key}`).
   - Supports `UploadAsync` (with `x-upsert: true`), `DownloadAsync`, and `DeleteAsync`. Validates path segments against directory traversal.
4. **Background Scheduling**:
   - Utilize **Quartz.NET** (`AddQuartz` + `AddQuartzHostedService`).
   - Standard cron job `DailyUpdateJob` configured to run at midnight (`0 0 0 * * ?`) using timezone configurable via `BackgroundJobs:TimeZone` (defaults to `Asia/Ho_Chi_Minh`).
5. **HTTP Pipeline Observability**:
   - All external service `HttpClient` instances register `LoggingHandler` to automatically log outbound requests and attach correlation IDs.

## Consequences
- **Positive**:
  - Stateless HTTP APIs eliminate connection pooling overhead and firewall/proxy TCP socket drops.
  - Consistent error handling and logging via `LoggingHandler`.
- **Negative**:
  - REST cache commands have slightly higher round-trip latency compared to persistent in-VPC TCP Redis sockets.
