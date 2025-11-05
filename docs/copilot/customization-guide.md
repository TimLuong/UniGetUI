# GitHub Copilot Customization Guide for UniGetUI

This guide explains how to customize GitHub Copilot's behavior when working on the UniGetUI project, and how to leverage the comprehensive instructions in `.github/copilot-instructions.md`.

## Table of Contents

- [Overview](#overview)
- [Understanding Copilot Instructions](#understanding-copilot-instructions)
- [Customizing for Your Workflow](#customizing-for-your-workflow)
- [Common Customization Scenarios](#common-customization-scenarios)
- [IDE-Specific Setup](#ide-specific-setup)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)
- [Advanced Techniques](#advanced-techniques)

## Overview

The `.github/copilot-instructions.md` file provides GitHub Copilot with comprehensive context about UniGetUI's architecture, coding standards, and best practices. This guide helps you understand and customize how Copilot uses these instructions.

### What Are Copilot Instructions?

GitHub Copilot instructions are markdown files that provide context to Copilot about:
- Project-specific conventions and patterns
- Architectural decisions and guidelines
- Coding standards and style preferences
- Common anti-patterns to avoid
- Example code patterns to follow

When placed in `.github/copilot-instructions.md`, these instructions are automatically read by Copilot when working in the repository.

### Benefits

Using the comprehensive Copilot instructions provides:
- **Consistent code generation** aligned with project standards
- **Fewer style violations** and code review comments
- **Better architectural decisions** following established patterns
- **Faster onboarding** for new contributors
- **Reduced manual corrections** to generated code

## Understanding Copilot Instructions

### Structure of `.github/copilot-instructions.md`

The instructions file is organized into sections:

1. **Project Overview**: Context about UniGetUI and its purpose
2. **Architectural Patterns**: Design patterns used in the codebase
3. **Coding Standards**: Naming conventions, file organization, and style
4. **Database Patterns**: File-based storage and data management
5. **UI/UX Guidelines**: WinUI 3 and localization practices
6. **Security Standards**: Input validation and secure coding
7. **Testing Standards**: Test organization and patterns
8. **Performance Considerations**: Optimization techniques
9. **Logging Patterns**: Proper logging usage
10. **Configuration Management**: Settings and configuration
11. **File Structure**: Project organization
12. **Code Examples**: Real-world usage patterns
13. **Anti-Patterns**: What to avoid

### How Copilot Uses These Instructions

When you:
- Write code comments describing what you want
- Start typing code that Copilot can autocomplete
- Use Copilot Chat to generate code
- Ask Copilot to refactor or explain code

Copilot will:
- Reference the instructions to understand project context
- Generate code following the specified patterns
- Apply the coding standards automatically
- Avoid anti-patterns listed in the document
- Use project-specific conventions

## Customizing for Your Workflow

### Personal Copilot Configuration

While the repository-level instructions apply to all contributors, you can add personal preferences:

#### VS Code Settings

Add to your user or workspace settings (`.vscode/settings.json`):

```json
{
  "github.copilot.enable": {
    "*": true,
    "yaml": true,
    "plaintext": false,
    "markdown": true,
    "csharp": true
  },
  "github.copilot.advanced": {
    "length": 500,
    "temperature": 0.2
  }
}
```

#### Visual Studio Settings

1. Go to **Tools** > **Options** > **GitHub** > **Copilot**
2. Configure:
   - Enable/disable for specific languages
   - Adjust suggestion length
   - Configure telemetry preferences

### Workspace-Specific Instructions

For team-specific or branch-specific customizations, create a `.copilot/instructions.md` file in your workspace:

```markdown
# Branch-Specific Guidelines

When working on feature branch XYZ:
- Use experimental feature flags
- Log all operations at DEBUG level
- Include extra validation for new feature
```

This supplements (not replaces) the main instructions.

## Common Customization Scenarios

### Scenario 1: Working on a Specific Package Manager

When implementing a new package manager, add context to your session:

```csharp
// COPILOT: I'm implementing a new package manager for Homebrew
// Follow the IPackageManager interface pattern
// Use CLI-based execution like other managers
// Reference WinGet manager as a template

public class HomebrewManager : IPackageManager
{
    // Copilot will generate implementation following project patterns
}
```

### Scenario 2: UI Component Development

When creating UI components, provide specific context:

```xml
<!-- COPILOT: Create a settings card for package manager preferences
     Use CommunityToolkit.WinUI.Controls.SettingsCard
     Follow the pattern from other settings pages
     Include proper localization with CoreTools.Translate -->

<controls:SettingsCard>
    <!-- Copilot generates appropriate XAML -->
</controls:SettingsCard>
```

### Scenario 3: Async Operation Implementation

For async code, reference the patterns:

```csharp
// COPILOT: Create an async method to fetch package updates
// Use TaskRecycler for performance optimization
// Include CancellationToken support
// Follow the fail-safe pattern returning empty array on error

public async Task<IReadOnlyList<IPackage>> GetUpdatesAsync(
    CancellationToken cancellationToken = default)
{
    // Copilot generates code following async patterns
}
```

### Scenario 4: Error Handling

When adding error handling:

```csharp
// COPILOT: Add comprehensive error handling
// Log errors using Logger.Error()
// Return safe defaults on failure
// Follow the error handling patterns from the instructions

try
{
    // Operation
}
catch (Exception ex)
{
    // Copilot generates proper error handling
}
```

## IDE-Specific Setup

### Visual Studio Code

#### Installation

1. Install the GitHub Copilot extension
2. Install the GitHub Copilot Chat extension
3. Sign in with your GitHub account
4. Reload VS Code

#### Keyboard Shortcuts

```
Ctrl+Enter     - Show all suggestions
Alt+]          - Next suggestion
Alt+[          - Previous suggestion
Ctrl+Right     - Accept next word
Ctrl+I         - Open Copilot Chat
```

#### Recommended Extensions

```json
{
  "recommendations": [
    "github.copilot",
    "github.copilot-chat",
    "ms-dotnettools.csharp",
    "ms-dotnettools.csdevkit"
  ]
}
```

Add this to `.vscode/extensions.json` in your workspace.

### Visual Studio 2022

#### Installation

1. Install Visual Studio 2022 version 17.8 or later
2. Install GitHub Copilot extension from Extensions > Manage Extensions
3. Sign in with your GitHub account
4. Restart Visual Studio

#### Keyboard Shortcuts

```
Alt+/          - Toggle Copilot completions
Alt+\          - Show inline suggestions
Ctrl+Alt+I     - Open Copilot Chat
Tab            - Accept suggestion
Esc            - Dismiss suggestion
```

#### Configuration

1. **Tools** > **Options** > **GitHub** > **Copilot**
2. Configure:
   - Enable for C#, XAML, and Markdown
   - Set suggestion display delay
   - Configure chat preferences

### JetBrains Rider

#### Installation

1. Install JetBrains AI Assistant plugin
2. Or install GitHub Copilot plugin
3. Configure in **Settings** > **Tools** > **GitHub Copilot**

## Best Practices

### Writing Effective Comments for Copilot

#### ✅ Good Comments

```csharp
// Create a package manager for Cargo following the IPackageManager interface
// Use CLI execution pattern similar to WinGet manager
// Include proper error handling with Logger

// Fetch package details from the package manager
// Use TaskRecycler to avoid duplicate concurrent requests
// Return empty array on error following fail-safe pattern
```

#### ❌ Poor Comments

```csharp
// Make this work
// TODO: implement
// Fix this
```

### Providing Context

#### Use Multi-Line Comments

```csharp
/*
 * COPILOT CONTEXT:
 * - This is a helper method for the WinGet package manager
 * - It parses CLI output in JSON format
 * - Should handle malformed JSON gracefully
 * - Return empty list on parse errors
 * - Log errors using Logger.Error()
 */
public List<IPackage> ParsePackageList(string jsonOutput)
{
    // Implementation
}
```

#### Reference Existing Code

```csharp
// COPILOT: Create a method similar to WinGetManager.GetInstalledPackages()
// but for the Chocolatey package manager
```

### Iterative Refinement

1. **Start with a comment** describing what you need
2. **Let Copilot generate** initial code
3. **Review and refine** the suggestion
4. **Add more context** if needed
5. **Accept or modify** the final result

### Code Review

Always review Copilot's suggestions:
- ✅ Verify it follows project patterns
- ✅ Check for proper error handling
- ✅ Ensure thread safety if needed
- ✅ Confirm logging is appropriate
- ✅ Validate against coding standards
- ✅ Test the generated code

## Troubleshooting

### Issue: Copilot Not Following Project Patterns

**Symptoms:**
- Generated code doesn't match project style
- Missing error handling or logging
- Incorrect naming conventions

**Solutions:**
1. Check that `.github/copilot-instructions.md` exists in the repository
2. Provide more specific context in your comments
3. Reference specific patterns from the instructions
4. Use examples from existing code

**Example:**
```csharp
// COPILOT: Use the exact same pattern as in WinGetManager.cs
// Follow naming conventions from .github/copilot-instructions.md
// Include Logger.Error() calls for exceptions
```

### Issue: Suggestions Not Appearing

**Symptoms:**
- Copilot doesn't show suggestions
- No autocompletion happening

**Solutions:**
1. Verify Copilot is enabled in your IDE
2. Check your GitHub Copilot subscription status
3. Ensure network connectivity
4. Restart your IDE
5. Check IDE-specific Copilot settings

**VS Code:**
```
Ctrl+Shift+P > "GitHub Copilot: Enable"
```

**Visual Studio:**
```
Tools > Options > GitHub > Copilot > Enable
```

### Issue: Generated Code Has Errors

**Symptoms:**
- Copilot generates code that doesn't compile
- Missing using statements
- Incorrect types or method signatures

**Solutions:**
1. Ensure project builds successfully before using Copilot
2. Let Copilot see more context (open related files)
3. Provide more specific type information in comments
4. Use Copilot Chat to ask for fixes

**Example:**
```csharp
// COPILOT: Use IReadOnlyList<IPackage> as return type
// Import UniGetUI.PackageEngine.Interfaces namespace
// Follow the pattern from IPackageManager interface
```

### Issue: Inconsistent Suggestions

**Symptoms:**
- Different patterns generated each time
- Copilot switches between styles

**Solutions:**
1. Be more explicit in comments about requirements
2. Reference specific examples from the codebase
3. Keep related files open for better context
4. Use more descriptive variable and method names

## Advanced Techniques

### Using Copilot Chat Effectively

#### Ask for Explanations

```
@workspace Explain the TaskRecycler pattern and when to use it
```

#### Request Refactoring

```
/refactor Use the TaskRecycler pattern to optimize this method
```

#### Generate Tests

```
@workspace Generate xUnit tests for this class following project patterns
```

#### Code Review Assistance

```
Review this code against the standards in .github/copilot-instructions.md
```

### Context Scoping

#### File-Level Context

```csharp
/* FILE CONTEXT:
 * This file implements the Scoop package manager adapter
 * Follows CLI-based execution pattern
 * Uses JSON parsing for command output
 * Implements IPackageManager interface
 */

namespace UniGetUI.PackageEngine.Managers.Scoop;

public class ScoopManager : IPackageManager
{
    // Copilot has context for the entire file
}
```

#### Method-Level Context

```csharp
public async Task<List<IPackage>> GetPackagesAsync()
{
    // COPILOT: This method should:
    // 1. Execute 'scoop list' command
    // 2. Parse JSON output
    // 3. Convert to List<IPackage>
    // 4. Use TaskRecycler with 60-second cache
    // 5. Return empty list on error
    
    // Implementation here
}
```

### Pattern Enforcement

Create custom comment templates for common patterns:

```csharp
// PATTERN: Async operation with TaskRecycler
// - Use TaskRecycler<T>.RunOrAttachAsync()
// - Include CancellationToken parameter
// - Add ConfigureAwait(false)
// - Return fail-safe default on error
// - Log exceptions with Logger.Error()
```

### Multi-File Refactoring

When refactoring across files:

1. Open all affected files in your editor
2. Use Copilot Chat with `@workspace` scope
3. Request changes with context:

```
@workspace Refactor the package loading logic to use the new 
caching pattern. Update WinGetManager, ScoopManager, and 
ChocolateyManager. Follow the pattern from .github/copilot-instructions.md
```

### Testing with Copilot

Generate comprehensive tests:

```csharp
// COPILOT: Generate xUnit tests for this class
// Include tests for:
// - Normal operation (Arrange-Act-Assert pattern)
// - Error conditions (exceptions, null inputs)
// - Edge cases (empty inputs, large datasets)
// - Async operations (using async/await)
// Follow naming: Test{MethodName}_{Scenario}_{Expected}
```

### Documentation Generation

Generate XML documentation:

```csharp
// COPILOT: Add XML documentation comments
// Include <summary>, <param>, <returns>, and <exception> tags
// Follow project documentation standards
/// 
```

## Tips for Maximum Productivity

### 1. Keep Related Files Open

Copilot uses open files for context. Keep relevant files visible:
- Interface definitions
- Base classes
- Similar implementations
- Test files

### 2. Use Descriptive Names

Better names = better suggestions:

```csharp
// ❌ Poor
public async Task<List<T>> Get() { }

// ✅ Good
public async Task<IReadOnlyList<IPackage>> GetInstalledPackagesAsync() { }
```

### 3. Leverage Code Comments

Comments guide Copilot's understanding:

```csharp
// Parse JSON output from package manager CLI
// Each line is a separate package in JSON format
var packages = ParseJsonLines(output);
```

### 4. Review and Learn

When Copilot generates good code:
- Review why it works well
- Note the patterns it follows
- Learn from the suggestions
- Save good examples

### 5. Combine with Traditional Tools

Copilot complements (doesn't replace):
- Code review
- Testing
- Static analysis
- Manual optimization

## Maintaining the Instructions

### When to Update `.github/copilot-instructions.md`

Update the instructions when:
- New architectural patterns are adopted
- Coding standards change
- New technologies are integrated
- Common mistakes are identified
- Better practices are discovered

### Contributing Updates

To propose updates to the Copilot instructions:

1. Create a branch: `git checkout -b update-copilot-instructions`
2. Edit `.github/copilot-instructions.md`
3. Test with Copilot to ensure improvements
4. Submit a PR with examples of better code generation
5. Document the rationale for changes

### Version Control

The instructions should evolve with the project:
- Keep in sync with coding standards
- Update when architecture changes
- Add examples from real code
- Remove outdated patterns

## Learning Resources

### Official Documentation

- [GitHub Copilot Documentation](https://docs.github.com/en/copilot)
- [Copilot Best Practices](https://github.blog/2023-06-20-how-to-write-better-prompts-for-github-copilot/)
- [VS Code Copilot Guide](https://code.visualstudio.com/docs/editor/artificial-intelligence)

### UniGetUI Resources

- `/docs/codebase-analysis/` - Detailed architecture documentation
- `/.github/copilot-instructions.md` - Comprehensive Copilot instructions
- `/CONTRIBUTING.md` - Contribution guidelines
- `/docs/codebase-analysis/07-best-practices/patterns-standards.md` - Coding standards

### Community

- [GitHub Discussions](https://github.com/marticliment/UniGetUI/discussions) - Ask questions
- [GitHub Issues](https://github.com/marticliment/UniGetUI/issues) - Report problems
- [Pull Requests](https://github.com/marticliment/UniGetUI/pulls) - See examples

## Conclusion

The comprehensive Copilot instructions in `.github/copilot-instructions.md` are designed to help you write better code faster while maintaining consistency with UniGetUI's architecture and standards. By understanding and customizing how you work with Copilot, you can maximize productivity while ensuring high code quality.

Remember:
- ✅ Copilot is a tool to enhance your development
- ✅ Always review and test generated code
- ✅ Provide clear context in comments
- ✅ Learn from Copilot's suggestions
- ✅ Contribute improvements to the instructions

Happy coding with Copilot! 🚀
