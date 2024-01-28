using System.Net.Mail;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotHideMessage.core.UpdateModels;

public class CallbackResponse : IResponse
{
    public ResponseType Type => ResponseType.CallbackQuery;
    
    public string ResponseMessage { get; set; }
    public InlineKeyboardMarkup? Buttons { get; set; }
    public CallbackResponseTypes CallbackType { get; set; }

    public enum CallbackResponseTypes
    {
        FromInlineRequest,
        FromBot
    }
}