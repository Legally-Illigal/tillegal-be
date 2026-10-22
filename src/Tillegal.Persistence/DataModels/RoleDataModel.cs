namespace Tillegal.Persistence.DataModels;

public sealed class RoleDataModel
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public string? Name { get; set; }

    public ICollection<UserDataModel> Users { get; set; } = new List<UserDataModel>();
}
