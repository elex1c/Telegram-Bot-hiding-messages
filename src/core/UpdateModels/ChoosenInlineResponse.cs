using BotHideMessage.core.Database;
using BotHideMessage.core.Validation;

namespace BotHideMessage.core.UpdateModels;

public class ChoosenInlineResponse : IResponse
{
    public ResponseType Type => ResponseType.ChosenInlineQuery;

    public InlineQueryResultTypes ResultType { get; set; }
    public UserMessage Message { get; set; }
}