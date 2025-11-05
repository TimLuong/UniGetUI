# Package Guidelines

## Overview

This document defines the standards and guidelines for managing dependencies, NuGet packages, and third-party libraries in the UniGetUI project. Following these guidelines ensures consistency, security, and maintainability across the codebase.

## Package Selection Criteria

### Stability and Quality

Before adding a new package dependency, evaluate the following criteria:

1. **Maintenance Status**
   - Package is actively maintained (commits/releases within the last 6 months)
   - Has a responsive maintainer team
   - Issues and pull requests are regularly addressed
   - Clear roadmap and documentation

2. **Popularity and Community**
   - High download count on NuGet (preferably > 1M downloads)
   - Active community and support channels
   - Good documentation and examples
   - Stack Overflow presence for troubleshooting

3. **Quality Indicators**
   - Well-tested with good code coverage
   - Follows semantic versioning
   - Has a clear changelog
   - Minimal open critical issues
   - Good performance characteristics

4. **Compatibility**
   - Supports .NET 8.0 and Windows 10.0.19041.0+
   - Compatible with Windows App SDK
   - Works with target runtime identifiers (win-x64, win-arm64)
   - No conflicts with existing dependencies

### Licensing Requirements

All dependencies must have compatible licenses:

1. **Approved Licenses**
   - MIT License (preferred)
   - Apache License 2.0
   - BSD Licenses (2-Clause, 3-Clause)
   - ISC License
   - CC0 (for assets/data)

2. **Licenses Requiring Legal Review**
   - MPL (Mozilla Public License)
   - LGPL (Lesser GNU Public License)
   - EPL (Eclipse Public License)

3. **Prohibited Licenses**
   - GPL (GNU Public License) - Not compatible with MIT license
   - AGPL (Affero GNU Public License)
   - Any "non-commercial use only" licenses
   - Proprietary licenses without explicit permission

4. **License Compliance**
   - Document all licenses in approved-packages.md
   - Include attribution where required
   - Maintain LICENSE file at repository root
   - Review transitive dependencies for license compatibility

### Security Considerations

1. **Known Vulnerabilities**
   - No critical or high-severity vulnerabilities
   - CVE history should be reviewed
   - Check WhiteSource/Mend scan results
   - Review GitHub Security Advisories

2. **Security Practices**
   - Package is signed (when available)
   - Published by verified/trusted publishers
   - Source code is publicly available for review
   - Has security policy (SECURITY.md)

3. **Supply Chain Security**
   - Verify package authenticity
   - Check for typosquatting attempts
   - Review package dependencies
   - Minimize dependency depth

## Version Management

### Version Pinning Strategy

The project uses **explicit version pinning** for all NuGet packages to ensure build reproducibility and prevent unexpected breaking changes.

#### Version Syntax

```xml
<!-- ✅ CORRECT: Explicit version pinning -->
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.7.250606001" />

<!-- ❌ INCORRECT: Version ranges (not allowed) -->
<PackageReference Include="Microsoft.WindowsAppSDK" Version="[1.7,2.0)" />
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.7.*" />
```

#### Rationale

- **Build Reproducibility**: Same source code produces identical builds
- **Predictable Behavior**: No surprise updates that break functionality
- **Easier Debugging**: Clear understanding of what version is being used
- **Controlled Updates**: Updates happen through deliberate review process

### Update Strategies

#### Automated Dependency Updates

The project uses **Renovate Bot** for automated dependency update management:

- Configured in `.github/renovate.json`
- Automatically creates PRs for package updates
- Dependency Dashboard tracks all available updates
- Manages NuGet packages exclusively

#### Update Review Process

1. **Renovate Creates PR**
   - PR includes version change details
   - Links to release notes and changelog
   - Shows all transitive dependency changes

2. **Review Checklist**
   - [ ] Review release notes for breaking changes
   - [ ] Check for new security vulnerabilities
   - [ ] Verify license hasn't changed
   - [ ] Review transitive dependency changes
   - [ ] Ensure CI/CD passes all checks

3. **Testing Requirements**
   - All unit tests pass (`dotnet test`)
   - Integration tests pass
   - Manual smoke testing for major updates
   - Performance regression testing (if applicable)

4. **Approval and Merge**
   - At least one code review approval
   - All CI checks passing
   - Security scans clear
   - Merge to main branch

#### Manual Update Process

For packages not managed by Renovate or requiring special handling:

1. **Check Current Version**
   ```bash
   dotnet list package --outdated
   ```

2. **Update Package Reference**
   ```bash
   dotnet add package PackageName --version X.Y.Z
   ```

3. **Test Changes**
   ```bash
   dotnet restore
   dotnet build
   dotnet test
   ```

4. **Commit with Context**
   ```
   Update PackageName from X.Y.Z to A.B.C
   
   Breaking changes: [list any breaking changes]
   New features: [list relevant new features]
   Fixes: [list relevant bug fixes]
   
   Related issue: TimLuong/UniGetUI#XXX
   ```

### Update Frequency

- **Security Updates**: Immediate (within 1-2 days)
- **Major Updates**: Quarterly review cycle
- **Minor/Patch Updates**: Monthly review cycle
- **Critical Dependencies**: Review within 1 week of release

## Dependency Management

### Centralized Package Management

The project uses **Directory.Build.props** for centralized configuration:

**Location**: `/src/Directory.Build.props`

**Purpose**:
- Defines common project properties
- Sets target framework (.NET 8.0)
- Configures Windows SDK version
- Enforces code style in build

**Benefits**:
- Consistent configuration across all projects
- Single source of truth for build settings
- Easier maintenance and updates

### Transitive Dependencies

#### Understanding Transitive Dependencies

Transitive dependencies are packages required by your direct dependencies. They are automatically resolved by NuGet but require monitoring.

#### Managing Transitive Dependencies

1. **Visibility**
   ```bash
   # List all dependencies including transitive
   dotnet list package --include-transitive
   ```

2. **Version Conflicts**
   - NuGet resolves to highest compatible version
   - Use explicit PackageReference to override if needed
   - Document any version overrides with reasoning

3. **Security Scanning**
   - WhiteSource/Mend scans all transitive dependencies
   - Renovate tracks transitive dependency updates
   - CodeQL analyzes code from all dependencies

4. **Dependency Graph**
   ```bash
   # Generate dependency graph
   dotnet list package --include-transitive --framework net8.0-windows10.0.26100.0
   ```

### Dependency Minimization

#### Principles

1. **Evaluate Need vs. Convenience**
   - Can functionality be implemented in-house?
   - Does the package provide significant value?
   - What is the maintenance cost?

2. **Prefer Lightweight Packages**
   - Smaller packages with focused functionality
   - Fewer transitive dependencies
   - Better performance characteristics

3. **Consolidate Similar Packages**
   - Use one JSON library, not multiple
   - Use one HTTP client approach
   - Standardize on one testing framework

#### Package Alternatives

Before adding a new package, consider:

1. **.NET Built-in Functionality**
   - System.Text.Json instead of Newtonsoft.Json (where possible)
   - HttpClient instead of third-party HTTP libraries
   - System.Linq instead of helper libraries

2. **Windows App SDK Features**
   - Use built-in controls before third-party controls
   - Leverage platform capabilities
   - Use Windows Community Toolkit when needed

3. **Existing Dependencies**
   - Can an existing package provide the functionality?
   - Extend existing packages before adding new ones

## Private NuGet Feeds

### When to Use Private Feeds

Private NuGet feeds are used for:

1. **Internal Packages**
   - Shared libraries across multiple projects
   - Internal tools and utilities
   - Pre-release packages for testing

2. **Forked Dependencies**
   - Modified versions of public packages
   - Bug fixes pending upstream merge
   - Custom builds for specific needs

### Setup and Configuration

#### Azure DevOps Artifacts (Recommended)

1. **Create Feed**
   - Navigate to Azure DevOps project
   - Create new Artifacts feed
   - Configure permissions and access

2. **Configure NuGet.config**
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
     <packageSources>
       <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
       <add key="UniGetUI-Private" value="https://pkgs.dev.azure.com/organization/project/_packaging/feed/nuget/v3/index.json" />
     </packageSources>
   </configuration>
   ```

3. **Authentication**
   - Use Azure Artifacts Credential Provider
   - Configure in CI/CD pipelines
   - Use Personal Access Tokens (PATs) for local development

#### GitHub Packages (Alternative)

1. **Publishing**
   ```bash
   dotnet nuget push package.nupkg --source github --api-key YOUR_PAT
   ```

2. **Configure Source**
   ```xml
   <packageSources>
     <add key="github" value="https://nuget.pkg.github.com/TimLuong/index.json" />
   </packageSources>
   ```

### Best Practices

1. **Package Versioning**
   - Follow semantic versioning strictly
   - Use pre-release tags for beta versions (1.0.0-beta.1)
   - Document changes in package release notes

2. **Package Metadata**
   - Clear package description
   - Project URL and documentation
   - License information
   - Tags for discoverability

3. **Retention Policies**
   - Keep at least last 3 major versions
   - Keep all versions for last 12 months
   - Archive old versions, don't delete

## Package Addition Workflow

### Step-by-Step Process

1. **Evaluate Need**
   - Document why the package is needed
   - Explore alternatives (built-in functionality, existing packages)
   - Get approval from team lead for significant dependencies

2. **Package Assessment**
   - [ ] Review package selection criteria (stability, maintenance, licensing)
   - [ ] Check security vulnerabilities on GitHub Advisory Database
   - [ ] Review license compatibility
   - [ ] Verify .NET 8.0 and Windows compatibility
   - [ ] Check package size and transitive dependencies

3. **Add Package Reference**
   ```bash
   cd src/ProjectName
   dotnet add package PackageName --version X.Y.Z
   ```

4. **Document Addition**
   - Add entry to `approved-packages.md`
   - Include justification and use case
   - Document license and version

5. **Test Integration**
   - [ ] Build solution (`dotnet build`)
   - [ ] Run tests (`dotnet test`)
   - [ ] Verify no conflicts with existing packages
   - [ ] Test relevant functionality

6. **Security Review**
   - [ ] Run WhiteSource/Mend scan
   - [ ] Review scan results
   - [ ] Address any vulnerabilities found

7. **Create Pull Request**
   ```
   Add PackageName dependency
   
   Purpose: [Why this package is needed]
   Use case: [Where and how it will be used]
   Alternatives considered: [What else was evaluated]
   
   Package details:
   - Version: X.Y.Z
   - License: MIT
   - Downloads: 10M+
   - Last updated: [Date]
   
   Security: No known vulnerabilities
   Testing: All tests passing
   
   Related issue: TimLuong/UniGetUI#XXX
   ```

8. **Review and Approval**
   - Code review by at least one maintainer
   - Security scan approval
   - CI/CD passes all checks

## Package Removal Workflow

### When to Remove a Package

Consider removing a package when:

1. **No Longer Needed**
   - Functionality no longer used
   - Replaced by built-in features
   - Feature removed from application

2. **Better Alternative Available**
   - More maintained package available
   - Better performance characteristics
   - Improved functionality

3. **Security Concerns**
   - Unresolved critical vulnerabilities
   - Package no longer maintained
   - Malicious package discovery

4. **License Issues**
   - License changed to incompatible terms
   - Legal compliance requirements

### Removal Process

1. **Impact Analysis**
   ```bash
   # Find all usages
   grep -r "using PackageName" src/
   
   # Check dependencies
   dotnet list package --include-transitive | grep PackageName
   ```

2. **Plan Migration**
   - Document alternative approach
   - Identify all affected code
   - Create migration checklist

3. **Remove Package References**
   ```bash
   dotnet remove package PackageName
   ```

4. **Update Code**
   - Remove using statements
   - Replace functionality with alternatives
   - Update related tests

5. **Testing**
   - [ ] All unit tests pass
   - [ ] Integration tests pass
   - [ ] Manual testing of affected features
   - [ ] Performance testing (if applicable)

6. **Update Documentation**
   - Remove from approved-packages.md
   - Update architecture documentation
   - Note removal in changelog

## Build and Restore

### NuGet Restore

```bash
# Restore all packages
dotnet restore

# Restore for specific project
dotnet restore src/ProjectName/ProjectName.csproj

# Force re-download of packages
dotnet restore --force

# Restore with specific source
dotnet restore --source https://api.nuget.org/v3/index.json
```

### Build Configuration

```bash
# Debug build
dotnet build

# Release build (with optimizations)
dotnet build -c Release

# Build specific project
dotnet build src/ProjectName/ProjectName.csproj
```

### Troubleshooting

#### Package Restore Failures

1. **Clear NuGet Cache**
   ```bash
   dotnet nuget locals all --clear
   dotnet restore
   ```

2. **Verify Package Source**
   ```bash
   dotnet nuget list source
   ```

3. **Check Network/Proxy**
   - Verify internet connectivity
   - Configure proxy if needed in NuGet.config

4. **Version Conflicts**
   ```bash
   # Check for conflicts
   dotnet list package --include-transitive
   
   # Force specific version if needed
   dotnet add package PackageName --version X.Y.Z
   ```

#### Build Errors After Package Update

1. **Check Breaking Changes**
   - Review package release notes
   - Check API changes

2. **Update Code**
   - Fix compilation errors
   - Update deprecated API usage

3. **Regenerate obj/bin**
   ```bash
   # Clean build artifacts
   dotnet clean
   
   # Rebuild
   dotnet build
   ```

## Compliance and Auditing

### Regular Audits

Perform quarterly dependency audits:

1. **Security Audit**
   - Review all WhiteSource/Mend alerts
   - Check for new CVEs
   - Update vulnerable packages

2. **License Audit**
   - Verify all licenses remain compatible
   - Check for license changes in updates
   - Update license documentation

3. **Maintenance Audit**
   - Identify unmaintained packages
   - Find packages with better alternatives
   - Plan for deprecation/replacement

4. **Usage Audit**
   - Remove unused packages
   - Consolidate similar functionality
   - Optimize dependency tree

### Compliance Reporting

Generate reports for compliance:

```bash
# List all packages with versions
dotnet list package --include-transitive > dependency-report.txt

# Check for vulnerabilities
dotnet list package --vulnerable --include-transitive
```

## Best Practices Summary

### DO

✅ Pin exact package versions  
✅ Review package security before adding  
✅ Document new dependencies in approved-packages.md  
✅ Test thoroughly after updates  
✅ Use Renovate for automated updates  
✅ Review transitive dependencies  
✅ Keep packages up-to-date for security  
✅ Remove unused dependencies  
✅ Use centralized configuration (Directory.Build.props)  
✅ Follow license compliance requirements  

### DON'T

❌ Use version ranges (1.0.*)  
❌ Add packages without evaluation  
❌ Ignore security vulnerabilities  
❌ Skip testing after updates  
❌ Use GPL-licensed packages  
❌ Add packages for minor convenience  
❌ Ignore transitive dependencies  
❌ Leave outdated packages indefinitely  
❌ Duplicate functionality across packages  
❌ Add packages without documentation  

## Related Documentation

- [Approved Packages](./approved-packages.md) - List of approved packages and their usage
- [Vulnerability Scanning](./vulnerability-scanning.md) - Security scanning tools and processes
- [Patterns & Standards](../codebase-analysis/07-best-practices/patterns-standards.md) - Coding standards and patterns

## References

- [NuGet Documentation](https://docs.microsoft.com/en-us/nuget/)
- [.NET Dependency Management](https://docs.microsoft.com/en-us/dotnet/core/tools/dependencies)
- [Semantic Versioning](https://semver.org/)
- [SPDX License List](https://spdx.org/licenses/)
- [GitHub Advisory Database](https://github.com/advisories)
