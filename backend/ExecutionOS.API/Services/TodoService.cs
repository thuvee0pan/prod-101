using ExecutionOS.API.Data;
using ExecutionOS.API.DTOs;
using ExecutionOS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExecutionOS.API.Services;

public class TodoService
{
    private readonly AppDbContext _db;
    private readonly ILogger<TodoService> _logger;

    public TodoService(AppDbContext db, ILogger<TodoService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<TodoResponse> Create(Guid userId, CreateTodoRequest request)
    {
        var category = Enum.TryParse<TodoCategory>(request.Category, true, out var cat)
            ? cat : TodoCategory.Personal;

        var dueDate = string.IsNullOrEmpty(request.DueDate)
            ? DateOnly.FromDateTime(DateTime.UtcNow)
            : DateOnly.Parse(request.DueDate);

        var maxOrder = await _db.TodoItems
            .Where(t => t.UserId == userId && t.DueDate == dueDate)
            .MaxAsync(t => (int?)t.SortOrder) ?? -1;

        var todo = new TodoItem
        {
            UserId = userId,
            Title = request.Title,
            Description = request.Description,
            Category = category,
            DueDate = dueDate,
            SortOrder = maxOrder + 1
        };

        _db.TodoItems.Add(todo);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Todo created — TodoId: {TodoId}, Date: {DueDate}, User: {UserId}",
            todo.Id.ToString()[..8], dueDate, userId.ToString()[..8]);

        return MapToResponse(todo);
    }

    public async Task<List<TodoResponse>> GetByDate(Guid userId, DateOnly date)
    {
        var todos = await _db.TodoItems
            .Where(t => t.UserId == userId && t.DueDate == date)
            .OrderBy(t => t.Status)
            .ThenBy(t => t.SortOrder)
            .ToListAsync();

        return todos.Select(MapToResponse).ToList();
    }

    public async Task<List<TodoResponse>> GetAll(Guid userId, string? category = null, string? status = null)
    {
        var query = _db.TodoItems.Where(t => t.UserId == userId);

        if (!string.IsNullOrEmpty(category) && Enum.TryParse<TodoCategory>(category, true, out var cat))
            query = query.Where(t => t.Category == cat);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<TodoStatus>(status, true, out var stat))
            query = query.Where(t => t.Status == stat);

        var todos = await query
            .OrderByDescending(t => t.DueDate)
            .ThenBy(t => t.Status)
            .ThenBy(t => t.SortOrder)
            .ToListAsync();

        return todos.Select(MapToResponse).ToList();
    }

    public async Task<TodoResponse> Update(Guid userId, Guid todoId, UpdateTodoRequest request)
    {
        var todo = await _db.TodoItems
            .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId)
            ?? throw new InvalidOperationException("Todo not found.");

        if (request.Title != null)
            todo.Title = request.Title;

        if (request.Description != null)
            todo.Description = request.Description;

        if (request.Category != null && Enum.TryParse<TodoCategory>(request.Category, true, out var cat))
            todo.Category = cat;

        if (request.Status != null && Enum.TryParse<TodoStatus>(request.Status, true, out var stat))
            todo.Status = stat;

        if (request.DueDate != null)
            todo.DueDate = DateOnly.Parse(request.DueDate);

        todo.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Todo updated — TodoId: {TodoId}, User: {UserId}",
            todoId.ToString()[..8], userId.ToString()[..8]);

        return MapToResponse(todo);
    }

    public async Task Delete(Guid userId, Guid todoId)
    {
        var todo = await _db.TodoItems
            .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == userId)
            ?? throw new InvalidOperationException("Todo not found.");

        _db.TodoItems.Remove(todo);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Todo deleted — TodoId: {TodoId}, User: {UserId}",
            todoId.ToString()[..8], userId.ToString()[..8]);
    }

    private static TodoResponse MapToResponse(TodoItem t) =>
        new(t.Id, t.Title, t.Description, t.Category.ToString(), t.Status.ToString(),
            t.DueDate.ToString("yyyy-MM-dd"), t.SortOrder, t.CreatedAt, t.UpdatedAt);
}
