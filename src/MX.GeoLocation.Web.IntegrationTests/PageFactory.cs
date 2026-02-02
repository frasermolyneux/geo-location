using MX.GeoLocation.Web.IntegrationTests.PageObject;
using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;

namespace MX.GeoLocation.Web.IntegrationTests
{
    public class PageFactory
    {
        private readonly Microsoft.Playwright.IPage page;
        private readonly IConfiguration configuration;

        private HomePage? homePage;
        private LookupAddressPage? lookupAddressPage;
        private BatchLookupPage? batchLookupPage;
        private PrivacyPage? privacyPage;
        private RemoveDataPage? removeMyDataPage;

        public PageFactory(Microsoft.Playwright.IPage page, IConfiguration configuration)
        {
            this.page = page;
            this.configuration = configuration;
        }

        public HomePage HomePage
        {
            get
            {
                homePage ??= new HomePage(page, configuration);
                return homePage;
            }
        }

        public LookupAddressPage LookupAddressPage
        {
            get
            {
                lookupAddressPage ??= new LookupAddressPage(page, configuration);
                return lookupAddressPage;
            }
        }

        public BatchLookupPage BatchLookupPage
        {
            get
            {
                batchLookupPage ??= new BatchLookupPage(page, configuration);
                return batchLookupPage;
            }
        }

        public PrivacyPage PrivacyPage
        {
            get
            {
                privacyPage ??= new PrivacyPage(page, configuration);
                return privacyPage;
            }
        }

        public RemoveDataPage RemoveMyDataPage
        {
            get
            {
                removeMyDataPage ??= new RemoveDataPage(page, configuration);
                return removeMyDataPage;
            }
        }
    }
}
