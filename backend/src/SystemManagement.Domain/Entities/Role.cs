using SystemManagement.Domain.Common;

namespace SystemManagement.Domain.Entities;

public sealed class Role : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
