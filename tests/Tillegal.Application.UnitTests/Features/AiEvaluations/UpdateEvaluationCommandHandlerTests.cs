using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.AiEvaluations.Commands.UpdateEvaluation;
using Tillegal.Application.Features.AiEvaluations.Mappings;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.AiEvaluations;

public sealed class UpdateEvaluationCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IAiEvaluationService _aiEvaluationService = Substitute.For<IAiEvaluationService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper;
    private readonly UpdateEvaluationCommandHandler _handler;

    public UpdateEvaluationCommandHandlerTests()
    {
        var config = MappingConfiguration.Create();
        new AiEvaluationMappingConfig().Register(config);
        _mapper = new Mapper(config);
        _handler = new UpdateEvaluationCommandHandler(
            _currentUserService,
            _aiEvaluationService,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new UpdateEvaluationCommand(Guid.CreateVersion7(), true, 5, "Cập nhật");

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

        var command = new UpdateEvaluationCommand(evalId, true, 5, "Cập nhật");

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

        var command = new UpdateEvaluationCommand(evaluation.Id, true, 4, "Cập nhật");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WhenUserIsOwner_ShouldUpdateAndCommit()
    {
        var userId = Guid.CreateVersion7();
        var evaluation = AiEvaluationLog.Create(Guid.CreateVersion7(), userId, true, 3, "Bình thường");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _aiEvaluationService.GetByIdAsync(evaluation.Id, Arg.Any<CancellationToken>())
            .Returns(evaluation);
        _aiEvaluationService.UpdateEvaluationAsync(
            evaluation,
            true,
            5,
            "Rất tốt",
            Arg.Any<CancellationToken>())
            .Returns(evaluation);

        var command = new UpdateEvaluationCommand(evaluation.Id, true, 5, "Rất tốt");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_ShouldAllowUpdatingOtherUserEvaluation()
    {
        var adminId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();
        var evaluation = AiEvaluationLog.Create(Guid.CreateVersion7(), otherUserId, true, 3);

        _currentUserService.UserId.Returns(adminId);
        _currentUserService.Role.Returns("Admin");
        _aiEvaluationService.GetByIdAsync(evaluation.Id, Arg.Any<CancellationToken>())
            .Returns(evaluation);
        _aiEvaluationService.UpdateEvaluationAsync(
            evaluation,
            true,
            5,
            "Admin cập nhật",
            Arg.Any<CancellationToken>())
            .Returns(evaluation);

        var command = new UpdateEvaluationCommand(evaluation.Id, true, 5, "Admin cập nhật");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

