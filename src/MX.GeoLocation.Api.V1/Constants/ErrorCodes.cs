namespace MX.GeoLocation.LookupWebApi.Constants
{
    /// <summary>
    /// Defines standardized error codes for the GeoLocation API.
    /// Error codes follow the pattern: CATEGORY_SPECIFIC_DESCRIPTION
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// The provided hostname or IP address is invalid.
        /// </summary>
        public const string INVALID_HOSTNAME = "INVALID_HOSTNAME";

        /// <summary>
        /// The requested address is a local or loopback address.
        /// </summary>
        public const string LOCAL_ADDRESS = "LOCAL_ADDRESS";

        /// <summary>
        /// Failed to resolve the provided hostname to an IP address.
        /// </summary>
        public const string HOSTNAME_RESOLUTION_FAILED = "HOSTNAME_RESOLUTION_FAILED";

        /// <summary>
        /// The specified address was not found in the GeoIP database.
        /// </summary>
        public const string ADDRESS_NOT_FOUND = "ADDRESS_NOT_FOUND";

        /// <summary>
        /// An error occurred while performing GeoIP lookup.
        /// </summary>
        public const string GEOIP_ERROR = "GEOIP_ERROR";

        /// <summary>
        /// The request body contains invalid JSON.
        /// </summary>
        public const string INVALID_JSON = "INVALID_JSON";

        /// <summary>
        /// The request body was null or empty.
        /// </summary>
        public const string NULL_REQUEST = "NULL_REQUEST";

        /// <summary>
        /// The requested resource was not found.
        /// </summary>
        public const string NOT_FOUND = "NOT_FOUND";

        /// <summary>
        /// The hostname parameter was null, empty, or whitespace.
        /// </summary>
        public const string EMPTY_HOSTNAME = "EMPTY_HOSTNAME";

        /// <summary>
        /// The request list was empty.
        /// </summary>
        public const string EMPTY_REQUEST_LIST = "EMPTY_REQUEST_LIST";

        /// <summary>
        /// An internal server error occurred while processing the request.
        /// </summary>
        public const string INTERNAL_ERROR = "INTERNAL_ERROR";
    }
}
