using System.ComponentModel.DataAnnotations;

using MX.GeoLocation.LookupApi.Abstractions.Models;

namespace MX.GeoLocation.PublicWebApp.Models
{
    public class BatchLookupViewModel
    {
        [MaxLength(1024, ErrorMessage = "The inputted address data is greater than the maximum length")]
        [DataType(DataType.MultilineText)]
        public string? AddressData { get; set; }

        public GeoLocationCollectionDto? GeoLocationCollectionDto { get; set; }
    }
}