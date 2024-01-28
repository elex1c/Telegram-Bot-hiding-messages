using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BotHideMessage.core.Database;

public class Profile
{
    [BsonElement("_id")]
    public ObjectId ObjectId { get; set; }
    [BsonElement("user_id")]
    public long UserId { get; set; }
    [BsonElement("ban_list")]
    public string[]? BanList { get; set; }
    [BsonElement("accessible_list")]
    public string[]? AccessibleList { get; set; }

    public string State { get; set; }
}