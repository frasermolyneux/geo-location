using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace MX.GeoLocation.Web.IntegrationTests
{
    public abstract class TestBase : IAsyncLifetime
    {
        private IPlaywright? _playwright;
        private IBrowser? _browser;
        private WebAppFactory? _webAppFactory;
        protected Microsoft.Playwright.IPage? Page { get; private set; }
        protected PageFactory? PageFactory { get; private set; }
        protected IConfiguration Configuration { get; private set; }

        protected TestBase()
        {
            Configuration = new ConfigurationBuilder().Build();
        }

        public async Task InitializeAsync()
        {
            // Start the web app on a random local port with mocked dependencies
            _webAppFactory = new WebAppFactory();
            await _webAppFactory.StartAsync();

            // Update configuration to point at the locally-hosted app
            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["SiteUrl"] = _webAppFactory.BaseUrl
                })
                .Build();

            _playwright = await Playwright.CreateAsync();

            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            Page = await _browser.NewPageAsync();
            // Slow CI environments can take longer to navigate/render.
            Page.SetDefaultTimeout(60000);
            Page.SetDefaultNavigationTimeout(60000);
            PageFactory = new PageFactory(Page, Configuration);

            // Navigate to the home page initially
            await PageFactory.HomePage.GoToPageAsync();
        }

        public async Task DisposeAsync()
        {
            if (Page is not null)
                await Page.CloseAsync();

            if (_browser is not null)
                await _browser.CloseAsync();

            _playwright?.Dispose();

            if (_webAppFactory is not null)
                await _webAppFactory.DisposeAsync();
        }
    }
}
