# Release Process

## Overview

This document outlines the process for creating and distributing UniGetUI releases.

## Release Types

### Stable Releases

- **Frequency**: When ready (typically monthly)
- **Version Format**: `X.Y.Z` (e.g., `3.1.0`)
- **Channels**: All distribution channels
- **Testing**: Extensive testing required

### Pre-releases

- **Frequency**: Weekly or as needed
- **Version Format**: `X.Y.Z-pre` (e.g., `3.1.0-pre`)
- **Channels**: GitHub, WinGet (pre-release)
- **Testing**: Basic testing required

## Version Numbering

UniGetUI follows **Semantic Versioning** (SemVer):

- **Major** (X.0.0): Breaking changes, major features
- **Minor** (X.Y.0): New features, backward compatible
- **Patch** (X.Y.Z): Bug fixes, minor improvements

## Release Checklist

### Pre-Release

- [ ] All planned features completed
- [ ] All tests passing
- [ ] Code review completed
- [ ] Documentation updated
- [ ] Translation files updated
- [ ] Version number updated in:
  - [ ] `scripts/BuildNumber`
  - [ ] Assembly version files
  - [ ] Package manifest
- [ ] Changelog prepared

### Build

- [ ] Run `build_release.cmd` script
- [ ] Verify installer created successfully
- [ ] Test installer on clean Windows installation
- [ ] Verify all package managers detected
- [ ] Test core functionality

### Release Creation

- [ ] Create Git tag: `git tag vX.Y.Z`
- [ ] Push tag: `git push origin vX.Y.Z`
- [ ] Create GitHub release
  - [ ] Upload installer
  - [ ] Upload ZIP archive
  - [ ] Add release notes
  - [ ] Mark as pre-release if applicable

### Distribution

- [ ] WinGet package published (automated)
- [ ] Scoop manifest updated (automated)
- [ ] Chocolatey package submitted
- [ ] Microsoft Store submission (manual)
- [ ] Update website download links

### Post-Release

- [ ] Monitor for critical issues
- [ ] Respond to user feedback
- [ ] Update documentation if needed
- [ ] Plan next release

## Build Scripts

### `build_release.cmd`

Main build script that:
1. Cleans previous builds
2. Runs tests
3. Builds solution in Release mode
4. Signs binaries
5. Creates ZIP archive
6. Generates installer with Inno Setup
7. Signs installer

**Usage**:
```cmd
build_release.cmd
```

### `apply_versions.py`

Python script that updates version numbers:
- Assembly versions
- Package manifests
- Configuration files

**Usage**:
```bash
python scripts/apply_versions.py
```

## Distribution Channels

### Microsoft Store

**Manual Submission Required**

1. Build MSIX package
2. Upload to Partner Center
3. Fill out store listing
4. Submit for certification
5. Wait for approval (1-3 days)

### WinGet

**Automated via GitHub Actions**

Workflow automatically creates PR to winget-pkgs repository when release is created.

### Scoop

**Automated via GitHub Actions**

Manifest automatically updated in scoop bucket repository.

### Chocolatey

**Manual Submission**

1. Create/update `.nuspec` file
2. Package with `choco pack`
3. Push to Chocolatey: `choco push`
4. Wait for moderation

### Direct Download

**Automated**

Installer attached to GitHub release, linked from website.

## Hotfix Process

For critical bugs requiring immediate fix:

1. Create hotfix branch from release tag
2. Apply fix
3. Test thoroughly
4. Increment patch version
5. Follow release process
6. Merge back to main

## Rollback Process

If critical issue discovered post-release:

1. Mark release as pre-release on GitHub
2. Update documentation warning users
3. Prepare hotfix release
4. Communicate on Discord/Discussions

## Communication

### Release Announcements

Announce releases on:
- GitHub Discussions
- Discord (if available)
- Twitter/Social media
- Website news

### Changelog Format

```markdown
## [X.Y.Z] - YYYY-MM-DD

### Added
- New feature descriptions

### Changed
- Modified feature descriptions

### Fixed
- Bug fix descriptions

### Removed
- Removed feature descriptions
```

## Related Documentation

- [Build & Deployment](./build-deployment.md)
- [CI/CD Pipelines](./ci-cd-pipelines.md)
- [Local Setup](./local-setup.md)
