using System;
using System.Threading;
using System.Threading.Tasks;
using DragonLegionBot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace DragonLegionBot
{
    class Program
    {
        private static readonly CancellationTokenSource LavalinkToken = new();
        private static DiscordClient client;
        
        static async Task Main(string[] args)
        {
            client = InitialiseClient();
            var lavalinkConfig = CreateLavalinkConfiguration();
            var lavalink = client.UseLavalink();
            var lavalinkClient = new LavalinkClient.LavalinkClient();
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => StopLavalink();

            Console.WriteLine("Starting Lavalink...");
            Task lavalinkProcess = Task.Run(async () =>
            {
                await lavalinkClient.StartAsync(LavalinkToken.Token);
            }, LavalinkToken.Token);
            
            var logs = lavalinkClient.GetNewLogs();
            while (!logs.Contains("Started Launcher in"))
            {
                logs = lavalinkClient.GetNewLogs();
                Console.WriteLine(logs);
                await Task.Delay(500);
            }
            Console.WriteLine(logs);
            
            await client.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig);
            await Task.Delay(-1);
        }

        private static DiscordClient InitialiseClient()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build();

            var botConfig = config.GetSection("Bot").Get<BotConfig>();

            var client = new DiscordClient(new DiscordConfiguration()
            {
                Token = botConfig.Token,
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Debug
            });

            var commands = client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = botConfig.Prefixes
            });
            
            commands.RegisterCommands<MusicModule>();

            return client;
        }

        private static LavalinkConfiguration CreateLavalinkConfiguration()
        {
            var endpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1",
                Port = 2333
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "youshallnotpass",
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            return lavalinkConfig;
        }

        private static void StopLavalink()
        {
            Console.WriteLine("Stopping Bot...");
            client.DisconnectAsync();
            client.Dispose();
            Console.WriteLine("Stopping Lavalink...");
            LavalinkToken.Cancel();
            Thread.Sleep(1500);
        }
    }
}
