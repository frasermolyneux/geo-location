using MX.GeoLocation.Web.IntegrationTests.PageObject.PageParts;
using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;

namespace MX.GeoLocation.Web.IntegrationTests.PageObject
{
    public class BatchLookupPage : IPageObject
    {
        private readonly Microsoft.Playwright.IPage page;
        private readonly IConfiguration configuration;

        public BatchLookupPage(Microsoft.Playwright.IPage page, IConfiguration configuration)
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
                var title = await page.TitleAsync();
                return title?.Contains("Batch Lookup") == true;
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
                await Navigation.ClickNavBarLookupDropdownAsync();
                await Navigation.ClickNavBarLookupBatchAsync();
            }
            else
            {
                var baseUrl = GetBaseUrl();
                await page.GotoAsync($"{baseUrl}/Home/BatchLookup");
            }
        }

        private string GetBaseUrl()
        {
            var url = configuration["SiteUrl"] ?? "https://dev.geo-location.net";
            return url.EndsWith("/") ? url.Substring(0, url.Length - 1) : url;
        }
    }
}
