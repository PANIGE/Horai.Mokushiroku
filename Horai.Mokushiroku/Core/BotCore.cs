using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Horai.Mokushiroku.Core
{
    public class BotCore
    {
        private DiscordSocketClient _client;
        private CommandService Commands { get; } = new CommandService();
        private IServiceProvider Services { get; set; }

        private DateTime rateLimiter { get; set; } = DateTime.MinValue;
        private bool rateLimiterAlready { get; set; } = false;

        public async Task RunBotAsync(string token)
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            });

            var services = new ServiceCollection();

            BuildServices(services);


            Services = services.BuildServiceProvider();

            _client.Log += Log;

            await InitializeAsync();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private void BuildServices(ServiceCollection services)
        {
            services.AddSingleton(_client);
            services.AddSingleton(Commands);
        }

        private async Task InitializeAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await Commands.AddModulesAsync(Assembly.GetExecutingAssembly(), Services);
        }

        private async Task HandleCommandAsync(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage message) return;
            if (message.Author.IsBot) return;

            var context = new SocketCommandContext(_client, message);

            int argPos = 0;

            if (message.HasCharPrefix('$', ref argPos)) 
            {
                var result = await Commands.ExecuteAsync(context, argPos, Services);

                if (!result.IsSuccess)
                {
                    await message.Channel.SendMessageAsync($"Erreur : {result.ErrorReason}");
                }
            }
            else
            {
                switch (message.Content)
                {
                    case string when message.Content.ToLowerInvariant().StartsWith("http") && message.Content.ToLowerInvariant().Contains("japanese") && message.Content.ToLowerInvariant().Contains("goblin"):
                    case string when message.Content.StartsWith("https://cdn.discordapp.com/attachments/1148705059538997421/1354460631549477005/suianese.gif"):
                    case "https://www.youtube.com/watch?v=UIp6_0kct_U":
                    case "https://www.youtube.com/watch?v=Tc8iu0XFUQc":
                        await context.Message.AddReactionAsync(new Emoji("🛑"));

                        if (rateLimiter.Day == DateTime.Now.Day)
                        {
                            if (!rateLimiterAlready)
                            {
                                rateLimiterAlready = true;
                                await context.Channel.SendMessageAsync("https://media.tenor.com/1lAqw2H56OAAAAAC/kaamelott-guenievre.gif");
                                return;
                            }
                        }
                        else
                        {
                            rateLimiter = DateTime.UtcNow;
                            rateLimiterAlready = false;
                            await context.Channel.SendFileAsync("japanese_goblin.png", ":octagonal_sign:");
                        }

                        return;
                }
                
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }
    }
}
