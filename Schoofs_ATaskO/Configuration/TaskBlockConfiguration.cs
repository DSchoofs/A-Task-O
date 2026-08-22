using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TaskBlockConfiguration : IEntityTypeConfiguration<TaskBlock>
{
    public void Configure(EntityTypeBuilder<TaskBlock> builder)
    {
        builder.ToTable("TaskBlocks");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Start).IsRequired();

        builder.Property(b => b.End).IsRequired();

        builder.Property(b => b.Status).IsRequired();

        builder.Property(b => b.Note).HasMaxLength(2000);
        
        builder.Property(block => block.Interval).IsRequired();

        builder.HasIndex(b => new { b.TaskItemId, b.Start }).IsUnique();

        builder.HasOne(b => b.TaskItem).WithMany(t => t.TaskBlocks).HasForeignKey(b => b.TaskItemId).OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(block => block.AssignedByUser).WithMany(user => user.AssignedTaskBlocks).HasForeignKey(block => block.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(block => block.CompletedByUser).WithMany(user => user.CompletedTaskBlocks).HasForeignKey(block => block.CompletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}