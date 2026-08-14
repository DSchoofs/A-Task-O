using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(user => user.UserId);

            builder.Property(user => user.FirstName).IsRequired().HasMaxLength(100);

            builder.Property(user => user.LastName).IsRequired().HasMaxLength(100);

            builder.Property(user => user.Email).IsRequired().HasMaxLength(255);

            builder.HasIndex(user => user.Email).IsUnique();

            builder.Property(user => user.PasswordHash).IsRequired().HasMaxLength(255);

            builder.Property(user => user.PasswordSalt).IsRequired().HasMaxLength(255);

            builder.Property(user => user.IsAdmin).IsRequired();

            builder.Property(user => user.LastLogin);

            builder.Property(user => user.CompletedTasks).HasDefaultValue(0);
        }
    }
}