using System.Security.Claims;
using ExecutionOS.API.DTOs;
using ExecutionOS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExecutionOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _projectService;

    public ProjectsController(ProjectService projectService) => _projectService = projectService;

    private Guid GetUserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create([FromBody] CreateProjectRequest request)
    {
        try
        {
            var result = await _projectService.CreateProject(GetUserId(), request);
            return CreatedAtAction(nameof(GetActive), result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<ProjectResponse>>> GetActive()
    {
        return Ok(await _projectService.GetActiveProjects(GetUserId()));
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjectResponse>>> GetAll()
    {
        return Ok(await _projectService.GetAllProjects(GetUserId()));
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ProjectResponse>> UpdateStatus(Guid id, [FromBody] UpdateProjectStatusRequest request)
    {
        try
        {
            return Ok(await _projectService.UpdateStatus(GetUserId(), id, request));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("change-request")]
    public async Task<ActionResult<ProjectChangeResponse>> SubmitChangeRequest([FromBody] ProjectChangeRequestDto request)
    {
        try
        {
            return Ok(await _projectService.SubmitChangeRequest(GetUserId(), request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("change-request/{id}/approve")]
    public async Task<ActionResult<ProjectResponse>> ApproveChangeRequest(Guid id)
    {
        try
        {
            return Ok(await _projectService.ApproveChangeRequest(GetUserId(), id));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
