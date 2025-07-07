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
        private IOptionsSnapshot<ApiClientOptions> fakeOptionsSnapshot;
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
            fakeOptionsSnapshot = A.Fake<IOptionsSnapshot<ApiClientOptions>>();
            fakeApiTokenProvider = A.Fake<IApiTokenProvider>();
            fakeRestClientService = A.Fake<IRestClientService>();

            A.CallTo(() => fakeOptionsSnapshot.Get(nameof(GeoLocationApiClientOptions))).Returns(validGeoLocationApiClientOptions);
        }

        [Test]
        public void GeoLookupApiShouldBeCreatedSuccessfully()
        {
            // Act
            var geoLookupApi = new GeoLookupApi(fakeLogger, fakeApiTokenProvider, fakeRestClientService, fakeOptionsSnapshot);

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
