namespace SystemManagement.Application.Common.Security;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string ManagerOrAdmin = "ManagerOrAdmin";
    public const string StaffOrHigher = "StaffOrHigher";
    public const string DepartmentManager = "DepartmentManager";
}
