using FluentAssertions;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Admin.Users.Models;
using Tillegal.Application.Features.Admin.Users.Queries.GetUserById;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Admin;

public sealed class GetUserByIdQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _handler = new GetUserByIdQueryHandler(_currentUserService, _userRepository, _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.IsAuthenticated.Returns(false);

        var act = () => _handler.Handle(new GetUserByIdQuery(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAdmin_ShouldThrowForbiddenAccessException()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.Role.Returns("User");

        var act = () => _handler.Handle(new GetUserByIdQuery(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.Role.Returns("Admin");
        _userRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var act = () => _handler.Handle(new GetUserByIdQuery(Guid.CreateVersion7()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldReturnUserDto()
    {
        var userId = Guid.CreateVersion7();
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.Role.Returns("Admin");

        var user = new User { Id = userId, FullName = "Test User", Email = "test@test.com" };
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        var expectedDto = new UserDto(userId, "Test User", "test@test.com", null, null, true, DateTimeOffset.UtcNow, null);
        _mapper.Map<UserDto>(user).Returns(expectedDto);

        var result = await _handler.Handle(new GetUserByIdQuery(userId), CancellationToken.None);

        result.Should().Be(expectedDto);
    }
}
