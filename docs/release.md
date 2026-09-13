# Release Process

This project uses Semantic Versioning for NuGet packages.

Preview versions should use a prerelease suffix:

```text
0.1.0-preview.1
0.1.0-preview.2
0.2.0-preview.1
```

Stable versions should use plain SemVer:

```text
1.0.0
1.1.0
2.0.0
```

## Before Release

1. Update package version metadata in `Directory.Build.props` or in each package project being released.
2. Update `CHANGELOG.md`.
3. Update the repository `README.md` and package-specific `src/<Package>/README.md` files if the public API, supported methods, or package list changed.
4. Run:

```bash
dotnet test TBankAcquiringNet.slnx
dotnet pack TBankAcquiringNet.slnx --configuration Release --output artifacts/packages
```

5. If T-Bank terminal credentials are available, run the real payments integration test:

```bash
set -a
source .env
set +a
dotnet test tests/TBankAcquiringNet.Payments.Tests.Integration/TBankAcquiringNet.Payments.Tests.Integration.csproj
```

6. Inspect the generated packages:

```bash
unzip -l artifacts/packages/TBankAcquiringNet.Payments.<version>.nupkg
```

## Package Versions

Packages can be versioned independently while the project is in preview, but aligned versions are preferred when publishing the whole package family from one release tag.

Current package identities:

```text
TBankAcquiringNet.Payments
TBankNet.Multisplit
TBankAcquiringNet.Multisplit.Payouts
```

`TBankAcquiringNet.Payments` and `TBankNet.Multisplit` have implemented runtime behavior. `TBankAcquiringNet.Multisplit.Payouts` is intentionally marked non-packable until it has a real SDK surface.

## GitHub Actions

CI runs on pull requests, pushes to `main`, and `v*` tags. It restores, builds, tests, packs all packable SDK packages, and uploads package artifacts.

Release automation runs when a `v*` tag is pushed. It creates a GitHub Release, attaches generated package artifacts, and publishes to NuGet.org when the repository secret `NUGET_API_KEY` is configured.

Manual publishing workflows are also available:

```text
Publish NuGet
Publish GitHub Packages
```

For manual NuGet publishing, provide:

```text
git_ref: v0.1.0-preview.1
version: 0.1.0-preview.1
package: TBankAcquiringNet.Payments
```

## Tagging

Use tags that match the package version prefixed with `v`:

```bash
git tag -a v0.1.0-preview.1 -m "TBankAcquiringNet packages 0.1.0-preview.1"
git push origin v0.1.0-preview.1
```

Preview tags such as `v0.1.0-preview.1` should be marked as GitHub prereleases.

If a tag was pushed before the release workflow existed, GitHub Actions will not retroactively run the tag workflow. In that case, create the GitHub Release manually from the existing tag, use the manual `Publish NuGet` workflow, or push the next preview tag after the workflow is merged.

## Publishing To NuGet

Manual publishing:

```bash
dotnet nuget push artifacts/packages/TBankAcquiringNet.Payments.<version>.nupkg \
  --source https://api.nuget.org/v3/index.json \
  --api-key <NUGET_API_KEY>
```

Use `--skip-duplicate` when retrying:

```bash
dotnet nuget push artifacts/packages/*.nupkg \
  --source https://api.nuget.org/v3/index.json \
  --api-key <NUGET_API_KEY> \
  --skip-duplicate
```

## GitHub Release

Create a GitHub release from the pushed tag and attach the generated package artifacts.

For `0.1.0-preview.1`, attach:

```text
TBankAcquiringNet.Payments.0.1.0-preview.1.nupkg
```

Attach all generated `.nupkg` and `.snupkg` artifacts for the released version.

## Repository Secrets

When CI publishing is added, use a repository secret named:

```text
NUGET_API_KEY
```

T-Bank integration-test credentials should be stored as separate CI secrets only in protected jobs:

```text
TBANK_ACQUIRING_TEST_TERMINAL_KEY
TBANK_ACQUIRING_TEST_PASSWORD
TBANK_ACQUIRING_TEST_BASE_URL
```
