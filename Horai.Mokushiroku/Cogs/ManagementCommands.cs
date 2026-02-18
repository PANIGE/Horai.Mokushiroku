using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horai.Mokushiroku.Cogs
{
    public class ManagementCommands : ModuleBase<SocketCommandContext>
    {
        public static IReadOnlyList<ulong> ManagementUsers { get; } = new List<ulong> { 248793051277950986, 212880817712660480, 343488613134368774 }.AsReadOnly();

        [Command("stamp")]
        public async Task StampUser(SocketGuildUser user)
        {
            if (await AssertUserPerms())
                return;

            var approvedRole = await Context.Guild.GetRoleAsync(1148705057341198389);
            var unaprovedRole = await Context.Guild.GetRoleAsync(1148705057169223794);

            if (user.Roles.Select(s => s.Id).ToList().Contains(approvedRole.Id))
            {
                await Context.Channel.SendMessageAsync("https://klipy.com/gifs/congratulations-your-character-has-been-approved-congratulations");
            }
            else
            {
                if (user.Roles.Select(s => s.Id).ToList().Contains(unaprovedRole.Id))
                    await user.RemoveRoleAsync(unaprovedRole);

                await user.AddRoleAsync(approvedRole);
                await Context.Channel.SendMessageAsync("https://klipy.com/gifs/congratulations-your-character-has-been-approved-congratulations");
                await Context.Channel.SendMessageAsync("Poste ta fiche dans https://discord.com/channels/1148705057169223790/1148930468822122556 et tu sera officiellement des notres");
            }
        }

        private async Task<bool> AssertUserPerms()
        {
            if (!ManagementUsers.Contains(Context.User.Id))
            {
                await Context.Channel.SendMessageAsync("https://klipy.com/gifs/lotr-lord-of-the-rings-24");
                return true;
            }
            return false;
        }
    }
}
