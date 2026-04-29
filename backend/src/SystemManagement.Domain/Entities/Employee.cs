using SystemManagement.Domain.Common;

namespace SystemManagement.Domain.Entities;

public sealed class Employee : AuditableEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = default!;
    public Guid PositionId { get; set; }
    public Position Position { get; set; } = default!;
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? ManagerEmployeeId { get; set; }
    public Employee? ManagerEmployee { get; set; }
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();
    public bool IsActive { get; set; } = true;
}
