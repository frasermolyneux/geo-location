using MX.GeoLocation.PublicWebApp.IntegrationTests.Extensions;
using MX.GeoLocation.PublicWebApp.IntegrationTests.PageObject.PageParts;

using OpenQA.Selenium;

namespace MX.GeoLocation.PublicWebApp.IntegrationTests.PageObject
{
    internal class PrivacyPage : IPage
    {
        private readonly IWebDriver driver;

        public PrivacyPage(IWebDriver driver)
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
                    return pageTitle.Text == "Privacy Policy";
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
                Navigation.ClickNavBarPrivacyPolicy();
            }
            else
            {
                driver.GoToPage("Home/Privacy");
            }
        }
    }
}
