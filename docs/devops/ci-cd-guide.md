# CI/CD Guide

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Workflows](#workflows)
- [Pipeline Configuration](#pipeline-configuration)
- [Build Automation](#build-automation)
- [Testing in Pipelines](#testing-in-pipelines)
- [Environment Configuration](#environment-configuration)
- [Secrets Management](#secrets-management)
- [Monitoring and Notifications](#monitoring-and-notifications)
- [Troubleshooting](#troubleshooting)

## Overview

This guide provides comprehensive documentation for the CI/CD pipelines used in the UniGetUI project. Our CI/CD strategy focuses on automation, quality assurance, and reliable deployment processes.

### Goals

- **Automation**: Minimize manual intervention in build and deployment processes
- **Quality**: Ensure code quality through automated testing and analysis
- **Speed**: Optimize pipeline execution time while maintaining quality
- **Reliability**: Implement robust error handling and rollback mechanisms
- **Transparency**: Provide clear feedback and reporting at every stage

### Technologies

- **CI/CD Platform**: GitHub Actions
- **Build Tool**: .NET SDK 8.0
- **Testing Framework**: xUnit, NUnit (as configured in projects)
- **Package Management**: NuGet
- **Artifact Storage**: GitHub Packages, GitHub Releases

## Architecture

### Pipeline Structure

```
┌─────────────────────────────────────────────────────────────┐
│                         Trigger                              │
│  (Push, PR, Release, Manual, Schedule)                      │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                    Validation Stage                          │
│  - Checkout code                                             │
│  - Validate syntax                                           │
│  - Check dependencies                                        │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                      Build Stage                             │
│  - Restore dependencies (with cache)                         │
│  - Build solution                                            │
│  - Publish artifacts                                         │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                      Test Stage                              │
│  - Unit tests                                                │
│  - Integration tests                                         │
│  - Code coverage analysis                                    │
│  - Security scanning (CodeQL)                                │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                    Package Stage                             │
│  - Create deployment packages                                │
│  - Generate integrity checksums                              │
│  - Create installers (Inno Setup)                            │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                    Deploy Stage                              │
│  - Deploy to staging (pre-release)                           │
│  - Deploy to production (release)                            │
│  - Update package managers (WinGet, etc.)                    │
└─────────────────────────────────────────────────────────────┘
```

## Workflows

### 1. Build Workflow (`.github/workflows/build.yml`)

**Purpose**: Compile the application and validate build integrity.

**Triggers**:
- Push to `main` or `develop` branches (when C# files change)
- Pull requests to `main` or `develop`
- Manual dispatch

**Key Features**:
- Builds on Windows runners (required for .NET Windows apps)
- Supports both Release and Debug configurations
- Caches NuGet packages for faster builds
- Uploads build artifacts for downstream jobs
- Generates build summaries

**Configuration**:
```yaml
env:
  DOTNET_VERSION: '8.0.x'
  BUILD_CONFIGURATION: 'Release'
  BUILD_PLATFORM: 'x64'
```

**Artifacts**:
- Build output in `unigetui-{configuration}-{sha}`
- Retention: 30 days

### 2. Test Workflow (`.github/workflows/test.yml`)

**Purpose**: Execute comprehensive test suites and code quality checks.

**Triggers**:
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop`
- Manual dispatch
- Scheduled (daily at 2 AM UTC)

**Test Types**:

1. **Unit Tests**
   - Framework: xUnit, NUnit
   - Code coverage collection with OpenCover format
   - Results published to GitHub PR checks
   - Fails the build on test failures

2. **Integration Tests**
   - Runs after unit tests pass
   - Only on non-PR events (push to main/develop)
   - Tests interactions between components

3. **Code Quality Analysis**
   - Runs .NET code analyzers
   - Enforces code style guidelines
   - Checks for code smells and anti-patterns

4. **Test Matrix**
   - Tests across multiple .NET versions (currently 8.0.x)
   - Can be expanded for compatibility testing

**Test Reports**:
- Test results in TRX format
- Code coverage in OpenCover XML format
- Visual test reports in PR checks

**Coverage Requirements**:
- Minimum coverage target: 70% (recommended)
- Coverage reports uploaded as artifacts

### 3. Deploy Workflow (`.github/workflows/deploy.yml`)

**Purpose**: Deploy releases using various strategies.

**Triggers**:
- GitHub release published (automatic)
- Manual dispatch with environment selection

**Deployment Strategies**:
Detailed in [Deployment Strategies Guide](./deployment-strategies.md)

### 4. Existing Workflows

#### CodeQL Security Scanning (`.github/workflows/codeql.yml`)
- **Purpose**: Automated security vulnerability scanning
- **Schedule**: Weekly (Monday at midnight)
- **Languages**: C#, Python
- **Output**: Security alerts in GitHub Security tab

#### Translation Test (`.github/workflows/translations-test.yml`)
- **Purpose**: Validate translation files
- **Ensures**: All language files are properly formatted

#### WinGet Stable Release (`.github/workflows/winget-stable.yml`)
- **Purpose**: Auto-submit to WinGet on stable releases
- **Trigger**: GitHub release (non-prerelease)
- **Package ID**: `MartiCliment.UniGetUI`

#### WinGet Pre-release (`.github/workflows/winget-prerelease.yml`)
- **Purpose**: Auto-submit to WinGet pre-release channel
- **Trigger**: GitHub pre-release
- **Package ID**: `MartiCliment.UniGetUI.Pre-Release`

## Pipeline Configuration

### Required Secrets

Configure these secrets in GitHub repository settings:

| Secret Name | Description | Required For |
|------------|-------------|--------------|
| `WINGET_TOKEN` | WinGet submission token | WinGet auto-deploy |
| `CODE_SIGNING_CERT` | Code signing certificate | Production builds |
| `CODE_SIGNING_PASSWORD` | Certificate password | Production builds |
| `TOLGEE_API_KEY` | Translation service API key | Translation updates |

### Environment Variables

**Build Environment**:
```yaml
DOTNET_VERSION: '8.0.x'
BUILD_CONFIGURATION: 'Release'
BUILD_PLATFORM: 'x64'
```

**Paths**:
```yaml
SRC_DIR: 'src'
OUTPUT_DIR: 'output'
ARTIFACTS_DIR: 'artifacts'
```

### Branch Protection Rules

**Main Branch**:
- Require pull request reviews (1 approver)
- Require status checks to pass:
  - `Build Application`
  - `Unit Tests`
  - `Code Quality Analysis`
- Require branches to be up to date
- Include administrators in restrictions

**Develop Branch**:
- Require pull request reviews (1 approver)
- Require status checks to pass:
  - `Build Application`
  - `Unit Tests`

## Build Automation

### Standard Build Process

1. **Checkout**: Fetch source code with full history
2. **Setup**: Install .NET SDK and required tools
3. **Cache**: Restore NuGet package cache
4. **Restore**: Download and install dependencies
5. **Build**: Compile solution
6. **Publish**: Create deployment-ready output
7. **Artifact**: Upload build output

### Build Optimization

**NuGet Package Caching**:
```yaml
- name: Cache NuGet packages
  uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/packages.lock.json') }}
    restore-keys: |
      ${{ runner.os }}-nuget-
```

Benefits:
- Reduces build time by 30-50%
- Saves bandwidth
- Improves reliability

**Incremental Builds**:
- Only rebuild changed projects
- Use `--no-restore` flag after restore step
- Minimize clean operations

### Build Versioning

Version information is managed through:

1. **Build Number**: Auto-incremented in `scripts/BuildNumber`
2. **Version Name**: Manually updated in `scripts/apply_versions.py`
3. **Git Tags**: Used for release versions

**Version Format**: `{major}.{minor}.{patch}[-{prerelease}]`

Examples:
- `3.3.6` (stable release)
- `3.3.7-beta.1` (pre-release)

## Testing in Pipelines

### Test Execution Strategy

**Pull Requests**:
- Unit tests only
- Fast feedback (< 5 minutes)
- Blocks merge if tests fail

**Main/Develop Branch Pushes**:
- Unit tests
- Integration tests
- Code quality checks
- Longer execution time acceptable

**Scheduled Runs**:
- Full test suite
- Extended integration tests
- Performance benchmarks
- Compatibility tests

### Test Results Publishing

**Test Reporter**:
```yaml
- name: Publish test results
  uses: dorny/test-reporter@v1
  if: always()
  with:
    name: Unit Test Results
    path: src/TestResults/*.trx
    reporter: dotnet-trx
    fail-on-error: true
```

**Benefits**:
- Visual test reports in PR checks
- Detailed failure information
- Historical trend analysis

### Code Coverage

**Collection**:
```yaml
dotnet test UniGetUI.sln \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
```

**Coverage Badges**:
Add to README for visibility:
```markdown
![Code Coverage](https://img.shields.io/badge/coverage-75%25-green)
```

### Flaky Test Handling

**Retry Strategy**:
```yaml
- name: Run tests with retry
  run: |
    for i in {1..3}; do
      dotnet test && break || sleep 10
    done
```

**Quarantine**:
- Mark flaky tests with `[Trait("Category", "Quarantine")]`
- Run in separate job
- Don't block pipeline

## Environment Configuration

### Environments

#### Staging
- **Purpose**: Pre-production testing
- **Trigger**: Pre-release creation
- **URL**: https://staging.example.com
- **Protection**: Manual approval not required
- **Secrets**: Staging-specific credentials

#### Production
- **Purpose**: Live releases
- **Trigger**: Stable release creation
- **URL**: https://github.com/marticliment/UniGetUI/releases
- **Protection**: Manual approval recommended
- **Secrets**: Production credentials

### Environment Variables per Environment

**Staging**:
```yaml
ENVIRONMENT_NAME: staging
DEPLOY_URL: https://staging.example.com
LOG_LEVEL: DEBUG
```

**Production**:
```yaml
ENVIRONMENT_NAME: production
DEPLOY_URL: https://github.com/marticliment/UniGetUI/releases
LOG_LEVEL: INFO
```

## Secrets Management

### Best Practices

1. **Never commit secrets**: Use GitHub Secrets
2. **Rotate regularly**: Update secrets every 90 days
3. **Least privilege**: Grant minimum required permissions
4. **Environment-specific**: Use separate secrets per environment
5. **Audit access**: Review who has access to secrets

### Adding Secrets

**Via GitHub UI**:
1. Navigate to repository Settings
2. Click on "Secrets and variables" → "Actions"
3. Click "New repository secret"
4. Enter name and value
5. Click "Add secret"

**Via GitHub CLI**:
```bash
gh secret set SECRET_NAME -b "secret_value"
```

### Using Secrets in Workflows

```yaml
steps:
  - name: Use secret
    run: |
      echo "Using secret value"
    env:
      MY_SECRET: ${{ secrets.MY_SECRET }}
```

**Security Notes**:
- Secrets are masked in logs
- Not available in forked repository PRs
- Use environment secrets for additional protection

## Monitoring and Notifications

### Workflow Status Monitoring

**GitHub Actions Dashboard**:
- View all workflow runs
- Filter by status, branch, or workflow
- Access logs and artifacts

**Status Badges**:
Add to README:
```markdown
![Build Status](https://github.com/marticliment/UniGetUI/workflows/Build/badge.svg)
![Test Status](https://github.com/marticliment/UniGetUI/workflows/Test/badge.svg)
```

### Failure Notifications

**Slack Integration**:
```yaml
- name: Notify Slack on failure
  if: failure()
  uses: 8398a7/action-slack@v3
  with:
    status: ${{ job.status }}
    webhook_url: ${{ secrets.SLACK_WEBHOOK }}
```

**Email Notifications**:
- Configure in GitHub account settings
- Receive emails on workflow failures
- Customize notification preferences

### Metrics and Analytics

**Key Metrics to Track**:
- Build success rate
- Average build time
- Test pass rate
- Code coverage trend
- Deployment frequency
- Mean time to recovery (MTTR)

**Tools**:
- GitHub Insights
- Third-party analytics (e.g., DataDog, New Relic)

## Troubleshooting

### Common Issues

#### Build Failures

**Symptom**: Build fails with compilation errors

**Solutions**:
1. Check for breaking changes in dependencies
2. Verify .NET SDK version matches requirements
3. Clear NuGet cache: `dotnet nuget locals all --clear`
4. Review recent code changes

**Debug Steps**:
```yaml
- name: Build with verbose output
  run: dotnet build --verbosity detailed
```

#### Test Failures

**Symptom**: Tests fail in CI but pass locally

**Possible Causes**:
1. Environment differences
2. Race conditions (timing issues)
3. Missing test data
4. Hardcoded paths

**Solutions**:
1. Use environment-independent paths
2. Add proper test data setup
3. Implement retry logic for flaky tests
4. Use `[Theory]` for data-driven tests

#### Cache Issues

**Symptom**: Build slower than expected or dependency errors

**Solutions**:
1. Clear cache manually:
   - Go to Actions → Caches
   - Delete old caches
2. Update cache key in workflow
3. Verify `packages.lock.json` is committed

#### Artifact Upload Failures

**Symptom**: Artifacts fail to upload

**Solutions**:
1. Check artifact size (< 10GB limit)
2. Verify path exists
3. Ensure runner has write permissions

#### Deployment Failures

**Symptom**: Deployment job fails

**Debug Checklist**:
1. Verify secrets are configured
2. Check environment permissions
3. Validate artifact availability
4. Review deployment logs
5. Verify target environment accessibility

### Debugging Workflows

**Enable Debug Logging**:

1. **Repository level**:
   - Settings → Secrets → Add `ACTIONS_RUNNER_DEBUG` = `true`
   - Add `ACTIONS_STEP_DEBUG` = `true`

2. **Workflow level**:
```yaml
- name: Debug step
  run: |
    echo "::debug::Debug message"
    echo "Current directory: $(pwd)"
    ls -la
```

**Interactive Debugging**:

Use `tmate` for SSH access to runner:
```yaml
- name: Setup tmate session
  uses: mxschmitt/action-tmate@v3
  if: failure()
```

### Performance Optimization

**Slow Builds**:
1. Enable NuGet package caching
2. Use incremental builds
3. Parallelize independent jobs
4. Use self-hosted runners for better performance

**Slow Tests**:
1. Run tests in parallel
2. Use test categorization
3. Run unit tests before integration tests
4. Skip UI tests in PR builds

### Getting Help

**Resources**:
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [.NET CLI Documentation](https://docs.microsoft.com/en-us/dotnet/core/tools/)
- Project maintainers

**Support Channels**:
- GitHub Issues
- Project Discord/Slack
- Developer forums

## Best Practices Checklist

- [ ] All workflows have descriptive names and comments
- [ ] Secrets are never hardcoded
- [ ] Dependencies are pinned to specific versions
- [ ] Caching is implemented for dependencies
- [ ] Tests run on every PR
- [ ] Build artifacts are retained appropriately
- [ ] Deployment requires manual approval for production
- [ ] Rollback procedures are documented
- [ ] Monitoring and alerting are configured
- [ ] Workflow files are linted and validated
- [ ] Documentation is kept up to date

## Related Documentation

- [Deployment Strategies Guide](./deployment-strategies.md)
- [Build & Deployment Process](../codebase-analysis/06-workflow/build-deployment.md)
- [Testing Strategy](../testing/testing-strategy.md) (if available)

---

**Last Updated**: November 2025  
**Maintainer**: DevOps Team  
**Version**: 1.0
