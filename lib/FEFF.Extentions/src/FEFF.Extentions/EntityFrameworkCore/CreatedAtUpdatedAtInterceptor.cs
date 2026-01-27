using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NodaTime.Extensions;

namespace FEFF.Extentions.EntityFrameworkCore;

/// <summary>
/// 
/// </summary>
public sealed class CreatedAtUpdatedAtInterceptor : SaveChangesInterceptor
{
    private readonly TimeProvider _time;

    public CreatedAtUpdatedAtInterceptor(TimeProvider t)
    {
        _time = t;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        OnSaving(eventData);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        OnSaving(eventData);
        return base.SavingChanges(eventData, result);
    }

    private void OnSaving(DbContextEventData eventData)
    {
        if (eventData.Context == null)
            return;

        var now = _time.GetCurrentInstant();

        foreach (var e in eventData.Context.ChangeTracker.Entries())
            Handle(e, now);
    }

    private static void Handle(EntityEntry entry, Instant now)
    {
        //nameof(.UpdatedAt)
        var created = FindMemberEntry(entry, "CreatedAt");
        var updated = FindMemberEntry(entry, "UpdatedAt");

        if (entry.State == EntityState.Added)
        {
            updated?.CurrentValue = now;
            created?.CurrentValue = now;
        }

        else if (entry.State == EntityState.Modified)
        {
            updated?.CurrentValue = now;
            // Ignore the CreatedTime updates on Modified entities. 
            created?.IsModified = false;
        }
    }

    private static MemberEntry? FindMemberEntry(EntityEntry e, string name)
    {
//TODO: other bindings: interface/attribute ?
        return e.Members
                .SingleOrDefault(x => x.Metadata.Name == name);
    }
}