---
name: prepare-release
description: Prepare a new NuGet release of Finance.NET — decide the next SemVer version, bump it, write the required release notes, and open the release PR to main. Use whenever the user mentions releasing, publishing, shipping, cutting a release, bumping the version for a release, or preparing release notes, even if they don't say "release" explicitly. Every release REQUIRES a release-notes/v<version>.md file; the deploy workflow reads it and fails without it.
---

# Prepare release

Prepare a new release of the Finance.NET NuGet package. You decide the next
version yourself, prepare the branch, and **open the release PR to `main` — then
stop.** Merging and publishing are manual maintainer steps and not part of this
skill:

- You do **not** merge the release PR.
- You do **not** trigger `deploy-nuget.yml`. Publishing is a manual
  `workflow_dispatch` ("Deploy Nuget") the maintainer runs **after** merging.

## The one thing that must not be forgotten

`deploy-nuget.yml` creates the GitHub Release with
`body_path: ./release-notes/v<version>.md` (see step "Create GitHub Release").
**If that file does not exist, the deploy workflow fails.** So the release-notes
file is not optional documentation — it is a hard requirement of every release.

## Workflow

1. **Confirm there is something to release.** Latest tag:
   `git tag --sort=-v:refname | head -1`. Then `git log --oneline <tag>..HEAD`.
   No commits since the tag → nothing to release, stop.

2. **Read the current version** — `<Version>` in `src/Finance.NET.csproj`.

3. **Decide the next version (SemVer)** from the commits since the last tag:
   - **MAJOR** breaking public-API changes · **MINOR** new backwards-compatible
     features · **PATCH** fixes, dependency updates, docs, internal refactors.
   - The public API is only the interfaces, models, enums, `FinanceNetConfiguration`,
     and `FinanceNetException`; services are `internal`, so refactors behind them
     are PATCH. State the chosen version and a one-line reason.

4. **Confirm a clean tree** — `git status --short`; if dirty, ask how to proceed.

5. **Branch from an up-to-date `main`** — `git checkout main && git pull`, then
   `git checkout -b release/v<version>`.

6. **(Optional) Update the shipped library's NuGet packages** in
   `src/Finance.NET.csproj` only (leave `tests`/`example` alone), then
   `dotnet restore Finance.NET.slnx`.

7. **Build & test green (required).**
   - `dotnet build Finance.NET.slnx --configuration Release`
   - `dotnet test tests/Tests.csproj --configuration Release --filter "TestCategory=Unit"`
     (unit tests are deterministic; the `Integration` tests hit live provider
     endpoints and are not a reliable local release gate).

8. **Bump `<Version>`** in `src/Finance.NET.csproj`.

9. **Update docs** — adjust `README.md` (e.g. version-specific notes, new
   dependency versions) if the changes warrant it.

10. **Write the release notes (required).** Copy
    `.claude/skills/prepare-release/assets/release-notes.md` to
    `release-notes/v<version>.md` and fill in the `### What's Changed` bullets with
    **user-facing** notes (not raw commit subjects). Match the existing files in
    `release-notes/` for tone and format.

11. **Commit** — stage `src/Finance.NET.csproj`, any docs, and the new
    `release-notes/v<version>.md`. Message: `release: v<version>`. Do **not** tag;
    the deploy workflow creates the tag.

12. **Push & open the PR to main — then stop.**
    - `git push -u origin release/v<version>`
    - `gh pr create --base main --head release/v<version> --title "release: v<version>" --body <notes>`

13. **Report** the PR link and remind the maintainer: after they merge it, they
    run **Actions → Deploy Nuget → Run workflow**. That workflow builds, tests,
    packs, pushes to NuGet + GitHub Packages, tags `v<version>`, and creates the
    GitHub Release from `release-notes/v<version>.md`. You do not trigger it.
