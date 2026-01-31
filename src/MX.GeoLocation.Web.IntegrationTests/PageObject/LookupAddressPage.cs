using MX.GeoLocation.Web.IntegrationTests.PageObject.PageParts;
using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;

namespace MX.GeoLocation.Web.IntegrationTests.PageObject
{
    public class LookupAddressPage : IPageObject
    {
        private readonly Microsoft.Playwright.IPage page;
        private readonly IConfiguration configuration;

        public LookupAddressPage(Microsoft.Playwright.IPage page, IConfiguration configuration)
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
                // Check if we're on the lookup address page using title or page content
                var title = await page.TitleAsync();
                return title?.Contains("Lookup Address") == true;
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
                // Navigate via the home page and click the IP Lookup link
                await page.GotoAsync(GetBaseUrl());
                var ipLookupLink = page.Locator("a", new() { HasTextString = "IP Lookup" });
                await ipLookupLink.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            }
            else
            {
                var baseUrl = GetBaseUrl();
                await page.GotoAsync($"{baseUrl}/Home/LookupAddress");
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            }
        }

        public async Task ExecuteLookupAddressFlowAsync(string address)
        {
            await AddressDataField.FillAsync(address);
            // The lookup posts results without a navigation; avoid waiting on a non-existent nav and allow extra time for the API call.
            await SearchButton.ClickAsync(new LocatorClickOptions { NoWaitAfter = true });

            // Give the API response and UI render more headroom in CI.
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 60000 });

            // Wait for either success result or error result - using proper selectors
            try
            {
                await page.WaitForSelectorAsync("text=Located in", new PageWaitForSelectorOptions { Timeout = 30000 });
            }
            catch
            {
                // If "Located in" doesn't appear, check for error messages
                await page.WaitForSelectorAsync(".validation-summary-errors", new PageWaitForSelectorOptions { Timeout = 30000 });
            }
        }

        public ILocator AddressDataField => page.Locator("#AddressData");
        public ILocator SearchButton => page.Locator("#search");
        public ILocator ValidationErrors => page.Locator(".validation-summary-errors");

        // Updated selectors to use text-based locators since the specific IDs don't exist
        public ILocator ResultLocationText => page.Locator("text=Located in");
        public ILocator ResultFoundText => page.Locator("text=Found");
        public ILocator GeographicLocationText => page.Locator("text=Geographic Location");

        // Method to check if results are visible
        public async Task<bool> AreResultsVisibleAsync()
        {
            try
            {
                // Check if we can find any of the result indicators
                var foundText = await ResultFoundText.IsVisibleAsync();
                var locatedText = await ResultLocationText.IsVisibleAsync();
                return foundText || locatedText;
            }
            catch
            {
                return false;
            }
        }

        // Method to get the location text from results
        public async Task<string> GetLocationResultAsync()
        {
            try
            {
                // First try to find the "Located in" text which contains the location
                var locatedInElement = page.Locator("text=Located in");
                if (await locatedInElement.CountAsync() > 0)
                {
                    var text = await locatedInElement.InnerTextAsync();
                    // Extract location from "Located in Geneva, Switzerland" format
                    if (text.StartsWith("Located in "))
                    {
                        return text.Substring("Located in ".Length).Trim();
                    }
                }

                // Fallback: Look for text that contains the location (like "Geneva, Switzerland")
                var geographicElement = page.Locator("text=Geographic Location").Locator("..").Locator("text=/[A-Za-z]+,\\s*[A-Za-z]+/");
                if (await geographicElement.CountAsync() > 0)
                {
                    return await geographicElement.InnerTextAsync();
                }

                return "";
            }
            catch
            {
                return "";
            }
        }

        private string GetBaseUrl()
        {
            var url = configuration["SiteUrl"] ?? "https://dev.geo-location.net";
            return url.EndsWith("/") ? url[..^1] : url;
        }
    }
}
