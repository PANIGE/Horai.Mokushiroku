using Discord.Commands;
using Discord;
using System.Threading.Tasks;

namespace Horai.Mokushiroku.Cogs
{
    internal class HelpCommand : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task Help()
        {
            var embed = new EmbedBuilder()
                .WithTitle("📖 Aide - Horai Mokushiroku Bot")
                .WithDescription("Liste des commandes disponibles et leur utilisation détaillée.")
                .WithColor(new Color(7377904))
                .WithFooter("Horai Mokushiroku Bot - Système de Gestion de Profils");

            // Section - Commandes de Lecture
            embed.AddField("🔍 Commandes de Lecture",
                $"**__`$pick [filtres]`__**\n" +
                "→ Choisit un profil aléatoire parmi ceux qui correspondent aux filtres donnés.\n" +
                "*Exemple :* `$pick genre=Horreur group>=5`\n" +
                "*Note :* Si aucun filtre n'est donné, l'ensemble des données est utilisé.\n\n" +

                $"**__`$list [page]`__**\n" +
                "→ Affiche une liste paginée des profils disponibles.\n" +
                "*Exemple :* `$list 1`\n" +
                "*Note :* Par défaut, affiche 20 entrées par page.\n\n" +

                $"**__`$describe [index]`__**\n" +
                "→ Affiche les spécificités d'un profil via son index.\n" +
                "*Exemple :* `$describe 0`\n" +
                "*Note :* L'index doit être un entier valide affiché par `$list`.\n\n" +

                $"**__`$fields`__**\n" +
                "→ Affiche les différentes valeurs possibles pour chaque champ des profils.\n" +
                "*Exemple :* `$fields`\n" +
                "*Note :* Donne un aperçu des données possibles pour mieux comprendre les filtres utilisables."
            );

            // Section - Commandes d'Écriture (Restreintes)
            embed.AddField("📝 Commandes d'Écriture (Restreintes)",
                $"**__`$dump`__**\n" +
                "→ Envoie le fichier JSON complet des profils par message privé.\n" +
                "*Exemple :* `$dump`\n" +
                "*Note :* Commande réservée aux utilisateurs autorisés.\n\n" +

                $"**__`$add [json]`__**\n" +
                "→ Ajoute un nouveau profil aux données. Nécessite un JSON valide.\n" +
                "*Exemple :* `$add { \"name\": \"Monstre\", \"category\": \"Creature\", ... }`\n" +
                "*Note :* Aucune confirmation demandée, assurez-vous que le JSON est correct.\n\n" +

                $"**__`$remove [index]`__**\n" +
                "→ Supprime un profil par son index après confirmation.\n" +
                "*Exemple :* `$remove 0`\n" +
                "*Note :* Un message de confirmation sera demandé (`oui` pour confirmer).\n\n" +

                $"**__`$set` (Avec Pièce Jointe)__**\n" +
                "→ Remplace entièrement la base de données par un nouveau fichier JSON envoyé en PJ.\n" +
                "*Exemple :* `$set` + Pièce jointe `data.json`\n" +
                "*Note :* Une confirmation est demandée avant de remplacer les données. Le fichier doit être au format `.json`."
            );

            // Section - Commande de Pouvoirs (Powerlisting)
            embed.AddField("⚡ **Commande de Pouvoirs (Powerlisting)**",
                "**`$power [catégories]`**\n" +
                "> Récupère un pouvoir aléatoire depuis Powerlisting Wiki. Si aucune catégorie n'est spécifiée, une catégorie est choisie au hasard.\n" +
                "> Exemple : `$power almighty, psychic`\n" +
                "> Catégories disponibles : `almighty`, `constructs`, `enhancements`, `magical`, `manipulations`, `meta`, `physiology`, `psychic`, `science`\n" +
                "> *Note :* Si une clé invalide est donnée, un message d'erreur sera envoyé."
            );

            // Section - Système de Filtrage
            embed.AddField("📌 Système de Filtrage",
                "Les filtres sont fournis sous la forme :\n" +
                "`clé=valeur`, `clé>valeur`, `clé<valeur`, `clé<=valeur`, `clé>=valeur`\n\n" +
                "**Exemple d'utilisation :** `$pick genre=Horreur group>=5 corruption=Faible`\n\n" +
                "**Clés disponibles :** `genre`, `category`, `description`, `corruption`, `puissance`, `group` (comparaison numérique)"
            );

            // Section - Notes Importantes
            embed.AddField("⚠️ Notes Importantes",
                "1. Les commandes d'ajout, de suppression, et de remplacement sont **restreintes aux utilisateurs autorisés**.\n" +
                "2. Les commandes d'ajout (`$add`) n'ont pas de confirmation, vérifiez bien votre JSON.\n" +
                "3. La commande `$set` nécessite une pièce jointe `.json` valide.\n" +
                "4. Le filtrage via `group` permet les opérateurs suivants : `>`, `<`, `>=`, `<=`, `=`.\n"
            );

            await ReplyAsync(embed: embed.Build());
        }
    }
}
