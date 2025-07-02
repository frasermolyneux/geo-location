using System.ComponentModel.DataAnnotations;

using MX.GeoLocation.LookupApi.Abstractions.Models;

namespace MX.GeoLocation.PublicWebApp.Models
{
    public class LookupAddressViewModel
    {
        [MaxLength(256, ErrorMessage = "The inputted address data is greater than the maximum length")]
        [DataType(DataType.Text)]
        public string? AddressData { get; set; }

        public GeoLocationDto? GeoLocationDto { get; set; }
    }
}