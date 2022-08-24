using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestSharp;

using System.Net;

namespace MX.GeoLocation.GeoLocationApi.Client.Tests
{
    internal class BaseApiTests
    {
        private ILogger fakeLogger;
        private IOptions<GeoLocationApiClientOptions> fakeOptions;
        private IApiTokenProvider fakeApiTokenProvider;
        private IRestClientSingleton fakeRestClientSingleton;

        private GeoLocationApiClientOptions validGeoLocationApiClientOptions => new GeoLocationApiClientOptions
        {
            BaseUrl = "https://google.co.uk",
            ApiKey = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
        };

        [SetUp]
        public void SetUp()
        {
            fakeLogger = A.Fake<ILogger>();
            fakeOptions = A.Fake<IOptions<GeoLocationApiClientOptions>>();
            fakeApiTokenProvider = A.Fake<IApiTokenProvider>();
            fakeRestClientSingleton = A.Fake<IRestClientSingleton>();
        }

        [TestCase(null)]
        [TestCase("")]
        public void BaseApiCtorShouldThrowNullReferenceWhenBaseUrlIsInvalid(string baseUrl)
        {
            // Arrange
            A.CallTo(() => fakeOptions.Value).Returns(new GeoLocationApiClientOptions
            {
                BaseUrl = baseUrl,
                ApiKey = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
            });

            // Act
            Action act = () => new BaseApi(fakeLogger, fakeOptions, fakeApiTokenProvider, fakeRestClientSingleton);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                            .WithMessage("Value cannot be null. (Parameter 'BaseUrl')");
        }

        [TestCase(null)]
        [TestCase("")]
        public void BaseApiCtorShouldThrowNullReferenceWhenApiKeyIsInvalid(string apiKey)
        {
            // Arrange
            A.CallTo(() => fakeOptions.Value).Returns(new GeoLocationApiClientOptions
            {
                BaseUrl = "https://google.co.uk",
                ApiKey = apiKey
            });

            // Act
            Action act = () => new BaseApi(fakeLogger, fakeOptions, fakeApiTokenProvider, fakeRestClientSingleton);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                            .WithMessage("Value cannot be null. (Parameter 'ApiKey')");
        }

        [Test]
        public void BaseApiCtorShouldCreateARestClientWithTheBaseUrlAndDefaultApiPath()
        {
            // Arrange
            A.CallTo(() => fakeOptions.Value).Returns(new GeoLocationApiClientOptions
            {
                BaseUrl = "https://google.co.uk",
                ApiKey = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
            });

            // Act
            var baseApi = new BaseApi(fakeLogger, fakeOptions, fakeApiTokenProvider, fakeRestClientSingleton);

            // Assert
            A.CallTo(() => fakeRestClientSingleton.ConfigureBaseUrl("https://google.co.uk/geolocation")).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void BaseApiCtorShouldCreateARestClientWithTheBaseUrlAndCustomApiPath()
        {
            // Arrange
            A.CallTo(() => fakeOptions.Value).Returns(new GeoLocationApiClientOptions
            {
                BaseUrl = "https://google.co.uk",
                ApiKey = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                ApiPathPrefix = "custom"
            });

            // Act
            var baseApi = new BaseApi(fakeLogger, fakeOptions, fakeApiTokenProvider, fakeRestClientSingleton);

            // Assert
            A.CallTo(() => fakeRestClientSingleton.ConfigureBaseUrl("https://google.co.uk/custom")).MustHaveHappenedOnceExactly();
        }

        [TestCase("custom/path/to/resource", Method.Get)]
        [TestCase("custom/path/to/resource/1234567890", Method.Post)]
        [TestCase("custom/path/to/resource/0987654321", Method.Put)]
        [TestCase("custom/path/to/resource/0192837465", Method.Patch)]
        public async Task CreateRequestShouldReturnAPopulatedRestRequest(string resource, Method method)
        {
            // Arrange
            A.CallTo(() => fakeOptions.Value).Returns(validGeoLocationApiClientOptions);
            A.CallTo(() => fakeApiTokenProvider.GetAccessToken()).Returns("mytoken");
            var baseApi = new BaseApi(fakeLogger, fakeOptions, fakeApiTokenProvider, fakeRestClientSingleton);

            // Act
            var result = await baseApi.CreateRequest(resource, method);

            // Assert
            result.Should().NotBeNull();
            result.Resource.Should().Be(resource);
            result.Method.Should().Be(method);

            result.Parameters.Should().Contain(new HeaderParameter("Ocp-Apim-Subscription-Key", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"));
            result.Parameters.Should().Contain(new HeaderParameter("Authorization", "Bearer mytoken"));
        }

        [Test]
        public async Task ExecuteAsyncShouldLogAndRethrowErrorWhenResponseContainsOne()
        {
            // Arrange
            A.CallTo(() => fakeOptions.Value).Returns(validGeoLocationApiClientOptions);
            var baseApi = new BaseApi(fakeLogger, fakeOptions, fakeApiTokenProvider, fakeRestClientSingleton);

            RestResponse restResponse = new()
            {
                ErrorException = new Exception("Test Exception")
            };

            A.CallTo(() => fakeRestClientSingleton.ExecuteAsync(A<RestRequest>.Ignored, default(CancellationToken)))
                .Returns(Task.FromResult(restResponse));

            var restRequest = new RestRequest("path/to/resource", Method.Get);

            // Act
            Func<Task> act = async () => await baseApi.ExecuteAsync(restRequest);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Test Exception");
        }

        [TestCase(HttpStatusCode.OK)]
        [TestCase(HttpStatusCode.NotFound)]
        public async Task ExecuteAsyncShouldPassthroughResponseForCertainStatusCodes(HttpStatusCode httpStatusCode)
        {
            // Arrange
            A.CallTo(() => fakeOptions.Value).Returns(validGeoLocationApiClientOptions);
            var baseApi = new BaseApi(fakeLogger, fakeOptions, fakeApiTokenProvider, fakeRestClientSingleton);

            RestResponse restResponse = new()
            {
                StatusCode = httpStatusCode
            };

            A.CallTo(() => fakeRestClientSingleton.ExecuteAsync(A<RestRequest>.Ignored, default(CancellationToken)))
                .Returns(Task.FromResult(restResponse));

            var restRequest = new RestRequest("path/to/resource", Method.Get);

            // Act
            var result = await baseApi.ExecuteAsync(restRequest);

            // Assert
            result.Should().Be(restResponse);
        }

        [TestCase(HttpStatusCode.ServiceUnavailable)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public async Task ExecuteAsyncShouldThrowExceptionForCertainStatusCodes(HttpStatusCode httpStatusCode)
        {
            // Arrange
            A.CallTo(() => fakeOptions.Value).Returns(validGeoLocationApiClientOptions);
            var baseApi = new BaseApi(fakeLogger, fakeOptions, fakeApiTokenProvider, fakeRestClientSingleton);

            RestResponse restResponse = new()
            {
                StatusCode = httpStatusCode
            };

            A.CallTo(() => fakeRestClientSingleton.ExecuteAsync(A<RestRequest>.Ignored, default(CancellationToken)))
                .Returns(Task.FromResult(restResponse));

            var restRequest = new RestRequest("path/to/resource", Method.Get);

            // Act
            Func<Task> act = async () => await baseApi.ExecuteAsync(restRequest);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage($"Failed GET to 'path/to/resource' with code '{httpStatusCode}'");
        }
    }
}
