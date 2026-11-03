using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Daycare.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Incident persistence.
/// </summary>
public sealed class IncidentRepository : IIncidentRepository
{
    private readonly DaycareDbContext _context;

    public IncidentRepository(DaycareDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Incident?> GetByIdAsync(IncidentId id, CancellationToken cancellationToken = default)
    {
        return await _context.Incidents
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Incident>> GetByChildIdAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Incidents
            .AsNoTracking()
            .Where(i => i.ChildId == childId);

        if (startDate.HasValue)
            query = query.Where(i => i.IncidentTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(i => i.IncidentTime <= endDate.Value);

        return await query.OrderByDescending(i => i.IncidentTime).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Incident>> GetByChildAndDateAsync(
        ChildId childId,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        return await _context.Incidents
            .AsNoTracking()
            .Where(i => i.ChildId == childId && i.IncidentTime >= startOfDay && i.IncidentTime <= endOfDay)
            .OrderByDescending(i => i.IncidentTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Incident>> GetByInjuryTypeAsync(
        InjuryType injuryType,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Incidents
            .AsNoTracking()
            .Where(i => i.InjuryType == injuryType);

        if (startDate.HasValue)
            query = query.Where(i => i.IncidentTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(i => i.IncidentTime <= endDate.Value);

        return await query.OrderByDescending(i => i.IncidentTime).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Incident>> GetNotSignedIncidentsAsync(
        ChildId childId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Incidents
            .AsNoTracking()
            .Where(i => i.ChildId == childId && string.IsNullOrEmpty(i.ParentSignature))
            .OrderByDescending(i => i.IncidentTime)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Incident incident, CancellationToken cancellationToken = default)
    {
        await _context.Incidents.AddAsync(incident, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Incident incident, CancellationToken cancellationToken = default)
    {
        _context.Incidents.Update(incident);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Incident incident, CancellationToken cancellationToken = default)
    {
        _context.Incidents.Remove(incident);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
