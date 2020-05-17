using System.Collections.Generic;
using App.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Database
{
  public class Player : IPlayer
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ID { get; set; }

    [BsonElement("dbname")]
    public string DBName { get; set; }

    [BsonElement("isPremium")]
    public bool IsPremium { get; set; }

    [BsonElement("cdKeys")]
    public List<string> CDKeys { get; set; }

    [BsonElement("games")]
    public List<string> Games { get; set; }
  }
}