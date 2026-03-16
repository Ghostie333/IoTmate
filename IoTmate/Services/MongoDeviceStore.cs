using IoTmate.Contracts.Devices;
using IoTmate.Data.Mongo;
using MongoDB.Driver;

namespace IoTmate.Services;

public sealed class MongoDeviceStore : IDeviceStore
{
    private readonly IMongoCollection<IoTmate.Data.Devices.DeviceDocument> _collection;

    public MongoDeviceStore(IMongoDb mongo)
    {
        _collection = mongo.Database.GetCollection<IoTmate.Data.Devices.DeviceDocument>("devices");
    }

    public IReadOnlyList<DeviceDto> GetAll(string ownerUserId)
    {
        return _collection
            .Find(d => d.OwnerUserId == ownerUserId)
            .SortBy(d => d.DeviceId)
            .ToList()
            .Select(ToDto)
            .ToArray();
    }

    public DeviceDto? GetById(string ownerUserId, string id)
    {
        var doc = _collection.Find(d => d.OwnerUserId == ownerUserId && d.DeviceId == id).FirstOrDefault();
        return doc is null ? null : ToDto(doc);
    }

    public DeviceDto Upsert(string ownerUserId, DeviceDto device)
    {
        var doc = new IoTmate.Data.Devices.DeviceDocument
        {
            DeviceId = device.Id,
            OwnerUserId = ownerUserId,
            Name = device.Name,
            Online = device.Online,
            LastSeenUtc = device.LastSeenUtc
        };

        _collection.ReplaceOne(
            d => d.OwnerUserId == ownerUserId && d.DeviceId == device.Id,
            doc,
            new ReplaceOptions { IsUpsert = true });

        return device;
    }

    private static DeviceDto ToDto(IoTmate.Data.Devices.DeviceDocument d)
        => new(d.DeviceId, d.Name, d.Online, d.LastSeenUtc);
}
