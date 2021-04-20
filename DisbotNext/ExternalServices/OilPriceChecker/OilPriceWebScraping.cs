using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.ExternalServices.OilPriceChecker
{
    public class OilPriceWebScraping : IOilPriceChecker
    {
        public Task<IEnumerable<OilRetail>> GetOilPriceAsync(CancellationToken cancellationToken = default)
        {
            using var webClient = new WebClient();
            var html = webClient.DownloadString(new Uri("https://bit.ly/3rWV4wN"));
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var gtodayDiv = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class,'gtoday')]");
            var table = gtodayDiv.SelectSingleNode("//table");
            var thead = table.SelectSingleNode("//thead");
            var tbody = table.SelectSingleNode("//tbody");

            var oilRetailer = thead.ChildNodes[0].ChildNodes.Select(x => x.InnerText).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            var retails = new List<OilRetail>();
            for (var i = 0; i < oilRetailer.Length; i++)
            {
                var retail = new OilRetail();
                retail.RetailName = oilRetailer[i];
                var types = new List<OilType>();
                foreach (var oilType in tbody.ChildNodes)
                {
                    var oilTypeDesc = oilType.ChildNodes[0].InnerHtml;
                    var price = oilType.ChildNodes[i + 1].InnerHtml;

                    var isValidPrice = decimal.TryParse(price, out var pricePerLitre);

                    types.Add(new OilType
                    {
                        RetailName = retail.RetailName,
                        Type = oilTypeDesc,
                        PricePerLitre = isValidPrice ? pricePerLitre : null,
                    });
                }
                retail.Types = types;
                retails.Add(retail);
            }

            return Task.FromResult(retails as IEnumerable<OilRetail>);
        }
    }
}
