

using Horai.Mokushiroku.Core;

var client = new BotCore();

DotNetEnv.Env.Load();

var token = System.Environment.GetEnvironmentVariable("TOKEN"); ;

await client.RunBotAsync(token);