using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Sprache;
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
        private Random _random = new();
        private CommandService Commands { get; } = new CommandService();
        private IServiceProvider Services { get; set; }

        private DateTime rateLimiter { get; set; } = DateTime.MinValue;
        private bool rateLimiterAlready { get; set; } = false;

        public async Task RunBotAsync(string token)
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.All
            });

            var services = new ServiceCollection();

            BuildServices(services);


            Services = services.BuildServiceProvider();

            _client.Log += Log;
            _client.UserJoined += OnUserJoined;
            _client.UserLeft += OnUserLeft;

            await InitializeAsync();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task OnUserLeft(SocketGuild guild, SocketUser user)
        {
            
        }

        private async Task OnUserJoined(SocketGuildUser user)
        {
            Console.WriteLine("a");
            var channel = (IMessageChannel)(await _client.GetChannelAsync(1148705059538997419));
            await UserJoinMessage(1148705059538997419, user.Id);
        }

        private async Task UserJoinMessage(ulong targetChannel, ulong ping)
        {
            var guild = _client.GetGuild(1148705057169223790);
            var channel = (IMessageChannel)(await _client.GetChannelAsync(targetChannel));
            string userJoinMessage = $@"# ""Bienvenue <@{ping}>! 😊 ""
Vous n'avez pas besoin d'avoir lu notre contexte pour commencer à nous poser des questions ! C'est toujours OK de créer des sujets dans <#1148937995769086002> ou de Pinger Ω pour toute interrogation, même si ces questions ont déjà été posées ailleurs !

En attendant de nous rejoindre, mettez vous dans l'ambiance en lisant [notre Wiki](https://horai.obsidianportal.com/) !";

            string[] names = new[] { "Shiba", "Kappa", "Neko", "Tanukitsune", "Daruma", "Bye world", "DarumaYokaï" };

            var stickers = guild.Stickers.Where(s => names.Any(t => s.Name.Equals(t, StringComparison.CurrentCultureIgnoreCase))).ToList();
            var i = _random.Next(0, stickers.Count() - 1);
            var sticker = stickers[i];


            await channel.SendMessageAsync(stickers: [sticker]);
            await channel.SendMessageAsync(userJoinMessage);
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

                try
                {
                    var result = await Commands.ExecuteAsync(context, argPos, Services);
                    if (!result.IsSuccess)
                    {
                        await message.Channel.SendMessageAsync($"Erreur : {result.ErrorReason}");
                        await message.Channel.SendMessageAsync($"https://tenor.com/view/i-tried-father-pokemon-terminalmontage-pokemon-tcg-gif-10883483966743148762");
                    }
                }
                catch (Exception e)
                {
                  
                    await message.Channel.SendMessageAsync($"Erreur : {e.Message}");
                    await message.Channel.SendMessageAsync($"https://tenor.com/view/i-tried-father-pokemon-terminalmontage-pokemon-tcg-gif-10883483966743148762");
                    
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

                    case "good bot":
                    case "goodbot":
                    case "bon toutou":
                        await message.Channel.SendMessageAsync($"https://tenor.com/view/thank-you-father-harry-partridge-happy-harry-thank-you-father-gif-22785772");
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
