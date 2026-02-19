using Microsoft.Extensions.Logging;
using MX.GeoLocation.Api.Client.V1;

using MX.Api.Client;
using MX.Api.Client.Auth;

namespace MX.GeoLocation.Api.Client.Tests.V1
{
    [Trait("Category", "Unit")]
    public class BaseApiTests : IDisposable
    {
        private readonly ILogger<BaseApi<GeoLocationApiClientOptions>> fakeLogger;
        private readonly IApiTokenProvider fakeApiTokenProvider;
        private readonly Mock<IRestClientService> mockRestClientService;

        private GeoLocationApiClientOptions validGeoLocationApiClientOptions => new GeoLocationApiClientOptions()
        {
            BaseUrl = "https://google.co.uk"
        };

        public BaseApiTests()
        {
            fakeLogger = Mock.Of<ILogger<BaseApi<GeoLocationApiClientOptions>>>();
            fakeApiTokenProvider = Mock.Of<IApiTokenProvider>();
            mockRestClientService = new Mock<IRestClientService>();
        }

        [Fact]
        public void GeoLookupApiShouldBeCreatedSuccessfully()
        {
            // Act
            var geoLookupApi = new GeoLookupApi(fakeLogger, fakeApiTokenProvider, mockRestClientService.Object, validGeoLocationApiClientOptions);

            // Assert
            Assert.NotNull(geoLookupApi);
        }

        public void Dispose()
        {
            mockRestClientService.Object?.Dispose();
        }
    }
}
