using FluentValidation;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetAdminAiEvaluationById;

public sealed class GetAdminAiEvaluationByIdQueryValidator : AbstractValidator<GetAdminAiEvaluationByIdQuery>
{
    public GetAdminAiEvaluationByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Evaluation Id is required.");
    }
}

