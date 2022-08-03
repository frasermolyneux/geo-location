
using MX.GeoLocation.PublicWebApp.UITests.Extensions;

using OpenQA.Selenium;

namespace MX.GeoLocation.PublicWebApp.UITests.PageObject.Components
{
    internal class NavigationComponent
    {
        private readonly IWebDriver driver;

        public NavigationComponent(IWebDriver driver)
        {
            this.driver = driver;
        }

        public IWebElement NavBarHomeControl
        {
            get
            {
                return driver.FindElementWithWait(By.Id("navBarHomeControl"));
            }
        }

        public IWebElement NavBarLookupDropdown
        {
            get
            {
                return driver.FindElementWithWait(By.Id("navbarLookupDropdown"));
            }
        }

        public IWebElement NavBarLookupAddress
        {
            get
            {
                return driver.FindElementWithWait(By.Id("navBarLookupAddress"));
            }
        }

        public IWebElement NavBarLookupBatch
        {
            get
            {
                return driver.FindElementWithWait(By.Id("navBarLookupBatch"));
            }
        }

        public void ClickNavBarHomeControl()
        {
            NavBarHomeControl.ClickElement(driver);
        }

        public void ClickNavBarLookupDropdown()
        {
            NavBarLookupDropdown.ClickElement(driver);
        }

        public void ClickNavBarLookupAddress()
        {
            NavBarLookupAddress.ClickElement(driver);
        }

        public void ClickNavBarLookupBatch()
        {
            NavBarLookupBatch.ClickElement(driver);
        }
    }
}
