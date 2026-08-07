# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Finance.NET is a NuGet library (`src/Finance.NET.csproj`, netstandard2.1) that retrieves financial data from four providers: Yahoo Finance (HTML scraping + API), Alpha Vantage (JSON API, requires API key), Xetra (HTML scraping + CSV download), and DataHub (CSV from GitHub). Solution file: `Finance.NET.slnx` (src, tests, example).

Naming gotcha: the NuGet package, repo, and solution are `Finance.NET`, but the assembly name, root namespace, and `using` are `Finance.Net`.

## Commands

Repo-root `.cmd` wrappers exist for the common flows (`run-tests.cmd`, `run-code-coverage.cmd`, `clean.cmd`, `deploy-nuget.cmd`); they are gitignored (`*.cmd`), so they exist only locally. Underlying commands:

```shell
dotnet build Finance.NET.slnx --configuration Release

# All tests except long-running integration tests (matches run-tests.cmd)
dotnet test --configuration Release --filter "TestCategory!=Long-Running"

# Single test
dotnet test tests/Tests.csproj --filter "FullyQualifiedName~YahooFinanceServiceTests.GetQuoteAsync"

# Unit tests only (integration tests hit live provider endpoints)
dotnet test tests/Tests.csproj --filter "TestCategory!=Integration"

# Coverage report (coverlet + reportgenerator, output in tests/TestResults/Reports)
run-code-coverage.cmd
```

Tests use NUnit + Moq and target net10.0. Three categories gate what runs: `Unit` (mocked, deterministic), `Integration` (live provider endpoints), and `Long-Running`. The main CI (`.github/workflows/ci.yml`) runs `TestCategory=Unit` only, so a provider outage cannot break the build. The live `Integration` tests run on schedule via the per-provider workflows (`tests-*.yml`, built on `_tests-template.yml`); `TestHelper.SetUpServiceProvider` raises `HttpRetryCount` to 10 there to tolerate transient provider blips. Integration tests for Alpha Vantage need the key `FinanceNet:AlphaVantageApiKey` via user secrets or environment variable (see `tests/TestHelper.cs`).

## Architecture

Entry point is `ServiceCollectionExtensions.AddFinanceNet()` (src/Extensions), which wires everything:

- Four scoped services in `src/Services/` behind public interfaces in `src/Interfaces/`: `YahooFinanceService`, `AlphaVantageService`, `XetraService`, `DataHubService`. Services are `internal`; only interfaces, models, enums, config (`FinanceNetConfiguration`), and the exception are public API.
- One named `HttpClient` per provider (names in `src/Constants.cs`, which also holds all provider URLs). Each client gets a randomized User-Agent (`Helper.CreateRandomUserAgent`).
- A shared Polly retry policy (`PollyPolicyFactory`) registered in a `PolicyRegistry` under `Constants.DefaultHttpRetryPolicy`; services resolve it from the registry and wrap their HTTP calls with it. Retry count and back-off base (`HttpRetryCount`, `HttpRetrySleepTime`) come from `FinanceNetConfiguration`; `HttpTimeout` is the HTTP client timeout only and must not be reused as the back-off base.
- Internal DTOs (`src/Models/*/Dtos/`) are converted to the public models (`src/Models/*/`) by hand-written extension-method mappers in `src/Mappings/` (`YahooQuoteMapper.ToQuote`, `XetraInstrumentMapper.ToInstrument`). The `*Mapping.cs` classes in the same folder are CsvHelper `ClassMap`s, not object-to-object mappers.

Yahoo requires session state: `YahooSessionState` and `YahooSessionManager` (singletons in `src/Utilities/`) fetch consent cookies and an API "crumb", guarded by a semaphore, valid for 6 hours. The Yahoo HttpClient shares the session's `CookieContainer`. HTML parsing uses AngleSharp with XPath (`YahooHtmlParser`); Alpha Vantage JSON parsing is in `AlphaVantageParser`; CSV parsing uses CsvHelper.

All failures are wrapped in `FinanceNetException`.

## Conventions

- Analyzers are enforced at build time (`EnforceCodeStyleInBuild`, SonarAnalyzer, `.editorconfig`); warnings will fail style checks in CI/SonarCloud.
- Public API surfaces carry XML `<summary>` docs (`GenerateDocumentationFile` is on).
- Unit tests mock `IHttpClientFactory` and feed fixture files from `tests/TestData/` (must be registered in `Tests.csproj` with `CopyToOutputDirectory`).

## Releases

The version is `<Version>` in `src/Finance.NET.csproj`; publishing is a manual `workflow_dispatch` (`deploy-nuget.yml`, "Deploy Nuget") that tags `v<version>`. Every release **requires** a `release-notes/v<version>.md` file (format `### What's Changed` + user-facing bullets) — the deploy's "Create GitHub Release" step reads it via `body_path` and **fails if it is missing**. Bumping the version without adding that file is the classic broken release. Use the `prepare-release` skill (`.claude/skills/prepare-release/`), which walks the version bump, notes, and release PR to `main`.
