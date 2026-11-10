using FluentAssertions;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Attachments.Commands.DeleteAttachment;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Attachments;

public sealed class DeleteAttachmentCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IUserAttachmentService _attachmentService = Substitute.For<IUserAttachmentService>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteAttachmentCommandHandler _handler;

    public DeleteAttachmentCommandHandlerTests()
    {
        _handler = new DeleteAttachmentCommandHandler(
            _currentUserService,
            _conversationService,
            _attachmentService,
            _storageService,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);
        var command = new DeleteAttachmentCommand(Guid.NewGuid(), Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_ShouldThrowForbiddenAccessException()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _currentUserService.Role.Returns("Admin");
        var command = new DeleteAttachmentCommand(Guid.NewGuid(), Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ForbiddenAccessException>();
        ex.WithMessage("*Admin is not allowed to upload or manage user files.*");
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);

        var command = new DeleteAttachmentCommand(conversationId, Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenConversationBelongsToOtherUser_ShouldThrowForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = Conversation.Create(otherUserId, "Hội thoại");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var command = new DeleteAttachmentCommand(conversationId, Guid.NewGuid());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WhenAttachmentNotFound_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, "Hội thoại");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _attachmentService.GetAttachmentByIdAsync(attachmentId, Arg.Any<CancellationToken>())
            .Returns((UserAttachment?)null);

        var command = new DeleteAttachmentCommand(conversationId, attachmentId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenAttachmentBelongsToOtherUser_ShouldThrowForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, "Hội thoại");
        var attachment = UserAttachment.Create(conversationId, otherUserId, "file.pdf", "path/file.pdf", 100, "application/pdf");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _attachmentService.GetAttachmentByIdAsync(attachmentId, Arg.Any<CancellationToken>())
            .Returns(attachment);

        var command = new DeleteAttachmentCommand(conversationId, attachmentId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldDeleteFromStorageAndSoftDeleteAttachment()
    {
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, "Hội thoại");
        var attachment = UserAttachment.Create(conversationId, userId, "file.pdf", "path/file.pdf", 100, "application/pdf");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _attachmentService.GetAttachmentByIdAsync(attachmentId, Arg.Any<CancellationToken>())
            .Returns(attachment);

        var command = new DeleteAttachmentCommand(conversationId, attachmentId);

        await _handler.Handle(command, CancellationToken.None);

        await _storageService.Received(1).DeleteAsync("path/file.pdf", Arg.Any<CancellationToken>());
        await _attachmentService.Received(1).DeleteAttachmentAsync(attachment, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

