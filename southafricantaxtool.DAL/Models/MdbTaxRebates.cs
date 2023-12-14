﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using southafricantaxtool.SARSScraper.Models;

namespace southafricantaxtool.DAL.Models;

public class MdbTaxRebates
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    
    public List<TaxRebate> TaxRebates { get; set; }
}