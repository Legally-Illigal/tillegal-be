using FluentAssertions;
using Mapster;
using MapsterMapper;
using NSubstitute;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Mappings;
using Tillegal.Application.Features.Attachments.Commands.UploadAttachment;
using Tillegal.Application.Features.Attachments.Mappings;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Services;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Attachments;

public sealed class UploadAttachmentCommandHandlerTests
{
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IConversationService _conversationService = Substitute.For<IConversationService>();
    private readonly IUserAttachmentService _attachmentService = Substitute.For<IUserAttachmentService>();
    private readonly IStorageService _storageService = Substitute.For<IStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper;
    private readonly UploadAttachmentCommandHandler _handler;

    public UploadAttachmentCommandHandlerTests()
    {
        var config = MappingConfiguration.Create();
        new AttachmentMappingConfig().Register(config);
        _mapper = new Mapper(config);
        _handler = new UploadAttachmentCommandHandler(
            _currentUserService,
            _conversationService,
            _attachmentService,
            _storageService,
            _unitOfWork,
            _mapper);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _currentUserService.UserId.Returns((Guid?)null);
        using var stream = new MemoryStream([1, 2, 3]);
        var command = new UploadAttachmentCommand(Guid.NewGuid(), "test.pdf", "application/pdf", 3, stream);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _currentUserService.Role.Returns("Admin");
        using var stream = new MemoryStream([1, 2, 3]);
        var command = new UploadAttachmentCommand(Guid.NewGuid(), "test.pdf", "application/pdf", 3, stream);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<ForbiddenAccessException>();
        ex.WithMessage("*Admin is not allowed to upload or manage user files.*");
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);

        using var stream = new MemoryStream([1, 2, 3]);
        var command = new UploadAttachmentCommand(conversationId, "test.pdf", "application/pdf", 3, stream);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenConversationBelongsToAnotherUser_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = Conversation.Create(otherUserId, "Hội thoại người khác");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        using var stream = new MemoryStream([1, 2, 3]);
        var command = new UploadAttachmentCommand(conversationId, "test.pdf", "application/pdf", 3, stream);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<ForbiddenAccessException>();
        ex.WithMessage("*You do not have access to this conversation.*");
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldUploadToStorageAndPersistAttachment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, "Hội thoại của tôi");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var createdAttachment = UserAttachment.Create(
            conversationId,
            userId,
            "test.pdf",
            $"attachments/{userId}/12345/test.pdf",
            3,
            "application/pdf");

        _attachmentService.CreateAttachmentAsync(
            conversationId,
            userId,
            "test.pdf",
            Arg.Any<string>(),
            3,
            "application/pdf",
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(createdAttachment);

        using var stream = new MemoryStream([1, 2, 3]);
        var command = new UploadAttachmentCommand(conversationId, "test.pdf", "application/pdf", 3, stream);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be("test.pdf");
        result.UserId.Should().Be(userId);
        result.ConversationId.Should().Be(conversationId);

        await _storageService.Received(1).UploadAsync(
            Arg.Is<string>(s => s.StartsWith($"attachments/{userId}/")),
            stream,
            "application/pdf",
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenConversationAlreadyHas5Attachments_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, "Hội thoại của tôi");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var existingAttachments = Enumerable.Range(1, 5)
            .Select(i => UserAttachment.Create(conversationId, userId, $"file{i}.pdf", $"path/file{i}.pdf", 100, "application/pdf"))
            .ToList();

        _attachmentService.GetAttachmentsByConversationIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(existingAttachments);

        using var stream = new MemoryStream([1, 2, 3]);
        var command = new UploadAttachmentCommand(conversationId, "sixth_file.pdf", "application/pdf", 3, stream);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.WithMessage("*at most 5 attachments*");
        await _storageService.DidNotReceive().UploadAsync(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSameContentUploaded_EvenWithDifferentName_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, "Hội thoại của tôi");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var fileContent = new byte[] { 1, 2, 3, 4, 5 };
        var hash = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(fileContent));

        var existingAttachment = UserAttachment.Create(
            conversationId,
            userId,
            "hop_dong_goc.pdf",
            "attachments/path/hop_dong_goc.pdf",
            fileContent.Length,
            "application/pdf",
            hash);

        _attachmentService.GetAttachmentsByConversationIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(new List<UserAttachment> { existingAttachment });

        using var stream = new MemoryStream(fileContent);
        var command = new UploadAttachmentCommand(conversationId, "hop_dong_doi_ten.pdf", "application/pdf", fileContent.Length, stream);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.WithMessage("*already been uploaded to this conversation*");
        await _storageService.DidNotReceive().UploadAsync(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSameNameDifferentContent_ShouldAutoDisambiguateFileName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, "Hội thoại của tôi");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var existingContent = new byte[] { 1, 2, 3 };
        var existingHash = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(existingContent));

        var existingAttachment = UserAttachment.Create(
            conversationId,
            userId,
            "image.png",
            "attachments/path/image.png",
            existingContent.Length,
            "image/png",
            existingHash);

        _attachmentService.GetAttachmentsByConversationIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(new List<UserAttachment> { existingAttachment });

        var newContent = new byte[] { 9, 9, 9, 9 };
        var newHash = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(newContent));

        var createdAttachment = UserAttachment.Create(
            conversationId,
            userId,
            "image (2).png",
            $"attachments/{userId}/123/image (2).png",
            newContent.Length,
            "image/png",
            newHash);

        _attachmentService.CreateAttachmentAsync(
            conversationId,
            userId,
            "image (2).png",
            Arg.Any<string>(),
            newContent.Length,
            "image/png",
            newHash,
            Arg.Any<CancellationToken>())
            .Returns(createdAttachment);

        using var stream = new MemoryStream(newContent);
        var command = new UploadAttachmentCommand(conversationId, "image.png", "image/png", newContent.Length, stream);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be("image (2).png");
        await _storageService.Received(1).UploadAsync(
            Arg.Is<string>(s => s.StartsWith($"attachments/{userId}/") && s.EndsWith("/image-2.png")),
            Arg.Any<Stream>(),
            "image/png",
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Ảnh1.png", "image/png", "/Anh1.png")]
    [InlineData("Đề cương hợp đồng (nháp).pdf", "application/pdf", "/De-cuong-hop-dong-nhap.pdf")]
    public async Task Handle_WhenFileNameHasVietnameseOrSpecialCharacters_ShouldKeepDisplayNameAndUseAsciiStoragePath(
        string fileName,
        string contentType,
        string expectedStorageSuffix)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversation = Conversation.Create(userId, "Hội thoại của tôi");

        _currentUserService.UserId.Returns(userId);
        _currentUserService.Role.Returns("User");
        _conversationService.GetConversationByIdAsync(conversationId, Arg.Any<CancellationToken>())
            .Returns(conversation);

        var fileContent = new byte[] { 10, 20, 30 };
        var createdAttachment = UserAttachment.Create(
            conversationId,
            userId,
            fileName,
            $"attachments/{userId}/12345{expectedStorageSuffix}",
            fileContent.Length,
            contentType);

        _attachmentService.CreateAttachmentAsync(
            conversationId,
            userId,
            fileName,
            Arg.Is<string>(s => s.StartsWith($"attachments/{userId}/") && s.EndsWith(expectedStorageSuffix)),
            fileContent.Length,
            contentType,
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(createdAttachment);

        using var stream = new MemoryStream(fileContent);
        var command = new UploadAttachmentCommand(conversationId, fileName, contentType, fileContent.Length, stream);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be(fileName);
        await _storageService.Received(1).UploadAsync(
            Arg.Is<string>(s => s.StartsWith($"attachments/{userId}/")
                && s.EndsWith(expectedStorageSuffix)
                && s.All(c => c < 128)),
            stream,
            contentType,
            Arg.Any<CancellationToken>());
    }
}

