using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BillyCDK.App.Models;

public record Player(
    [property: BsonId]
    [property: BsonRepresentation(BsonType.ObjectId)]
    string ID,

    [property: BsonElement("dbname")]
    string DBName,

    [property: BsonElement("name")]
    string Name,

    [property: BsonElement("bio")]
    string Bio,

    [property: BsonElement("isPremium")]
    bool? IsPremium,

    [property: BsonElement("games")]
    List<string> Games
);

