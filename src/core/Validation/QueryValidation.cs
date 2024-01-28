namespace BotHideMessage.core.Validation;

public static class QueryValidation
{
    public static ValidationResponse IsAnonMessageRequestValid(string query)
    {
        if (!string.IsNullOrEmpty(query) && query.Length <= 256)
        {
            string[] splittedQuery = query.Trim()
                .Split(' ');
            string usernames = splittedQuery.Last();
        
            if (splittedQuery.Length > 1 && usernames[0] == '@' && usernames.Length >= 6)
            {
                string message = string.Join(' ', splittedQuery.Take(splittedQuery.Length - 1));

                if (message.Length <= 200)
                {
                    return new ValidationResponse
                    {
                        Success = true,
                        Type = ValidationTypes.TextAndUsernames
                    };
                }
            }
            else
            {
                return new ValidationResponse
                {
                    Success = true,
                    Type = ValidationTypes.OnlyText
                };
            }
        }
        
        return new ValidationResponse
        {
            Success = false
        };
    }

    public static string[] GetReceiversUsernames(string query)
    {
        return query.Split(' ')
            .Last()
            .Split(';')
            .Where(user => user.Contains('@') && user.Length >= 6)
            .Select(n => n.ToLower())
            .ToArray();
    }
    
    public static string GetMessage(string query)
    {
        string[] splittedQuery = query.Trim()
            .Split(' ');

        var validation = IsAnonMessageRequestValid(query);
        
        if (validation.Type == ValidationTypes.TextAndUsernames)
        {
            return string.Concat(splittedQuery
                    .Take(splittedQuery.Length - 1)
                    .Select(x => x + " ")
                    .ToArray()
                )
                .Trim();
        }
        else
        {
            return query;
        }
    }
}