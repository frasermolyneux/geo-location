using MX.GeoLocation.PublicWebApp.IntegrationTests.PageObject;

using OpenQA.Selenium;

namespace MX.GeoLocation.PublicWebApp.IntegrationTests
{
    internal class PageFactory
    {
        private readonly IWebDriver driver;

        private HomePage? homePage;
        private LookupAddressPage? lookupAddressPage;
        private BatchLookupPage? batchLookupPage;
        private PrivacyPage? privacyPage;
        private RemoveDataPage? removeMyDataPage;

        public PageFactory(IWebDriver driver)
        {
            this.driver = driver;
        }

        public HomePage HomePage
        {
            get
            {
                if (homePage == null)
                    homePage = new HomePage(driver);

                return homePage;
            }
        }

        public LookupAddressPage LookupAddressPage
        {
            get
            {
                if (lookupAddressPage == null)
                    lookupAddressPage = new LookupAddressPage(driver);

                return lookupAddressPage;
            }
        }

        public BatchLookupPage BatchLookupPage
        {
            get
            {
                if (batchLookupPage == null)
                    batchLookupPage = new BatchLookupPage(driver);

                return batchLookupPage;
            }
        }

        public PrivacyPage PrivacyPage
        {
            get
            {
                if (privacyPage == null)
                    privacyPage = new PrivacyPage(driver);

                return privacyPage;
            }
        }

        public RemoveDataPage RemoveMyDataPage
        {
            get
            {
                if (removeMyDataPage == null)
                    removeMyDataPage = new RemoveDataPage(driver);

                return removeMyDataPage;
            }
        }
    }
}
