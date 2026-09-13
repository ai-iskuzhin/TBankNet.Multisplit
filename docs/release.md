# Release

`TBankNet.Multisplit` is a standalone package: one project, one package id, one version.

## Preflight

```bash
dotnet restore TBankNet.Multisplit.slnx
dotnet build TBankNet.Multisplit.slnx --configuration Release --no-restore
dotnet test TBankNet.Multisplit.slnx --configuration Release --no-build
dotnet pack src/TBankNet.Multisplit/TBankNet.Multisplit.csproj \
  --configuration Release --no-build --output artifacts/packages
```

Check the package carries its README, icon and XML docs before publishing anything:

```bash
unzip -l artifacts/packages/TBankNet.Multisplit.<version>.nupkg
```

Expect `README.md`, `icon.png`, `lib/net10.0/TBankNet.Multisplit.dll` and
`lib/net10.0/TBankNet.Multisplit.xml`.

## Versioning

`Version` lives in [Directory.Build.props](../Directory.Build.props). A version containing `-` is
treated as a prerelease by the release workflow and marked as such on GitHub.

## Publishing

Both workflows authenticate with **NuGet Trusted Publishing** (OIDC): the GitHub token is exchanged
for a short-lived NuGet.org key at run time, so no long-lived `NUGET_API_KEY` secret exists in this
repository.

Prerequisites, once:

1. A trusted publishing policy on NuGet.org for `ai-iskuzhin/TBankNet.Multisplit`.
2. A repository variable `NUGET_USER` set to the owning NuGet.org account.

### Tagged release

```bash
git tag -a v0.3.0-preview.1 -m "TBankNet.Multisplit 0.3.0-preview.1"
git push origin v0.3.0-preview.1
```

[`release.yml`](../.github/workflows/release.yml) builds, tests, packs, creates the GitHub Release
with the packages attached, and pushes to NuGet.org. Without `NUGET_USER` it still creates the
release and skips only the push.

### Manual

[`publish-nuget.yml`](../.github/workflows/publish-nuget.yml) — run it from the Actions tab with a
git ref and a version. Use it to republish a version or to ship from a ref that was never tagged.

[`publish-github-packages.yml`](../.github/workflows/publish-github-packages.yml) does the same
against GitHub Packages, authenticating with the built-in `GITHUB_TOKEN`.

Every push uses `--skip-duplicate`, so re-running a workflow on an already-published version is a
no-op rather than a failure.
