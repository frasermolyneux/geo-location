using MX.GeoLocation.Web.IntegrationTests.Extensions;
using MX.GeoLocation.Web.IntegrationTests.PageObject.PageParts;

using OpenQA.Selenium;

namespace MX.GeoLocation.Web.IntegrationTests.PageObject
{
    internal class HomePage : IPage
    {
        private readonly IWebDriver driver;

        public HomePage(IWebDriver driver)
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
                    return pageTitle.Text.Contains("Welcome - ");
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
                Navigation.ClickNavBarHome();
            }
            else
            {
                driver.GoToPage();
            }
        }
    }
}
