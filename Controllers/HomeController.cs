using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using TVScreener.Models;
using TVScreener.ZoneBourse;

namespace TVScreener.Controllers
{
    public class HomeController : Controller
    {
        private readonly List<string> SupportedCountryIds = JsonConvert.DeserializeObject<List<string>>("[\"11\",\"12\",\"40\",\"43\",\"47\",\"48\",\"49\",\"50\",\"51\",\"53\",\"55\",\"56\",\"57\",\"58\",\"59\",\"62\",\"69\",\"70\",\"71\",\"72\",\"73\",\"77\",\"78\",\"79\",\"80\",\"82\"]");
        private const string ZoneBourseDataFolder = "zonebourse";

        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly Grabber _grabber = new Grabber();

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            /*

            TODO : move this somewhere else...

            _grabber.GetCountriesAsync(out var countries);
            _grabber.GetSectorsAsync(out var sectors);
            
            //Serialize to JSON string.
            ViewBag.Sectors = JsonConvert.SerializeObject(sectors);
            ViewBag.Countries = JsonConvert.SerializeObject(countries);

            System.IO.File.WriteAllText(Path.Combine (_environment.WebRootPath, ZoneBourseDataFolder, "sectors.json"), ViewBag.Sectors);
            System.IO.File.WriteAllText(Path.Combine(_environment.WebRootPath, ZoneBourseDataFolder, "countries.json"), ViewBag.Countries);
            
            foreach (var sector in sectors.Where(s => !s.HasChildren))
            {
                foreach (var country in SupportedCountryIds)
                {
                    var file = Path.Combine(_environment.WebRootPath, ZoneBourseDataFolder, sector.Id, $"{country}.json");
                    if (!System.IO.File.Exists(file))
                    {
                        var stocks = await _grabber.GetStocksAsync(new List<string>(new[] {country}), new List<string>(new[] {sector.Id}));
                        var json = JsonConvert.SerializeObject(stocks);
                        Directory.CreateDirectory(Path.GetDirectoryName(file));
                        System.IO.File.WriteAllText(file, json);
                    }
                }
            }
            */
            var sectors = System.IO.File.ReadAllText(Path.Combine(_environment.WebRootPath, ZoneBourseDataFolder, "sectors.json"));
            var countries = JsonConvert.DeserializeObject<List<Country>>(System.IO.File.ReadAllText(Path.Combine(_environment.WebRootPath, ZoneBourseDataFolder, "countries.json")));

            //Serialize to JSON string.
            ViewBag.Sectors = sectors;
            ViewBag.Countries = JsonConvert.SerializeObject(countries.Where(c => SupportedCountryIds.Contains(c.Id) || c.Id == "3" || c.Id == "8"));
            //ViewBag.Countries = countries;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(ErrorViewModel vm)
        {
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SaveAsync(string selectedSectorsIdJson, string selectedCountriesIdJson, CancellationToken cancellationToken)
        {
            void AddItem(StringBuilder sb, string item)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(item);
            }

            var selectedSectorId = !string.IsNullOrEmpty(selectedSectorsIdJson) ? JsonConvert.DeserializeObject<List<string>>(selectedSectorsIdJson) : null;
            var selectedCountriesId = !string.IsNullOrEmpty(selectedCountriesIdJson) ? JsonConvert.DeserializeObject<List<string>>(selectedCountriesIdJson) : null;

            if (selectedSectorId == null || selectedSectorId.Count == 0 || selectedCountriesId == null || selectedCountriesId?.Count == 0)
                return RedirectToAction("Error", new ErrorViewModel {Message = "Please select at least one sector and one country."});

            var sectors = JsonConvert.DeserializeObject<List<Sector>>(System.IO.File.ReadAllText(Path.Combine(_environment.WebRootPath, ZoneBourseDataFolder, "sectors.json")));

            var sb = new StringBuilder();
            foreach (var sectorId in selectedSectorId)
            {
                var writePrefix = false;
                if (Directory.Exists(Path.Combine(_environment.WebRootPath, ZoneBourseDataFolder, sectorId)))
                {
                    foreach (var countryId in selectedCountriesId)
                    {
                        var jsonFile = Path.Combine(_environment.WebRootPath, ZoneBourseDataFolder, sectorId,  $"{countryId}.json");
                        if (System.IO.File.Exists(jsonFile))
                        {
                            var stocks =  JsonConvert.DeserializeObject<List<Stock>>(System.IO.File.ReadAllText(jsonFile));
                            if (stocks != null)
                            {
                                foreach (var stock in stocks)
                                {
                                    if (!writePrefix)
                                    {
                                        AddItem(sb, $"###{sectors.Single(s => s.Id == sectorId).Name}");
                                        writePrefix = true;
                                    }
                                    AddItem(sb, stock.TradingViewCode);
                                }
                            }
                        }
                    }
                }
            }

            if (sb.Length == 0)
                return RedirectToAction("Error", new ErrorViewModel { Message = "No result for theses criteria." });

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            memoryStream.Position = 0;
            return File(memoryStream, "text/plain", "TradingView watchlist.txt");

            /*
            if (selectedSectorId?.Count > 0 && selectedCountriesId?.Count > 0)
            {
                var stocks = await _grabber.GetStocksAsync(selectedCountriesId, selectedSectorId, cancellationToken);

                var sb = new StringBuilder();
                foreach (var stock in stocks)
                {
                    if (!string.IsNullOrEmpty(stock.Exchange))
                    {
                        if (sb.Length > 0) sb.Append(", ");
                        sb.Append(stock.TradingViewCode);
                    }
                }

                var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
                memoryStream.Position = 0;
                return File(memoryStream, "text/plain", "TradingView watchlist.txt");
            }
            else
            {
                return RedirectToAction("Index");
            }*/
        }

    }
}