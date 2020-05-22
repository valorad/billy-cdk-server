using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Models
{
  public class Player : IPlayer
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ID { get; set; }

    [BsonElement("dbname")]
    [BsonRequired]
    public string DBName { get; set; }

    [BsonElement("isPremium")]
    [BsonRequired]
    public bool IsPremium { get; set; }

    [BsonElement("games")]
    [BsonRequired]
    public List<string> Games { get; set; }
  }
}