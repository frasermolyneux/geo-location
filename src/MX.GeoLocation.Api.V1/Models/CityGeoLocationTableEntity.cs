using Azure;
using Azure.Data.Tables;

using MX.GeoLocation.Abstractions.Models.V1_1;

using Newtonsoft.Json;

namespace MX.GeoLocation.LookupWebApi.Models
{
    public class CityGeoLocationTableEntity : ITableEntity
    {
#pragma warning disable CS8618
        public CityGeoLocationTableEntity()
#pragma warning restore CS8618
        {
        }

        public CityGeoLocationTableEntity(CityGeoLocationDto dto)
        {
            PartitionKey = "addresses";
            RowKey = dto.TranslatedAddress ?? throw new ArgumentNullException(nameof(dto.TranslatedAddress));

            Address = dto.Address;
            TranslatedAddress = dto.TranslatedAddress;
            ContinentCode = dto.ContinentCode;
            ContinentName = dto.ContinentName;
            CountryCode = dto.CountryCode;
            CountryName = dto.CountryName;
            IsEuropeanUnion = dto.IsEuropeanUnion;
            CityName = dto.CityName;
            PostalCode = dto.PostalCode;
            RegisteredCountry = dto.RegisteredCountry;
            RepresentedCountry = dto.RepresentedCountry;
            Latitude = dto.Latitude;
            Longitude = dto.Longitude;
            AccuracyRadius = dto.AccuracyRadius;
            Timezone = dto.Timezone;

            SubdivisionsSerialised = JsonConvert.SerializeObject(dto.Subdivisions);
            NetworkTraitsSerialised = JsonConvert.SerializeObject(dto.NetworkTraits);
            AnonymizerSerialised = dto is InsightsGeoLocationDto insights
                ? JsonConvert.SerializeObject(insights.Anonymizer)
                : null;
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

        public string? SubdivisionsSerialised { get; set; }
        public string? NetworkTraitsSerialised { get; set; }
        public string? AnonymizerSerialised { get; set; }

        public CityGeoLocationDto ToCityDto()
        {
            return PopulateBase(new CityGeoLocationDto());
        }

        public InsightsGeoLocationDto ToInsightsDto()
        {
            var dto = PopulateBase(new InsightsGeoLocationDto());
            dto.Anonymizer = DeserializeOrDefault<AnonymizerDto>(AnonymizerSerialised);
            return dto;
        }

        public bool HasAnonymizerData => AnonymizerSerialised is not null;

        private T PopulateBase<T>(T dto) where T : CityGeoLocationDto
        {
            dto.Address = Address;
            dto.TranslatedAddress = TranslatedAddress;
            dto.ContinentCode = ContinentCode;
            dto.ContinentName = ContinentName;
            dto.CountryCode = CountryCode;
            dto.CountryName = CountryName;
            dto.IsEuropeanUnion = IsEuropeanUnion;
            dto.CityName = CityName;
            dto.PostalCode = PostalCode;
            dto.RegisteredCountry = RegisteredCountry;
            dto.RepresentedCountry = RepresentedCountry;
            dto.Latitude = Latitude;
            dto.Longitude = Longitude;
            dto.AccuracyRadius = AccuracyRadius;
            dto.Timezone = Timezone;
            dto.Subdivisions = DeserializeList(SubdivisionsSerialised);
            dto.NetworkTraits = DeserializeOrDefault<NetworkTraitsDto>(NetworkTraitsSerialised);
            return dto;
        }

        private static List<string> DeserializeList(string? json)
        {
            if (json is null) return [];
            return JsonConvert.DeserializeObject<List<string>>(json) ?? [];
        }

        private static T DeserializeOrDefault<T>(string? json) where T : new()
        {
            if (json is null) return new T();
            return JsonConvert.DeserializeObject<T>(json) ?? new T();
        }
    }
}
