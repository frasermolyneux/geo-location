using MX.GeoLocation.Web.IntegrationTests.PageObject.PageParts;

namespace MX.GeoLocation.Web.IntegrationTests
{
    public interface IPageObject
    {
        public NavigationBar Navigation { get; }

        public Task GoToPageAsync(bool useNavigation = false);
        public Task<bool> IsOnPageAsync();
    }
}
