
using MX.GeoLocation.PublicWebApp.UITests.PageObject.Components;

using OpenQA.Selenium;

namespace MX.GeoLocation.PublicWebApp.UITests.PageObject
{
    internal class HomePage
    {
        private readonly IWebDriver driver;

        public NavigationComponent Navigation { get; private set; }

        public HomePage(IWebDriver driver)
        {
            this.driver = driver;

            Navigation = new NavigationComponent(driver);
        }
    }
}
