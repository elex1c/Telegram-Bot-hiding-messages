using BotHideMessage.core.Buttons;
using BotHideMessage.core.Database;
using BotHideMessage.core.UpdateModels;
using BotHideMessage.core.Validation;
using MongoDB.Bson;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotHideMessage.core;

public static class MessageChecker
{
    public static async Task<IResponse?> ProcessUpdate(Update update)
    {
        switch (update.Type)
        {
            case UpdateType.InlineQuery:
                var validationResponse = QueryValidation.IsAnonMessageRequestValid(update.InlineQuery!.Query);
                
                if (validationResponse.Success)
                {
                    return new InlineResponse
                    {
                        ValidationType = validationResponse.Type, 
                        QueryId = update.InlineQuery.Id
                    };
                }
                
                break;
            case UpdateType.ChosenInlineResult:
                if (QueryValidation.IsAnonMessageRequestValid(update.ChosenInlineResult!.Query).Success)
                {
                    switch (update.ChosenInlineResult!.ResultId)
                    {
                        case "ToUsers":
                            return new ChoosenInlineResponse
                            {
                                Message = new UserMessage
                                {
                                    ObjectId = ObjectId.GenerateNewId(),
                                    ReceiversUsernames = QueryValidation.GetReceiversUsernames(update.ChosenInlineResult!.Query),
                                    TextMessage = QueryValidation.GetMessage(update.ChosenInlineResult.Query),
                                    SenderUserId = update.ChosenInlineResult.From.Id,
                                    MessageId = update.ChosenInlineResult.InlineMessageId!
                                },
                                ResultType = InlineQueryResultTypes.ToUsers
                            };
                        case "ExceptUsers":
                            return new ChoosenInlineResponse
                            {
                                Message = new UserMessage
                                {
                                    ObjectId = ObjectId.GenerateNewId(),
                                    ReceiversUsernames = QueryValidation.GetReceiversUsernames(update.ChosenInlineResult!.Query),
                                    TextMessage = QueryValidation.GetMessage(update.ChosenInlineResult.Query),
                                    SenderUserId = update.ChosenInlineResult.From.Id,
                                    MessageId = update.ChosenInlineResult.InlineMessageId!
                                },
                                ResultType = InlineQueryResultTypes.ExceptUsers
                            };
                    }
                }

                break;
            case UpdateType.CallbackQuery:
                switch (update.CallbackQuery!.Data)
                {
                    case "ShowToUsers":
                        var messageToMany = await DbCommands.GetMessageAsync(update.CallbackQuery!.InlineMessageId!);

                        if (messageToMany != null)
                        {
                            if (messageToMany.ReceiversUsernames.Contains("@" + update.CallbackQuery.From.Username?.ToLower())
                                || update.CallbackQuery.From.Id == messageToMany.SenderUserId)
                            {
                                return new CallbackResponse
                                {
                                    ResponseMessage = messageToMany.TextMessage,
                                    CallbackType = CallbackResponse.CallbackResponseTypes.FromInlineRequest
                                };
                            }
                            else
                            {
                                return new CallbackResponse
                                {
                                    ResponseMessage = "This message was not for you! You cannot open it.",
                                    CallbackType = CallbackResponse.CallbackResponseTypes.FromInlineRequest
                                };
                            }
                        }

                        break;
                    case "ShowExceptUsers":
                        var exceptMany = await DbCommands.GetMessageAsync(update.CallbackQuery!.InlineMessageId!);

                        if (exceptMany != null)
                        {
                            if (!exceptMany.ReceiversUsernames.Contains("@" + update.CallbackQuery.From.Username?.ToLower()))
                            {
                                return new CallbackResponse
                                {
                                    ResponseMessage = exceptMany.TextMessage,
                                    CallbackType = CallbackResponse.CallbackResponseTypes.FromInlineRequest
                                };
                            }
                            else
                            {
                                return new CallbackResponse
                                {
                                    ResponseMessage = "This message was not for you! You cannot open it.",
                                    CallbackType = CallbackResponse.CallbackResponseTypes.FromInlineRequest
                                };
                            }
                        }

                        break;
                    case "EditBanUserList":
                        var userProfile = await DbCommands.GetUserProfile(update.CallbackQuery.From.Id);
                        
                        if (userProfile != null)
                        {
                            await DbCommands.SetStateAsync(update.CallbackQuery.From.Id, "EditBanUserList");
                            
                            if (userProfile.BanList != null)
                            {
                                return new CallbackResponse
                                {
                                    ResponseMessage = "Here your ban list:\n" + string.Join(", ", userProfile.BanList),
                                    CallbackType = CallbackResponse.CallbackResponseTypes.FromBot,
                                    Buttons = InlineButtons.GetEditBanListButtons()
                                };
                            }
                            else
                            {
                                return new CallbackResponse
                                {
                                    ResponseMessage = "You don't have users in your ban list yet..",
                                    CallbackType = CallbackResponse.CallbackResponseTypes.FromBot,
                                    Buttons = InlineButtons.GetEditBanListButtons()
                                };
                            }    
                        }
                        
                        break;
                    case "AddUsersInBanList":
                        await DbCommands.SetStateAsync(update.CallbackQuery!.From.Id, "AddUsersInBanList");
                        
                        return new CallbackResponse
                        {
                            ResponseMessage = "Send users you want to add.. \nExample with one username: @username\nExample: @username1;@username2;@username",
                            CallbackType = CallbackResponse.CallbackResponseTypes.FromBot
                        };
                    case "DeleteUsersFromBanList":
                        await DbCommands.SetStateAsync(update.CallbackQuery!.From.Id, "DeleteUsersFromBanList");
                        
                        return new CallbackResponse
                        {
                            ResponseMessage = "Send users you want to delete.. \nExample with one username: @username\nExample: @username1;@username2;@username",
                            CallbackType = CallbackResponse.CallbackResponseTypes.FromBot
                        };
                    case "EditAccessibleUserList":
                        var userAccessProfile = await DbCommands.GetUserProfile(update.CallbackQuery.From.Id);

                        if (userAccessProfile != null)
                        {
                            await DbCommands.SetStateAsync(update.CallbackQuery.From.Id, "EditAccessibleUserList");

                            if (userAccessProfile.AccessibleList != null)
                            {
                                return new CallbackResponse
                                {
                                    ResponseMessage = "Here your accessible list:\n" +
                                                      string.Join(", ", userAccessProfile.AccessibleList),
                                    CallbackType = CallbackResponse.CallbackResponseTypes.FromBot,
                                    Buttons = InlineButtons.GetEditAccessibleListButtons()
                                };
                            }
                            else
                            {
                                return new CallbackResponse
                                {
                                    ResponseMessage = "You don't have users in your accessible list yet..",
                                    CallbackType = CallbackResponse.CallbackResponseTypes.FromBot,
                                    Buttons = InlineButtons.GetEditAccessibleListButtons()
                                };
                            }

                        }

                        break;
                    case "AddAccessibleInBanList":
                        await DbCommands.SetStateAsync(update.CallbackQuery!.From.Id, "AddUsersAccessibleList");
                        
                        return new CallbackResponse
                        {
                            ResponseMessage = "Send users you want to add.. \nExample with one username: @username\nExample: @username1;@username2;@username",
                            CallbackType = CallbackResponse.CallbackResponseTypes.FromBot
                        };
                    case "DeleteUsersFromAccessibleList":
                        await DbCommands.SetStateAsync(update.CallbackQuery!.From.Id, "DeleteUsersFromAccessibleList");
                        
                        return new CallbackResponse
                        {
                            ResponseMessage = "Send users you want to delete.. \nExample with one username: @username\nExample: @username1;@username2;@username",
                            CallbackType = CallbackResponse.CallbackResponseTypes.FromBot
                        };
                    case "MainMenu":
                        var mainMenuUserProfile = await DbCommands.GetUserProfile(update.CallbackQuery!.From.Id);
                        
                        await DbCommands.SetStateAsync(mainMenuUserProfile!.UserId, "MainMenu");
                        
                        return new CallbackResponse
                        {
                            ResponseMessage = "Hello! It is bot you can send secret message with. You are in start page.",
                            Buttons = InlineButtons.GetMainMenuButtons(),
                            CallbackType = CallbackResponse.CallbackResponseTypes.FromBot
                        };
                    default:
                        return null;
                }
                break;
            case UpdateType.Message:
                string? textMessaage = update.Message!.Text;
                
                switch (textMessaage)
                {
                    case "/start":
                        var userProfile = await DbCommands.GetUserProfile(update.Message!.From!.Id);

                        if (userProfile == null)
                        {
                            userProfile = new Profile
                            {
                                UserId = update.Message.From.Id,
                                BanList = null,
                                AccessibleList = null,
                                ObjectId = ObjectId.GenerateNewId()
                            };
                            
                            await DbCommands.AddProfileAsync(userProfile);
                        }

                        await DbCommands.SetStateAsync(userProfile.UserId, "MainMenu");
                        
                        return new MessageResponse
                        {
                            Message = "Hello! It is bot you can send secret message with. You are in the start page.\nHere is our bot validation options:\n 1. Request you send to bot can't contain more than 256 symbols and message that the request provides can't contain more than 200 symbols.\n 2. User should contain @ and username can't contain less than 5 letters.\n 3. Request example 1: hello, how are you? @username1\n 4. hello, how are you? @username1;@username",
                            UserId = update.Message.From!.Id,
                            Buttons = InlineButtons.GetMainMenuButtons()
                        };
                    default:
                        var profile = await DbCommands.GetUserProfile(update.Message!.From!.Id);
                        switch (profile?.State)
                        {
                            case "AddUsersInBanList":
                                if (!string.IsNullOrEmpty(update.Message.Text))
                                {
                                    string[] addingUsernames = QueryValidation.GetReceiversUsernames(update.Message.Text);

                                    if (addingUsernames.Length > 0)
                                    {
                                        await DbCommands.AddUsersToBanListAsync(update.Message.From.Id, addingUsernames);
                                        
                                        return new MessageResponse
                                        {
                                            Message = "Usernames were successfully added! To continue, please, write /start",
                                            UserId = update.Message.From!.Id
                                        };
                                    }
                                }
                                
                                return new MessageResponse
                                {
                                    Message = "You sent usernames in incorrect format",
                                    UserId = update.Message.From!.Id
                                };
                            case "DeleteUsersFromBanList":
                                if (!string.IsNullOrEmpty(update.Message.Text))
                                {
                                    string[] addingUsernames = QueryValidation.GetReceiversUsernames(update.Message.Text);

                                    if (addingUsernames.Length > 0)
                                    {
                                        if (profile.BanList != null)
                                        {
                                            await DbCommands.DeleteUsersFromBanListAsync(update.Message.From.Id,
                                                addingUsernames);

                                            return new MessageResponse
                                            {
                                                Message =
                                                    "Usernames were successfully deleted! To continue, please, write /start",
                                                UserId = update.Message.From!.Id
                                            };
                                        }
                                        else
                                        {
                                            return new MessageResponse
                                            {
                                                Message = "You can't delete users from ban list!",
                                                UserId = update.Message.From!.Id
                                            };
                                        }
                                    }
                                }
                                
                                return new MessageResponse
                                {
                                    Message = "You sent usernames in incorrect format",
                                    UserId = update.Message.From!.Id
                                };
                            case "AddUsersAccessibleList":
                                if (!string.IsNullOrEmpty(update.Message.Text))
                                {
                                    string[] addingUsernames = QueryValidation.GetReceiversUsernames(update.Message.Text);

                                    if (addingUsernames.Length > 0)
                                    {
                                        await DbCommands.AddUsersToAccessibleListAsync(update.Message.From.Id, addingUsernames);
                                        
                                        return new MessageResponse
                                        {
                                            Message = "Usernames were successfully added! To continue, please, write /start",
                                            UserId = update.Message.From!.Id
                                        };
                                    }
                                }
                                
                                return new MessageResponse
                                {
                                    Message = "You sent usernames in incorrect format",
                                    UserId = update.Message.From!.Id
                                };
                            case "DeleteUsersFromAccessibleList":
                                if (!string.IsNullOrEmpty(update.Message.Text))
                                {
                                    string[] addingUsernames = QueryValidation.GetReceiversUsernames(update.Message.Text);

                                    if (addingUsernames.Length > 0)
                                    {
                                        if (profile.AccessibleList != null)
                                        {
                                            await DbCommands.DeleteUsersFromAccessibleListAsync(update.Message.From.Id, addingUsernames);
                                        
                                            return new MessageResponse
                                            {
                                                Message = "Usernames were successfully deleted! To continue, please, write /start",
                                                UserId = update.Message.From!.Id
                                            };
                                        }
                                        else
                                        {
                                            return new MessageResponse
                                            {
                                                Message = "You can't delete users from ban list!",
                                                UserId = update.Message.From!.Id
                                            };
                                        }
                                    }
                                }
                                
                                return new MessageResponse
                                {
                                    Message = "You sent usernames in incorrect format",
                                    UserId = update.Message.From!.Id
                                };
                            default:
                                return new MessageResponse
                                {
                                    Message = "You sent incorrect message.",
                                    UserId = update.Message.From!.Id
                                };
                        }
                }
        }
        
        return null;
    }
}