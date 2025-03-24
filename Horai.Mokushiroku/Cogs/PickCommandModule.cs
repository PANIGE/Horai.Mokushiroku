using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Horai.Mokushiroku.Models;
using Horai.Mokushiroku.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Group = Horai.Mokushiroku.Models.Group;

namespace Horai.Mokushiroku.Cogs
{
    public class PickCommandModule : ModuleBase<SocketCommandContext>
    {
        private List<EncounterProfile>? _data;
        Dictionary<string, string> _powerUris = new()
        {
            ["constructs"] = "https://powerlisting.fandom.com/wiki/Special:RandomInCategory/Constructs",
            ["enhancements"] = "https://powerlisting.fandom.com/wiki/Special:RandomInCategory/Enhancements",
            ["magical"] = "https://powerlisting.fandom.com/wiki/Special:RandomInCategory/Magical_Powers",
            ["manipulations"] = "https://powerlisting.fandom.com/wiki/Special:RandomInCategory/Manipulations",
            ["physiology"] = "https://powerlisting.fandom.com/wiki/Special:RandomInCategory/Physiology",
            ["psychic"] = "https://powerlisting.fandom.com/wiki/Special:RandomInCategory/Psychic_Powers",
            ["science"] = "https://powerlisting.fandom.com/wiki/Special:RandomInCategory/Science_Powers"
        };

        public PickCommandModule()
        {
            if (!File.Exists("data.json"))
                File.WriteAllText("data.json", "[]");

            string raw = File.ReadAllText("data.json");
            _data = JsonConvert.DeserializeObject<List<EncounterProfile>>(raw);
        }

        [Command("pick")]
        public async Task Pick([Remainder] string filterString = "")
        {
            if (_data == null || !_data.Any())
                throw new Exception("Could not read data.json or data is empty");

            var filteredData = ApplyFilters(_data, filterString);

            if (!filteredData.Any())
            {
                await ReplyAsync("Aucun profil ne correspond aux filtres donnés.");
                return;
            }

            EncounterProfile profile = filteredData.PickRandom();
            Embed embed = DiscordUtils.CreateCharacterEmbed(profile);
            await ReplyAsync(embed: embed);
            await ReplyAsync($"```diff\n- Vous pouvez ajouter un pouvoir avec $power [power (optionnel)]```");
        }

        [Command("list")]
        public async Task List(int page = 1)
        {
            int limit = 20;
            int pageCount = (int) Math.Ceiling((_data.Count / (double)limit));
            int offset = (page-1) * limit;

            page = Math.Clamp(page, 1, pageCount);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"📜 **{_data.Count} Entrées**");
            builder.AppendLine($"**Page {page}/{pageCount}**");
            builder.AppendLine($"```");

            for (int i = offset; i < Math.Min(_data.Count, offset+limit); i++)
            {
                builder.AppendLine($"({i}) - {_data[i].Name}");
            }

            builder.AppendLine($"```");
            await ReplyAsync(builder.ToString());
        }

        [Command("describe")]
        public async Task Describe(int index)
        {
            if (index > _data.Count - 1)
            {
                await ReplyAsync("L'index indiqué est en dehors de la liste.");
                return;
            }

            EncounterProfile? profile = _data[index];

            Embed embed = GenerateProfileEmbed(profile);
            await ReplyAsync(embed: embed);
        }

        private Embed GenerateProfileEmbed(EncounterProfile profile)
        {
            var embedBuilder = new EmbedBuilder()
            .WithTitle($"📋 Spécificités de {profile.Name}")
            .WithColor(new Color(7377904));

            embedBuilder.AddField("Category", profile.Category)
                        .AddField("Description", profile.Description)
                        .AddField("Corruption", profile.Corruption)
                        .AddField("Drop Rate", profile.DropRate.ToString())
                        .AddField("Group", $"Min : {profile.Group.Min}, Max : {profile.Group.Max}");

            if (profile.Genre.Any())
            {
                var genres = string.Join(", ", profile.Genre.Select(g => g.Name));
                embedBuilder.AddField("Genre", genres);
            }

            if (profile.PowerLevel.Any())
            {
                var powers = string.Join(", ", profile.PowerLevel.Select(p => p.Name));
                embedBuilder.AddField("Puissance", powers);
            }

            if (profile.Type1.Any())
            {
                var type1 = string.Join(", ", profile.Type1.Select(t => t.Name));
                embedBuilder.AddField("Type 1", type1);
            }

            if (profile.Type2.Any())
            {
                var type2 = string.Join(", ", profile.Type2.Select(t => t.Name));
                embedBuilder.AddField("Type 2", type2);
            }

            if (profile.Status.Any())
            {
                var status = string.Join(", ", profile.Status);
                embedBuilder.AddField("Status", status);
            }

            return embedBuilder.Build();
        }


        [Command("fields")]
        public async Task Fields()
        {
            if (_data == null || !_data.Any())
            {
                await ReplyAsync("Les données sont introuvables ou vides.");
                return;
            }

            var embed = GenerateFieldsEmbed(_data);
            await ReplyAsync(embed: embed);
        }

        [Command("dump")]
        public async Task Dump()
        {
            var allowedUsers = new List<ulong> { 248793051277950986, 212880817712660480 };
            
            if (!allowedUsers.Contains(Context.User.Id))
            {
                await ReplyAsync("Tu n'as pas l'autorisation d'utiliser cette commande.");
                return;
            }

            if (_data == null || !_data.Any())
            {
                await ReplyAsync("Les données sont introuvables ou vides.");
                return;
            }

            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    string jsonData = JsonConvert.SerializeObject(_data, Formatting.Indented);

                    string tempFilePath = Path.Combine(Path.GetTempPath(), "data_dump.json");
                    await File.WriteAllTextAsync(tempFilePath, jsonData);

                    await Context.User.SendFileAsync(tempFilePath, "Voici le dump des données !");

                    File.Delete(tempFilePath);

                    await ReplyAsync("Le fichier a été envoyé en message privé !");
                }
                catch (Exception ex)
                {
                    await ReplyAsync($"Une erreur est survenue : {ex.Message}");
                }
            } 
            
        }

        [Command("add")]
        public async Task Add([Remainder] string json)
        {
            var allowedUsers = new List<ulong> { 248793051277950986, 212880817712660480 };

            if (!allowedUsers.Contains(Context.User.Id))
            {
                await ReplyAsync("Tu n'as pas l'autorisation d'utiliser cette commande.");
                return;
            }

            using (Context.Channel.EnterTypingState()) 
            {
                try
                {
                    var newProfile = JsonConvert.DeserializeObject<EncounterProfile>(json);

                    if (newProfile == null)
                    {
                        await ReplyAsync("Le JSON fourni est invalide ou incomplet.");
                        return;
                    }


                    _data.Add(newProfile);
                    await File.WriteAllTextAsync("data.json",JsonConvert.SerializeObject(_data, Formatting.Indented));
         
                }
                catch (Exception ex)
                {
                    await ReplyAsync($"Une erreur est survenue : {ex.Message}");
                }

            }
        }

        [Command("remove")]
        public async Task Remove(int index)
        {
            var allowedUsers = new List<ulong> { 248793051277950986, 212880817712660480 };

            if (!allowedUsers.Contains(Context.User.Id))
            {
                await ReplyAsync("Tu n'as pas l'autorisation d'utiliser cette commande.");
                return;
            }

            if (index < 0 || index >= _data.Count)
            {
                await ReplyAsync("Index invalide.");
                return;
            }

            var profileName = _data[index].Name;
            _data.RemoveAt(index);
            await File.WriteAllTextAsync("data.json", JsonConvert.SerializeObject(_data, Formatting.Indented));
            await ReplyAsync($"L'entrée `{profileName}` a été supprimée avec succès !");
        }

        [Command("set")]
        public async Task Set()
        {
            var allowedUsers = new List<ulong> { 248793051277950986, 212880817712660480 };

            if (!allowedUsers.Contains(Context.User.Id))
            {
                await ReplyAsync("Tu n'as pas l'autorisation d'utiliser cette commande.");
                return;
            }

            if (Context.Message.Attachments.Count == 0)
            {
                await ReplyAsync("Veuillez envoyer un fichier JSON en pièce jointe avec cette commande.");
                return;
            }

            var attachment = Context.Message.Attachments.First();
            if (!attachment.Filename.EndsWith(".json"))
            {
                await ReplyAsync("Le fichier doit être au format `.json`.");
                return;
            }

            try
            {
                using var httpClient = new HttpClient();
                var jsonContent = await httpClient.GetStringAsync(attachment.Url);

                var newData = JsonConvert.DeserializeObject<List<EncounterProfile>>(jsonContent);

                if (newData == null || !newData.Any())
                {
                    await ReplyAsync("Le JSON fourni est invalide ou vide.");
                    return;
                }

                using (Context.Channel.EnterTypingState()) 
                {
                    _data = newData;
                    await File.WriteAllTextAsync("data.json", JsonConvert.SerializeObject(_data, Formatting.Indented));
                    await ReplyAsync("Les données ont été remplacées avec succès !");
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Une erreur est survenue : {ex.Message}");
            }
        }


        [Command("power")]
        public async Task GetPower([Remainder] string data = "")
        {
            

            List<string> pool = new List<string>();

            if (string.IsNullOrWhiteSpace(data))
            {
                pool = _powerUris.Values.ToList();
            }
            else
            {
                string[] splitted = data.Split(',').Select(s => s.Trim()).ToArray();
                foreach (string key in splitted)
                {
                    if (!_powerUris.ContainsKey(key))
                    {
                        await ReplyAsync($"Clé invalide : {key}\n les clés valide sont : {string.Join(',', _powerUris.Select(s => $"`{s.Key}`"))}");
                        return;
                    }
                    pool.Add(_powerUris[key]);
                }
            }

            string uri = pool.PickRandom();
            string redirectedUrl = await OnlineUtils.GetRedirectedUrlAsync(uri);

            await ReplyAsync(redirectedUrl);


        }

        private Embed GenerateFieldsEmbed(List<EncounterProfile> data)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle("📝 Champs disponibles")
                .WithColor(Color.Blue);

            // Liste unique des valeurs de chaque champ
            var categories = data.Select(p => p.Category).Distinct();
            var genres = data.SelectMany(p => p.Genre.Select(g => g.Name)).Distinct();
            var puissances = data.SelectMany(p => p.PowerLevel.Select(p => p.Name)).Distinct();
            var status = data.SelectMany(p => p.Status).Distinct();
            var corruption = data.Select(p => p.Corruption).Distinct();

            embedBuilder
                .AddField("category", string.Join(", ", categories))
                .AddField("genre", string.Join(", ", genres))
                .AddField("puissance", string.Join(", ", puissances))
                .AddField("status", string.Join(", ", status))
                .AddField("corruption", string.Join(", ", corruption));

            return embedBuilder.Build();
        }

        private List<EncounterProfile> ApplyFilters(List<EncounterProfile> data, string filterString)
        {
            if (string.IsNullOrWhiteSpace(filterString))
                return data;

            var filters = ParseFilters(filterString);
            var filteredData = data.AsEnumerable();

            foreach (var filter in filters)
            {
                filteredData = ApplyFilter(filteredData, filter.Key, filter.Value.value, filter.Value.comparator);
            }

            return filteredData.ToList();
        }

        private Dictionary<string, (string comparator, string value)> ParseFilters(string filterString)
        {
            Dictionary<string, (string comparator, string value)> filters = new();

            Regex regex = new(@"(\w+)([<>=]{1,2})([\w\s]+)", RegexOptions.IgnoreCase);

            foreach (string part in filterString.Split(' '))
            {
                MatchCollection matches = regex.Matches(part);
                foreach (Match match in matches)
                {
                    if (match.Groups.Count != 4)
                        continue;

                    string key = match.Groups[1].Value.ToLower();
                    string comparator = match.Groups[2].Value;
                    string value = match.Groups[3].Value;
                    filters[key] = (comparator, value);
                }
            }

            return filters;
        }

        private IEnumerable<EncounterProfile> ApplyFilter(IEnumerable<EncounterProfile> data, string key, string value, string comparator)
        {
            if (comparator is "=" or "==")
                switch (key)
                {
                    case "genre":
                        {
                            IEnumerable<EncounterProfile> fdata = data.Where(profile =>
                            profile.Genre.Any(g => g.Name.Equals(value, StringComparison.OrdinalIgnoreCase)));

                            var encounterProfiles = fdata as EncounterProfile[] ?? fdata.ToArray();

                            foreach (var profile in encounterProfiles)
                            {
                                profile.Genre.RemoveAll(g => !g.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
                            }
                            return encounterProfiles;
                        }
                    case "status":
                        {
                            IEnumerable<EncounterProfile> fdata = data.Where(profile =>
                            profile.Status.Any(g => g.Equals(value, StringComparison.OrdinalIgnoreCase)));

                            var encounterProfiles = fdata as EncounterProfile[] ?? fdata.ToArray();

                            foreach (var profile in encounterProfiles)
                            {
                                profile.Status.RemoveAll(g => !g.Equals(value, StringComparison.OrdinalIgnoreCase));
                            }
                            return encounterProfiles;
                        }
                    case "category":
                        {
                            EncounterProfile[] encounterProfiles = data.Where(profile =>
                            profile.Category.Equals(value, StringComparison.OrdinalIgnoreCase)).ToArray();

                            return encounterProfiles;
                        }
                    case "description":
                        {
                            var encounterProfiles = data.Where(profile =>
                            profile.Description.Contains(value, StringComparison.OrdinalIgnoreCase)).ToArray();
                            return encounterProfiles;
                        }
                    case "corruption":
                        {
                            var encounterProfiles = data.Where(profile =>
                            profile.Corruption.Equals(value, StringComparison.OrdinalIgnoreCase)).ToArray();
                            return encounterProfiles;
                        }
                    case "puissance":
                        {
                            IEnumerable<EncounterProfile> fdata = data.Where(profile =>
                            profile.PowerLevel.Any(p => p.Name.Equals(value, StringComparison.OrdinalIgnoreCase)));

                            var encounterProfiles = fdata as EncounterProfile[] ?? fdata.ToArray();

                            foreach (var profile in encounterProfiles)
                            {
                                profile.PowerLevel.RemoveAll(p => !p.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
                            }
                            return encounterProfiles;
                        }
                    default:
                        return data;
                }
            else
            {
                switch (key)
                {
                    case "group":
                        return CompareGroup(data, comparator, int.Parse(value));
                    default:
                        return data;
                }
            }
        }

        private IEnumerable<EncounterProfile> CompareGroup(IEnumerable<EncounterProfile> data, string comparison, int number)
        {

            var filteredData = new List<EncounterProfile>();

            foreach (var profile in data)
            {
                var group = profile.Group;

                bool isValid = comparison switch
                {
                    ">" => group.Max > number,
                    "<" => group.Min < number,
                    ">=" => group.Max >= number,
                    "<=" => group.Min <= number,
                    "=" or "" => group.Min <= number && group.Max >= number,
                    _ => false
                };

                if (isValid)
                {
                    int adjustedMin = Math.Max(group.Min, number);
                    int adjustedMax = Math.Min(group.Max, number);

                    profile.Group = new Group
                    {
                        Min = adjustedMin,
                        Max = adjustedMax
                    };

                    filteredData.Add(profile);
                }
            }

            return filteredData;
        }



        private IEnumerable<EncounterProfile> CompareNumbers(IEnumerable<EncounterProfile> data, Func<EncounterProfile, int> selector, string filterValue)
        {
            var comparisonRegex = new Regex(@"(>=|<=|>|<|=)?\s*(\d+)");
            var match = comparisonRegex.Match(filterValue);

            if (!match.Success) return data;

            var comparison = match.Groups[1].Value;
            var number = int.Parse(match.Groups[2].Value);

            return comparison switch
            {
                ">" => data.Where(profile => selector(profile) > number),
                "<" => data.Where(profile => selector(profile) < number),
                ">=" => data.Where(profile => selector(profile) >= number),
                "<=" => data.Where(profile => selector(profile) <= number),
                "=" or "" => data.Where(profile => selector(profile) == number),
                _ => data
            };
        }
    }
}
