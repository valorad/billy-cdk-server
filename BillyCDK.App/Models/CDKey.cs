using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BillyCDK.App.Models;

public record CDKey(
    [property: BsonId]
    [property: BsonRepresentation(BsonType.ObjectId)]
    string ID,

    [property:BsonElement("updatedDate")]
    DateTime UpdatedDate,

    [property:BsonElement("deletedDate")]
    DateTime? DeletedDate,

    [property:BsonElement("player")]
    string? Player,

    [property:BsonElement("game")]
    string Game,

    [property:BsonElement("value")]
    string Value,

    [property:BsonElement("isActivated")]
    bool IsActivated,

    [property:BsonElement("price")]
    double? Price,

    [property:BsonElement("platform")]
    string Platform
);
