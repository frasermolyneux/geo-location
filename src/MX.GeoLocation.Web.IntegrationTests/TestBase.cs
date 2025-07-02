using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using System.Runtime.InteropServices;

namespace MX.GeoLocation.PublicWebApp.IntegrationTests
{
    internal class TestBase
    {
        private IWebDriver driver;

        public PageFactory PageFactory { get; }

        public TestBase(string browser)
        {
            switch (browser)
            {
                case "Chrome":
                    var options = new ChromeOptions();
                    options.AddArgument("--headless=new");
                    driver = new ChromeDriver(options);
                    break;
                case "Firefox":
                    driver = new FirefoxDriver();
                    break;
                case "Edge":
                    driver = new EdgeDriver();
                    break;
                default:
                    throw new ArgumentException($"'{browser}': Unknown browser");
            }

            // Wait until the page is fully loaded on every page navigation or page reload.
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);

            PageFactory = new PageFactory(driver);
        }

        [SetUp]
        public async Task Setup()
        {
            await WarmUp();

            PageFactory.HomePage.GoToPage();
        }

        private async Task WarmUp()
        {
            var url = Environment.GetEnvironmentVariable("SITE_URL") ?? "https://localhost:7201";
            url = url.EndsWith("/") ? url.Substring(0, url.Length - 1) : url;

            using (HttpClient client = new HttpClient() { BaseAddress = new Uri(url)})
            {
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        Console.WriteLine($"Performing warmup request to {url}");
                        await client.GetAsync("/");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error performing warmup request");
                        Console.WriteLine(ex);

                        // Sleep for five seconds before trying again.
                        Thread.Sleep(5000);
                    }
                }
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            if (driver != null)
            {
                driver.Quit();
            }
            driver?.Dispose();
        }
    }
}
