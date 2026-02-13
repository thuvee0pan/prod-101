using ExecutionOS.API.Data;
using ExecutionOS.API.DTOs;
using ExecutionOS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExecutionOS.API.Services;

public class WarningService
{
    private readonly AppDbContext _db;

    public WarningService(AppDbContext db) => _db = db;

    public async Task<List<WarningResponse>> GetActiveWarnings(Guid userId)
    {
        var warnings = await _db.InactivityWarnings
            .Where(w => w.UserId == userId && !w.Acknowledged)
            .OrderByDescending(w => w.TriggeredAt)
            .ToListAsync();

        return warnings.Select(w => new WarningResponse(
            w.Id, w.WarningType, w.Message, w.TriggeredAt, w.Acknowledged
        )).ToList();
    }

    public async Task AcknowledgeWarning(Guid userId, Guid warningId)
    {
        var warning = await _db.InactivityWarnings
            .FirstOrDefaultAsync(w => w.Id == warningId && w.UserId == userId)
            ?? throw new InvalidOperationException("Warning not found.");

        warning.Acknowledged = true;
        warning.AcknowledgedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task CreateWarning(Guid userId, string warningType, string message)
    {
        var warning = new InactivityWarning
        {
            UserId = userId,
            WarningType = warningType,
            Message = message
        };

        _db.InactivityWarnings.Add(warning);
        await _db.SaveChangesAsync();
    }
}
