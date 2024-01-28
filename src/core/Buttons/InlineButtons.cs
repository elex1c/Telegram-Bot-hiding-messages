using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotHideMessage.core.Buttons;

public static class InlineButtons
{
    public static InlineKeyboardMarkup GetMainMenuButtons()
    {
        return new InlineKeyboardMarkup(
            new []
            {
                new InlineKeyboardButton("Edit always accessible user list")
                {
                    CallbackData = "EditAccessibleUserList"
                },
                new InlineKeyboardButton("Edit ban user list")
                {
                    CallbackData = "EditBanUserList"
                }
            });
    }
    
    public static InlineKeyboardMarkup GetEditBanListButtons()
    {
        return new InlineKeyboardMarkup(new []
        {
            new []
            {
                new InlineKeyboardButton("Edit (Add users)") { CallbackData = "AddUsersInBanList" }, 
                new InlineKeyboardButton("Edit (Delete users)") { CallbackData = "DeleteUsersFromBanList" }
            },
            new []
            {
                new InlineKeyboardButton("Main menu") { CallbackData = "MainMenu" }
            }
        });
    }
    
    public static InlineKeyboardMarkup GetEditAccessibleListButtons()
    {
        return new InlineKeyboardMarkup(new []
        {
            new []
            {
                new InlineKeyboardButton("Edit (Add users)") { CallbackData = "AddAccessibleInBanList" }, 
                new InlineKeyboardButton("Edit (Delete users)") { CallbackData = "DeleteUsersFromAccessibleList" }
            },
            new []
            {
                new InlineKeyboardButton("Main menu") { CallbackData = "MainMenu" }
            }
        });
    }
}