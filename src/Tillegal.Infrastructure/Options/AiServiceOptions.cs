namespace Tillegal.Infrastructure.Options;

public sealed class AiServiceOptions
{
    public const string SectionName = "AiService";

    public string BaseUrl { get; set; } = "http://127.0.0.1:8000";

    public string ConsultationEndpoint { get; set; } = "/consultation";

    public string DefaultWorkflowMode { get; set; } = "single_agent";

    public string LawyerWorkflowMode { get; set; } = "multi_agent";
}
