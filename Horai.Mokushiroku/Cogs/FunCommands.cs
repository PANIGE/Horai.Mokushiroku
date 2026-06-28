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
        [Command("rpq", Aliases = ["norpq"])]
        public async Task NoRpQ()
        {
            using (Context.Channel.EnterTypingState())
            {
                await Context.Channel.SendFileAsync("norpq.png");
            }
        }

        [Command("playnice", Aliases = ["pn"])]
        public async Task PlayNice()
        {
            using (Context.Channel.EnterTypingState())
            {
                await Context.Channel.SendFileAsync("Good_boys_and_girls.webp");
            }
        }

        [Command("playnice1", Aliases = ["pn1"])]
        public async Task PlayNice1()
        {
            using (Context.Channel.EnterTypingState())
            {
                await Context.Channel.SendFileAsync("Good_boys_and_girls_awaken.webp");
            }
        }

        [Command("comingthrough", Aliases = ["ct"])]
        public async Task ComingThrough()
        {
            using (Context.Channel.EnterTypingState())
            {
                await Context.Channel.SendFileAsync("Omega_Dynamic_entry.webp");
            }
        }
    }
}
