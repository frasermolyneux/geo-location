using Microsoft.Playwright;

namespace MX.GeoLocation.Web.IntegrationTests.PageObject.PageParts
{
    public class NavigationBar
    {
        private readonly Microsoft.Playwright.IPage page;

        public NavigationBar(Microsoft.Playwright.IPage page)
        {
            this.page = page;
        }

        public ILocator NavBarHome => page.Locator("#navBarHome");
        public ILocator NavBarLookupDropdown => page.Locator("#navBarLookupDropdown");
        public ILocator NavBarLookupAddress => page.Locator("#navBarLookupAddress");
        public ILocator NavBarLookupBatch => page.Locator("#navBarLookupBatch");
        public ILocator NavBarPrivacyDropdown => page.Locator("#navBarPrivacyDropdown");
        public ILocator NavBarPrivacyPolicy => page.Locator("#navBarPrivacyPolicy");
        public ILocator NavBarPrivacyRemoveMyData => page.Locator("#navBarPrivacyRemoveMyData");

        public async Task ClickNavBarHomeAsync()
        {
            await NavBarHome.ClickAsync();
        }

        public async Task ClickNavBarLookupDropdownAsync()
        {
            await NavBarLookupDropdown.ClickAsync();
        }

        public async Task ClickNavBarLookupAddressAsync()
        {
            await NavBarLookupAddress.ClickAsync();
        }

        public async Task ClickNavBarLookupBatchAsync()
        {
            await NavBarLookupBatch.ClickAsync();
        }

        public async Task ClickNavBarPrivacyDropdownAsync()
        {
            await NavBarPrivacyDropdown.ClickAsync();
        }

        public async Task ClickNavBarPrivacyPolicyAsync()
        {
            await NavBarPrivacyPolicy.ClickAsync();
        }

        public async Task ClickNavBarPrivacyRemoveMyDataAsync()
        {
            await NavBarPrivacyRemoveMyData.ClickAsync();
        }
    }
}
