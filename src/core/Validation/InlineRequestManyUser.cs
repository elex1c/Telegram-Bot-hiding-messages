namespace BotHideMessage.core.Validation;

public class InlineRequestManyUser
{
    public List<string> Receivers { get; set; }
    public string Message { get; set; }
}