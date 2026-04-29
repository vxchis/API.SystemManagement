using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemManagement.Application.Common.Security;
using SystemManagement.Application.DTOs.Employees;
using SystemManagement.Application.Services;

namespace SystemManagement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.ManagerOrAdmin)]
public sealed class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeesController(IEmployeeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<EmployeeDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(cancellationToken));
    }

    [HttpGet("subordinates")]
    public async Task<ActionResult<IReadOnlyCollection<EmployeeDto>>> GetSubordinates(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetSubordinatesAsync(cancellationToken));
    }

    [HttpGet("positions")]
    public async Task<ActionResult<IReadOnlyCollection<PositionDto>>> GetPositions(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetPositionsAsync(cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult<EmployeeDto>> Create(CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _service.CreateAsync(request, cancellationToken));
    }
}
