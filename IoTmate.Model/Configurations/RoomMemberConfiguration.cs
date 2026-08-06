using IoTmate.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IoTmate.Model.Configurations
{
    public class RoomMemberConfiguration : IEntityTypeConfiguration<RoomMember>
    {
        public void Configure(EntityTypeBuilder<RoomMember> builder)
        {
            // one membership per user per room
            builder.HasKey(x => new { x.RoomId, x.UserId });

            builder.Property(x => x.Role)
                .IsRequired();

            // Room -> Members
            builder.HasOne(x => x.Room)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Memberships
            builder.HasOne(x => x.User)
                .WithMany(x => x.RoomMemberships)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}