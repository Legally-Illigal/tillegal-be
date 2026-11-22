using FluentAssertions;
using MapsterMapper;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Users.Models;
using Tillegal.Domain.Entities;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Users;

public sealed class UserMappingTests
{
    private readonly IMapper _mapper;

    public UserMappingTests()
    {
        var config = MappingConfiguration.Create();
        _mapper = new Mapper(config);
    }

    [Fact]
    public void Map_User_To_UserProfileDto_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-10);
        var lastActiveAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        var user = new User
        {
            Id = userId,
            Email = "me@example.com",
            FullName = "Nguyen Van B",
            PhoneNumber = "0912345678",
            Role = new Role { Id = Guid.NewGuid(), Name = "Lawyer" },
            IsActive = true,
            CreatedAt = createdAt,
            LastActiveAt = lastActiveAt
        };

        // Act
        var result = _mapper.Map<UserProfileDto>(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Email.Should().Be("me@example.com");
        result.FullName.Should().Be("Nguyen Van B");
        result.PhoneNumber.Should().Be("0912345678");
        result.Role.Should().Be("Lawyer");
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().Be(createdAt);
        result.LastActiveAt.Should().Be(lastActiveAt);
    }
}
