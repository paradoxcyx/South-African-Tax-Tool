using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using southafricantaxtool.Interface.Models;

namespace southafricantaxtool.DAL.Models;

public class MdbTaxBrackets
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    
    public List<TaxBracket> TaxBrackets { get; set; }
}