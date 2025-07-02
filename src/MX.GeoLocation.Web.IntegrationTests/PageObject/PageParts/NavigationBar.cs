using MX.GeoLocation.Web.IntegrationTests.Extensions;

using OpenQA.Selenium;

namespace MX.GeoLocation.Web.IntegrationTests.PageObject.PageParts
{
    internal class NavigationBar
    {
        private readonly IWebDriver driver;

        public NavigationBar(IWebDriver driver)
        {
            this.driver = driver;
        }

        public IWebElement NavBarHome
        {
            get
            {
                return driver.FindElementWithWait(By.Id("navBarHome"));
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

        public IWebElement NavBarPrivacyDropdown
        {
            get
            {
                return driver.FindElementWithWait(By.Id("navBarPrivacyDropdown"));
            }
        }

        public IWebElement NavBarPrivacyPolicy
        {
            get
            {
                return driver.FindElementWithWait(By.Id("navBarPrivacyPolicy"));
            }
        }

        public IWebElement NavBarPrivacyRemoveMyData
        {
            get
            {
                return driver.FindElementWithWait(By.Id("navBarPrivacyRemoveMyData"));
            }
        }

        public void ClickNavBarHome()
        {
            NavBarHome.ClickElement(driver);
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

        public void ClickNavBarPrivacyDropdown()
        {
            NavBarPrivacyDropdown.ClickElement(driver);
        }

        public void ClickNavBarPrivacyPolicy()
        {
            NavBarPrivacyPolicy.ClickElement(driver);
        }

        public void ClickNavBarPrivacyRemoveMyData()
        {
            NavBarPrivacyRemoveMyData.ClickElement(driver);
        }
    }
}
