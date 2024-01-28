using BotHideMessage.core.Validation;

namespace BotHideMessage.core.UpdateModels;

public class InlineResponse : IResponse
{
    public ResponseType Type => ResponseType.InlineQuery;

    public ValidationTypes ValidationType { get; set; }
    public string QueryId { get; set; }
}