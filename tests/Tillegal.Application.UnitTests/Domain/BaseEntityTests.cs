using FluentAssertions;
using Tillegal.Domain.Common;
using Xunit;

namespace Tillegal.Application.UnitTests.Domain;

public sealed class BaseEntityTests
{
    private sealed class TestEntity : BaseEntity;

    [Fact]
    public void Constructor_Should_Initialize_ValidGuidV7_And_CreatedAt()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().NotBeEmpty();
        entity.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        entity.UpdatedAt.Should().BeNull();
        entity.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void SoftDelete_Should_Set_DeletedAt_And_UpdatedAt()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.SoftDelete();

        // Assert
        entity.DeletedAt.Should().NotBeNull();
        entity.UpdatedAt.Should().NotBeNull();
        entity.DeletedAt.Should().Be(entity.UpdatedAt);
        entity.DeletedAt.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void SoftDelete_WhenCalledTwice_Should_Not_OverwriteData()
    {
        // Arrange
        var entity = new TestEntity();
        entity.SoftDelete();
        var initialDeletedAt = entity.DeletedAt;

        // Act
        entity.SoftDelete();

        // Assert
        entity.DeletedAt.Should().Be(initialDeletedAt);
    }
}
