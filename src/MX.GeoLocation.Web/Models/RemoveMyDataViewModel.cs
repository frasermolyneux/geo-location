using System.ComponentModel.DataAnnotations;

namespace MX.GeoLocation.Web.Models
{
    public class RemoveMyDataViewModel
    {
        public RemoveMyDataViewModel(string addressData)
        {
            AddressData = addressData;
        }

        [MaxLength(256, ErrorMessage = "The inputted address data is greater than the maximum length")]
        [DataType(DataType.Text)]
        public string AddressData { get; set; }

        public bool Removed { get; set; }
    }
}