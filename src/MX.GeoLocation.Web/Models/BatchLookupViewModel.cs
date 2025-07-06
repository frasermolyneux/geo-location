using System.ComponentModel.DataAnnotations;

using MX.GeoLocation.Abstractions.Models.V1;
using MX.Api.Abstractions;

namespace MX.GeoLocation.Web.Models
{
    public class BatchLookupViewModel
    {
        [MaxLength(1024, ErrorMessage = "The inputted address data is greater than the maximum length")]
        [DataType(DataType.MultilineText)]
        public string? AddressData { get; set; }

        public CollectionModel<GeoLocationDto>? GeoLocationCollectionDto { get; set; }
    }
}