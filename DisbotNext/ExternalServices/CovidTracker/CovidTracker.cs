using Newtonsoft.Json;
using System.Net.Http;
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
                var response = await this._httpClient.GetAsync(url);
                var result = JsonConvert.DeserializeObject<CovidTrackerModel>(await response.Content.ReadAsStringAsync());
                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}
