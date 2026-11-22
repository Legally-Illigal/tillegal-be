using System.Text.Json;

namespace Tillegal.Application.Common.Models;

public sealed record AiConsultationResult(
    string Content,
    JsonDocument? Citations = null);
