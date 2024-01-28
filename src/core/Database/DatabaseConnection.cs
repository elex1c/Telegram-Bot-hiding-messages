using MongoDB.Driver;

namespace BotHideMessage.core.Database;

public static class DatabaseConnection
{
    private static string _connectionString => GetConnectionString();
    private const string _dataBaseName = "Telegram";
    public static IMongoDatabase Database; 
    
    private static string GetConnectionString()
    {
        string path = Environment.CurrentDirectory + "\\db_token.txt";

        if (!File.Exists(path))
            throw new FileNotFoundException("ERROR: db_token.txt file does not exist in program folder.");
        
        return File.ReadAllText(path).Trim();
    }

    public static void ConnectAndSetDatabase()
    {
        var client = new MongoClient(_connectionString);

        Database = client.GetDatabase(_dataBaseName);
    }
}