using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BillyCDK.DataAccess.Models;

public record Game(
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

    [property: BsonElement("description")]
    string Description,

    [property: BsonElement("price")]
    decimal Price
);

