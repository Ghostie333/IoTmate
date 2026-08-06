using IoTmate.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoTmate.Model.Configurations
{
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            // Primary Key
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            // DeviceId (MAC)
            builder.Property(x => x.DeviceId)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(x => x.DeviceId)
                .IsUnique();

            // Name
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Secrets
            builder.Property(x => x.DeviceSecret) // Unique device string
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.SecretToken) // password for ESP to MQTT
                .IsRequired()
                .HasMaxLength(256);

            // Status and Type
            builder.Property(x => x.DeviceType)
                .IsRequired();

            builder.Property(x => x.IsOnline)
                .HasDefaultValue(false);

            // Latest reading snapshot (jsonb)
            builder.Property(x => x.LastReading)
                .HasColumnType("jsonb");

            // Room assignment (nullable - a device may be unassigned)
            builder.HasOne(x => x.Room)
                .WithMany(x => x.Devices)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
