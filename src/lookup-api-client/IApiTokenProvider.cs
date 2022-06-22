namespace MX.GeoLocation.GeoLocationApi.Client;

public interface IApiTokenProvider
{
    Task<string> GetAccessToken();
}