using IoTmate.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTmate.Model.Configurations
{
    public class DeviceRelayStateConfiguration : IEntityTypeConfiguration<DeviceRelayState>
    {
        public void Configure(EntityTypeBuilder<DeviceRelayState> builder)
        {
            // Primary and foreign key at the same time
            builder.HasKey(x => x.DeviceId);

            builder.HasOne(x => x.Device)
                .WithOne(x => x.RelayState)
                .HasForeignKey<DeviceRelayState>(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
