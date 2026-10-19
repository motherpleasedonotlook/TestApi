using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data;

public sealed class TaskItemDbContext : DbContext
{
    public TaskItemDbContext(DbContextOptions<TaskItemDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(builder =>
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Priority)
                .IsRequired();

            builder.Property(x => x.IsCompleted)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.CompletedAt)
                .IsRequired(false);

            builder.Property(x => x.RowVersion)
                .IsRowVersion()
                .HasColumnType("xid")
                .HasColumnName("xmin");
        });
    }
}