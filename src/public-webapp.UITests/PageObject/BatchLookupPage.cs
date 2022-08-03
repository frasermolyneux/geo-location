
using OpenQA.Selenium;

namespace MX.GeoLocation.PublicWebApp.UITests.PageObject
{
    internal class BatchLookupPage
    {
        private readonly IWebDriver driver;

        public BatchLookupPage(IWebDriver driver)
        {
            this.driver = driver;
        }
    }
}
