using FluentValidation;

namespace Tillegal.Application.Features.AiEvaluations.Commands.DeleteEvaluation;

public sealed class DeleteEvaluationCommandValidator : AbstractValidator<DeleteEvaluationCommand>
{
    public DeleteEvaluationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Evaluation Id is required.");
    }
}

