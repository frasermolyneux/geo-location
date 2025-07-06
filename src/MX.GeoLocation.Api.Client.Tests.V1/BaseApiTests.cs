using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MX.GeoLocation.Api.Client.V1;

using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.Api.Client.Configuration;

namespace MX.GeoLocation.Api.Client.Tests.V1
{
    internal class BaseApiTests
    {
        private ILogger<GeoLookupApi> fakeLogger;
        private IOptions<GeoLocationApiClientOptions> fakeOptions;
        private IApiTokenProvider fakeApiTokenProvider;
        private IRestClientService fakeRestClientService;

        private GeoLocationApiClientOptions validGeoLocationApiClientOptions => new GeoLocationApiClientOptions()
        {
            BaseUrl = "https://google.co.uk"
        };

        [SetUp]
        public void SetUp()
        {
            fakeLogger = A.Fake<ILogger<GeoLookupApi>>();
            fakeOptions = A.Fake<IOptions<GeoLocationApiClientOptions>>();
            fakeApiTokenProvider = A.Fake<IApiTokenProvider>();
            fakeRestClientService = A.Fake<IRestClientService>();

            A.CallTo(() => fakeOptions.Value).Returns(validGeoLocationApiClientOptions);
        }

        [Test]
        public void GeoLookupApiShouldBeCreatedSuccessfully()
        {
            // Act
            var geoLookupApi = new GeoLookupApi(fakeLogger, fakeApiTokenProvider, fakeRestClientService, fakeOptions);

            // Assert
            geoLookupApi.Should().NotBeNull();
        }

        [TearDown]
        public void TearDown()
        {
            fakeRestClientService?.Dispose();
        }
    }
}
