namespace IoTmate.Data.Mongo;

public sealed class MongoOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Database { get; set; } = "iotmate";
}
