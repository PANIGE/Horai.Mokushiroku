using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horai.Mokushiroku.Cogs
{
    public class FunCommands : ModuleBase<SocketCommandContext>
    {
        [Command("rpq")]
        public async Task NoRpQ()
        {
            using (Context.Channel.EnterTypingState())
            {
                await Context.Channel.SendFileAsync("norpq.png", "<:NORPQ:1353838072525094973>");
            }
        }
    }
}
