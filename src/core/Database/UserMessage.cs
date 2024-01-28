using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BotHideMessage.core.Database;

public class UserMessage
{
    [BsonElement("_id")]
    public ObjectId ObjectId { get; set; }
    [BsonElement("sender_user_id")]
    public long SenderUserId { get; set; }
    public string TextMessage { get; set; }
    [BsonElement("receivers_usernames")]
    public string[] ReceiversUsernames { get; set; }
    [BsonElement("message_id")]
    public string MessageId { get; set; }
}