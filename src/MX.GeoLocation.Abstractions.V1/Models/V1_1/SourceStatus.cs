namespace MX.GeoLocation.Abstractions.Models.V1_1
{
    /// <summary>
    /// Indicates the outcome of fetching data from a specific source.
    /// </summary>
    public enum SourceStatus
    {
        /// <summary>Data was successfully retrieved from the source.</summary>
        Success,

        /// <summary>The source was called but returned an error or timed out.</summary>
        Failed,

        /// <summary>The source was not configured or not available.</summary>
        Unavailable
    }
}
