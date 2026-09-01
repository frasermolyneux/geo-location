# geo-location

.NET 9 GeoLocation service with an Entra-protected API, MVC web application, published API client packages, integration tests, and Azure infrastructure in Terraform.

## Locations

- Solution: `src/MX.GeoLocation.slnx`
- Applications: `src/MX.GeoLocation.Api.V1`, `src/MX.GeoLocation.Web`
- Packages: `src/MX.GeoLocation.Abstractions.V1`, `src/MX.GeoLocation.Api.Client.V1`, `src/MX.GeoLocation.Api.Client.Testing`
- Unit and integration tests: matching projects under `src/`
- Infrastructure: `terraform/`
- Architecture and operations: `docs/`

## Commands

```pwsh
dotnet build src/MX.GeoLocation.slnx
dotnet test src/MX.GeoLocation.slnx --filter "FullyQualifiedName!~IntegrationTests"
dotnet test src/MX.GeoLocation.slnx --filter "FullyQualifiedName~MyTestClass.MyTestMethod"
dotnet format src/MX.GeoLocation.slnx --verify-no-changes
terraform -chdir=terraform fmt -check -recursive
```

## Constraints

- Preserve the v1.0 and v1.1 HTTP, DTO, client, testing-helper, OpenAPI, and APIM routing contracts.
- Keep Entra authorization and the `LookupApiUser` role on protected endpoints; retain only documented anonymous endpoints.
- Do not weaken hostname/local-address validation, batch limits, cache semantics, secret handling, or forwarded-header trust configuration.
- OpenAPI documents are generated at runtime; do not create source-controlled generated specifications.
- Package identities, target frameworks, and NBGV behavior in `version.json` are release boundaries.
- Terraform lock files are local-only and ignored; do not commit `.terraform.lock.hcl`.
- Do not publish packages, deploy applications, apply Terraform, or run state-backed plans during validation.

## Documentation

- [API versioning, APIM, and OpenAPI](docs/api-versioning-and-apim.md)
- [IP intelligence](docs/ip-intelligence.md)
- [Client testing helpers](docs/testing.md)
- [Manual configuration](docs/manual-steps.md)
