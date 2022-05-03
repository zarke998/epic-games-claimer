using EpicGamesClaimer.Core.Services;

var scraper = new EpicGamesScraper();
var freeGames = scraper.GetFreeGames().Result;

Console.WriteLine("Hello, World!");
