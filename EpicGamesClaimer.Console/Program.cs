using EpicGamesClaimer.Core.Services;

var scraper = new EpicGamesScraper();
scraper.ClaimGameAsync("https://store.epicgames.com/en-US/p/terraforming-mars-18c3ad").Wait();
//scraper.OpenBrowser();

Console.WriteLine("Hello, World!");
