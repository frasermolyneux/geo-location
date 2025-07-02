using MX.GeoLocation.PublicWebApp.IntegrationTests.PageObject.PageParts;

namespace MX.GeoLocation.PublicWebApp.IntegrationTests
{
    internal interface IPage
    {
        public NavigationBar Navigation { get; }

        public void GoToPage(bool useNavigation = false);
        public bool IsOnPage { get; }
    }
}
