namespace MX.GeoLocation.Web.IntegrationTests
{
    public class LookupAddressTests : TestBase
    {
        [Theory]
        [InlineData("13.64.69.151", "San Jose, United States")]
        [InlineData("51.107.144.68", "Geneva, Switzerland")]
        [InlineData("2603:1040:1302::580", "Taipei, Taiwan")]
        [InlineData("20.21.82.128", "Doha, Qatar")]
        [InlineData("40.78.195.16", "Chennai, India")]
        public async Task CanPerformLookupOfWellKnownAddress(string address, string expectedLocation)
        {
            await PageFactory!.LookupAddressPage.GoToPageAsync(true);
            var isOnPage = await PageFactory.LookupAddressPage.IsOnPageAsync();
            Assert.True(isOnPage);

            await PageFactory.LookupAddressPage.ExecuteLookupAddressFlowAsync(address);

            // Wait for results to be visible
            var resultsVisible = await PageFactory.LookupAddressPage.AreResultsVisibleAsync();
            Assert.True(resultsVisible, "Results should be visible after address lookup");

            // Get the location from the results
            var resultLocation = await PageFactory.LookupAddressPage.GetLocationResultAsync();
            Assert.Contains(expectedLocation, resultLocation, StringComparison.OrdinalIgnoreCase);
        }
    }
}
