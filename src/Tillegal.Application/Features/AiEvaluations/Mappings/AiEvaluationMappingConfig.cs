using Mapster;
using Tillegal.Application.Features.AiEvaluations.Models;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Models;

namespace Tillegal.Application.Features.AiEvaluations.Mappings;

public sealed class AiEvaluationMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AiEvaluationLog, AiEvaluationDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.MessageId, src => src.MessageId)
            .Map(dest => dest.EvaluatorUserId, src => src.EvaluatorUserId)
            .Map(dest => dest.EvaluatorRole, src => src.EvaluatorRole)
            .Map(dest => dest.IsHelpful, src => src.IsHelpful)
            .Map(dest => dest.Rating, src => src.Rating)
            .Map(dest => dest.FeedbackComment, src => src.FeedbackComment)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);

        config.NewConfig<AiEvaluationSummary, AiEvaluationSummaryDto>();
    }
}

