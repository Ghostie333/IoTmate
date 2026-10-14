namespace IoTmate.Model.Entities
{
    public class DeviceRelayState
    {
        public Guid DeviceId { get; set; }
        public Device Device { get; set; } = null!;

        public bool[] OutletsState { get; set; } = [];
        public string[] OutletsNames { get; set; } = [];
    }
}
