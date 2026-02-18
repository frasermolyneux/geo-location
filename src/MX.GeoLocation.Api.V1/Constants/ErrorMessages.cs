namespace MX.GeoLocation.LookupWebApi.Constants
{
    /// <summary>
    /// Defines standardized error messages for the GeoLocation API.
    /// These messages correspond to the error codes defined in <see cref="ErrorCodes"/>.
    /// </summary>
    public static class ErrorMessages
    {
        /// <summary>
        /// Error message for invalid hostname or IP address.
        /// </summary>
        public const string INVALID_HOSTNAME = "The address provided is invalid. IP or DNS is acceptable.";

        /// <summary>
        /// Error message for local or loopback addresses.
        /// </summary>
        public const string LOCAL_ADDRESS = "Local addresses are not supported for geo location";

        /// <summary>
        /// Error message for hostname resolution failure.
        /// </summary>
        public const string HOSTNAME_RESOLUTION_FAILED = "Failed to resolve the hostname";

        /// <summary>
        /// Error message for address not found in GeoIP database.
        /// </summary>
        public const string ADDRESS_NOT_FOUND = "The specified address was not found";

        /// <summary>
        /// Error message for invalid JSON in request body.
        /// </summary>
        public const string INVALID_JSON = "Could not deserialize request body";

        /// <summary>
        /// Error message for null or empty request body.
        /// </summary>
        public const string NULL_REQUEST = "Request body was null";

        /// <summary>
        /// Error message for resource not found.
        /// </summary>
        public const string NOT_FOUND = "No geo-location data found for the specified address";

        /// <summary>
        /// Error message for local address deletion attempt.
        /// </summary>
        public const string LOCAL_ADDRESS_DELETE = "Cannot delete data for local addresses";

        /// <summary>
        /// Error message for hostname resolution failure during deletion.
        /// </summary>
        public const string HOSTNAME_RESOLUTION_FAILED_DELETE = "Could not resolve the provided address";

        /// <summary>
        /// Error message for batch lookup local address.
        /// </summary>
        public const string LOCAL_ADDRESS_BATCH = "Hostname is a loopback or local address, geo location data is unavailable";

        /// <summary>
        /// Error message for null or whitespace hostname parameter.
        /// </summary>
        public const string EMPTY_HOSTNAME = "The hostname parameter is required and cannot be empty.";

        /// <summary>
        /// Error message for empty request list.
        /// </summary>
        public const string EMPTY_REQUEST_LIST = "The request must contain at least one hostname.";
    }
}
