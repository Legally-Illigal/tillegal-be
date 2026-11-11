using FluentAssertions;
using MapsterMapper;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Admin.Users.Models;
using Tillegal.Domain.Entities;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Admin;

public sealed class AdminUserMappingTests
{
    private readonly IMapper _mapper;

    public AdminUserMappingTests()
    {
        var config = MappingConfiguration.Create();
        _mapper = new Mapper(config);
    }

    [Fact]
    public void Map_UserWithLawerRole_ShouldNormalizeToLawyer()
    {
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            FullName = "Luật sư Nguyễn Văn A",
            Email = "lawyer@example.com",
            PhoneNumber = "0901234567",
            Role = new Role(Guid.CreateVersion7(), "Lawer"),
            IsActive = true
        };

        var dto = _mapper.Map<UserDto>(user);

        dto.Should().NotBeNull();
        dto.RoleName.Should().Be("Lawyer");
    }

    [Fact]
    public void Map_UserWithStandardRole_ShouldPreserveRoleName()
    {
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            FullName = "Admin User",
            Email = "admin@example.com",
            PhoneNumber = null,
            Role = new Role(Guid.CreateVersion7(), "Admin"),
            IsActive = true
        };

        var dto = _mapper.Map<UserDto>(user);

        dto.Should().NotBeNull();
        dto.RoleName.Should().Be("Admin");
    }
}

