using IoTmate.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoTmate.Model.Configurations
{
    public class SensorReadingConfiguration : IEntityTypeConfiguration<SensorReading>
    {
        public void Configure(EntityTypeBuilder<SensorReading> builder)
        {
            // Primary key
            builder.HasKey(x => x.Id);

            // Relation 1:N Device -> SensorReading
            builder.HasOne(x => x.Device)
                .WithMany(x => x.SensorReadings)
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            // JsonB column for data from sensor
            builder.Property(x => x.Data)
                .HasColumnType("jsonb");

            // Index for fast search
            builder.HasIndex(x => new { x.DeviceId, x.Timestamp });
    
        }
    }
}
