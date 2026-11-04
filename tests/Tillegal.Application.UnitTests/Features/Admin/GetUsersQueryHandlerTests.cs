using FluentAssertions;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Admin.Users.Models;
using Tillegal.Application.Features.Admin.Users.Queries.GetUsers;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Admin;

public sealed class GetUsersQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly GetUsersQueryHandler _handler;

    public GetUsersQueryHandlerTests()
    {
        _handler = new GetUsersQueryHandler(_currentUserService, _userRepository, _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.IsAuthenticated.Returns(false);

        var act = () => _handler.Handle(new GetUsersQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAdmin_ShouldThrowForbiddenAccessException()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.Role.Returns("User");

        var act = () => _handler.Handle(new GetUsersQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WhenAdminRequestsUsers_ShouldReturnPagedResult()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.Role.Returns("Admin");

        var users = new List<User>
        {
            new() { Id = Guid.CreateVersion7(), FullName = "Test User", Email = "test@test.com" }
        };
        _userRepository.GetPagedAsync(0, 10, null, Arg.Any<CancellationToken>())
            .Returns((users as IReadOnlyList<User>, 1));

        var userDto = new UserDto(users[0].Id, "Test User", "test@test.com", null, null, true, DateTimeOffset.UtcNow, null);
        _mapper.Map<UserDto>(Arg.Any<User>()).Returns(userDto);

        var result = await _handler.Handle(new GetUsersQuery(1, 10), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
}
