using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TaskBlockConfiguration : IEntityTypeConfiguration<TaskBlock>
{
    public void Configure(EntityTypeBuilder<TaskBlock> builder)
    {
        builder.ToTable("TaskBlocks");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Start)
            .IsRequired();

        builder.Property(b => b.End)
            .IsRequired();

        builder.Property(b => b.Status)
            .IsRequired();

        builder.Property(b => b.Note)
            .HasMaxLength(2000);

        builder.HasIndex(b => new { b.TaskItemId, b.Start })
            .IsUnique();

        builder.HasOne(b => b.TaskItem)
            .WithMany(t => t.TaskBlocks)
            .HasForeignKey(b => b.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}