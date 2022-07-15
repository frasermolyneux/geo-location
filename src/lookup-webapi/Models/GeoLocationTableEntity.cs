using Azure;
using Azure.Data.Tables;

using MX.GeoLocation.LookupApi.Abstractions.Models;

using Newtonsoft.Json;

namespace MX.GeoLocation.LookupWebApi.Models
{
    public class GeoLocationTableEntity : GeoLocationDto, ITableEntity
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable. // Required for Table Client
        public GeoLocationTableEntity()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {

        }

        public GeoLocationTableEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public GeoLocationTableEntity(GeoLocationDto geoLocationDto)
        {
            PartitionKey = "addresses";
            RowKey = geoLocationDto.Address ?? throw new NullReferenceException(nameof(geoLocationDto.Address));

            Address = geoLocationDto.Address;
            TranslatedAddress = geoLocationDto.TranslatedAddress;
            ContinentCode = geoLocationDto.ContinentCode;
            ContinentName = geoLocationDto.ContinentName;
            CountryCode = geoLocationDto.CountryCode;
            CountryName = geoLocationDto.CountryName;
            IsEuropeanUnion = geoLocationDto.IsEuropeanUnion;
            CityName = geoLocationDto.CityName;
            PostalCode = geoLocationDto.PostalCode;
            RegisteredCountry = geoLocationDto.RegisteredCountry;
            Latitude = geoLocationDto.Latitude;
            Longitude = geoLocationDto.Longitude;
            AccuracyRadius = geoLocationDto.AccuracyRadius;
            Timezone = geoLocationDto.Timezone;

            TraitsSerialised = JsonConvert.SerializeObject(geoLocationDto.Traits);
        }

        public new Dictionary<string, string> Traits
        {
            get
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(TraitsSerialised) ?? new Dictionary<string, string>();
            }
            private set { } // Private set will prevent persistence to table storage
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string TraitsSerialised { get; set; }
    }
}
