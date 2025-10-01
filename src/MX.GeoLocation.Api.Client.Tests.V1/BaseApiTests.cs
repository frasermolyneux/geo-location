using Microsoft.Extensions.Logging;
using MX.GeoLocation.Api.Client.V1;

using MX.Api.Client;
using MX.Api.Client.Auth;

namespace MX.GeoLocation.Api.Client.Tests.V1
{
    internal class BaseApiTests
    {
        private ILogger<BaseApi<GeoLocationApiClientOptions>> fakeLogger = null!;
        private IApiTokenProvider fakeApiTokenProvider = null!;
        private IRestClientService fakeRestClientService = null!;

        private GeoLocationApiClientOptions validGeoLocationApiClientOptions => new GeoLocationApiClientOptions()
        {
            BaseUrl = "https://google.co.uk"
        };

        [SetUp]
        public void SetUp()
        {
            fakeLogger = A.Fake<ILogger<BaseApi<GeoLocationApiClientOptions>>>();
            fakeApiTokenProvider = A.Fake<IApiTokenProvider>();
            fakeRestClientService = A.Fake<IRestClientService>();
        }

        [Test]
        public void GeoLookupApiShouldBeCreatedSuccessfully()
        {
            // Act
            var geoLookupApi = new GeoLookupApi(fakeLogger, fakeApiTokenProvider, fakeRestClientService, validGeoLocationApiClientOptions);

            // Assert
            Assert.That(geoLookupApi, Is.Not.Null);
        }

        [TearDown]
        public void TearDown()
        {
            fakeRestClientService?.Dispose();
        }
    }
}
