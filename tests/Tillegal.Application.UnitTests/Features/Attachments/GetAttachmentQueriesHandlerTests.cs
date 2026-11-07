using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Attachments.Mappings;
using Tillegal.Application.Features.Attachments.Queries.DownloadAttachment;
using Tillegal.Application.Features.Attachments.Queries.GetAttachmentById;
using Tillegal.Application.Features.Attachments.Queries.GetAttachmentsByConversationId;
using Tillegal.Application.Features.Attachments.Queries.GetUserAttachments;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Attachments;

public sealed class GetAttachmentQueriesHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IUserAttachmentService _attachmentService = Substitute.For<IUserAttachmentService>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IMapper _mapper;

    public GetAttachmentQueriesHandlerTests()
    {
        var config = MappingConfiguration.Create();
        new AttachmentMappingConfig().Register(config);
        _mapper = new Mapper(config);
    }

    [Fact]
    public async Task GetAttachmentsByConversationId_WhenUserIsAdmin_ShouldThrowForbiddenAccessException()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _currentUserService.Role.Returns("Admin");

        var handler = new GetAttachmentsByConversationIdQueryHandler(
            _currentUserService,
            _conversationService,
            _attachmentService,
            _mapper);

        var query = new GetAttachmentsByConversationIdQuery(Guid.NewGuid());
        var act = () => handler.Handle(query, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ForbiddenAccessException>();
        ex.WithMessage("*Admin is not allowed to access user files.*");
    }

    [Fact]
    public async Task GetAttachmentsByConversationId_WhenConversationBelongsToOtherUser_ShouldThrowForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = Conversation.Create(otherUserId, "Other conversation");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var handler = new GetAttachmentsByConversationIdQueryHandler(
            _currentUserService,
            _conversationService,
            _attachmentService,
            _mapper);

        var query = new GetAttachmentsByConversationIdQuery(conversationId);
        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task GetAttachmentsByConversationId_WhenOwner_ShouldReturnAttachments()
    {
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, "My conversation");
        var attachment = UserAttachment.Create(conversationId, userId, "doc.pdf", "path/doc.pdf", 100, "application/pdf");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _attachmentService.GetAttachmentsByConversationIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(new List<UserAttachment> { attachment });

        var handler = new GetAttachmentsByConversationIdQueryHandler(
            _currentUserService,
            _conversationService,
            _attachmentService,
            _mapper);

        var query = new GetAttachmentsByConversationIdQuery(conversationId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].FileName.Should().Be("doc.pdf");
    }

    [Fact]
    public async Task GetAttachmentById_WhenUserIsAdmin_ShouldThrowForbiddenAccessException()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _currentUserService.Role.Returns("Admin");

        var handler = new GetAttachmentByIdQueryHandler(
            _currentUserService,
            _conversationService,
            _attachmentService,
            _mapper);

        var query = new GetAttachmentByIdQuery(Guid.NewGuid(), Guid.NewGuid());
        var act = () => handler.Handle(query, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ForbiddenAccessException>();
        ex.WithMessage("*Admin is not allowed to access user files.*");
    }

    [Fact]
    public async Task GetAttachmentById_WhenAttachmentBelongsToOtherUser_ShouldThrowForbiddenAccessException()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, "My conversation");
        var attachment = UserAttachment.Create(conversationId, otherUserId, "doc.pdf", "path/doc.pdf", 100, "application/pdf");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _attachmentService.GetAttachmentByIdAsync(attachmentId, Arg.Any<CancellationToken>())
            .Returns(attachment);

        var handler = new GetAttachmentByIdQueryHandler(
            _currentUserService,
            _conversationService,
            _attachmentService,
            _mapper);

        var query = new GetAttachmentByIdQuery(conversationId, attachmentId);
        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task GetAttachmentById_WhenOwner_ShouldReturnAttachment()
    {
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, "My conversation");
        var attachment = UserAttachment.Create(conversationId, userId, "doc.pdf", "path/doc.pdf", 100, "application/pdf");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _attachmentService.GetAttachmentByIdAsync(attachmentId, Arg.Any<CancellationToken>())
            .Returns(attachment);

        var handler = new GetAttachmentByIdQueryHandler(
            _currentUserService,
            _conversationService,
            _attachmentService,
            _mapper);

        var query = new GetAttachmentByIdQuery(conversationId, attachmentId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.FileName.Should().Be("doc.pdf");
    }

    [Fact]
    public async Task DownloadAttachment_WhenUserIsAdmin_ShouldThrowForbiddenAccessException()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _currentUserService.Role.Returns("Admin");

        var handler = new DownloadAttachmentQueryHandler(
            _currentUserService,
            _conversationService,
            _attachmentService,
            _storageService);

        var query = new DownloadAttachmentQuery(Guid.NewGuid(), Guid.NewGuid());
        var act = () => handler.Handle(query, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ForbiddenAccessException>();
        ex.WithMessage("*Admin is not allowed to access user files.*");
    }

    [Fact]
    public async Task DownloadAttachment_WhenValid_ShouldReturnStoredObjectContent()
    {
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, "My conversation");
        var attachment = UserAttachment.Create(conversationId, userId, "doc.pdf", "path/doc.pdf", 100, "application/pdf");
        var storedObject = new StoredObject([10, 20, 30], "application/pdf");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);
        _attachmentService.GetAttachmentByIdAsync(attachmentId, Arg.Any<CancellationToken>())
            .Returns(attachment);
        _storageService.DownloadAsync("path/doc.pdf", Arg.Any<CancellationToken>())
            .Returns(storedObject);

        var handler = new DownloadAttachmentQueryHandler(
            _currentUserService,
            _conversationService,
            _attachmentService,
            _storageService);

        var query = new DownloadAttachmentQuery(conversationId, attachmentId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Content.Should().Equal([10, 20, 30]);
        result.ContentType.Should().Be("application/pdf");
        result.FileName.Should().Be("doc.pdf");
    }

    [Fact]
    public async Task GetUserAttachments_WhenUserIsAdmin_ShouldThrowForbiddenAccessException()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _currentUserService.Role.Returns("Admin");

        var handler = new GetUserAttachmentsQueryHandler(
            _currentUserService,
            _attachmentService,
            _mapper);

        var query = new GetUserAttachmentsQuery();
        var act = () => handler.Handle(query, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ForbiddenAccessException>();
        ex.WithMessage("*Admin is not allowed to access user files.*");
    }

    [Fact]
    public async Task GetUserAttachments_WhenOwner_ShouldReturnAllUserAttachments()
    {
        var userId = Guid.NewGuid();
        var attachment = UserAttachment.Create(Guid.NewGuid(), userId, "all.pdf", "path/all.pdf", 50, "application/pdf");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _attachmentService.GetAttachmentsByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<UserAttachment> { attachment });

        var handler = new GetUserAttachmentsQueryHandler(
            _currentUserService,
            _attachmentService,
            _mapper);

        var query = new GetUserAttachmentsQuery();
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].FileName.Should().Be("all.pdf");
    }
}

