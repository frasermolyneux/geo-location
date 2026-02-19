namespace MX.GeoLocation.Web.IntegrationTests
{
    [Trait("Category", "Integration")]
    public class NavigationTests : TestBase
    {
        [Fact]
        public async Task CanNavigateToLookupAddressPage()
        {
            await PageFactory!.LookupAddressPage.GoToPageAsync(true);
            var isOnPage = await PageFactory.LookupAddressPage.IsOnPageAsync();
            Assert.True(isOnPage);
        }

        [Fact]
        public async Task CanNavigateToBatchLookupPage()
        {
            await PageFactory!.BatchLookupPage.GoToPageAsync(true);
            var isOnPage = await PageFactory.BatchLookupPage.IsOnPageAsync();
            Assert.True(isOnPage);
        }

        [Fact]
        public async Task CanNavigateToPrivacyPolicyPage()
        {
            await PageFactory!.PrivacyPage.GoToPageAsync(true);
            var isOnPage = await PageFactory.PrivacyPage.IsOnPageAsync();
            Assert.True(isOnPage);
        }

        [Fact]
        public async Task CanNavigateToRemoveMyDataPage()
        {
            await PageFactory!.RemoveMyDataPage.GoToPageAsync(true);
            var isOnPage = await PageFactory.RemoveMyDataPage.IsOnPageAsync();
            Assert.True(isOnPage);
        }

        [Fact]
        public async Task CanNavigateToHomePage()
        {
            await PageFactory!.HomePage.GoToPageAsync(true);
            var isOnPage = await PageFactory.HomePage.IsOnPageAsync();
            Assert.True(isOnPage);
        }
    }
}