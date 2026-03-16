using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace IoTmate.Data.Mongo;

public interface IMongoDb
{
    IMongoDatabase Database { get; }
}

public sealed class MongoDb : IMongoDb
{
    public IMongoDatabase Database { get; }

    public MongoDb(IOptions<MongoOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        Database = client.GetDatabase(options.Value.Database);
    }
}
