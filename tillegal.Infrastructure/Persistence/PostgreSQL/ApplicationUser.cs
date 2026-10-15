using Microsoft.AspNetCore.Identity;

namespace Tillegal.Infrastructure.Persistence.PostgreSQL;

public sealed class ApplicationUser : IdentityUser<Guid>;
