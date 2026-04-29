namespace SystemManagement.Domain.Entities;

public sealed class DepartmentGroupDepartment
{
    public Guid DepartmentGroupId { get; set; }
    public DepartmentGroup DepartmentGroup { get; set; } = default!;

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = default!;

    public bool IsActive { get; set; } = true;
}
