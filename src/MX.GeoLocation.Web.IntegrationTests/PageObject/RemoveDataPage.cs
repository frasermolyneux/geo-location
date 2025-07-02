using MX.GeoLocation.Web.IntegrationTests.Extensions;
using MX.GeoLocation.Web.IntegrationTests.PageObject.PageParts;

using OpenQA.Selenium;

namespace MX.GeoLocation.Web.IntegrationTests.PageObject
{
    internal class RemoveDataPage : IPage
    {
        private readonly IWebDriver driver;

        public RemoveDataPage(IWebDriver driver)
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
                    return pageTitle.Text == "Remove My Data";
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
                Navigation.ClickNavBarPrivacyDropdown();
                Navigation.ClickNavBarPrivacyRemoveMyData();
            }
            else
            {
                driver.GoToPage("Home/RemoveData");
            }
        }
    }
}
