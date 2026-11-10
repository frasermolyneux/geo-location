# Manual Steps

* `maxmind_apikey` and `maxmind_userid` secrets need to be added to the Key Vault
* `ProxyCheck:ApiKey` secret needs to be added to the Key Vault (ProxyCheck.io API key for IP risk assessment)

## NuGet Trusted Publishing

* Configure a NuGet.org trusted publishing policy for GitHub owner `frasermolyneux`, repository `geo-location`, workflow filename `release-publish-nuget.yml`, and GitHub environment `NuGet`.
* Set the GitHub Actions variable `NUGET_USER` in the `NuGet` environment to the NuGet.org username of the trusted publishing policy creator (not necessarily the package owner). Publishing uses GitHub OIDC and does not read a stored `NUGET_API_KEY` secret. The login action requires runner version 2.327.1 or later; the workflow uses GitHub-hosted `ubuntu-latest`.
* To exercise publishing, dispatch **Release - Version and Tag** (`release-version-and-tag.yml`) on `main`. It calculates the normal Nerdbank.GitVersioning version, builds and checks the packages, and tags the release. Its successful completion triggers **Release - Publish NuGet**, which publishes the producer run's artifacts only when its source commit has a release tag.
* Verify the publishing run and NuGet.org acceptance of the new package and symbol packages. Do not override versions or bypass release gates to test authentication.
