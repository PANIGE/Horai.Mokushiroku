using Discord.Commands;
using Discord;
using System.Threading.Tasks;

namespace Horai.Mokushiroku.Cogs
{
    public class HelpCommand : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task Help()
        {
            var embed = new EmbedBuilder()
                .WithTitle("📖 Aide - Horai Mokushiroku Bot")
                .WithDescription("Liste des commandes disponibles et leur fonctionnement.")
                .WithColor(new Color(7377904))
                .WithFooter("Horai Mokushiroku Bot - Gestion de Profils & Encounter System");

            // Lecture
            embed.AddField("🔍 Commandes de Consultation",
                "**`$pick [categorie] [filtres]`**\n" +
                "→ Choisit un profil aléatoire selon les filtres fournis.\n" +
                "*Exemple :* `$pick yokai genre=Horreur group>=3`\n\n" +

                "**`$list [page]`**\n" +
                "→ Affiche une liste paginée des profils disponibles.\n" +
                "*Exemple :* `$list 2`\n\n" +

                "**`$describe [index]`**\n" +
                "→ Affiche en détail un profil selon son index dans la liste.\n" +
                "*Exemple :* `$describe 5`\n\n" +

                "**`$fields`**\n" +
                "→ Affiche les valeurs possibles pour les filtres (`genre`, `corruption`, etc.).\n\n" +

                "**`$power [catégorie|any]`**\n" +
                "→ Récupère un pouvoir aléatoire depuis Powerlisting.\n" +
                "*Catégories supportées :* `creation`, `augmentation`, `magie`, `manipulation`, `transformation`, `psychique`, `science`\n\n" +

                "**`$export`**\n" +
                "→ Génère et télécharge un PDF listant tous les profils de manière stylisée."
            );

            // Admin
            embed.AddField("🛠️ Commandes d'Administration (Restreintes)",
                "**`$add [json]`**\n" +
                "→ Ajoute un nouveau profil depuis un objet JSON.\n\n" +

                "**`$remove [index]`**\n" +
                "→ Supprime un profil selon son index.\n\n" +

                "**`$set`** *(avec pièce jointe)*\n" +
                "→ Remplace la base de données avec un nouveau fichier JSON.\n\n" +

                "**`$dump`**\n" +
                "→ Envoie une copie complète de la base de données en message privé.\n\n" +

                "**`$status [int] [texte]`**\n" +
                "→ Change le status du bot de façon personnalisée.\n\n" +

                "**`$stamp [ping utilisateur]`**\n" +
                "→ Approuve l'utilisateur."
            );

            // Filtres
            embed.AddField("🎯 Système de Filtres",
                "Les filtres peuvent être combinés pour affiner la recherche.\n" +
                "Format : `champ=valeur`, `champ>=valeur`, etc.\n\n" +
                "**Champs disponibles :** `category`, `genre`, `description`, `corruption`, `puissance`, `status`, `group`\n" +
                "*Exemple :* `$pick yokai genre=Tragique group>=3 corruption=Faible`"
            );

            // Notes
            embed.AddField("📌 Remarques Importantes",
                "- Les commandes d'écriture sont **réservées aux admins autorisés**.\n" +
                "- `$set` nécessite une pièce jointe `.json` valide.\n" +
                "- `$add` ne vérifie pas la validité des champs, soyez vigilant.\n" +
                "- Les champs `group` peuvent être comparés avec : `>`, `<`, `>=`, `<=`, `=`.\n"
            );

            await ReplyAsync(embed: embed.Build());
        }
    }
}
