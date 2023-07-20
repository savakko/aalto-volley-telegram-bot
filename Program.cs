﻿using Telegram.Bot;
using aalto_volley_bot;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Initializing...");

// Configuration --->
var appConfig = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

if (appConfig["TelegramBotToken"] is not { } token)
{
    Console.WriteLine("Telegram Bot Token not found, quitting the application.");
    return;
}

Console.WriteLine("Configuration successful");
// <--- Configuration


var botClient = new TelegramBotClient(token);
using CancellationTokenSource cts = new();
var updateHandler = new UpdateHandler(botClient, cts.Token);

Console.WriteLine($"Start listening for @{(await botClient.GetMeAsync()).Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();


