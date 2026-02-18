namespace MX.GeoLocation.Web.IntegrationTests
{
    public class LookupAddressTests : TestBase
    {
        [Theory]
        [InlineData("8.8.8.8", "Mountain View, United States")]
        [InlineData("1.1.1.1", "Los Angeles, United States")]
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
