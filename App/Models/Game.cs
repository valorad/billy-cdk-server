using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Models
{
  public class Game : IGame
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ID { get; set; }

    [BsonElement("dbname")]
    [BsonRequired]
    public string DBName { get; set; }
  }

}