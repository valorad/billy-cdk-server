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
    public string DBName { get; set; }

    [BsonElement("isPremium")]
    public bool IsPremium { get; set; }

    [BsonElement("games")]
    public List<string> Games { get; set; }
  }
}