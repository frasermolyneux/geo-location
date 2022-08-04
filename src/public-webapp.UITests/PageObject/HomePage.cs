
using MX.GeoLocation.PublicWebApp.UITests.Extensions;
using MX.GeoLocation.PublicWebApp.UITests.PageObject.Components;

using OpenQA.Selenium;

namespace MX.GeoLocation.PublicWebApp.UITests.PageObject
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
