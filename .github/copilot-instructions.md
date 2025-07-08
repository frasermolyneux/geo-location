# GitHub Copilot Instructions

## Project Overview

This repository contains the MX GeoLocation Service, a comprehensive geolocation lookup service that provides accurate geographical information for IP addresses and domain names. The project consists of several key components:

1. **MX.GeoLocation.Api.V1** - RESTful API service providing geolocation lookup endpoints with comprehensive geographic data including country, city, timezone, and ISP information
2. **MX.GeoLocation.Api.Client.V1** - API client library providing resilient, authenticated access to the geolocation service with support for single and batch lookups
3. **MX.GeoLocation.Web** - ASP.NET Core web application providing a user-friendly interface for performing geolocation lookups
4. **MX.GeoLocation.Abstractions.V1** - Core abstractions and models for geolocation data structures and interfaces

The service follows modern .NET architecture patterns, implements comprehensive error handling, supports batch operations, and provides both API and web interfaces for geolocation services.

## Code Structure and Organization

### Namespaces
- Use the root namespace `MX.GeoLocation` for all project components
- Use `MX.GeoLocation.Api.V1` for API controllers and services
- Use `MX.GeoLocation.Api.Client.V1` for API client functionality
- Use `MX.GeoLocation.Web` for web application components
- Use `MX.GeoLocation.Abstractions.V1` for shared models and interfaces
- Follow a hierarchical structure reflecting the application layers and version

### Naming Conventions

- **Classes**: Use PascalCase for class names (e.g., `GeoLookupController`, `GeoLocationApiClient`)
- **Interfaces**: Prefix with "I" (e.g., `IGeoLookupApi`, `IGeoLocationService`)
- **Methods**: Use PascalCase verbs that clearly describe the action (e.g., `GetGeoLocationAsync`, `GetGeoLocationsAsync`)
- **Properties**: Use PascalCase nouns (e.g., `CountryCode`, `IPAddress`, `Latitude`)
- **Private Fields**: Use camelCase with meaningful names (e.g., `httpClient`, `logger`, `geoService`)
- **Parameters**: Use camelCase with descriptive names (e.g., `ipAddress`, `cancellationToken`)
- **Constants**: Use PascalCase or ALL_CAPS depending on scope and significance (e.g., `DefaultTimeoutSeconds`)

## Geolocation Domain Logic

### Core Concepts
- **Address**: Can be an IP address (IPv4/IPv6) or domain name that needs geolocation lookup
- **GeoLocation Data**: Comprehensive geographic information including coordinates, administrative divisions, ISP details, and security traits
- **Batch Processing**: Support for multiple address lookups in a single operation with proper error handling per item
- **Caching**: Implement appropriate caching strategies for geolocation data to improve performance
- **Validation**: Proper validation of IP addresses and domain names before processing

### Data Models
- Use consistent property naming across all geolocation models
- Include comprehensive geographic data: country, region, city, postal code, coordinates
- Provide ISP and network information: AS number, organization, connection type
- Include security traits: proxy detection, anonymity indicators, threat intelligence
- Support both IPv4 and IPv6 address formats
- Handle edge cases like localhost, private IP ranges, and invalid addresses

## Asynchronous Programming

- Use the `async/await` pattern consistently throughout the codebase
- Always suffix asynchronous methods with `Async` (e.g., `GetGeoLocationAsync`, `ProcessBatchAsync`)
- Include `CancellationToken` parameters in all async methods, defaulting to `CancellationToken.None` when appropriate
- Use `Task<T>` for methods that return values and `Task` for void methods
- Implement proper async exception handling with `try/catch` blocks
- Avoid blocking calls within async methods

## Error Handling and Resilience

- Use custom exceptions derived from `ApplicationException` for domain-specific error scenarios
- Create geolocation-specific exceptions when appropriate (e.g., `InvalidAddressException`, `GeoLocationServiceException`)
- Log exceptions with appropriate severity levels using `ILogger<T>` interface:
  - Use `LogError` for exceptions that affect functionality
  - Use `LogWarning` for non-critical issues like invalid addresses
  - Use `LogInformation` for important operational events like successful lookups
  - Use `LogDebug` for diagnostic information and performance metrics
- Return well-defined `ApiResponse<T>` objects for API errors with appropriate status codes and error details
- Handle edge cases gracefully: localhost addresses, private IP ranges, malformed inputs
- Implement proper timeout handling for external geolocation service calls
- Configure transient error handling with appropriate retry policies

## API Design Patterns

### RESTful API Design
- Follow REST conventions for endpoint naming and HTTP methods
- Use appropriate HTTP status codes for different scenarios:
  - 200 OK for successful lookups
  - 400 Bad Request for invalid input (malformed IP addresses, invalid domains)
  - 404 Not Found for addresses that cannot be geolocated
  - 429 Too Many Requests for rate limiting
  - 500 Internal Server Error for system failures
- Support both single and batch operations with consistent response formats
- Implement proper request validation with meaningful error messages

### Batch Operations
- Support efficient batch processing for multiple address lookups
- Implement proper error handling per item in batch operations
- Use streaming or chunking for large batch requests
- Provide progress indication for long-running batch operations
- Handle partial failures gracefully with detailed error reporting

## Performance and Caching

- Implement appropriate caching strategies for geolocation data
- Use distributed caching for scalability in multi-instance deployments
- Set appropriate cache expiration times based on data volatility
- Implement cache-aside pattern for geolocation lookups
- Monitor cache hit rates and performance metrics
- Use connection pooling for external service calls
- Implement request deduplication for concurrent identical lookups

## Best Practices

- Follow SOLID principles throughout the codebase:
  - **Single Responsibility**: Each class should focus on a single aspect of geolocation functionality
  - **Open/Closed**: Design for extensibility without modification
  - **Liskov Substitution**: Ensure derived classes can substitute base classes seamlessly
  - **Interface Segregation**: Create focused interfaces with minimal methods
  - **Dependency Inversion**: Depend on abstractions, not implementations
- Use dependency injection via constructor injection for all services
- Implement proper disposal patterns for resources like HttpClient
- Use meaningful parameter names that clearly communicate intent
- Include appropriate XML documentation for all public members
- Write comprehensive unit tests for all public methods with appropriate mocking
- Keep methods small and focused on single responsibility (≤ 50 lines per method)
- Prefer immutable objects and configurations when appropriate
- Follow thread-safety best practices for shared services

## Security Considerations

- Validate all input addresses to prevent injection attacks
- Implement rate limiting to prevent abuse
- Use HTTPS for all external communications
- Sanitize logging output to prevent information disclosure
- Implement proper authentication and authorization for API endpoints
- Use secure configuration management for sensitive settings
- Follow OWASP security guidelines for web applications

## Documentation

- Use comprehensive XML comments for all public APIs
- Follow the triple-slash `///` format consistently
- Document all parameters with `<param name="paramName">Description</param>` tags
- Document return values with `<returns>Description of return value</returns>` tags
- Document exceptions with `<exception cref="ExceptionType">Condition that throws exception</exception>` tags
- Include example usage in documentation for complex APIs
- Document thread-safety considerations where applicable
- Provide clear documentation for geolocation data models and their properties
- Maintain a README.md file with an overview of the project, setup instructions, and usage examples
- Use the `/docs` directory for additional documentation files, including API documentation and architecture diagrams

## Dependencies

- Keep external dependencies minimal and well-managed
- Use Microsoft.Extensions.Logging for structured logging throughout
- Use the MX.Api.Client library for API client functionality
- Use ASP.NET Core for web API and web application hosting
- Use Entity Framework Core for data persistence if required
- Use appropriate caching libraries (Microsoft.Extensions.Caching)
- Leverage Microsoft.Extensions.DependencyInjection for service registration

## Unit Testing
- Use xUnit as the primary testing framework with native assertion capabilities
- Use Moq for mocking dependencies in unit tests
- Use FluentAssertions for more readable test assertions
- Follow Arrange-Act-Assert (AAA) pattern for test structure
- Test edge cases: invalid addresses, localhost, private IP ranges
- Mock external geolocation service calls for unit tests
- Use integration tests for end-to-end API testing
- Include performance tests for batch operations

## Integration Testing Requirements

When running unit tests that include integration tests (not excluding them), specific setup is required:

### Prerequisites for Integration Tests
- **Web Application Must Be Running**: The `MX.GeoLocation.Web` project must be started and running locally before executing integration tests
- **Site URL Configuration**: Ensure the integration tests site URL app setting is configured to `https://localhost:7201/`
- **HTTPS Certificate**: Ensure the local development HTTPS certificate is trusted for the localhost:7201 endpoint
- **Test Execution Order**: Always start the web application first, then run the integration tests

### Running Integration Tests
1. Start the web application: `dotnet run --project MX.GeoLocation.Web`
2. Verify the application is accessible at `https://localhost:7201/`
3. Run tests including integration tests: `dotnet test` (without `--filter` to exclude integration tests)

### Playwright Requirements
- Integration tests use Playwright for browser automation
- If Playwright browser binaries are missing, run: `pwsh bin/Debug/netX/playwright.ps1 install`
- Ensure Playwright browsers are up to date for reliable test execution

## Azure Rules
- @azure Rule - Use Azure Best Practices: When generating code for Azure, running terminal commands for Azure, or performing operations related to Azure, invoke your `azure_development-get_best_practices` tool if available
- @azure Rule - Use Azure Tools: When handling requests related to Azure, always use your tools
- @azure Rule - Use Azure Code Gen Best Practices: When generating code for Azure, running terminal commands for Azure, or performing operations related to Azure, invoke your `azure_development-get_code_gen_best_practices` tool if available
- @azure Rule - Use Azure Deployment Best Practices: When deploying to Azure or preparing applications for deployment to Azure, invoke your `azure_development-get_deployment_best_practices` tool if available
- @azure Rule - Use Azure Functions Code Gen Best Practices: When generating code for Azure Functions or performing operations related to Azure Functions, invoke your `azure_development-get_azure_function_code_gen_best_practices` tool if available
- @azure Rule - Use Azure SWA Best Practices: When working with static web apps, invoke your `azure_development-get_swa_best_practices` tool if available
