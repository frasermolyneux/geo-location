using System.ComponentModel.DataAnnotations;

using MX.GeoLocation.Abstractions.Models.V1_1;

namespace MX.GeoLocation.Web.Models
{
    public class IntelligenceLookupViewModel
    {
        [MaxLength(256, ErrorMessage = "The inputted address data is greater than the maximum length")]
        [DataType(DataType.Text)]
        public string? AddressData { get; set; }

        public IpIntelligenceDto? Intelligence { get; set; }
    }
}
