using System.Collections.ObjectModel;

using OpenQA.Selenium;

namespace MX.GeoLocation.PublicWebApp.UITests.Extensions
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
    }
}
