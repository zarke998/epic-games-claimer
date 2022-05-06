using EpicGamesClaimer.Core.Models;
using PuppeteerExtraSharp.Plugins.AnonymizeUa;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerExtraSharp.Plugins.ExtraStealth.Evasions;
using PuppeteerExtraSharp.Plugins.Recaptcha;
using PuppeteerSharp;
using RandomUserAgent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EpicGamesClaimer.Core.Services
{
    public class EpicGamesScraper
    {
        public const string EPIC_GAMES_URL = "https://store.epicgames.com";
        public const string FREE_GAMES_URL = EPIC_GAMES_URL + "/en-US/free-games";

        private string _browserExePath;

        public EpicGamesScraper()
        {
            var revisionInfo = new BrowserFetcher().DownloadAsync().Result;
            _browserExePath = revisionInfo.ExecutablePath;
        }
        public async Task<IEnumerable<GameInfo>> GetFreeGames()
        {
            var result = new List<GameInfo>();

            await using var browser = await CreateBrowserAsync();
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

        public async Task<bool> IsLoggedInAsync()
        {
            await using var browser = await CreateBrowserAsync();
            await using var page = await browser.NewPageAsync();

            await page.GoToAsync(FREE_GAMES_URL);

            var signInTag = await page.XPathAsync(".//div[contains(@data-component, 'SignIn')]");

            return signInTag.Length == 0;
        }

        public void OpenBrowser()
        {
            var userDataDir = Path.Combine(AppContext.BaseDirectory, "BrowserCache").Replace(@"\", @"\\");


            Process.Start(_browserExePath, $"--user-data-dir=\"{userDataDir}\"");
        }

        public async Task ClaimGameAsync(string url)
        {
            await using var browser = await CreateBrowserAsync();
            await using var page = await browser.NewPageAsync();

            await page.SetJavaScriptEnabledAsync(true);
            await page.GoToAsync(url);

            var getButton = (await page.XPathAsync(".//button[contains(@data-testid, 'purchase-cta-button')]"))[0];
            await getButton.ClickAsync();

            await page.WaitForXPathAsync(".//div[contains(@class,'webPurchaseContainer')]/iframe");
            var orderIframeContainer = (await page.XPathAsync(".//div[contains(@class,'webPurchaseContainer')]/iframe"))[0];
            var orderIframe = await orderIframeContainer.ContentFrameAsync();

            await orderIframe.WaitForXPathAsync(".//button[contains(@class,'payment-order-confirm__btn')]", new WaitForSelectorOptions() { Visible = true});

            var placeOrderButton = (await orderIframe.XPathAsync(".//button[contains(@class,'payment-order-confirm__btn')]"))[0];            
            await placeOrderButton.ClickAsync(new PuppeteerSharp.Input.ClickOptions());

            await Task.Delay(50000);
        }

        private async Task<Browser> CreateBrowserAsync()
        {
            var userDataDir = Path.Combine(AppContext.BaseDirectory, "BrowserCache");

            return await Puppeteer.LaunchAsync(new LaunchOptions() { Headless = false, UserDataDir = userDataDir });
        }
    }
}
