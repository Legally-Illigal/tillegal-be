# ADR 0002: PostgreSQL, EF Core, Guid v7, and Unit of Work

## Status
Accepted

## Context
The application requires a robust relational database with strong ACID transactions, performant indexing, auditable entity lifecycle tracking, and soft-delete capability across all business models.

## Decision
1. **Database Engine**: **PostgreSQL** accessed via **Entity Framework Core** (`Npgsql.EntityFrameworkCore.PostgreSQL`).
2. **Primary Key Strategy**:
   - Primary keys use **Guid Version 7** (`Guid.CreateVersion7()`), combining UUID uniqueness with sequential timestamp ordering to eliminate B-tree index fragmentation.
3. **Auditing & Soft Deletion**:
   - All domain entities inherit from `BaseEntity`.
   - Properties: `Id` (Guid v7), `CreatedAt` (DateTimeOffset UTC), `UpdatedAt` (DateTimeOffset? UTC), `DeletedAt` (DateTimeOffset? UTC).
   - In `AppDbContext.SaveChangesAsync()`, any modified `BaseEntity` automatically has `UpdatedAt` set to `DateTimeOffset.UtcNow`.
   - Soft-delete method `entity.SoftDelete()` sets `DeletedAt` and `UpdatedAt`.
   - Global query filter `DeletedAt == null` is automatically applied to all `BaseEntity` configurations in `AppDbContext.OnModelCreating`.
4. **Unit of Work**:
   - `AppDbContext` implements `IUnitOfWork` exposing explicit transaction boundaries (`BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`) and `SaveChangesAsync`.
5. **Development Seeding**:
   - `IDevelopmentDataSeed` and `DevelopmentDatabaseSeeder` run automatically in Development environment during host startup.

## Consequences
- **Positive**:
  - Predictable sequential primary keys friendly to PostgreSQL indexes.
  - Consistent soft-delete without requiring manual `WHERE is_deleted = false` on every query.
  - Transactions managed uniformly across multiple aggregate changes.
- **Negative**:
  - Developers must use `.IgnoreQueryFilters()` when explicitly querying soft-deleted entities.
