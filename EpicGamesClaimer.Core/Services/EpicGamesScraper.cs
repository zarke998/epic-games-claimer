using EpicGamesClaimer.Core.Models;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EpicGamesClaimer.Core.Services
{
    public class EpicGamesScraper
    {
        public const string EPIC_GAMES_URL = "https://store.epicgames.com";
        public const string FREE_GAMES_URL = "https://store.epicgames.com/en-US/free-games";

        public EpicGamesScraper()
        {
            var browserFetcher = new BrowserFetcher();
            browserFetcher.DownloadAsync().Wait();
        }
        public async Task<IEnumerable<GameInfo>> GetFreeGames()
        {
            var result = new List<GameInfo>();

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions() { Headless = false });

            await using var page = await browser.NewPageAsync();
            await page.GoToAsync(FREE_GAMES_URL);

            await page.WaitForXPathAsync(".//div[contains(@data-component, 'FreeOfferCard')]");

            var freeGameTags = await page.XPathAsync(".//div[contains(@data-component, 'FreeOfferCard')]//ancestor::div[@data-component='CardGridDesktopBase'][.//span[contains(text(), 'Free Now')]]");

            foreach (var freeGameTag in freeGameTags)
            {
                var linkTag = (await freeGameTag.XPathAsync(".//a"))[0];
                var link = (await page.EvaluateFunctionAsync("e => e.getAttribute('href')", linkTag)).ToString();

                var titleTag = (await freeGameTag.XPathAsync(".//span[contains(@data-testid, 'offer-title-info-title')]/div"))[0];
                var title = (await page.EvaluateFunctionAsync("e => e.innerText", titleTag)).ToString();

                result.Add(new GameInfo()
                {
                    Title = title,
                    Url = EPIC_GAMES_URL + link
                });
            }

            return result;
        }
    }
}
