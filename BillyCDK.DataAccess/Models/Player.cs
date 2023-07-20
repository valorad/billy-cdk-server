using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BillyCDK.DataAccess.Models;

public record Player(
    [property: BsonId]
    [property: BsonRepresentation(BsonType.ObjectId)]
    string ID,

    [property: BsonElement("dbname")]
    string DBName,

    [property:BsonElement("updatedDate")]
    DateTime UpdatedDate,

    [property:BsonElement("deletedDate")]
    DateTime? DeletedDate,

    [property: BsonElement("name")]
    string Name,

    [property: BsonElement("bio")]
    string Bio,

    [property: BsonElement("isPremium")]
    bool? IsPremium,

    [property: BsonElement("games")]
    List<string> Games
);

