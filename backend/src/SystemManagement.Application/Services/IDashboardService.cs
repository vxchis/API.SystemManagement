using SystemManagement.Application.DTOs.Dashboard;

namespace SystemManagement.Application.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken);
}
