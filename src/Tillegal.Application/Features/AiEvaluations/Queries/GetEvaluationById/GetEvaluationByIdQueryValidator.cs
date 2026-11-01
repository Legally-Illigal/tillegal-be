using FluentValidation;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetEvaluationById;

public sealed class GetEvaluationByIdQueryValidator : AbstractValidator<GetEvaluationByIdQuery>
{
    public GetEvaluationByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Evaluation Id is required.");
    }
}

