using FluentValidation;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetAdminAiEvaluations;

public sealed class GetAdminAiEvaluationsQueryValidator : AbstractValidator<GetAdminAiEvaluationsQuery>
{
    public GetAdminAiEvaluationsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageNumber must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .When(x => x.Rating.HasValue)
            .WithMessage("Rating must be between 1 and 5.");
    }
}

