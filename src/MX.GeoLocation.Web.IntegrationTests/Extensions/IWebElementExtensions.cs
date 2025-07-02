using OpenQA.Selenium;

namespace MX.GeoLocation.Web.IntegrationTests.Extensions
{
    public static class IWebElementExtensions
    {
        public static void ClickElement(this IWebElement element, IWebDriver driver)
        {
            // We expect the driver to implement IJavaScriptExecutor.
            // IJavaScriptExecutor enables us to execute JavaScript code during the tests.
            IJavaScriptExecutor? js = driver as IJavaScriptExecutor;

            // Through JavaScript, run the click() method on the underlying HTML object.
            js?.ExecuteScript("arguments[0].click();", element);
        }
    }
}
