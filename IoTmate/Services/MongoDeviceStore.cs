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
        var filter = Builders<IoTmate.Data.Devices.DeviceDocument>.Filter.Where(d => d.OwnerUserId == ownerUserId && d.DeviceId == device.Id);

        var update = Builders<IoTmate.Data.Devices.DeviceDocument>.Update
            .Set(d => d.Name, device.Name)
            .Set(d => d.Online, device.Online)
            .Set(d => d.LastSeenUtc, device.LastSeenUtc)
            .SetOnInsert(d => d.OwnerUserId, ownerUserId)
            .SetOnInsert(d => d.DeviceId, device.Id);

        _collection.UpdateOne(filter, update, new UpdateOptions { IsUpsert = true });

        return device;
    }

    public void Delete(string ownerUserId, string deviceId)
    {
        _collection.DeleteOne(d => d.OwnerUserId == ownerUserId && d.DeviceId == deviceId);
    }

    private static DeviceDto ToDto(IoTmate.Data.Devices.DeviceDocument d)
        => new(d.DeviceId, d.Name, d.Online, d.LastSeenUtc);
}
