using aalto_volley_bot.src.Controllers;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace aalto_volley_bot.src
{
    internal class UpdateRouter
    {
        private readonly HbvController _hbvController = new();
        private readonly NimenhuutoController _nimenhuutoController = new();
        private readonly ReceiverOptions _receiverOptions = new()
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message,
                UpdateType.CallbackQuery
            }
        };

        public UpdateRouter(TelegramBotClient botClient, CancellationToken cancellationToken)
        {
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: _receiverOptions,
                cancellationToken: cancellationToken
            );
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.Write($"{DateTime.Now}: Received {update.Type}");  // Timestamp for received update

            switch (update.Type)
            {
                case UpdateType.Message:
                    if (update.Message is not { } message)
                        break;

                    Console.WriteLine($" with text '{message.Text}' from {message.From}");

                    var command = await ValidateAndParseCommandAsync(message, botClient);
                    if (string.IsNullOrEmpty(command))
                        break;

                    var commandAction = RouteCommand(command);
                    await commandAction(message, botClient, cancellationToken);
                    return;

                case UpdateType.CallbackQuery:
                    if (update.CallbackQuery is not { } query)
                        return;
                    if (query.Data is not { } queryData)
                        return;

                    Console.WriteLine($" with data '{queryData}' from {query.From}");

                    var queryAction = RouteCallBackQuery(queryData);
                    await queryAction(query, botClient, cancellationToken);
                    return;

                default:
                    break;
            }

            Console.WriteLine("-> Update was discarded");
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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

        private static async Task<string> ValidateAndParseCommandAsync(Message message, ITelegramBotClient botClient)
        {
            if (message.Text is not { } messageText)
                return string.Empty;
            if (!messageText.StartsWith('/'))
                return string.Empty;

            var directedCommand = messageText.Contains('@');

            if (message.Chat.Type != ChatType.Private && !directedCommand)
                return string.Empty;

            if (directedCommand)
            {
                var parsedCommand = messageText.Split('@');
                var me = await botClient.GetMeAsync();

                if (parsedCommand.Last() != me.Username)
                    return string.Empty;

                return parsedCommand.First();
            }

            return messageText;
        }

        private Func<Message, ITelegramBotClient, CancellationToken, Task> RouteCommand(string command)
        {
            return command switch
            {
                "/hbv" => _hbvController.SendMainMenuAsync,
                "/nimenhuuto" => _nimenhuutoController.SendMainMenuAsync,
                "/start" => ControllerUtils.SendHelpTextAsync,
                "/help" => ControllerUtils.SendHelpTextAsync,
                "/menu" => ControllerUtils.SendHelpTextAsync,
                "/hello" => ControllerUtils.SendHelpTextAsync,
                "/hi" => ControllerUtils.SendHelpTextAsync,
                _ => ControllerUtils.SendDefaultMessageAsync,
            };
        }

        private Func<CallbackQuery, ITelegramBotClient, CancellationToken, Task> RouteCallBackQuery(string queryData)
        {
            return queryData.Split('?').First() switch
            {
                "Hbv:ActiveEvents" => _hbvController.SendActiveEventsAsync,
                "Hbv:Keskarit" => _hbvController.SendWeeklyGamesMenuAsync,
                "Hbv:Keskarit-Specific" => _hbvController.SendSpecificWeeklyGameMenuAsync,
                "Hbv:Tirsat" => _hbvController.SendWeeklyGamesMenuAsync,
                "Hbv:Tirsat-Specific" => _hbvController.SendSpecificWeeklyGameMenuAsync,
                "Nimenhuuto:Main" => _nimenhuutoController.SendMainMenuAsync,
                "Nimenhuuto:Manager" => _nimenhuutoController.SendManagerMenuAsync,
                "Nimenhuuto:List specific events" => _nimenhuutoController.SendUpcomingEventsMenuAsync,
                _ => ControllerUtils.SendDefaultCallBackQueryAsync,
            };
        }
    }
}
