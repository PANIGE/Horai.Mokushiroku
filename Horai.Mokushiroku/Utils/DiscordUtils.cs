using Discord;
using Horai.Mokushiroku.Models;
using System.Collections.Generic;

namespace Horai.Mokushiroku.Utils
{
    public static class DiscordUtils
    {
        private static Random Random { get; } = new Random();
        public static Embed CreateCharacterEmbed(EncounterProfile profile)
        {
            var builder = new EmbedBuilder()
            .WithTitle(profile.Name)
            .WithDescription(profile.Description)
            .WithColor(new Color(7377904))
            .WithFooter(footer => footer.Text = $"Nombre : {Random.Next(profile.Group.Min, profile.Group.Max+1)}");

            Func<IEnumerable<IHasDropRate>, string> getField = (t) => Codify(t.PickRandom()?.Name ?? "N/A");


            builder.AddField("Catégorie", Codify(profile.Category), true)
            .AddField("Genre", getField(profile.Genre), true)
            .AddField("Puissance", getField(profile.PowerLevel), true)
            .AddField("Statut", Codify(profile.Status.PickRandom()), true)
            .AddField("Type 1", getField(profile.Type1), true)
            .AddField("Type 2", getField(profile.Type2), true)
            .AddField("Corruption", Codify(profile.Corruption), true);
            var embed = builder.Build();
            return embed;
        }

        private static string Codify(string text)
        {
            return $"```{text}```";
        }

        public static string PickRandom(this IEnumerable<string> items)
        {
            string[] enumerable = items.ToArray();

            if (enumerable.Length == 0)
                return "N/A";

            int r = Random.Next(enumerable.Count());
            return enumerable[r];
        }

        public static T? PickRandom<T>(this IEnumerable<T> items) where T : IHasDropRate
        {
            IEnumerable<T> hasDropRates = items as T[] ?? items.ToArray();
            int totalWeight = 0;

            foreach (T item in hasDropRates)
                totalWeight += item.DropRate;

            int roll = Random.Next(0, totalWeight);

            int cumulativeWeight = 0;
            T? prev = default;
            foreach (T item in hasDropRates)
            {
                prev = item;
                cumulativeWeight += item.DropRate;
                if (roll < cumulativeWeight)
                {
                    return item;
                }
            }
            return prev;
        }
    }
}
