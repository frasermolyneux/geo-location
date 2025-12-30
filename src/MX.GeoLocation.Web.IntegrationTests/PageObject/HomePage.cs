using MX.GeoLocation.Web.IntegrationTests.PageObject.PageParts;
using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;

namespace MX.GeoLocation.Web.IntegrationTests.PageObject
{
    public class HomePage : IPageObject
    {
        private readonly Microsoft.Playwright.IPage page;
        private readonly IConfiguration configuration;

        public HomePage(Microsoft.Playwright.IPage page, IConfiguration configuration)
        {
            this.page = page;
            this.configuration = configuration;

            Navigation = new NavigationBar(page);
        }

        public NavigationBar Navigation { get; private set; }

        public async Task<bool> IsOnPageAsync()
        {
            try
            {
                // Check the page title instead of looking for a pageTitle element
                var title = await page.TitleAsync();
                return title?.Contains("Home Page") == true;
            }
            catch
            {
                return false;
            }
        }

        public async Task GoToPageAsync(bool useNavigation = false)
        {
            if (useNavigation)
            {
                await Navigation.ClickNavBarHomeAsync();
            }
            else
            {
                var baseUrl = GetBaseUrl();
                await page.GotoAsync(baseUrl, new PageGotoOptions
                {
                    Timeout = 60000,
                    WaitUntil = WaitUntilState.NetworkIdle
                });
            }
        }

        private string GetBaseUrl()
        {
            var url = configuration["SiteUrl"] ?? "https://dev.geo-location.net";
            return url.EndsWith("/") ? url.Substring(0, url.Length - 1) : url;
        }
    }
}
