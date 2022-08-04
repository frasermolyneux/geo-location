using FluentAssertions;

namespace MX.GeoLocation.PublicWebApp.UITests
{
    [TestFixture("Chrome")]
    internal class LookupAddressTests : TestBase
    {
        public LookupAddressTests(string browser) : base(browser)
        {
        }

        [TestCase("209.163.116.89", "New York, United States")]
        [TestCase("81.174.169.80", "Chesterfield, United Kingdom")]
        [TestCase("35.6.5.8", "US")]
        [TestCase("1.1.1.1", "Unknown")]
        [TestCase("8.8.8.8", "US")]
        public void CanPerformLookupOfWellKnownAddress(string address, string expectedLocation)
        {
            PageFactory.LookupAddressPage.GoToPage(true);
            PageFactory.LookupAddressPage.IsOnPage.Should().BeTrue();

            PageFactory.LookupAddressPage.ExecuteLookupAddressFlow(address);

            PageFactory.LookupAddressPage.ResultAddressData.Text.Should().Be($"You searched for {address}");
            PageFactory.LookupAddressPage.ResultLocationSummary.Text.Should().Be($"The address is located in {expectedLocation}");
        }
    }
}
