using MX.GeoLocation.Web.IntegrationTests.PageObject.PageParts;

namespace MX.GeoLocation.Web.IntegrationTests
{
    internal interface IPage
    {
        public NavigationBar Navigation { get; }

        public void GoToPage(bool useNavigation = false);
        public bool IsOnPage { get; }
    }
}
