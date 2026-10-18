# tillegal-be

Tillegal - Hệ thống Backend (Clean Architecture + DDD) trên nền tảng **.NET 10**.

---

## 1. Cấu trúc Giải pháp (Solution Structure)

```text
tillegal-be/
├── src/
│   ├── Tillegal.Domain/             # Enterprise Business Rules (Entities, Value Objects, Domain Events)
│   ├── Tillegal.Application/        # Application Business Rules (CQRS, Use Cases, Behaviors, DTOs)
│   ├── Tillegal.Persistence/        # PostgreSQL Data Access (EF Core DbContext, Configurations, Migrations)
│   ├── Tillegal.Infrastructure/     # External Adapters (Upstash Redis, Resend Mail, Supabase Storage, Quartz)
│   ├── Tillegal.Api/                # HTTP Presentation Library (Controllers, Middlewares, OpenAPI/Swagger)
│   └── Tillegal.AppHost/            # Executable Web Host & Composition Root (Program.cs, DI Bootstrapper)
└── tests/
    ├── Tillegal.Application.UnitTests/  # Unit tests cho MediatR Behaviors, Services, Entities
    └── Tillegal.ArchitectureTests/     # Architecture tests (NetArchTest) bảo vệ ranh giới Clean Architecture
```

---

## 2. Quy tắc Phụ thuộc Kiến trúc (Architectural Boundaries)

- **`Domain`**: Độc lập 100%, không phụ thuộc vào bất kỳ project nào.
- **`Application`** $\rightarrow$ `Domain`.
- **`Persistence`** $\rightarrow$ `Domain`, `Application`.
- **`Infrastructure`** $\rightarrow$ `Application`.
- **`Api`** $\rightarrow$ `Application` (không tham chiếu Infrastructure hay Persistence).
- **`AppHost`** $\rightarrow$ `Domain`, `Application`, `Persistence`, `Infrastructure`, `Api` (đóng vai trò Composition Root duy nhất ráp nối DI).

---

## 3. Lệnh Phát triển & Kiểm thử (Developer Commands)

### Khôi phục & Biên dịch
```bash
dotnet restore tillegal-be.slnx
dotnet build tillegal-be.slnx
```

### Chạy Kiểm thử Tự động (Unit & Architecture Tests)
```bash
dotnet test tillegal-be.slnx
```

### Định dạng Mã nguồn Chuẩn hóa (.editorconfig)
```bash
dotnet format tillegal-be.slnx --verify-no-changes
```

### Khởi chạy Ứng dụng
```bash
# Thiết lập biến môi trường hoặc copy cấu hình mẫu
cp src/Tillegal.AppHost/appsettings.Example.json src/Tillegal.AppHost/appsettings.Development.json

# Chạy server
dotnet run --project src/Tillegal.AppHost
```

- Swagger UI: `http://localhost:5063/swagger` hoặc `https://localhost:7001/swagger`
- Liveness Probe: `http://localhost:5063/healthz`
- Readiness Probe: `http://localhost:5063/readyz`
