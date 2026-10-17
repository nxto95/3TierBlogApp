using BlogApp.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.DAL.DatabaseContext;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    public override int SaveChanges()
    {
        ApplyAuditInformation();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(x => x.CreatedAt).CurrentValue = now;
                continue;
            }

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Property(x => x.IsDeleted).CurrentValue = true;
                entry.Property(x => x.DeletedAt).CurrentValue = now;
                continue;
            }

            if (entry.State != EntityState.Modified)
                continue;

            entry.Property(x => x.CreatedAt).IsModified = false;
            entry.Property(x => x.UpdatedAt).CurrentValue = now;

            var isDeleted = entry.Property(x => x.IsDeleted);

            if (isDeleted.IsModified)
            {
                if (isDeleted.CurrentValue && !isDeleted.OriginalValue)
                {
                    entry.Property(x => x.DeletedAt).CurrentValue = now;
                }
                else if (!isDeleted.CurrentValue && isDeleted.OriginalValue)
                {
                    entry.Property(x => x.DeletedAt).CurrentValue = null;
                }
            }
        }
    }
}