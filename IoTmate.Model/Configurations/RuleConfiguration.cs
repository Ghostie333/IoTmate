using IoTmate.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTmate.Model.Configurations
{
    public class RuleConfiguration : IEntityTypeConfiguration<Rule>
    {
        public void Configure(EntityTypeBuilder<Rule> builder)
        {
            // Primary Key
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(x => x.RuleType)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(200);

            // "temperature", "humidity", ...
            builder.Property(x => x.SensorFieldName)
                .HasMaxLength(100);

            // Rule -> Room (delete rules when the room is deleted)
            builder.HasOne(x => x.Room)
                .WithMany(x => x.Rules)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Rule -> TargetDevice. SetNull keeps a broken rule instead of failing the delete.
            builder.HasOne(x => x.TargetDevice)
                .WithMany()
                .HasForeignKey(x => x.TargetDeviceId)
                .OnDelete(DeleteBehavior.SetNull);

            // Rule -> SourceDevice
            builder.HasOne(x => x.SourceDevice)
                .WithMany()
                .HasForeignKey(x => x.SourceDeviceId)
                .OnDelete(DeleteBehavior.SetNull);

            // Cover the common queries: rules of a room, and rules by trigger device.
            builder.HasIndex(x => new { x.RoomId, x.IsEnabled });
            builder.HasIndex(x => x.SourceDeviceId);
            builder.HasIndex(x => x.TargetDeviceId);
        }
    }
}