using FluentAssertions;
using Tillegal.Application.Features.AiEvaluations.Commands.CreateEvaluation;
using Tillegal.Application.Features.AiEvaluations.Commands.DeleteEvaluation;
using Tillegal.Application.Features.AiEvaluations.Commands.UpdateEvaluation;
using Tillegal.Application.Features.AiEvaluations.Queries.GetAiEvaluations;
using Tillegal.Application.Features.AiEvaluations.Queries.GetEvaluationById;
using Tillegal.Application.Features.AiEvaluations.Queries.GetEvaluationByMessageId;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.AiEvaluations;

public sealed class AiEvaluationCommandValidatorTests
{
    private readonly CreateEvaluationCommandValidator _createValidator = new();
    private readonly UpdateEvaluationCommandValidator _updateValidator = new();
    private readonly DeleteEvaluationCommandValidator _deleteValidator = new();
    private readonly GetEvaluationByIdQueryValidator _getByIdValidator = new();
    private readonly GetEvaluationByMessageIdQueryValidator _getByMessageIdValidator = new();
    private readonly GetAiEvaluationsQueryValidator _getPagedValidator = new();

    [Fact]
    public void CreateCommand_WithValidData_ShouldPassValidation()
    {
        var command = new CreateEvaluationCommand(Guid.CreateVersion7(), true, 5, "Hài lòng");
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateCommand_WithEmptyMessageId_ShouldFailValidation()
    {
        var command = new CreateEvaluationCommand(Guid.Empty, true, 5, "Hài lòng");
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateEvaluationCommand.MessageId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void CreateCommand_WithOutOfRangeRating_ShouldFailValidation(int invalidRating)
    {
        var command = new CreateEvaluationCommand(Guid.CreateVersion7(), true, invalidRating, null);
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateEvaluationCommand.Rating));
    }

    [Fact]
    public void CreateCommand_WithNoAttributes_ShouldFailValidation()
    {
        var command = new CreateEvaluationCommand(Guid.CreateVersion7(), null, null, null);
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateCommand_WithValidData_ShouldPassValidation()
    {
        var command = new UpdateEvaluationCommand(Guid.CreateVersion7(), false, 2, "Cần sửa");
        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateCommand_WithEmptyId_ShouldFailValidation()
    {
        var command = new UpdateEvaluationCommand(Guid.Empty, false, 2, "Cần sửa");
        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEvaluationCommand.Id));
    }

    [Fact]
    public void DeleteCommand_WithValidId_ShouldPassValidation()
    {
        var command = new DeleteEvaluationCommand(Guid.CreateVersion7());
        var result = _deleteValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeleteCommand_WithEmptyId_ShouldFailValidation()
    {
        var command = new DeleteEvaluationCommand(Guid.Empty);
        var result = _deleteValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(DeleteEvaluationCommand.Id));
    }

    [Fact]
    public void GetByIdQuery_WithValidId_ShouldPassValidation()
    {
        var query = new GetEvaluationByIdQuery(Guid.CreateVersion7());
        var result = _getByIdValidator.Validate(query);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetByIdQuery_WithEmptyId_ShouldFailValidation()
    {
        var query = new GetEvaluationByIdQuery(Guid.Empty);
        var result = _getByIdValidator.Validate(query);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GetByMessageIdQuery_WithValidMessageId_ShouldPassValidation()
    {
        var query = new GetEvaluationByMessageIdQuery(Guid.CreateVersion7());
        var result = _getByMessageIdValidator.Validate(query);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetByMessageIdQuery_WithEmptyMessageId_ShouldFailValidation()
    {
        var query = new GetEvaluationByMessageIdQuery(Guid.Empty);
        var result = _getByMessageIdValidator.Validate(query);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GetPagedQuery_WithValidData_ShouldPassValidation()
    {
        var query = new GetAiEvaluationsQuery(PageNumber: 1, PageSize: 20, IsHelpful: true, Rating: 5);
        var result = _getPagedValidator.Validate(query);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetPagedQuery_WithInvalidPageNumber_ShouldFailValidation(int invalidPageNumber)
    {
        var query = new GetAiEvaluationsQuery(PageNumber: invalidPageNumber, PageSize: 20);
        var result = _getPagedValidator.Validate(query);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void GetPagedQuery_WithInvalidPageSize_ShouldFailValidation(int invalidPageSize)
    {
        var query = new GetAiEvaluationsQuery(PageNumber: 1, PageSize: invalidPageSize);
        var result = _getPagedValidator.Validate(query);
        result.IsValid.Should().BeFalse();
    }
}

