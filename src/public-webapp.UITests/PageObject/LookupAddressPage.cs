
using MX.GeoLocation.PublicWebApp.UITests.Extensions;

using OpenQA.Selenium;

namespace MX.GeoLocation.PublicWebApp.UITests.PageObject
{
    internal class LookupAddressPage
    {
        private readonly IWebDriver driver;

        public LookupAddressPage(IWebDriver driver)
        {
            this.driver = driver;
        }

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
    }
}
