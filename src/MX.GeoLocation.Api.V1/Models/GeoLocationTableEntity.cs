using System.Runtime.Serialization;

using Azure;
using Azure.Data.Tables;

using MX.GeoLocation.Abstractions.Models.V1;

using Newtonsoft.Json;

namespace MX.GeoLocation.LookupWebApi.Models
{
    public class GeoLocationTableEntity : ITableEntity
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
            RowKey = geoLocationDto.TranslatedAddress ?? throw new NullReferenceException(nameof(geoLocationDto.TranslatedAddress));

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

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string? Address { get; set; }
        public string? TranslatedAddress { get; set; }
        public string? ContinentCode { get; set; }
        public string? ContinentName { get; set; }
        public string? CountryCode { get; set; }
        public string? CountryName { get; set; }
        public bool IsEuropeanUnion { get; set; }
        public string? CityName { get; set; }
        public string? PostalCode { get; set; }
        public string? RegisteredCountry { get; set; }
        public string? RepresentedCountry { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? AccuracyRadius { get; set; }
        public string? Timezone { get; set; }

        public string? TraitsSerialised { get; set; }

        [IgnoreDataMember]
        public Dictionary<string, string?> Traits
        {
            get
            {
                if (TraitsSerialised is not null)
                    return JsonConvert.DeserializeObject<Dictionary<string, string?>>(TraitsSerialised) ?? new();
                return new();
            }
            private set { }
        }

        public GeoLocationDto GeoLocationDto()
        {
            return new GeoLocationDto()
            {
                Address = Address,
                TranslatedAddress = TranslatedAddress,
                ContinentCode = ContinentCode,
                ContinentName = ContinentName,
                CountryCode = CountryCode,
                CountryName = CountryName,
                IsEuropeanUnion = IsEuropeanUnion,
                CityName = CityName,
                PostalCode = PostalCode,
                RegisteredCountry = RegisteredCountry,
                RepresentedCountry = RepresentedCountry,
                Latitude = Latitude,
                Longitude = Longitude,
                AccuracyRadius = AccuracyRadius,
                Timezone = Timezone,
                Traits = Traits
            };
        }
    }
}
