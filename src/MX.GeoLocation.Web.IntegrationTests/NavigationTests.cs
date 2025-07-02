using FluentAssertions;

namespace MX.GeoLocation.Web.IntegrationTests
{
    [TestFixture("Chrome")]
    //[TestFixture("Firefox")]
    //[TestFixture("Edge")]
    internal class NavigationTests : TestBase
    {
        public NavigationTests(string browser) : base(browser)
        {
        }

        [Test]
        public void CanNavigateToLookupAddressPage()
        {
            PageFactory.LookupAddressPage.GoToPage(true);
            PageFactory.LookupAddressPage.IsOnPage.Should().BeTrue();
        }

        [Test]
        public void CanNavigateToBatchLookupPage()
        {
            PageFactory.BatchLookupPage.GoToPage(true);
            PageFactory.BatchLookupPage.IsOnPage.Should().BeTrue();
        }

        [Test]
        public void CanNavigateToPrivacyPolicyPage()
        {
            PageFactory.PrivacyPage.GoToPage(true);
            PageFactory.PrivacyPage.IsOnPage.Should().BeTrue();
        }

        [Test]
        public void CanNavigateToRemoveMyDataPage()
        {
            PageFactory.RemoveMyDataPage.GoToPage(true);
            PageFactory.RemoveMyDataPage.IsOnPage.Should().BeTrue();
        }

        [Test]
        public void CanNavigateToHomePage()
        {
            PageFactory.HomePage.GoToPage(true);
            PageFactory.HomePage.IsOnPage.Should().BeTrue();
        }
    }
}