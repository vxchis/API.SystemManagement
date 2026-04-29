namespace SystemManagement.Application.DTOs.Dashboard;

public sealed record DashboardSummaryDto(int TotalTasks, int MyTasks, int CompletedTasks, int OverdueTasks, int VisibleDepartments);
