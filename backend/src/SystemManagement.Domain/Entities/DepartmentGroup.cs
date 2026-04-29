using SystemManagement.Domain.Common;

namespace SystemManagement.Domain.Entities;

public sealed class DepartmentGroup : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<DepartmentGroupDepartment> Departments { get; set; } = new List<DepartmentGroupDepartment>();
}
