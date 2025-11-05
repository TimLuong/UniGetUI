# Deployment Strategies Guide

## Table of Contents

- [Overview](#overview)
- [Deployment Strategies](#deployment-strategies)
- [Strategy Selection Guide](#strategy-selection-guide)
- [Implementation Details](#implementation-details)
- [Rollback Procedures](#rollback-procedures)
- [Environment Promotion](#environment-promotion)
- [Release Notes Automation](#release-notes-automation)
- [Container Deployment](#container-deployment)
- [Best Practices](#best-practices)

## Overview

This guide describes the various deployment strategies available for the UniGetUI project and provides guidance on when and how to use each strategy.

### Deployment Goals

- **Zero-downtime**: Minimize or eliminate service interruption
- **Fast rollback**: Quick recovery from problematic releases
- **Risk mitigation**: Reduce impact of deployment issues
- **Gradual rollout**: Control exposure to new versions
- **Monitoring**: Track deployment health and metrics

## Deployment Strategies

### 1. Direct Deployment

**Description**: Immediate replacement of the old version with the new version.

**Characteristics**:
- Simplest strategy
- Fastest deployment time
- Brief downtime possible
- All users get new version immediately

**When to Use**:
- Stable releases with comprehensive testing
- Patch releases with critical fixes
- Low-risk updates
- Non-production environments

**Workflow**:
```
┌──────────────┐
│   Build      │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│   Deploy     │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│   Verify     │
└──────────────┘
```

**Implementation**:

The direct deployment is the default strategy in `deploy.yml`:

```yaml
deploy-production-direct:
  name: Deploy to Production (Direct)
  runs-on: ubuntu-latest
  environment:
    name: production
  steps:
    - name: Download artifacts
      uses: actions/download-artifact@v4
    - name: Deploy to production
      run: |
        # Deploy new version immediately
        # For UniGetUI: GitHub Release + WinGet auto-update
```

**Advantages**:
- ✅ Simple and straightforward
- ✅ Fast deployment
- ✅ Easy to understand
- ✅ Minimal infrastructure required

**Disadvantages**:
- ❌ Higher risk
- ❌ Potential downtime
- ❌ All users affected simultaneously
- ❌ Rollback requires new deployment

**Rollback Time**: 5-15 minutes

### 2. Blue-Green Deployment

**Description**: Two identical production environments (Blue and Green). Traffic switches between them.

**Characteristics**:
- Zero-downtime deployments
- Instant rollback capability
- Requires double infrastructure
- Easy to test in production-like environment

**When to Use**:
- Major version releases
- High-availability requirements
- When instant rollback is critical
- Infrastructure supports parallel environments

**Workflow**:
```
┌──────────────┐      ┌──────────────┐
│  Blue (Live) │      │ Green (Idle) │
│   Version 1  │      │              │
└──────────────┘      └──────┬───────┘
                             │
                             ▼
                      ┌──────────────┐
                      │    Deploy    │
                      │  Version 2   │
                      └──────┬───────┘
                             │
                             ▼
                      ┌──────────────┐
                      │  Health      │
                      │  Check       │
                      └──────┬───────┘
                             │
                             ▼
┌──────────────┐      ┌──────────────┐
│  Blue (Idle) │◄─────┤Switch Traffic│
│   Version 1  │      │              │
└──────────────┘      └──────┬───────┘
                             │
                             ▼
                      ┌──────────────┐
                      │ Green (Live) │
                      │   Version 2  │
                      └──────────────┘
```

**Implementation**:

```yaml
deploy-production-blue-green:
  name: Deploy to Production (Blue-Green)
  steps:
    - name: Deploy to green environment
      run: |
        echo "Deploying to GREEN environment"
        # Deploy new version to green
        
    - name: Run health checks on green
      run: |
        echo "Running health checks"
        # Verify green environment is healthy
        
    - name: Switch traffic to green
      run: |
        echo "Switching traffic from BLUE to GREEN"
        # Update load balancer/DNS
        
    - name: Monitor green environment
      run: |
        echo "Monitoring GREEN for 60 seconds"
        # Watch metrics
        
    - name: Mark blue as backup
      run: |
        echo "BLUE retained for rollback"
```

**For UniGetUI**:

Since UniGetUI is a desktop application distributed via package managers:

1. **GitHub Releases**: 
   - Old release (Blue) remains available
   - New release (Green) is published
   - Users can choose to update

2. **WinGet/Package Managers**:
   - New version published alongside old version
   - Users update at their own pace
   - Can pin to specific version

3. **Auto-update Feature**:
   - Configure auto-update to roll out gradually
   - Monitor error rates before full rollout

**Advantages**:
- ✅ Zero-downtime deployment
- ✅ Instant rollback (switch back to blue)
- ✅ Full production testing before cutover
- ✅ Easy to validate new version

**Disadvantages**:
- ❌ Requires duplicate infrastructure
- ❌ More complex setup
- ❌ Database migrations can be challenging
- ❌ Increased costs

**Rollback Time**: < 1 minute (instant traffic switch)

### 3. Canary Deployment

**Description**: Gradual rollout to a subset of users, expanding as confidence grows.

**Characteristics**:
- Progressive rollout
- Risk mitigation through limited exposure
- Real-world validation
- Monitoring-driven decision making

**When to Use**:
- High-risk changes
- New features with uncertain impact
- Large user base
- When gradual validation is needed

**Workflow**:
```
Stage 1: Deploy to 10% of users
   ↓
Monitor metrics for 5-10 minutes
   ↓
Stage 2: Expand to 25%
   ↓
Monitor metrics for 5-10 minutes
   ↓
Stage 3: Expand to 50%
   ↓
Monitor metrics for 5-10 minutes
   ↓
Stage 4: Full rollout (100%)
```

**Implementation**:

```yaml
deploy-production-canary:
  name: Deploy to Production (Canary)
  steps:
    - name: Deploy canary (10% traffic)
      run: |
        echo "Deploying to 10% of users"
        
    - name: Monitor canary metrics
      run: |
        echo "Monitoring for 5 minutes"
        sleep 300
        # Check error rates, performance metrics
        
    - name: Increase to 25% traffic
      run: |
        echo "Increasing to 25%"
        sleep 180
        
    - name: Increase to 50% traffic
      run: |
        echo "Increasing to 50%"
        sleep 180
        
    - name: Full rollout (100%)
      run: |
        echo "Full deployment"
```

**For UniGetUI (Canary Approach)**:

1. **Ring Deployment**:
   - Ring 1 (Insiders/Beta testers): 5% - 24 hours
   - Ring 2 (Early adopters): 20% - 48 hours
   - Ring 3 (General users): 75% - 72 hours
   - Ring 4 (Conservative users): 100% - Auto-update

2. **Implementation via Pre-releases**:
   ```
   v3.4.0-beta.1  → Beta testers (Ring 1)
          ↓
   v3.4.0-beta.2  → Early adopters (Ring 2)
          ↓
   v3.4.0         → General release (Rings 3-4)
   ```

3. **WinGet Pre-release Channel**:
   - Use `MartiCliment.UniGetUI.Pre-Release` for canary
   - Monitor for issues
   - Promote to stable channel when validated

**Advantages**:
- ✅ Minimal blast radius
- ✅ Early problem detection
- ✅ Real-world validation
- ✅ Gradual user impact

**Disadvantages**:
- ❌ Complex routing logic
- ❌ Longer deployment time
- ❌ Requires sophisticated monitoring
- ❌ Multiple versions in production simultaneously

**Rollback Time**: Depends on rollout percentage (5-30 minutes)

### 4. Rolling Deployment

**Description**: Gradually replace instances with new version, one or a few at a time.

**Characteristics**:
- Incremental replacement
- Always maintains capacity
- Automatic rollback on failure
- Good for multi-instance applications

**When to Use**:
- Microservices architecture
- Applications with multiple instances
- Load-balanced environments
- Kubernetes/container deployments

**Workflow**:
```
Instance 1: v1 → v2 (Deploy + Health Check)
Instance 2: v1 → v2 (Deploy + Health Check)
Instance 3: v1 → v2 (Deploy + Health Check)
Instance N: v1 → v2 (Deploy + Health Check)
```

**Note**: Less applicable to desktop applications like UniGetUI, but relevant for supporting web services.

### 5. Feature Flags (Feature Toggle)

**Description**: Deploy code but control feature activation through configuration.

**Characteristics**:
- Decouple deployment from release
- Fine-grained control
- A/B testing capability
- Instant feature rollback

**When to Use**:
- Experimental features
- A/B testing scenarios
- Gradual feature rollout
- Quick feature disable without redeployment

**Implementation Example**:

```csharp
// Feature flag service
public class FeatureFlags
{
    public bool IsNewUIEnabled => GetFlag("new_ui_enabled", false);
    public bool IsAnalyticsEnabled => GetFlag("analytics", true);
    
    private bool GetFlag(string name, bool defaultValue)
    {
        // Check configuration service
        // Can be controlled remotely
        return _configService.GetBool(name, defaultValue);
    }
}

// Usage in code
if (_featureFlags.IsNewUIEnabled)
{
    // Show new UI
}
else
{
    // Show legacy UI
}
```

**For UniGetUI**:

```csharp
// In CoreData or configuration service
public static class FeatureFlags
{
    public static bool EnableNewPackageEngine => 
        GetRemoteFlag("enable_new_package_engine", false);
        
    public static bool EnableTelemetry => 
        GetRemoteFlag("enable_telemetry", true);
}
```

**Advantages**:
- ✅ Instant feature control
- ✅ No redeployment needed
- ✅ A/B testing support
- ✅ Easy rollback

**Disadvantages**:
- ❌ Code complexity
- ❌ Technical debt if not cleaned up
- ❌ Testing challenges
- ❌ Requires flag management system

## Strategy Selection Guide

### Decision Matrix

| Criteria | Direct | Blue-Green | Canary | Rolling | Feature Flags |
|----------|--------|------------|--------|---------|---------------|
| **Risk Level** | High | Low | Very Low | Low | Very Low |
| **Complexity** | Low | Medium | High | Medium | High |
| **Rollback Speed** | Slow | Instant | Medium | Medium | Instant |
| **Infrastructure Cost** | Low | High | Medium | Medium | Low |
| **User Impact** | All at once | All at once | Gradual | Gradual | Controlled |
| **Best for Desktop Apps** | ✅ | ⚠️ | ⚠️ | ❌ | ✅ |
| **Best for Web Services** | ⚠️ | ✅ | ✅ | ✅ | ✅ |

### Recommended Strategy by Release Type

| Release Type | Recommended Strategy | Rationale |
|-------------|---------------------|-----------|
| **Patch Release (x.x.1)** | Direct | Low risk, quick deployment |
| **Minor Release (x.1.0)** | Blue-Green or Canary | Medium risk, new features |
| **Major Release (1.0.0)** | Canary | High risk, significant changes |
| **Hotfix** | Direct | Urgent, critical fixes |
| **Beta/Pre-release** | Canary | User opt-in, testing focus |
| **Experimental Feature** | Feature Flags | Controlled exposure |

## Implementation Details

### UniGetUI Deployment Process

#### Current Setup

1. **Build**: GitHub Actions builds the application
2. **Test**: Automated tests validate functionality
3. **Package**: Create installer and ZIP packages
4. **Release**: Publish to GitHub Releases
5. **Distribute**: Auto-submit to WinGet, manual for other channels

#### Enhanced Deployment Workflow

**1. Pre-release (Canary)**:
```bash
# Developer creates pre-release
git tag v3.4.0-beta.1
git push origin v3.4.0-beta.1

# GitHub Actions triggers
# → Build & test
# → Create installer
# → Publish pre-release
# → Submit to WinGet Pre-release channel
# → Notify beta testers
```

**2. Stable Release (Direct/Blue-Green)**:
```bash
# After beta validation
git tag v3.4.0
git push origin v3.4.0

# GitHub Actions triggers
# → Build & test
# → Create installer
# → Publish stable release
# → Submit to WinGet stable channel
# → Update Microsoft Store
# → Update Chocolatey/Scoop
# → Generate release notes
```

### Deployment Checklist

Pre-deployment:
- [ ] All tests passing
- [ ] Code review completed
- [ ] Security scan passed (CodeQL)
- [ ] Version numbers updated
- [ ] Release notes prepared
- [ ] Breaking changes documented
- [ ] Rollback plan ready

Deployment:
- [ ] Backup current version
- [ ] Deploy to staging first
- [ ] Run smoke tests
- [ ] Deploy to production
- [ ] Monitor key metrics
- [ ] Verify functionality

Post-deployment:
- [ ] Update documentation
- [ ] Announce release
- [ ] Monitor error reports
- [ ] Track adoption rate
- [ ] Gather user feedback

## Rollback Procedures

### Automated Rollback

**Trigger Conditions**:
- Error rate exceeds threshold (> 5%)
- Critical functionality broken
- Performance degradation (> 50%)
- Health check failures

**Automated Rollback Script**:
```yaml
- name: Monitor and auto-rollback
  run: |
    # Monitor for 10 minutes
    for i in {1..60}; do
      ERROR_RATE=$(check_error_rate)
      if [ $ERROR_RATE -gt 5 ]; then
        echo "Error rate too high: $ERROR_RATE%"
        rollback_deployment
        notify_team "Auto-rollback triggered"
        exit 1
      fi
      sleep 10
    done
```

### Manual Rollback Procedures

#### 1. Rolling Back a GitHub Release

**Quick Rollback (Mark as Pre-release)**:
```bash
# Via GitHub CLI
gh release edit v3.4.0 --prerelease

# Or manually:
# 1. Go to Releases page
# 2. Edit the release
# 3. Check "This is a pre-release"
# 4. Add warning message
```

**Full Rollback (Delete and Recreate)**:
```bash
# Delete problematic release
gh release delete v3.4.0 --yes

# Users can downgrade via WinGet
winget install --version 3.3.9 MartiCliment.UniGetUI
```

#### 2. Rolling Back WinGet Package

**Option A: Submit Previous Version**:
```bash
# WinGet maintains version history
# Users can install specific version:
winget install --version 3.3.9 MartiCliment.UniGetUI
```

**Option B: Update Package Manifest**:
```bash
# Fork WinGet repository
# Update manifest to point to stable version
# Submit PR to winget-pkgs
```

#### 3. Rolling Back Blue-Green Deployment

**Instant Rollback**:
```bash
# Switch traffic back to blue environment
# This is the primary advantage of blue-green

# In deploy.yml
- name: Rollback to blue
  run: |
    echo "Switching traffic back to BLUE environment"
    # Reverse load balancer configuration
```

#### 4. Rolling Back Canary Deployment

**Stop Rollout**:
```bash
# Halt further expansion
# Keep affected users on canary
# Fix issue and redeploy

- name: Halt canary rollout
  run: |
    echo "Stopping at current percentage"
    # Don't expand to next stage
    # Monitor and fix
```

### Rollback Communication

**User Communication Template**:
```markdown
## Rollback Notice - Version 3.4.0

We've identified an issue with version 3.4.0 and are rolling back to version 3.3.9.

**Impact**: [Describe the issue]

**Action Required**: 
- If you're experiencing issues, please downgrade:
  ```
  winget install --version 3.3.9 MartiCliment.UniGetUI
  ```

**Timeline**:
- Issue detected: [Time]
- Rollback initiated: [Time]
- Estimated resolution: [Time]

**Next Steps**: We're working on a fix and will release version 3.4.1 once resolved.

Thank you for your patience.
```

## Environment Promotion

### Environment Hierarchy

```
Development → Staging → Production
    ↓            ↓           ↓
 Feature    Pre-release   Stable
  Branch       Beta       Release
```

### Promotion Process

#### Development → Staging

**Trigger**: Pull request merged to `develop` branch

**Process**:
1. Automated build triggered
2. Unit tests executed
3. Deploy to staging environment
4. Integration tests executed
5. Smoke tests executed

**Approval**: Automatic (if tests pass)

#### Staging → Production

**Trigger**: Release tag created

**Process**:
1. Full test suite executed
2. Security scan completed
3. Build release artifacts
4. Manual approval required (for major releases)
5. Deploy to production
6. Post-deployment verification

**Approval**: Manual (recommended) or automatic (patch releases)

### Configuration Management

**Environment-Specific Configuration**:

```yaml
# staging.config.json
{
  "environment": "staging",
  "apiUrl": "https://api-staging.example.com",
  "logLevel": "debug",
  "telemetry": true,
  "featureFlags": {
    "enableBetaFeatures": true
  }
}

# production.config.json
{
  "environment": "production",
  "apiUrl": "https://api.example.com",
  "logLevel": "info",
  "telemetry": true,
  "featureFlags": {
    "enableBetaFeatures": false
  }
}
```

**Loading Configuration**:
```csharp
public class EnvironmentConfig
{
    public static Config Load()
    {
        var env = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "production";
        var configFile = $"{env}.config.json";
        return JsonSerializer.Deserialize<Config>(File.ReadAllText(configFile));
    }
}
```

## Release Notes Automation

### Automatic Generation

**Using GitHub CLI**:
```bash
# Generate release notes from commits
gh release create v3.4.0 \
  --title "Version 3.4.0" \
  --generate-notes \
  --notes-file RELEASE_NOTES.md
```

**Custom Release Notes Script**:
```python
# scripts/generate_release_notes.py
import subprocess
import sys

def get_commits(from_tag, to_tag):
    """Get commits between two tags"""
    cmd = f"git log {from_tag}..{to_tag} --pretty=format:'- %s (%h)' --no-merges"
    result = subprocess.run(cmd, shell=True, capture_output=True, text=True)
    return result.stdout

def categorize_commits(commits):
    """Categorize commits by type"""
    features = []
    fixes = []
    other = []
    
    for commit in commits.split('\n'):
        if 'feat:' in commit.lower():
            features.append(commit)
        elif 'fix:' in commit.lower():
            fixes.append(commit)
        else:
            other.append(commit)
    
    return features, fixes, other

def generate_notes(version, from_tag, to_tag):
    """Generate formatted release notes"""
    commits = get_commits(from_tag, to_tag)
    features, fixes, other = categorize_commits(commits)
    
    notes = f"""# Release Notes - Version {version}

## 🎉 New Features
{chr(10).join(features) if features else '- No new features'}

## 🐛 Bug Fixes
{chr(10).join(fixes) if fixes else '- No bug fixes'}

## 🔧 Other Changes
{chr(10).join(other) if other else '- No other changes'}

## 📦 Installation

Download from the [releases page](https://github.com/marticliment/UniGetUI/releases).

Via WinGet:
```
winget install MartiCliment.UniGetUI
```

## 🙏 Contributors

Thank you to all contributors who made this release possible!
"""
    
    return notes

if __name__ == "__main__":
    version = sys.argv[1]
    from_tag = sys.argv[2]
    to_tag = sys.argv[3]
    
    notes = generate_notes(version, from_tag, to_tag)
    print(notes)
```

**Usage in Workflow**:
```yaml
- name: Generate release notes
  run: |
    PREVIOUS_TAG=$(git describe --tags --abbrev=0 HEAD^)
    python scripts/generate_release_notes.py \
      ${{ needs.validate-release.outputs.version }} \
      $PREVIOUS_TAG \
      ${{ github.ref }} > release-notes.md
```

### Release Notes Template

```markdown
# UniGetUI v{VERSION}

## 📋 Overview
Brief description of this release and its main highlights.

## ✨ What's New
### New Features
- Feature 1: Description
- Feature 2: Description

### Improvements
- Improvement 1: Description
- Improvement 2: Description

## 🐛 Bug Fixes
- Fix 1: Description
- Fix 2: Description

## ⚠️ Breaking Changes
- Breaking change 1: Description and migration guide
- Breaking change 2: Description and migration guide

## 📦 Downloads
- [UniGetUI.Installer.exe](link) (Recommended)
- [UniGetUI.x64.zip](link) (Portable)

**SHA256 Checksums:**
```
UniGetUI.Installer.exe: {CHECKSUM}
UniGetUI.x64.zip: {CHECKSUM}
```

## 📝 Upgrade Instructions
For WinGet users:
```bash
winget upgrade MartiCliment.UniGetUI
```

For manual installations:
1. Download the installer
2. Run the installer
3. Existing settings will be preserved

## 🔄 Rollback Instructions
If you encounter issues:
```bash
winget install --version {PREVIOUS_VERSION} MartiCliment.UniGetUI
```

## 🙏 Acknowledgments
Thank you to all contributors:
- @contributor1
- @contributor2

## 📚 Documentation
- [Full Changelog](link)
- [Documentation](link)
- [Known Issues](link)

---
Released: {DATE}
```

## Container Deployment

### Docker Support (Optional)

While UniGetUI is primarily a Windows desktop application, supporting services may benefit from containerization.

**Dockerfile Example** (for supporting web services):
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Service.csproj", "./"]
RUN dotnet restore "Service.csproj"
COPY . .
RUN dotnet build "Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Service.dll"]
```

**Docker Compose** (for multi-container setup):
```yaml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    depends_on:
      - database
      
  database:
    image: postgres:15
    environment:
      - POSTGRES_PASSWORD=secure_password
    volumes:
      - db-data:/var/lib/postgresql/data

volumes:
  db-data:
```

**Kubernetes Deployment** (for cloud deployments):
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: unigetui-api
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  selector:
    matchLabels:
      app: unigetui-api
  template:
    metadata:
      labels:
        app: unigetui-api
        version: "3.4.0"
    spec:
      containers:
      - name: api
        image: ghcr.io/timluong/unigetui-api:3.4.0
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: unigetui-api-service
spec:
  selector:
    app: unigetui-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: LoadBalancer
```

## Best Practices

### 1. Version Control

- ✅ Use semantic versioning (MAJOR.MINOR.PATCH)
- ✅ Tag releases in Git
- ✅ Maintain changelog
- ✅ Document breaking changes

### 2. Testing

- ✅ Automated tests before every deployment
- ✅ Smoke tests after deployment
- ✅ Integration tests in staging
- ✅ Performance benchmarks

### 3. Monitoring

- ✅ Track deployment metrics
- ✅ Monitor error rates
- ✅ Set up alerts for anomalies
- ✅ Log all deployment activities

### 4. Communication

- ✅ Notify users of upcoming releases
- ✅ Provide clear release notes
- ✅ Announce deployments to team
- ✅ Document rollback procedures

### 5. Security

- ✅ Scan for vulnerabilities (CodeQL)
- ✅ Sign code releases
- ✅ Use secure secrets management
- ✅ Audit deployment access

### 6. Documentation

- ✅ Keep deployment docs updated
- ✅ Document environment configurations
- ✅ Maintain runbooks
- ✅ Record lessons learned

### 7. Automation

- ✅ Automate repetitive tasks
- ✅ Use CI/CD pipelines
- ✅ Implement automated rollbacks
- ✅ Auto-generate release notes

## Conclusion

Choosing the right deployment strategy depends on:
- Application architecture
- Risk tolerance
- User base size
- Infrastructure capabilities
- Team expertise

For UniGetUI:
- **Patch releases**: Direct deployment
- **Minor releases**: Canary (via pre-releases)
- **Major releases**: Canary with extended beta period
- **Hotfixes**: Direct deployment with close monitoring

Always prioritize user experience and system stability when planning deployments.

---

**Last Updated**: November 2025  
**Maintainer**: DevOps Team  
**Version**: 1.0

## See Also

- [CI/CD Guide](./ci-cd-guide.md)
- [Build & Deployment Process](../codebase-analysis/06-workflow/build-deployment.md)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
