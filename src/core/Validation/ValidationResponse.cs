namespace BotHideMessage.core.Validation;

public class ValidationResponse
{
    public ValidationTypes Type { get; set; }
    public bool Success { get; set; }
}

public enum ValidationTypes
{
    OnlyText,
    TextAndUsernames
}