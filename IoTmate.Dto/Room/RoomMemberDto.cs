using IoTmate.Model.Enums;

namespace IoTmate.Dto.Room
{
    public record RoomMemberDto
    {
        public required string UserId { get; set; }
        public required string Username { get; set; }
        public RoomRole Role { get; set; }
    }
}
