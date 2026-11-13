using FluentAssertions;
using MapsterMapper;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Auth.Commands.VerifyRegistration;
using Tillegal.Application.Features.Auth.Queries.GetCurrentUser;
using Tillegal.Domain.Entities;

namespace Tillegal.Application.UnitTests.Features.Auth;

public sealed class AuthMappingTests
{
    private readonly IMapper _mapper;

    public AuthMappingTests()
    {
        var config = MappingConfiguration.Create();
        _mapper = new Mapper(config);
    }

    [Fact]
    public void Map_User_To_VerifyRegistrationResult_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            FullName = "Nguyen Van A",
            PhoneNumber = "0987654321",
            RoleId = Guid.NewGuid()
        };

        // Act
        var result = _mapper.Map<VerifyRegistrationResult>(user);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Email.Should().Be("test@example.com");
        result.FullName.Should().Be("Nguyen Van A");
        result.Message.Should().Be("Account registration and verification completed successfully.");
    }

    [Fact]
    public void Map_UserWithNullEmail_To_VerifyRegistrationResult_ShouldHandleNullGracefully()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var user = new User
        {
            Id = userId,
            Email = null,
            FullName = "No Email User"
        };

        // Act
        var result = _mapper.Map<VerifyRegistrationResult>(user);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Email.Should().BeEmpty();
        result.FullName.Should().Be("No Email User");
        result.Message.Should().Be("Account registration and verification completed successfully.");
    }

    [Fact]
    public void Map_User_To_CurrentUserDto_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var user = new User
        {
            Id = userId,
            Email = "me@example.com",
            FullName = "Nguyen Van B",
            PhoneNumber = "0912345678",
            Role = new Role { Id = Guid.NewGuid(), Name = "Lawyer" },
            IsActive = true
        };

        // Act
        var result = _mapper.Map<CurrentUserDto>(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Email.Should().Be("me@example.com");
        result.FullName.Should().Be("Nguyen Van B");
        result.PhoneNumber.Should().Be("0912345678");
        result.Role.Should().Be("Lawyer");
        result.IsActive.Should().BeTrue();
    }
}
