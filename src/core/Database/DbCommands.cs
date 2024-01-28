using MongoDB.Driver;

namespace BotHideMessage.core.Database;

public static class DbCommands
{
    private const string MessageCollectionName = "Message";
    private const string ProfileCollectionName = "Profile";
    private const string MessageIdColumnName = "message_id";
    private const string UserIdColumnName = "user_id";
    private const string BanListName = "ban_list";
    
    // Message table commands
    public static async Task AddMessageAsync(UserMessage message)
    {
        var collection = GetCollection<UserMessage>(MessageCollectionName);
        
        await collection.InsertOneAsync(message);
    }

    public static async Task<UserMessage> GetMessageAsync(string messageId)
    {
        var collection = GetCollection<UserMessage>(MessageCollectionName);

        var filter = Builders<UserMessage>
            .Filter
            .Eq(MessageIdColumnName, messageId);
        
        return await collection.Find(filter).FirstOrDefaultAsync();
    }

    // Profile table commands
    public static async Task AddProfileAsync(Profile profile)
    {
        var collection = GetCollection<Profile>(ProfileCollectionName);

        await collection.InsertOneAsync(profile);
    } 
    
    public static async Task<Profile?> GetUserProfile(long userId)
    {
        var collection = GetCollection<Profile>(ProfileCollectionName);
        
        var filter = Builders<Profile>
            .Filter
            .Eq(UserIdColumnName, userId);
        
        return await collection.Find(filter).FirstOrDefaultAsync();
    }

    public static async Task SetStateAsync(long userId, string state)
    {
        var collection = GetCollection<Profile>(ProfileCollectionName);
        
        var filter = Builders<Profile>
            .Filter
            .Eq(UserIdColumnName, userId);

        var update = Builders<Profile>
            .Update
            .Set(p => p.State, state);

        await collection.UpdateOneAsync(filter, update);
    }
    
    public static async Task AddUsersToBanListAsync(long userId, string[] usernames)
    {
        var collection = GetCollection<Profile>(ProfileCollectionName);
        
        var filter = Builders<Profile>
            .Filter
            .Eq(UserIdColumnName, userId);

        var profileFromDb = (await collection.FindAsync(filter))
            .First();

        UpdateDefinition<Profile> update;
        
        if (profileFromDb.BanList == null)
        {
            update = Builders<Profile>
                .Update
                .Set(p => p.BanList, usernames);
        }
        else
        {
            update = Builders<Profile>
                .Update
                .PushEach(p => p.BanList, usernames);   
        }

        await collection.UpdateOneAsync(filter, update);
    }

    public static async Task DeleteUsersFromBanListAsync(long userId, string[] usernames)
    {
        var collection = GetCollection<Profile>(ProfileCollectionName);
        
        var filter = Builders<Profile>
            .Filter
            .Eq(UserIdColumnName, userId);
        
        var update = Builders<Profile>
            .Update
            .PullAll(p => p.BanList, usernames);

        await collection.UpdateOneAsync(filter, update);
    }
    
    public static async Task AddUsersToAccessibleListAsync(long userId, string[] usernames)
    {
        var collection = GetCollection<Profile>(ProfileCollectionName);
        
        var filter = Builders<Profile>
            .Filter
            .Eq(UserIdColumnName, userId);

        var profileFromDb = (await collection.FindAsync(filter))
            .First();

        UpdateDefinition<Profile> update;
        
        if (profileFromDb.AccessibleList == null)
        {
            update = Builders<Profile>
                .Update
                .Set(p => p.AccessibleList, usernames);
        }
        else
        {
            update = Builders<Profile>
                .Update
                .PushEach(p => p.AccessibleList, usernames);   
        }

        await collection.UpdateOneAsync(filter, update);
    }
    
    public static async Task DeleteUsersFromAccessibleListAsync(long userId, string[] usernames)
    {
        var collection = GetCollection<Profile>(ProfileCollectionName);
        
        var filter = Builders<Profile>
            .Filter
            .Eq(UserIdColumnName, userId);

        var update = Builders<Profile>
            .Update
            .PullAll(p => p.BanList, usernames);

        await collection.UpdateOneAsync(filter, update);
    }
    
    // Other commands
    private static IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return DatabaseConnection
            .Database
            .GetCollection<T>(collectionName);
    }
}