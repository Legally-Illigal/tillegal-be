using FluentAssertions;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.AiEvaluations.Commands.DeleteEvaluation;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.AiEvaluations;

public sealed class DeleteEvaluationCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IAiEvaluationService _aiEvaluationService = Substitute.For<IAiEvaluationService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteEvaluationCommandHandler _handler;

    public DeleteEvaluationCommandHandlerTests()
    {
        _handler = new DeleteEvaluationCommandHandler(
            _currentUserService,
            _aiEvaluationService,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new DeleteEvaluationCommand(Guid.CreateVersion7());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenEvaluationNotFound_ShouldThrowNotFoundException()
    {
        var userId = Guid.CreateVersion7();
        var evalId = Guid.CreateVersion7();
        _currentUserService.UserId.Returns(userId);
        _aiEvaluationService.GetByIdAsync(evalId, Arg.Any<CancellationToken>())
            .Returns((AiEvaluationLog?)null);

        var command = new DeleteEvaluationCommand(evalId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserNotOwnerAndNotAdmin_ShouldThrowForbiddenAccessException()
    {
        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();
        var evaluation = AiEvaluationLog.Create(Guid.CreateVersion7(), otherUserId, true, 5);

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _aiEvaluationService.GetByIdAsync(evaluation.Id, Arg.Any<CancellationToken>())
            .Returns(evaluation);

        var command = new DeleteEvaluationCommand(evaluation.Id);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WhenUserIsOwner_ShouldSoftDeleteAndCommit()
    {
        var userId = Guid.CreateVersion7();
        var evaluation = AiEvaluationLog.Create(Guid.CreateVersion7(), userId, true, 5);

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _aiEvaluationService.GetByIdAsync(evaluation.Id, Arg.Any<CancellationToken>())
            .Returns(evaluation);

        var command = new DeleteEvaluationCommand(evaluation.Id);

        await _handler.Handle(command, CancellationToken.None);

        await _aiEvaluationService.Received(1).DeleteEvaluationAsync(evaluation, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_ShouldAllowDeletingOtherUserEvaluation()
    {
        var adminId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();
        var evaluation = AiEvaluationLog.Create(Guid.CreateVersion7(), otherUserId, true, 5);

        _currentUserService.UserId.Returns(adminId);
        _currentUserService.Role.Returns("Admin");
        _aiEvaluationService.GetByIdAsync(evaluation.Id, Arg.Any<CancellationToken>())
            .Returns(evaluation);

        var command = new DeleteEvaluationCommand(evaluation.Id);

        await _handler.Handle(command, CancellationToken.None);

        await _aiEvaluationService.Received(1).DeleteEvaluationAsync(evaluation, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

