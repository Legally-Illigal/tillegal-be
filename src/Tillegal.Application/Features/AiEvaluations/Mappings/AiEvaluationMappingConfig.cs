using Mapster;
using Tillegal.Application.Features.AiEvaluations.Models;
using Tillegal.Domain.Entities;

namespace Tillegal.Application.Features.AiEvaluations.Mappings;

public sealed class AiEvaluationMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AiEvaluationLog, AiEvaluationDto>();
    }
}

