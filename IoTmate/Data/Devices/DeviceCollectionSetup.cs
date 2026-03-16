using IoTmate.Data.Mongo;
using MongoDB.Driver;

namespace IoTmate.Data.Devices;

public static class DeviceCollectionSetup
{
    public static void EnsureIndexes(IMongoDb mongo, ILogger? logger = null)
    {
        var coll = mongo.Database.GetCollection<DeviceDocument>("devices");

        // Drop legacy unique index (from older schema) if present.
        // Older versions could have a unique index over DeviceId only which breaks multi-user.
        try
        {
            var indexes = coll.Indexes.List().ToList();
            logger?.LogInformation("Mongo devices indexes: {Indexes}", indexes.Select(i => i.GetValue("name", "").AsString).ToArray());
            foreach (var idx in indexes)
            {
                var name = idx.GetValue("name", "").AsString;
                // Drop any legacy unique index that enforces global uniqueness on DeviceId.
                if (name.Equals("ux_deviceId", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("DeviceId_1", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("DeviceId", StringComparison.OrdinalIgnoreCase) && name.Contains("unique", StringComparison.OrdinalIgnoreCase))
                {
                    coll.Indexes.DropOne(name);
                    logger?.LogWarning("Dropped legacy index {IndexName}", name);
                }
            }
        }
        catch
        {
            // ignore index inspection failures
        }

        var keys = Builders<DeviceDocument>.IndexKeys
            .Ascending(x => x.OwnerUserId)
            .Ascending(x => x.DeviceId);

        var model = new CreateIndexModel<DeviceDocument>(keys, new CreateIndexOptions
        {
            Unique = true,
            Name = "ux_owner_deviceId"
        });

        coll.Indexes.CreateOne(model);
        logger?.LogInformation("Ensured index ux_owner_deviceId");
    }
}
