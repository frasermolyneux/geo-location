﻿using MX.GeoLocation.Abstractions.Interfaces;

namespace MX.GeoLocation.Api.Client.V1
{
    /// <summary>
    /// Implementation of the GeoLocation API client that provides access to versioned API endpoints
    /// </summary>
    public class GeoLocationApiClient : IGeoLocationApiClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoLocationApiClient"/> class
        /// </summary>
        /// <param name="versionedGeoLookupApi">The versioned GeoLookup API</param>
        public GeoLocationApiClient(IVersionedGeoLookupApi versionedGeoLookupApi)
        {
            GeoLookup = versionedGeoLookupApi;
        }

        /// <summary>
        /// Gets the versioned GeoLookup API
        /// </summary>
        public IVersionedGeoLookupApi GeoLookup { get; }
    }
}