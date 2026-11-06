using Mapster;
using Tillegal.Api.Contracts.AiEvaluations;
using Tillegal.Application.Features.AiEvaluations.Models;

namespace Tillegal.Api.Mappings;

public sealed class AiEvaluationApiMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AiEvaluationDto, AiEvaluationApiResponse>();
        config.NewConfig<AiEvaluationSummaryDto, AiEvaluationSummaryApiResponse>();
        config.NewConfig<PagedAiEvaluationsDto, PagedAiEvaluationsApiResponse>();
    }
}

