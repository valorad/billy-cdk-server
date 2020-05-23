using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App.Models
{
  public class CDKey : ICDKey
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ID { get; set; }

    [BsonElement("player")]
    public string Player { get; set; }

    [BsonElement("game")]
    [BsonRequired]
    public string Game { get; set; }

    [BsonElement("value")]
    [BsonRequired]
    public string Value { get; set; }

    [BsonElement("isActivated")]
    [BsonRequired]
    public bool IsActivated { get; set; }

    [BsonElement("price")]
    [BsonRequired]
    public double Price { get; set; }

    [BsonElement("platform")]
    [BsonRequired]
    public string Platform { get; set; }

  }

}