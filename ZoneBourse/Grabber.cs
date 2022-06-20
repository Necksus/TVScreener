using System.Text;
using HtmlAgilityPack;
using TVScreener.Controllers;

namespace TVScreener.ZoneBourse
{
    public class Grabber
    {
        private const string ZoneBourseUrl = "https://www.zonebourse.com";
        private const string ScreenerUrl = $"{ZoneBourseUrl}/outils/stock-screener/";
        private const string FindStocksUrl = $"{ZoneBourseUrl}/outils/mods_a/moteurs_results.php";


        public void GetCountriesAsync(out List<Country> countries)
        {
            void ParseCountry(List<Country> countries, HtmlNode rootNode, Country parent)
            {
                var id = rootNode.SelectSingleNode("./input[starts-with(@class, 'cb_sector')]").GetAttributeValue("value", "");
                var name = Trim(rootNode.SelectSingleNode("./label[starts-with(@for, 'country_')]").InnerText);
                var subCountries = rootNode.SelectNodes(".//ul[starts-with(@id, 'sub_countries_')]/li");
                
                var country = new Country(id, name, parent?.Id ?? JsTreeModel.RootId, subCountries?.Count > 0);
                countries.Add(country);
                //Console.WriteLine($"{country.Name} - {country.Id}");

                if (subCountries != null)
                {
                    foreach (var subCountry in subCountries)
                    {
                        ParseCountry(countries, subCountry, country);
                    }
                }
            }

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(ScreenerUrl);

            var expendableDivSector = doc.DocumentNode.SelectSingleNode("//ul[@id='sub_countries_all']");
            countries = new List<Country>();

            foreach (var mainCountry in expendableDivSector.SelectNodes("./li"))
            {
                ParseCountry(countries, mainCountry, null);
            }
        }

        public void GetSectorsAsync(out List<Sector> sectors)
        {
            void ParseSector(List<Sector> sectors, HtmlNode rootNode, Sector parent)
            {
                var sectorInfo = ExtractSectorInfo(rootNode);
                var subSectorNodes = rootNode.SelectNodes("./ul/li");

                var sector = new Sector(sectorInfo.Id, sectorInfo.Label, parent?.Id ?? JsTreeModel.RootId, subSectorNodes?.Count > 0);
                sectors.Add(sector);
                //Console.WriteLine($"{parent?.Id} - {sectorInfo.Id} - {sectorInfo.Label}");

                if (subSectorNodes != null)
                {
                    foreach (var subSector in rootNode.SelectNodes("./ul/li"))
                    {
                        ParseSector(sectors, subSector, sector);
                    }
                }
            }

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(ScreenerUrl);

            var expendableDivSector = doc.DocumentNode.SelectSingleNode("//div[@id='listeSecteurs']");
            sectors = new List<Sector>();

            foreach (var mainSectorLi in expendableDivSector.SelectNodes("./ul/li"))
            {
                ParseSector(sectors, mainSectorLi, null);
            }
        }

        public async Task<List<Stock>> GetStocksAsync(List<string> countryIdentifiers, List<string> sectorIdentifiers, CancellationToken cancellationToken = default)
        {
            var result = new List<Stock>();

            HttpContent GetQueryContent(List<string> countryIdentifiers, List<string> sectorIdentifiers, int pageNumber)
            {
                var sectors = FormatSectors(sectorIdentifiers);
                var markets = string.Join(",", countryIdentifiers.Select(s => $"\"{s}\""));
                var request = $"{{ \"aSectors\": [ {{}}, {{}}, {{}}, {{}}, {{ {sectors} }}], \"markets\": [ {markets} ], \"capi_min\": 2, \"sMode\": \"AF2\",	\"page\": {pageNumber} }}";
                return new FormUrlEncodedContent(
                    new List<KeyValuePair<string, string>>
                    {
                    new("Req", request),
                    new("bJSON", "true"),
                    new("scrollMode", "false")
                    }
                );
            }

            var pageNumber = 1;
            while (!cancellationToken.IsCancellationRequested)
            {
                HttpClient client = new HttpClient();
                HtmlDocument doc = new HtmlDocument();
                var response = await client.PostAsync(FindStocksUrl, GetQueryContent(countryIdentifiers, sectorIdentifiers, pageNumber), cancellationToken);
                response.EnsureSuccessStatusCode();

                doc.LoadHtml(await response.Content.ReadAsStringAsync(cancellationToken));
                var stocks = doc.DocumentNode.SelectNodes("//a[starts-with(@href, '/cours/action/')]");
                if (stocks != null)
                {
                    foreach (var stock in stocks)
                    {
                        var href = stock.GetAttributeValue("href", "");
                        try
                        {
                            result.Add(ExtractStockInfo(href));
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"Fail to extract {href}, {ex.Message}");
                        }

                        //Console.WriteLine(result.Last().TradingViewCode);
                    }

                    if (doc.DocumentNode.SelectNodes("//a[@class='nPageEndTab']") == null)
                        break;
                    pageNumber++;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private string FormatSectors(List<string> sectorIdentifiers)
        {
            var sb = new StringBuilder();
            var currentGroup = long.MinValue;

            foreach (var sectorIdentifier in sectorIdentifiers.OrderBy(s => s))
            {
                var group = long.Parse(sectorIdentifier) / 100000000;
                if (group != currentGroup)
                {
                    currentGroup = group;
                    if (sb.Length > 0) sb.Append("], ");
                    sb.Append($"\"{group}\": [ \"{sectorIdentifier}\"");
                }
                else
                {
                    sb.Append($", \"{sectorIdentifier}\"");
                }
            }

            sb.Append("]");

            return sb.ToString();
        }

        private Stock ExtractStockInfo(string detailUrl)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(ZoneBourseUrl + detailUrl);

            //var tableNode = doc.DocumentNode.SelectSingleNode("//div[@class='bc_pos']")?.NextSibling?.NextSibling;
            var anchorNode = doc.DocumentNode.SelectSingleNode("//a[@itemprop='name']"); //tableNode.SelectSingleNode(".//a");
            var stockName = anchorNode.SelectSingleNode("./h1").InnerText;
            var stockCode = anchorNode.LastChild.InnerText.Trim(' ', '(', ')');
            var currency = doc.DocumentNode.SelectSingleNode("//td[@class='fvCur colorBlack']")?.InnerText?.Trim();
            var zbExchange = doc.DocumentNode.SelectSingleNode(".//td[@class='tabTitleLeftWhite']/nobr/span")?.InnerText
                .Replace("&nbsp;", "")
                .Replace("Temps Différé ", "")
                .Replace("Temps réel ", "")
                .Replace("Cours en clôture ", "");

            return StockFactory.CreateStock(stockCode, stockName, zbExchange, currency);
        }


        private (string Id, string Label) ExtractSectorInfo(HtmlNode node)
        {
            var id = node.SelectSingleNode(".//input[starts-with(@class, 'cb_sector')]").GetAttributeValue("id", null);
            var label = Trim(node.SelectSingleNode($".//label[@for='{id}']").InnerText);

            return (id, label);
        }

        private string Trim(string text)
            => text.Replace("&nbsp;", "");
    }
}
