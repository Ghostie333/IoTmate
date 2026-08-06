namespace IoTmate.Dto.Room
{
    public record RoomDto
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required string TimeZoneId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
