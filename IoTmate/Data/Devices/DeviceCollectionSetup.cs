using IoTmate.Data.Mongo;
using MongoDB.Driver;

namespace IoTmate.Data.Devices;

public static class DeviceCollectionSetup
{
    public static void EnsureIndexes(IMongoDb mongo)
    {
        var coll = mongo.Database.GetCollection<DeviceDocument>("devices");

        var keys = Builders<DeviceDocument>.IndexKeys
            .Ascending(x => x.OwnerUserId)
            .Ascending(x => x.DeviceId);

        var model = new CreateIndexModel<DeviceDocument>(keys, new CreateIndexOptions
        {
            Unique = true,
            Name = "ux_owner_deviceId"
        });

        coll.Indexes.CreateOne(model);
    }
}
