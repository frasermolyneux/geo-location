# .NET Multi-Targeting Strategy

## Overview

The NuGet packages published by this repository target both .NET 9 and .NET 10 to ensure compatibility with downstream consumers using either framework version.

## Target Framework Configuration

### NuGet Package Projects (Multi-Targeted)

The following projects produce NuGet packages and are configured to multi-target both .NET 9 and .NET 10:

- `MX.GeoLocation.Abstractions.V1` - Targets `net9.0;net10.0`
- `MX.GeoLocation.Api.Client.V1` - Targets `net9.0;net10.0`

These projects use the `<TargetFrameworks>` (plural) property in their `.csproj` files to specify multiple target frameworks.

### Web Application Projects (Single Target)

Web applications remain single-targeted to .NET 9:

- `MX.GeoLocation.Api.V1` - Targets `net9.0`
- `MX.GeoLocation.Web` - Targets `net9.0`

### Test Projects (Single Target)

Test projects target .NET 9:

- `MX.GeoLocation.Api.Client.Tests.V1` - Targets `net9.0`
- `MX.GeoLocation.Api.Tests.V1` - Targets `net9.0`
- `MX.GeoLocation.Web.Tests` - Targets `net9.0`
- `MX.GeoLocation.Web.IntegrationTests` - Targets `net9.0`

## Dependency Management

### Package Version Strategy

All NuGet dependencies are updated to versions that support both .NET 9 and .NET 10. Key dependencies include:

- Microsoft.Extensions.* packages (v10.0.2) - Support both .NET 9 and .NET 10
- Azure.Identity (v1.17.1)
- MX.Api.* packages (v2.2.31)
- RestSharp (v113.1.0)

### Conditional References

No conditional package references based on target framework are used. All dependencies are compatible with both target frameworks.

## Dependabot Configuration

Dependabot is configured to automatically update:

- **Minor and patch versions** - Automated via daily updates
- **Major versions** - Excluded from automated updates; require manual review

This is configured in `.github/dependabot.yml` using the `ignore` setting with `update-types: ["version-update:semver-major"]`.

## Build and Test Matrix

### GitHub Actions

The `build-and-test.yml` workflow runs builds and tests against both .NET 9 and .NET 10 using a matrix strategy. Each commit and pull request is validated against both target frameworks.

### Azure Pipelines

Azure Pipelines handle deployment builds and use .NET 9.x for building and packaging all projects, including the multi-targeted NuGet packages.

## Package Publishing

When NuGet packages are built with multi-targeting:

- Each package contains assemblies for both `net9.0` and `net10.0` in separate framework folders
- NuGet automatically selects the appropriate assembly based on the consuming project's target framework
- Consumers targeting .NET 9 receive the `net9.0` assembly
- Consumers targeting .NET 10 receive the `net10.0` assembly

## Maintenance Guidelines

### Adding New Dependencies

When adding new dependencies to multi-targeted projects:

1. Verify the package supports both .NET 9 and .NET 10
2. Use the highest version that supports both frameworks
3. Avoid framework-conditional package references unless absolutely necessary

### Framework Updates

When .NET 11 or future versions are released:

1. Update multi-targeted projects to include the new framework (e.g., `net9.0;net10.0;net11.0`)
2. Update all dependencies to versions supporting the new framework
3. Update the build-and-test workflow matrix to include the new version
4. Consider removing the oldest framework version if backwards compatibility is no longer required
