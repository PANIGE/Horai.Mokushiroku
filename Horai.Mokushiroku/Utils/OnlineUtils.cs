using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horai.Mokushiroku.Utils
{
    public static class OnlineUtils
    {

        public static async Task<string> GetRedirectedUrlAsync(string url)
        {
            using HttpClient httpClient = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = false  // On empêche automatiquement de suivre la redirection
            });

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.StatusCode != System.Net.HttpStatusCode.Found && response.StatusCode != System.Net.HttpStatusCode.MovedPermanently)
                    return "Aucune redirection trouvée ou requête échouée.";

                if (response.Headers.Location == null)
                    return "Aucune redirection trouvée ou requête échouée.";

                string redirectedUrl = response.Headers.Location.IsAbsoluteUri
                ? response.Headers.Location.ToString()
                : new Uri(new Uri(url), response.Headers.Location).ToString();

                return redirectedUrl;

            }
            catch (Exception ex)
            {
                return $"Erreur : {ex.Message}";
            }
        }
    }

}
