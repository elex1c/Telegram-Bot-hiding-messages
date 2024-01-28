using BotHideMessage.core;
using BotHideMessage.core.Database;
using BotHideMessage.core.UpdateModels;
using BotHideMessage.core.Validation;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

string path = Environment.CurrentDirectory + "\\token.txt";

if (!File.Exists(path))
{
    throw new FileNotFoundException("ERROR: token.txt file does not exist in program folder.");
}

string token = File.ReadAllText(path).Trim();

// Connection to a database
DatabaseConnection.ConnectAndSetDatabase();
// Creating telegram bot
var bot = new TelegramBotClient(token);
Console.WriteLine("Bot has been successfully started.");
// Cancellation token
using var cts = new CancellationTokenSource();

bot.StartReceiving(
    updateHandler: UpdateHandler,
    pollingErrorHandler: PollingErrorHandler,
    cancellationToken: cts.Token
    );

Console.WriteLine("Send any key to stop the bot..");
Console.ReadKey();


async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    var response = await MessageChecker.ProcessUpdate(update);
    
    try
    {
        if (response is not null)
        {
            switch (response.Type)
            {
                case ResponseType.InlineQuery:
                    var inlineResponse = (InlineResponse)response;
                    var inlQueryProfile = await DbCommands.GetUserProfile(update.InlineQuery!.From.Id);

                    if (inlineResponse.ValidationType == ValidationTypes.TextAndUsernames ||
                        (inlQueryProfile?.BanList != null && inlQueryProfile.AccessibleList != null))
                    {
                        await bot.AnswerInlineQueryAsync(
                            inlineResponse.QueryId,
                            new[]
                            {
                                new InlineQueryResultArticle(
                                    "ToUsers",
                                    "Send message to specific users",
                                    new InputTextMessageContent("The message was sent to some users")
                                )
                                {
                                    ReplyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton("Show message")
                                    {
                                        CallbackData = "ShowToUsers"
                                    }),
                                    Description =
                                        "Syntax: \nhello, how are you? @username1\nhello, how are you? @username1;@username\nNote: User should contain @ and username can't contain less than 5 letters."
                                },
                                new InlineQueryResultArticle(
                                    "ExceptUsers",
                                    "Send message except some users",
                                    new InputTextMessageContent("The message was sent to all except some users")
                                )
                                {
                                    ReplyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton("Show message")
                                    {
                                        CallbackData = "ShowExceptUsers"
                                    }),
                                    Description =
                                        "Syntax: \nhello, how are you? @username1\nhello, how are you? @username1;@username\nNote: User should contain @ and username can't contain less than 5 letters.\nAnd you can also add constantly excepting users in our bot."
                                }
                            },
                            isPersonal: true);
                    }
                    else if (inlQueryProfile?.AccessibleList != null)
                    {
                        await bot.AnswerInlineQueryAsync(
                            inlineResponse.QueryId,
                            new[]
                            {
                                new InlineQueryResultArticle(
                                    "ToUsers",
                                    "Send message to specific users",
                                    new InputTextMessageContent("The message was sent to some users")
                                )
                                {
                                    ReplyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton("Show message")
                                    {
                                        CallbackData = "ShowToUsers"
                                    }),
                                    Description =
                                        "Syntax: \nhello, how are you? @username1\nhello, how are you? @username1;@username\nNote: User should contain @ and username can't contain less than 5 letters."
                                }
                            },
                            isPersonal: true);
                    }
                    else if (inlQueryProfile?.BanList != null)
                    {
                        await bot.AnswerInlineQueryAsync(
                            inlineResponse.QueryId,
                            new[]
                            {
                                new InlineQueryResultArticle(
                                    "ExceptUsers",
                                    "Send message except some users",
                                    new InputTextMessageContent("The message was sent to all except some users")
                                )
                                {
                                    ReplyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton("Show message")
                                    {
                                        CallbackData = "ShowExceptUsers"
                                    }),
                                    Description =
                                        "Syntax: \nhello, how are you? @username1\nhello, how are you? @username1;@username\nNote: User should contain @ and username can't contain less than 5 letters."
                                }
                            },
                            isPersonal: true);
                    }

                    break;
                case ResponseType.ChosenInlineQuery:
                    var choosenInlineResponse = (ChoosenInlineResponse)response;
                    var botUserProfile = await DbCommands.GetUserProfile(update.ChosenInlineResult!.From.Id);

                    if (choosenInlineResponse.Message.TextMessage.Length <= 200)
                    {
                        if (choosenInlineResponse.ResultType == InlineQueryResultTypes.ToUsers &&
                            botUserProfile?.AccessibleList != null)
                        {
                            choosenInlineResponse.Message.ReceiversUsernames = choosenInlineResponse.Message
                                .ReceiversUsernames
                                .Concat(botUserProfile.AccessibleList)
                                .Where(u => !string.IsNullOrEmpty(u))
                                .ToArray();
                        }

                        if (choosenInlineResponse.ResultType == InlineQueryResultTypes.ExceptUsers &&
                            botUserProfile?.BanList != null)
                        {
                            choosenInlineResponse.Message.ReceiversUsernames = choosenInlineResponse.Message
                                .ReceiversUsernames
                                .Concat(botUserProfile.BanList)
                                .Where(u => !string.IsNullOrEmpty(u))
                                .ToArray();
                        }

                        await DbCommands.AddMessageAsync(choosenInlineResponse.Message);
                    }
                    break;
                case ResponseType.CallbackQuery:
                    var callbackResponse = (CallbackResponse)response;

                    switch (callbackResponse.CallbackType)
                    {
                        case CallbackResponse.CallbackResponseTypes.FromInlineRequest:
                            await bot.AnswerCallbackQueryAsync(
                                update.CallbackQuery!.Id,
                                callbackResponse.ResponseMessage,
                                showAlert: true);
                            break;
                        case CallbackResponse.CallbackResponseTypes.FromBot:
                            if (string.IsNullOrEmpty(callbackResponse.ResponseMessage))
                            {
                                await bot.EditMessageTextAsync(
                                    new ChatId(update.CallbackQuery!.From.Id),
                                    update.CallbackQuery.Message!.MessageId,
                                    callbackResponse.ResponseMessage,
                                    replyMarkup: callbackResponse.Buttons);
                            }
                            else
                            {
                                await bot.SendTextMessageAsync(
                                    new ChatId(update.CallbackQuery!.From.Id),
                                    callbackResponse.ResponseMessage,
                                    replyMarkup: callbackResponse.Buttons);
                            }

                            break;
                    }

                    break;
                case ResponseType.Message:
                    var messageResponse = (MessageResponse)response;

                    await bot.SendTextMessageAsync(
                        new ChatId(messageResponse.UserId),
                        messageResponse.Message,
                        replyMarkup: messageResponse.Buttons
                    );
                    break;
            }
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

Task PollingErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };
    
    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

Console.ReadLine();