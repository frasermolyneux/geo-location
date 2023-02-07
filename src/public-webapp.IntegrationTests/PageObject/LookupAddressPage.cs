using MX.GeoLocation.PublicWebApp.IntegrationTests.Extensions;
using MX.GeoLocation.PublicWebApp.IntegrationTests.PageObject.PageParts;

using OpenQA.Selenium;

namespace MX.GeoLocation.PublicWebApp.IntegrationTests.PageObject
{
    internal class LookupAddressPage : IPage
    {
        private readonly IWebDriver driver;

        public LookupAddressPage(IWebDriver driver)
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
                    return pageTitle.Text == "Lookup Address";
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
                Navigation.ClickNavBarLookupAddress();
            }
            else
            {
                driver.GoToPage("Home/LookupAddress");
            }
        }

        public void ExecuteLookupAddressFlow(string address)
        {
            AddressDataField.SendKeys(address);
            SearchButton.ClickElement(driver);
        }

        public IWebElement AddressDataField
        {
            get
            {
                return driver.FindElementWithWait(By.Id("AddressData"));
            }
        }

        public IWebElement SearchButton
        {
            get
            {
                return driver.FindElementWithWait(By.Id("search"));
            }
        }

        public IWebElement ValidationErrors
        {
            get
            {
                return driver.FindElementWithWait(By.ClassName("validation-summary-errors"));
            }
        }

        public IWebElement ResultAddressData
        {
            get
            {
                return driver.FindElementWithWait(By.Id("resultAddressData"));
            }
        }

        public IWebElement ResultLocationSummary
        {
            get
            {
                return driver.FindElementWithWait(By.Id("resultLocationSummary"));
            }
        }
    }
}
