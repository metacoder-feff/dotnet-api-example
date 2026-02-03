using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NodaTime.Extensions;

namespace FEFF.Extentions.EntityFrameworkCore;

/// <summary>
/// Automate setting properties on saving to DB:
/// <list type="bullet">
///     <item>
///         <description>Instant CreatedAt { get; private init; }</description>
///     </item>
///     <item>
///         <description>Instant UpdatedAt { get; private init; }</description>
///     </item>
/// </list>
/// Attention:
/// <list type="bullet">
///     <item>
///         <description>Model properties should be name exactly: 'CreatedAt'/'UpdatedAt' </description>
///     </item>
///     <item>
///         <description>Only 'NodaTime.Instant' is supported as a type for the properies.</description>
///     </item>
///     <item>
///         <description>Any other modifications of this properties would be reverted/overwritten.</description>
///     </item>
///     <item>
///         <description>Uses TimeProvider as a source of time as a constructor dependency.</description>
///     </item>
/// </list>
/// </summary>
public sealed class CreatedAtUpdatedAtInterceptor : SaveChangesInterceptor
{
//TODO: err logging
//TODO: DatetimeOffset support
//TODO: other property bindings & type checks: interface/attribute/... ?
//TODO: Nested (OwnsOne) support/test

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
        return e.Members
                .SingleOrDefault(x => x.Metadata.Name == name && x.Metadata.ClrType == typeof(Instant));
    }
}