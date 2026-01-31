using MX.GeoLocation.Web.IntegrationTests.PageObject.PageParts;
using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;

namespace MX.GeoLocation.Web.IntegrationTests.PageObject
{
    public class RemoveDataPage : IPageObject
    {
        private readonly Microsoft.Playwright.IPage page;
        private readonly IConfiguration configuration;

        public RemoveDataPage(Microsoft.Playwright.IPage page, IConfiguration configuration)
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
                return title?.Contains("Remove") == true;
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
                await Navigation.ClickNavBarPrivacyDropdownAsync();
                await Navigation.ClickNavBarPrivacyRemoveMyDataAsync();
            }
            else
            {
                var baseUrl = GetBaseUrl();
                await page.GotoAsync($"{baseUrl}/Home/RemoveData");
            }
        }

        private string GetBaseUrl()
        {
            var url = configuration["SiteUrl"] ?? "https://dev.geo-location.net";
            return url.EndsWith("/") ? url[..^1] : url;
        }
    }
}
