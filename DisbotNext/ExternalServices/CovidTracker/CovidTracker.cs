using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.ExternalServices.CovidTracker
{
    public class CovidTracker : ICovidTracker
    {
        private readonly HttpClient _httpClient;
        public CovidTracker(IHttpClientFactory httpClientFactory)
        {
            this._httpClient = httpClientFactory.CreateClient();
        }

        public async Task<CovidTrackerModel?> GetCovidTrackerDataAsync(string country, CancellationToken cancellationToken = default)
        {
            try
            {
                var requestCountry = country.ToLowerInvariant() == "all" ? "all" : $"countries/{country}";
                var url = $"https://coronavirus-19-api.herokuapp.com/{requestCountry}";

                using var response = await this._httpClient.GetAsync(url);

                await using var memoryStream = new MemoryStream();

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

                var result = await JsonSerializer.DeserializeAsync<CovidTrackerModel>(stream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                }, cancellationToken);

                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
