namespace IoTmate.Model.Entities
{
    public class Room
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string TimeZoneId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        public ICollection<RoomMember> Members { get; set; } = [];
        public ICollection<Device> Devices { get; set; } = [];
        public ICollection<Rule> Rules { get; set; } = [];
    }
}