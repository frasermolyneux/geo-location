using FluentAssertions;

namespace MX.GeoLocation.PublicWebApp.UITests
{
    [TestFixture("Chrome")]
    internal class LookupAddressTests : TestBase
    {
        public LookupAddressTests(string browser) : base(browser)
        {
        }

        [TestCase("13.64.69.151", "San Jose, United States")]
        [TestCase("51.107.144.68", "Geneva, Switzerland")]
        [TestCase("2603:1040:1302::580", "Taipei, Taiwan")]
        [TestCase("20.21.82.128", "Doha, Qatar")]
        [TestCase("40.78.195.16", "Chennai, India")]
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
