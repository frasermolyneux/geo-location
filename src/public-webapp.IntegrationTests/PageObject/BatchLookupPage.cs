using MX.GeoLocation.PublicWebApp.IntegrationTests.Extensions;
using MX.GeoLocation.PublicWebApp.IntegrationTests.PageObject.PageParts;

using OpenQA.Selenium;

namespace MX.GeoLocation.PublicWebApp.IntegrationTests.PageObject
{
    internal class BatchLookupPage : IPage
    {
        private readonly IWebDriver driver;

        public BatchLookupPage(IWebDriver driver)
        {
            this.driver = driver;
            Navigation = new NavigationBar(driver);
        }

        public NavigationBar Navigation { get; private set; }

        public bool IsOnPage
        {
            get
            {
                try
                {
                    var pageTitle = driver.FindElementWithWait(By.Id("pageTitle"));
                    return pageTitle.Text == "Batch Lookup";
                }
                catch
                {
                    return false;
                }
            }
        }

        public void GoToPage(bool useNavigation = false)
        {
            if (useNavigation)
            {
                Navigation.ClickNavBarLookupDropdown();
                Navigation.ClickNavBarLookupBatch();
            }
            else
            {
                driver.GoToPage("Home/BatchLookup");
            }
        }
    }
}
