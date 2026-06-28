using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Horai.Mokushiroku.Utils
{
    public static class OnlineUtils
    {
        private const string WikiBaseUrl =
            "https://powerlisting.fandom.com";

        private static readonly HttpClient HttpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Horai.Mokushiroku/1.0");

            return client;
        }

        public static async Task<string?> GetRandomCategoryPageAsync(
            string categoryName)
        {
            string normalizedCategory = categoryName.StartsWith(
                "Category:",
                StringComparison.OrdinalIgnoreCase)
                    ? categoryName
                    : $"Category:{categoryName}";

            List<CategoryMember> members = new();

            string? continueToken = null;

            do
            {
                string requestUrl =
                    $"{WikiBaseUrl}/api.php" +
                    "?action=query" +
                    "&list=categorymembers" +
                    $"&cmtitle={Uri.EscapeDataString(normalizedCategory)}" +
                    "&cmnamespace=0" +
                    "&cmlimit=500" +
                    "&format=json";

                if (!string.IsNullOrWhiteSpace(continueToken))
                {
                    requestUrl +=
                        $"&cmcontinue={Uri.EscapeDataString(continueToken)}";
                }

                using HttpResponseMessage response =
                    await HttpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                CategoryMembersResponse? result =
                    await response.Content
                        .ReadFromJsonAsync<CategoryMembersResponse>();

                if (result?.Query?.CategoryMembers is not null)
                {
                    members.AddRange(result.Query.CategoryMembers);
                }

                continueToken = result?.Continue?.CategoryMembersContinue;
            }
            while (!string.IsNullOrWhiteSpace(continueToken));

            if (members.Count == 0)
            {
                return null;
            }

            CategoryMember selected =
                members[Random.Shared.Next(members.Count)];

            return BuildWikiPageUrl(selected.Title);
        }

        private static string BuildWikiPageUrl(string pageTitle)
        {
            string normalizedTitle =
                pageTitle.Replace(' ', '_');

            string encodedTitle = Uri.EscapeDataString(normalizedTitle)
                .Replace("%2F", "/", StringComparison.OrdinalIgnoreCase);

            return $"{WikiBaseUrl}/wiki/{encodedTitle}";
        }
    }

    public sealed class CategoryMembersResponse
    {
        [JsonPropertyName("continue")]
        public CategoryContinue? Continue { get; set; }

        [JsonPropertyName("query")]
        public CategoryMembersQuery? Query { get; set; }
    }

    public sealed class CategoryContinue
    {
        [JsonPropertyName("cmcontinue")]
        public string? CategoryMembersContinue { get; set; }
    }

    public sealed class CategoryMembersQuery
    {
        [JsonPropertyName("categorymembers")]
        public List<CategoryMember> CategoryMembers { get; set; } = [];
    }

    public sealed class CategoryMember
    {
        [JsonPropertyName("pageid")]
        public int PageId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";
    }

}
