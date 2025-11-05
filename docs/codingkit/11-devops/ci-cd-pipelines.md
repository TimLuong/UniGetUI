# CI/CD Pipelines

## Overview

UniGetUI uses **GitHub Actions** for continuous integration and deployment. The workflows automate testing, building, code quality checks, and releases.

## Workflows

### .NET Tests (`dotnet-test.yml`)

**Purpose**: Runs unit tests on every push and pull request

**Triggers**:
- Push to main branch
- Pull request to main branch
- Manual workflow dispatch

**Steps**:
1. Checkout code
2. Setup .NET 8.0 SDK
3. Restore dependencies
4. Build solution
5. Run tests with coverage

**Location**: `.github/workflows/dotnet-test.yml`

### CodeQL Analysis (`codeql.yml`)

**Purpose**: Security and code quality scanning

**Triggers**:
- Push to main branch
- Pull request to main branch
- Weekly schedule

**Steps**:
1. Initialize CodeQL
2. Build solution
3. Perform analysis
4. Upload results

**Location**: `.github/workflows/codeql.yml`

### Translation Tests (`translations-test.yml`)

**Purpose**: Validates translation files

**Triggers**:
- Changes to translation files
- Manual workflow dispatch

**Steps**:
1. Checkout code
2. Setup Python
3. Run translation validation scripts

**Location**: `.github/workflows/translations-test.yml`

### WinGet Releases

**Purpose**: Publish releases to WinGet package repository

**Workflows**:
- `winget-prerelease.yml` - Pre-release versions
- `winget-stable.yml` - Stable releases

**Triggers**:
- GitHub release creation
- Manual workflow dispatch

**Location**: `.github/workflows/`

### Icon Updates (`update-icons.yaml`)

**Purpose**: Updates application icons

**Triggers**:
- Manual workflow dispatch
- Changes to icon files

**Location**: `.github/workflows/update-icons.yaml`

### Translation Updates (`update-tolgee.yml`)

**Purpose**: Syncs translations from Tolgee platform

**Triggers**:
- Scheduled (daily/weekly)
- Manual workflow dispatch

**Location**: `.github/workflows/update-tolgee.yml`

## Local Development

### Running Tests Locally

```bash
# All tests
dotnet test

# Specific project
dotnet test src/UniGetUI.Core.Classes.Tests/

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Checking Code Quality

```bash
# Build with code analysis
dotnet build /p:EnforceCodeStyleInBuild=true

# Run CodeQL locally (requires CodeQL CLI)
codeql database create ...
```

## Continuous Deployment

### Release Process

1. **Create Release**
   - Tag version in Git
   - Create GitHub release
   - Attach installer artifacts

2. **Automated Workflows**
   - Build and sign installer
   - Publish to WinGet
   - Update documentation
   - Notify users

3. **Distribution Channels**
   - Microsoft Store (manual)
   - WinGet (automated)
   - Direct download (automated)
   - Scoop (automated)
   - Chocolatey (automated)

## Best Practices

### Pull Request Checks

All PRs must pass:
- ✅ Build succeeds
- ✅ Tests pass
- ✅ Code analysis passes
- ✅ No merge conflicts

### Adding New Workflows

When adding workflows:
1. Test locally first
2. Use appropriate triggers
3. Document in this file
4. Keep secrets secure
5. Use caching for dependencies

## Troubleshooting

### Build Failures

Check:
- .NET SDK version
- Dependencies restored
- Code analysis errors
- Test failures

### Workflow Failures

Check:
- Workflow logs in GitHub Actions
- Environment secrets configured
- Permissions set correctly

## Related Documentation

- [Build & Deployment](./build-deployment.md)
- [Local Setup](./local-setup.md)
- [Release Process](./release-process.md)
