# Contribution Guide

## Welcome Contributors!

Thank you for your interest in contributing to UniGetUI! This guide will help you get started.

## Ways to Contribute

### Code Contributions
- Fix bugs
- Add new features
- Improve performance
- Add new package manager support

### Documentation
- Fix typos or unclear explanations
- Add examples and tutorials
- Translate documentation
- Improve code comments

### Testing
- Report bugs with detailed reproduction steps
- Test new releases
- Verify fixes

### Translation
- Add new language support
- Improve existing translations
- Review translation submissions

## Getting Started

1. **Read the Documentation**
   - [Quick Start](../01-getting-started/quick-start.md)
   - [System Architecture](../02-architecture/system-architecture.md)
   - [Coding Standards](../03-development-standards/coding-standards.md)

2. **Set Up Development Environment**
   - Follow [Local Setup](../11-devops/local-setup.md)
   - Build and run the application
   - Verify tests pass

3. **Find an Issue**
   - Check [GitHub Issues](https://github.com/marticliment/UniGetUI/issues)
   - Look for "good first issue" labels
   - Ask in discussions if unsure

## Contribution Workflow

### 1. Fork and Clone
```bash
# Fork the repository on GitHub
# Clone your fork
git clone https://github.com/YOUR_USERNAME/UniGetUI.git
cd UniGetUI
```

### 2. Create a Branch
```bash
# Create feature branch
git checkout -b feature/your-feature-name

# Or bug fix branch
git checkout -b fix/issue-description
```

### 3. Make Changes
- Write clean, readable code
- Follow coding standards
- Add tests for new functionality
- Update documentation

### 4. Test Your Changes
```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Manual testing
# Run the application and verify your changes
```

### 5. Commit Your Changes
```bash
# Stage changes
git add .

# Commit with descriptive message
git commit -m "Add feature: description of what you did"
```

Follow commit message guidelines:
- Brief description (50 chars or less)
- Detailed explanation if needed
- Reference issues: "Fixes #123"

### 6. Push and Create Pull Request
```bash
# Push to your fork
git push origin feature/your-feature-name

# Create Pull Request on GitHub
# Fill out the PR template
# Link related issues
```

## Code Review Process

1. **Automated Checks**
   - CI builds must pass
   - Tests must succeed
   - Code analysis must pass

2. **Human Review**
   - Maintainers will review your code
   - Address feedback and questions
   - Make requested changes

3. **Approval and Merge**
   - Once approved, maintainer will merge
   - Your contribution is now part of UniGetUI!

## Coding Standards

- Follow [Coding Standards](../03-development-standards/coding-standards.md)
- Use [Naming Conventions](../03-development-standards/naming-conventions.md)
- Implement appropriate [Design Patterns](../03-development-standards/design-patterns.md)
- Write tests for new functionality

## Pull Request Guidelines

### PR Title
- Be descriptive and concise
- Examples: "Fix: Package installation error", "Feature: Add Homebrew support"

### PR Description
- Explain what changes you made and why
- Link related issues
- Include screenshots for UI changes
- List any breaking changes

### Checklist
Before submitting, ensure:
- [ ] Code builds successfully
- [ ] All tests pass
- [ ] New tests added for new functionality
- [ ] Documentation updated
- [ ] Follows coding standards
- [ ] No unnecessary changes (formatting, whitespace)

## Reporting Issues

### Bug Reports
Include:
- UniGetUI version
- Windows version
- Steps to reproduce
- Expected vs actual behavior
- Screenshots if applicable
- Log files if relevant

### Feature Requests
Include:
- Use case / problem to solve
- Proposed solution
- Alternative solutions considered
- Willingness to implement

## Translation Contributions

Translations are managed via [Tolgee platform](https://app.tolgee.io/):
1. Create account on Tolgee
2. Find UniGetUI project
3. Choose language to translate
4. Submit translations
5. Maintainers will review and merge

## Community Guidelines

- Be respectful and constructive
- Help others in discussions
- Follow [Code of Conduct](../../../CODE_OF_CONDUCT.md)
- Ask questions if unsure

## Getting Help

- **GitHub Discussions**: Ask questions and discuss ideas
- **Issues**: Report bugs and request features
- **Discord**: (Link if available) Real-time chat

## Recognition

Contributors are recognized in:
- GitHub contributors page
- Release notes for significant contributions
- About page in application

## Thank You!

Every contribution, no matter how small, helps make UniGetUI better. We appreciate your time and effort!

## Related Documentation

- [Adding Features](./adding-features.md)
- [Adding Package Managers](./adding-package-managers.md)
- [Testing Strategy](../06-testing/testing-strategy.md)
- [Code of Conduct](../../../CODE_OF_CONDUCT.md)
- [Contributing Guidelines](../../../CONTRIBUTING.md)
