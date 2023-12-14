using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using southafricantaxtool.SARSScraper.Models;

namespace southafricantaxtool.DAL.Models;

public class MdbImportantDates
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    
    public List<ImportantDate> ImportantDates { get; set; }
}