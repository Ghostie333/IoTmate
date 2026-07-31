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
            // Id
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            // DeviceId
            builder.Property(x => x.DeviceId)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(x => x.DeviceId)
                .IsUnique();

            // Name
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Secret token for MQTT auth
            builder.Property(x => x.SecretToken)
                .IsRequired()
                .HasMaxLength(256);

            // Status
            builder.Property(x => x.IsOnline).
                HasDefaultValue(false);
        }
    }
}
