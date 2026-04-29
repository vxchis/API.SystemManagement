using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemManagement.Application.Common.Security;
using SystemManagement.Application.DTOs.DepartmentGroups;
using SystemManagement.Application.Services;

namespace SystemManagement.WebApi.Controllers;

[ApiController]
[Route("api/department-groups")]
[Authorize(Policy = AuthorizationPolicies.ManagerOrAdmin)]
public sealed class DepartmentGroupsController : ControllerBase
{
    private readonly IDepartmentGroupService _service;

    public DepartmentGroupsController(IDepartmentGroupService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DepartmentGroupDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<ActionResult<DepartmentGroupDto>> Create(CreateDepartmentGroupRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _service.CreateAsync(request, cancellationToken));
    }

    [HttpPost("{id:guid}/departments")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> AddDepartment(Guid id, AddDepartmentToGroupRequest request, CancellationToken cancellationToken)
    {
        await _service.AddDepartmentAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/departments/{departmentId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> RemoveDepartment(Guid id, Guid departmentId, CancellationToken cancellationToken)
    {
        await _service.RemoveDepartmentAsync(id, departmentId, cancellationToken);
        return NoContent();
    }
}
