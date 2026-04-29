using SystemManagement.Domain.Common;

namespace SystemManagement.Domain.Entities;

public sealed class Department : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<DepartmentGroupDepartment> GroupDepartments { get; set; } = new List<DepartmentGroupDepartment>();
}
