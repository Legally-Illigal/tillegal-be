namespace Tillegal.Persistence.DataModels;

public sealed class ConversationDataModel : BaseDataModel
{
    public Guid UserId { get; set; }

    public string Title { get; set; } = null!;

    // Navigation properties
    public UserDataModel User { get; set; } = null!;

    public ICollection<ConversationMessageDataModel> Messages { get; set; } = new List<ConversationMessageDataModel>();

    public ICollection<UserAttachmentDataModel> Attachments { get; set; } = new List<UserAttachmentDataModel>();
}
