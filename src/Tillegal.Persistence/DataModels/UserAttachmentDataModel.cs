namespace Tillegal.Persistence.DataModels;

public sealed class UserAttachmentDataModel : BaseDataModel
{
    public Guid ConversationId { get; set; }

    public Guid UserId { get; set; }

    public string FileName { get; set; } = null!;

    public string StoragePath { get; set; } = null!;

    public long FileSize { get; set; }

    public string ContentType { get; set; } = null!;

    public string ProcessingStatus { get; set; } = "Pending";

    public int? TokenCount { get; set; }

    // Navigation properties
    public ConversationDataModel Conversation { get; set; } = null!;

    public UserDataModel User { get; set; } = null!;
}
