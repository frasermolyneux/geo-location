using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace MX.GeoLocation.Web.IntegrationTests
{
    public abstract class TestBase : IAsyncLifetime
    {
        private IPlaywright? _playwright;
        private IBrowser? _browser;
        protected Microsoft.Playwright.IPage? Page { get; private set; }
        protected PageFactory? PageFactory { get; private set; }
        protected IConfiguration Configuration { get; private set; }

        protected TestBase()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public async Task InitializeAsync()
        {
            _playwright = await Playwright.CreateAsync();

            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            Page = await _browser.NewPageAsync();
            PageFactory = new PageFactory(Page, Configuration);

            // Navigate to the home page initially
            await PageFactory.HomePage.GoToPageAsync();
        }

        public async Task DisposeAsync()
        {
            if (Page != null)
                await Page.CloseAsync();

            if (_browser != null)
                await _browser.CloseAsync();

            _playwright?.Dispose();
        }
    }
}
