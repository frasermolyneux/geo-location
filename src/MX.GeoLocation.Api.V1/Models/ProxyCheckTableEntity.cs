using Azure;
using Azure.Data.Tables;

using MX.GeoLocation.Abstractions.Models.V1_1;

namespace MX.GeoLocation.LookupWebApi.Models
{
    public class ProxyCheckTableEntity : ITableEntity
    {
#pragma warning disable CS8618
        public ProxyCheckTableEntity()
#pragma warning restore CS8618
        {
        }

        public ProxyCheckTableEntity(ProxyCheckDto dto)
        {
            PartitionKey = "addresses";
            RowKey = dto.TranslatedAddress;
            Address = dto.Address;
            TranslatedAddress = dto.TranslatedAddress;
            RiskScore = dto.RiskScore;
            IsProxy = dto.IsProxy;
            IsVpn = dto.IsVpn;
            ProxyType = dto.ProxyType;
            Country = dto.Country;
            Region = dto.Region;
            AsNumber = dto.AsNumber;
            AsOrganization = dto.AsOrganization;
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string? Address { get; set; }
        public string? TranslatedAddress { get; set; }
        public int RiskScore { get; set; }
        public bool IsProxy { get; set; }
        public bool IsVpn { get; set; }
        public string? ProxyType { get; set; }
        public string? Country { get; set; }
        public string? Region { get; set; }
        public string? AsNumber { get; set; }
        public string? AsOrganization { get; set; }

        public ProxyCheckDto ToDto()
        {
            return new ProxyCheckDto
            {
                Address = Address ?? string.Empty,
                TranslatedAddress = TranslatedAddress ?? string.Empty,
                RiskScore = RiskScore,
                IsProxy = IsProxy,
                IsVpn = IsVpn,
                ProxyType = ProxyType ?? string.Empty,
                Country = Country ?? string.Empty,
                Region = Region ?? string.Empty,
                AsNumber = AsNumber ?? string.Empty,
                AsOrganization = AsOrganization ?? string.Empty
            };
        }
    }
}
