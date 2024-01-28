using Telegram.Bot.Types.ReplyMarkups;

namespace BotHideMessage.core.UpdateModels;

public class MessageResponse : IResponse
{
    public ResponseType Type => ResponseType.Message;

    public long UserId { get; set; }
    public string Message { get; set; }
    public IReplyMarkup? Buttons { get; set; }
}