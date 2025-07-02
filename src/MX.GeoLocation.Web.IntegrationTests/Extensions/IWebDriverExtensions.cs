using System.Collections.ObjectModel;

using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace MX.GeoLocation.Web.IntegrationTests.Extensions
{
    public static class IWebDriverExtensions
    {
        public static IWebElement FindElementWithWait(this IWebDriver driver, By identifier, int maxAttempts = 3, int delayMs = 1000)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    driver.FindElement(identifier);
                    break;
                }
                catch (Exception)
                {
                    if (attempts > maxAttempts)
                        throw;

                    attempts++;
                    Thread.Sleep(delayMs);
                }
            }

            return driver.FindElement(identifier);
        }

        public static ReadOnlyCollection<IWebElement> FindElementsWithWait(this IWebDriver driver, By identifier, int maxAttempts = 3, int delayMs = 1000)
        {
            int attempts = 0;
            while (true)
            {
                if (attempts > maxAttempts)
                    throw new Exception("Failed to find elements");

                var elements = driver.FindElements(identifier);

                if (elements.Count > 0)
                    break;
                else
                {
                    attempts++;
                    Thread.Sleep(delayMs);
                }
            }

            return driver.FindElements(identifier);
        }

        public static void GoToPage(this IWebDriver driver, string? path = null)
        {
            var url = Environment.GetEnvironmentVariable("SITE_URL") ?? "https://localhost:7201";
            url = url.EndsWith("/") ? url.Substring(0, url.Length - 1) : url;

            if (!string.IsNullOrEmpty(path))
                url = $"{url}/{path}";

            Console.WriteLine($"Navigating to site URL: '{url}'");

            driver.Navigate().GoToUrl(url);

            new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                .Until(d => ((IJavaScriptExecutor)d)
                    .ExecuteScript("return document.readyState")
                    .Equals("complete"));
        }
    }
}
