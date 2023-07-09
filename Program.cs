using Telegram.Bot;
using aalto_volley_bot;

Console.WriteLine("Initializing...");

var botClient = new TelegramBotClient("5874683757:AAG9Vi_Ej-8mUu3GkyAmLB6Uqkqf462D_hk");
using CancellationTokenSource cts = new();
var updateHandler = new UpdateHandler(botClient, cts.Token);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();


