namespace southafricantaxtool.DAL.Configuration;

public class MongoDbConfiguration
{
    public string ConnectionString { get; init; } = null!;

    public string DatabaseName { get; init; } = null!;
}