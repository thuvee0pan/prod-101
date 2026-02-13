using System.Security.Claims;
using ExecutionOS.API.DTOs;
using ExecutionOS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExecutionOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly TodoService _todoService;

    public TodosController(TodoService todoService) => _todoService = todoService;

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<TodoResponse>> Create([FromBody] CreateTodoRequest request)
    {
        var result = await _todoService.Create(GetUserId(), request);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpGet]
    public async Task<ActionResult<List<TodoResponse>>> GetAll(
        [FromQuery] string? category = null,
        [FromQuery] string? status = null)
    {
        return Ok(await _todoService.GetAll(GetUserId(), category, status));
    }

    [HttpGet("date/{date}")]
    public async Task<ActionResult<List<TodoResponse>>> GetByDate(string date)
    {
        var parsed = DateOnly.Parse(date);
        return Ok(await _todoService.GetByDate(GetUserId(), parsed));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TodoResponse>> Update(Guid id, [FromBody] UpdateTodoRequest request)
    {
        try
        {
            return Ok(await _todoService.Update(GetUserId(), id, request));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await _todoService.Delete(GetUserId(), id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
