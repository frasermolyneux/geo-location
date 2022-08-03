using FluentAssertions;

using Microsoft.Edge.SeleniumTools;

using MX.GeoLocation.PublicWebApp.UITests.PageObject;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace MX.GeoLocation.PublicWebApp.UITests
{
    [TestFixture("Chrome")]
    //[TestFixture("Firefox")]
    //[TestFixture("Edge")]
    public class NavigationTests
    {
        private string browser;
        private IWebDriver driver;

        private HomePage homePage;
        private LookupAddressPage lookupAddressPage;

        public NavigationTests(string browser)
        {
            this.browser = browser;
        }

        [SetUp]
        public void Setup()
        {
            try
            {
                // Create the driver for the current browser.
                switch (browser)
                {
                    case "Chrome":
                        driver = new ChromeDriver();
                        break;
                    case "Firefox":
                        driver = new FirefoxDriver();
                        break;
                    case "Edge":
                        driver = new EdgeDriver(
                            new EdgeOptions
                            {
                                UseChromium = true
                            }
                        );
                        break;
                    default:
                        throw new ArgumentException($"'{browser}': Unknown browser");
                }

                // Wait until the page is fully loaded on every page navigation or page reload.
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);

                // Navigate to the site.
                // The site name is stored in the SITE_URL environment variable to make 
                // the tests more flexible.
                string url = Environment.GetEnvironmentVariable("SITE_URL") ?? "https://localhost:7201";
                driver.Navigate().GoToUrl(url + "/");

                homePage = new HomePage(driver);
                lookupAddressPage = new LookupAddressPage(driver);

                // Wait for the page to be completely loaded.
                new WebDriverWait(driver, TimeSpan.FromSeconds(10))
                    .Until(d => ((IJavaScriptExecutor)d)
                        .ExecuteScript("return document.readyState")
                        .Equals("complete"));
            }
            catch (DriverServiceNotFoundException)
            {
                Console.WriteLine("DriverServiceNotFoundException");
            }
            catch (WebDriverException)
            {
                Console.WriteLine("WebDriverException");
                Cleanup();
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            if (driver != null)
            {
                driver.Quit();
            }
        }

        [Test]
        public void CanNavigateToLookupAddressPage()
        {
            homePage.Navigation.ClickNavBarLookupDropdown();
            homePage.Navigation.ClickNavBarLookupAddress();

            lookupAddressPage.IsOnPage.Should().BeTrue();
        }
    }
}