# Azure DevOps Pipeline Templates

This directory contains Azure DevOps pipeline templates for the UniGetUI project. These pipelines provide an alternative to GitHub Actions for organizations using Azure DevOps.

## 📋 Available Pipelines

### [Build Pipeline](./build-pipeline.yml)
**Purpose**: Compile and validate the UniGetUI application

**Features**:
- Multi-branch support (main, develop)
- NuGet package caching
- Artifact publishing
- Build summaries

**Triggers**:
- Push to main/develop branches
- Pull requests to main/develop
- Manual trigger

**Usage**:
1. Create a new pipeline in Azure DevOps
2. Select existing YAML file
3. Choose `azure-pipelines/build-pipeline.yml`
4. Run the pipeline

### [Test Pipeline](./test-pipeline.yml)
**Purpose**: Execute comprehensive test suites

**Features**:
- Unit tests with code coverage
- Test result publishing
- Code quality analysis
- Scheduled daily runs

**Triggers**:
- Push to main/develop branches
- Pull requests
- Scheduled (daily at 2 AM UTC)
- Manual trigger

**Usage**:
1. Create a new pipeline in Azure DevOps
2. Select existing YAML file
3. Choose `azure-pipelines/test-pipeline.yml`
4. Run the pipeline

### [Deploy Pipeline](./deploy-pipeline.yml)
**Purpose**: Deploy releases with multiple strategies

**Features**:
- Multiple deployment strategies (direct, blue-green, canary)
- Environment-specific deployments (staging, production)
- Release validation
- Automated release notes generation

**Triggers**:
- Manual trigger only (with parameters)

**Parameters**:
- `environment`: staging or production
- `deploymentStrategy`: direct, blue-green, or canary
- `releaseTag`: version tag to deploy

**Usage**:
1. Create a new pipeline in Azure DevOps
2. Select existing YAML file
3. Choose `azure-pipelines/deploy-pipeline.yml`
4. Run with parameters

## 🚀 Getting Started

### Prerequisites

1. **Azure DevOps Organization**: Create or use existing organization
2. **Project**: Create a project for UniGetUI
3. **Service Connections**: Configure connections to external services
4. **Variable Groups**: Set up required variables

### Setup Instructions

#### 1. Create Pipeline

```bash
# Using Azure CLI
az pipelines create \
  --name "UniGetUI-Build" \
  --repository UniGetUI \
  --branch main \
  --yml-path azure-pipelines/build-pipeline.yml
```

Or via Azure DevOps UI:
1. Go to Pipelines → New Pipeline
2. Select repository
3. Choose "Existing Azure Pipelines YAML file"
4. Select the pipeline YAML file
5. Save and run

#### 2. Configure Variables

Create a variable group named `UniGetUI-Variables`:

| Variable | Value | Secret |
|----------|-------|--------|
| `NUGET_PACKAGES` | `$(Pipeline.Workspace)/.nuget/packages` | No |
| `CODE_SIGNING_CERT` | (Certificate content) | Yes |
| `CODE_SIGNING_PASSWORD` | (Certificate password) | Yes |
| `WINGET_TOKEN` | (WinGet submission token) | Yes |

#### 3. Set Up Environments

Create environments in Azure DevOps:

**Staging Environment**:
- Name: `staging`
- Approvals: None (automatic)
- Checks: None

**Production Environment**:
- Name: `production`
- Approvals: At least 1 approver required
- Checks: 
  - Branch control (only from main)
  - Business hours restriction (optional)

#### 4. Configure Service Connections

If deploying to external services:
1. Go to Project Settings → Service connections
2. Create connections as needed:
   - GitHub (for releases)
   - Azure Storage (for artifacts)
   - Other deployment targets

## 🔧 Pipeline Configuration

### Build Configuration

**Variables**:
```yaml
variables:
  buildConfiguration: 'Release'
  buildPlatform: 'x64'
  dotnetVersion: '8.0.x'
```

**Customize**:
Edit `azure-pipelines/build-pipeline.yml` to modify:
- Build configuration
- Target platform
- .NET version
- Artifact paths

### Test Configuration

**Variables**:
```yaml
variables:
  buildConfiguration: 'Release'
  dotnetVersion: '8.0.x'
```

**Customize**:
Edit `azure-pipelines/test-pipeline.yml` to modify:
- Test categories
- Code coverage thresholds
- Schedule

### Deploy Configuration

**Parameters**:
```yaml
parameters:
- name: environment
  default: 'staging'
  values: [staging, production]
- name: deploymentStrategy
  default: 'direct'
  values: [direct, blue-green, canary]
- name: releaseTag
  default: 'latest'
```

## 📊 Pipeline Features

### Caching

All pipelines use Azure Pipelines caching for NuGet packages:

```yaml
- task: Cache@2
  inputs:
    key: 'nuget | "$(Agent.OS)" | **/packages.lock.json'
    path: $(NUGET_PACKAGES)
```

**Benefits**:
- Faster builds (30-50% faster)
- Reduced bandwidth usage
- Improved reliability

### Artifacts

Build artifacts are published and retained according to project settings:

```yaml
- task: PublishBuildArtifacts@1
  inputs:
    PathtoPublish: '$(Build.ArtifactStagingDirectory)'
    ArtifactName: 'unigetui-$(buildConfiguration)'
```

**Artifact Types**:
- Build output
- Test results
- Code coverage reports
- Deployment packages
- Release notes

### Multi-Stage Pipelines

Deploy pipeline uses stages for better organization:

```
Validate → Build → Deploy → Generate Release Notes → Summary
```

Each stage can have:
- Dependencies on other stages
- Approval gates
- Environment targeting
- Custom conditions

## 🔐 Security Best Practices

1. **Secrets Management**:
   - Use Azure Key Vault for sensitive data
   - Never hardcode secrets in YAML
   - Use variable groups with secret variables

2. **Service Connections**:
   - Limit scope of service connections
   - Use managed identities when possible
   - Regularly rotate credentials

3. **Branch Protection**:
   - Require build validation for PRs
   - Require minimum reviewers
   - Protect main branch

4. **Approvals**:
   - Require manual approval for production
   - Use approval gates with checks
   - Configure timeout policies

## 🔍 Monitoring and Logs

### Pipeline Analytics

View pipeline analytics in Azure DevOps:
1. Go to Pipelines → Analytics
2. View metrics:
   - Pass rate
   - Duration
   - Test results
   - Code coverage

### Logs

Access detailed logs:
1. Select pipeline run
2. Click on job/task
3. View logs in real-time or download

### Notifications

Configure notifications:
1. Project Settings → Notifications
2. Create subscription:
   - Build completion
   - Build failure
   - Test failures
   - Deployment completion

## 🆚 Comparison with GitHub Actions

| Feature | Azure Pipelines | GitHub Actions |
|---------|----------------|----------------|
| **Platform** | Azure DevOps | GitHub |
| **YAML Syntax** | Similar but different | YAML |
| **Environments** | Built-in | Built-in |
| **Approvals** | Native support | Via environments |
| **Caching** | Cache task | actions/cache |
| **Artifacts** | Artifacts tab | Actions artifacts |
| **Minutes (Free)** | 1,800/month | 2,000/month |
| **Self-hosted** | Yes | Yes |

**When to use Azure Pipelines**:
- Already using Azure DevOps
- Need advanced approval workflows
- Prefer integrated project management
- Using Azure services extensively

**When to use GitHub Actions**:
- Code hosted on GitHub
- Simpler setup
- Better GitHub integration
- Community actions ecosystem

## 📚 Documentation

### Azure Pipelines Documentation
- [Azure Pipelines Docs](https://docs.microsoft.com/en-us/azure/devops/pipelines/)
- [YAML Schema Reference](https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema/)
- [Pipeline Tasks](https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/)

### Related Documentation
- [CI/CD Guide](../docs/devops/ci-cd-guide.md)
- [Deployment Strategies](../docs/devops/deployment-strategies.md)
- [Build & Deployment Process](../docs/codebase-analysis/06-workflow/build-deployment.md)

## 🤝 Contributing

When modifying Azure Pipelines:

1. Test in a separate branch first
2. Validate YAML syntax
3. Update this README if adding new pipelines
4. Document any new variables or parameters
5. Test all deployment strategies

## 📞 Support

For issues with Azure Pipelines:
- Check Azure DevOps status page
- Review pipeline logs
- Consult Azure Pipelines documentation
- Open an issue in the repository

---

**Last Updated**: November 2025  
**Maintained by**: DevOps Team
