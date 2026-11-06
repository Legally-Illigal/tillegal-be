using FluentValidation.TestHelper;
using Tillegal.Application.Features.AiEvaluations.Commands.CreateMessageEvaluation;
using Tillegal.Application.Features.AiEvaluations.Commands.DeleteMessageEvaluation;
using Tillegal.Application.Features.AiEvaluations.Commands.UpdateMessageEvaluation;
using Tillegal.Application.Features.AiEvaluations.Queries.GetAdminAiEvaluationById;
using Tillegal.Application.Features.AiEvaluations.Queries.GetAdminAiEvaluations;
using Tillegal.Application.Features.AiEvaluations.Queries.GetMessageEvaluation;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.AiEvaluations;

public sealed class MessageEvaluationCommandValidatorTests
{
    private readonly CreateMessageEvaluationCommandValidator _createValidator = new();
    private readonly UpdateMessageEvaluationCommandValidator _updateValidator = new();
    private readonly DeleteMessageEvaluationCommandValidator _deleteValidator = new();
    private readonly GetMessageEvaluationQueryValidator _getQueryValidator = new();
    private readonly GetAdminAiEvaluationsQueryValidator _adminQueryValidator = new();
    private readonly GetAdminAiEvaluationByIdQueryValidator _adminByIdQueryValidator = new();

    [Fact]
    public void CreateValidator_WhenValid_ShouldNotHaveErrors()
    {
        var command = new CreateMessageEvaluationCommand(Guid.NewGuid(), Guid.NewGuid(), true, 5, "Good");
        var result = _createValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateValidator_WhenEmptyIds_ShouldHaveErrors()
    {
        var command = new CreateMessageEvaluationCommand(Guid.Empty, Guid.Empty, true, 5, "Good");
        var result = _createValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ConversationId);
        result.ShouldHaveValidationErrorFor(x => x.MessageId);
    }

    [Fact]
    public void CreateValidator_WhenNoAttributesProvided_ShouldHaveError()
    {
        var command = new CreateMessageEvaluationCommand(Guid.NewGuid(), Guid.NewGuid(), null, null, null);
        var result = _createValidator.TestValidate(command);
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void UpdateValidator_WhenValid_ShouldNotHaveErrors()
    {
        var command = new UpdateMessageEvaluationCommand(Guid.NewGuid(), Guid.NewGuid(), true, 4, "Updated");
        var result = _updateValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DeleteValidator_WhenValid_ShouldNotHaveErrors()
    {
        var command = new DeleteMessageEvaluationCommand(Guid.NewGuid(), Guid.NewGuid());
        var result = _deleteValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void GetQueryValidator_WhenValid_ShouldNotHaveErrors()
    {
        var query = new GetMessageEvaluationQuery(Guid.NewGuid(), Guid.NewGuid());
        var result = _getQueryValidator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AdminQueryValidator_WhenInvalidPage_ShouldHaveError()
    {
        var query = new GetAdminAiEvaluationsQuery(PageNumber: 0, PageSize: 500, Rating: 7);
        var result = _adminQueryValidator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void AdminByIdQueryValidator_WhenValid_ShouldNotHaveErrors()
    {
        var query = new GetAdminAiEvaluationByIdQuery(Guid.NewGuid());
        var result = _adminByIdQueryValidator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AdminByIdQueryValidator_WhenEmptyId_ShouldHaveError()
    {
        var query = new GetAdminAiEvaluationByIdQuery(Guid.Empty);
        var result = _adminByIdQueryValidator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}

