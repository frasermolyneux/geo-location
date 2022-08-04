using MX.GeoLocation.PublicWebApp.UITests.PageObject.Components;

namespace MX.GeoLocation.PublicWebApp.UITests
{
    internal interface IPage
    {
        public NavigationBar Navigation { get; }

        public void GoToPage(bool useNavigation = false);
        public bool IsOnPage { get; }
    }
}
