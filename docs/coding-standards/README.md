# Coding Standards Documentation

Welcome to the UniGetUI coding standards documentation. This directory contains comprehensive guidelines for all languages and technologies used in the project.

## 📚 Available Standards

### Language-Specific Standards

- **[C# Coding Standards](csharp-standards.md)** - Naming conventions, formatting, async/await patterns, and best practices for C# development
- **[XAML Best Practices](xaml-standards.md)** - UI layout, data binding, styling, and performance optimization for XAML
- **[TypeScript/JavaScript Standards](typescript-standards.md)** - Type safety, modern JavaScript patterns, and coding conventions
- **[SQL Coding Standards](sql-standards.md)** - Query optimization, indexing strategy, and database best practices

### Process Documentation

- **[Code Review Checklist](../code-review-checklist.md)** - Comprehensive checklist for conducting thorough code reviews
- **[.editorconfig](../../.editorconfig)** - Editor configuration for consistent formatting across the project

## 🎯 Quick Start

### For New Contributors

1. **Read the relevant standards** for the language(s) you'll be working with
2. **Install EditorConfig plugin** for your IDE to automatically apply formatting rules
3. **Review the code review checklist** to understand quality expectations
4. **Follow SOLID principles** and the established patterns in the codebase

### For Code Reviewers

1. **Use the [Code Review Checklist](../code-review-checklist.md)** as a guide
2. **Reference specific standards** when providing feedback
3. **Be constructive** and explain the "why" behind suggestions
4. **Focus on important issues** rather than being overly pedantic

## 📖 Document Overview

### C# Coding Standards

**Key Topics:**
- Naming conventions (PascalCase, camelCase, interfaces with 'I' prefix)
- Formatting standards (Allman braces, 4-space indentation)
- Async/await patterns (cancellation tokens, ConfigureAwait)
- File organization and code quality
- Anti-patterns to avoid
- Common pitfalls

**Target Audience:** All C# developers working on backend, core libraries, and UI code-behind

### XAML Best Practices

**Key Topics:**
- Naming conventions for XAML elements
- Data binding best practices (x:Bind vs Binding)
- Resource management and styling
- Performance optimization (virtualization, x:Load)
- Layout guidelines
- Accessibility requirements

**Target Audience:** UI developers working with WinUI 3 and XAML

### TypeScript/JavaScript Standards

**Key Topics:**
- TypeScript over JavaScript preference
- Naming conventions and formatting
- Type definitions and generics
- Async/await patterns
- Error handling
- Module organization

**Target Audience:** Frontend developers working on hybrid applications or web-based components

### SQL Coding Standards

**Key Topics:**
- Database naming conventions
- Query formatting and optimization
- Indexing strategy
- Transaction management
- Security best practices (SQL injection prevention)
- Performance tuning

**Target Audience:** Database developers and backend engineers working with SQL databases

## 🔧 Tool Configuration

### EditorConfig

The `.editorconfig` file at the root of the repository automatically configures your IDE for:

- **Indentation**: 4 spaces for C#, XAML, SQL; 2 spaces for TypeScript/JavaScript, JSON
- **Line endings**: CRLF for Windows files, LF for shell scripts
- **Encoding**: UTF-8 for all files
- **C# specific rules**: Naming conventions, formatting, code quality analyzers

**Supported Editors:**
- Visual Studio (built-in support)
- Visual Studio Code (requires EditorConfig extension)
- JetBrains Rider (built-in support)
- Many other editors via plugins

### Linting Tools

#### C# - StyleCop and Roslyn Analyzers

The project uses built-in Roslyn analyzers configured via `.editorconfig`:

```xml
<!-- In .csproj files -->
<PropertyGroup>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  <EnableNETAnalyzers>true</EnableNETAnalyzers>
  <AnalysisLevel>latest</AnalysisLevel>
</PropertyGroup>
```

**Key Rules:**
- Code quality (CA rules)
- Code style (IDE rules)
- Naming conventions
- Performance recommendations

#### TypeScript/JavaScript - ESLint

Recommended ESLint configuration:

```json
{
  "extends": [
    "eslint:recommended",
    "plugin:@typescript-eslint/recommended"
  ],
  "rules": {
    "@typescript-eslint/explicit-function-return-type": "warn",
    "@typescript-eslint/no-explicit-any": "error",
    "prefer-const": "error",
    "no-var": "error"
  }
}
```

#### SQL - SQL Formatter

Use SQL formatting tools like:
- **SQL Prompt** (for SQL Server Management Studio)
- **Poor Man's T-SQL Formatter** (free, open-source)
- **VS Code SQL Formatter** extensions

## 📋 Code Review Process

### Steps

1. **Pre-Review**
   - Verify CI/CD checks pass
   - Read PR description and linked issues
   - Understand the context and goals

2. **Review**
   - Check against relevant coding standards
   - Use the code review checklist
   - Test the changes locally if needed
   - Provide constructive feedback

3. **Post-Review**
   - Follow up on requested changes
   - Approve when standards are met
   - Ensure all discussions are resolved

### Review Priorities

**Must Fix (Blocking):**
- Security vulnerabilities
- Breaking changes without approval
- Major architectural issues
- Critical bugs

**Should Fix (Non-Blocking but Important):**
- Code quality issues
- Missing tests
- Performance concerns
- Unclear naming or documentation

**Nice to Have (Suggestions):**
- Minor optimizations
- Style preferences beyond standards
- Additional test cases
- Code organization improvements

## 🎓 Best Practices Summary

### General Principles

1. **Write Clean Code**
   - Self-documenting code with clear names
   - Single responsibility principle
   - DRY (Don't Repeat Yourself)

2. **Prioritize Readability**
   - Code is read more often than written
   - Clear intent over clever tricks
   - Consistent formatting and style

3. **Think About Maintainability**
   - Easy to modify and extend
   - Minimal dependencies
   - Clear separation of concerns

4. **Consider Performance**
   - But not at the expense of readability
   - Optimize when necessary, not prematurely
   - Measure before optimizing

5. **Ensure Security**
   - Validate all inputs
   - Use parameterized queries
   - Follow least privilege principle
   - Protect sensitive data

### Language-Specific Tips

**C#:**
- Use async/await for I/O operations
- Implement IDisposable for resource management
- Prefer LINQ for collection operations
- Enable nullable reference types

**XAML:**
- Use x:Bind for better performance
- Implement MVVM pattern
- Virtualize large lists
- Set AutomationProperties for accessibility

**TypeScript:**
- Enable strict mode in tsconfig.json
- Avoid 'any' type
- Use interfaces for data structures
- Prefer async/await over promises

**SQL:**
- Use explicit column lists
- Parameterize all queries
- Create appropriate indexes
- Use transactions for multi-statement operations

## 🔄 Updating These Standards

These standards are living documents that should evolve with the project:

### When to Update

- New language features become available
- Team identifies common issues or patterns
- Industry best practices change
- New tools or frameworks are adopted

### How to Update

1. **Propose Changes** - Open an issue or PR with proposed updates
2. **Team Discussion** - Discuss with the team, especially for controversial changes
3. **Update Documentation** - Modify the relevant standard document(s)
4. **Update .editorconfig** - If formatting rules change
5. **Communicate Changes** - Announce updates to the team
6. **Apply Gradually** - New standards apply to new code; refactoring old code is optional

### Review Schedule

- **Quarterly Review** - Check if standards need updates based on team feedback
- **Major Version Updates** - Review when upgrading language versions or frameworks
- **Post-Mortems** - Update standards after identifying issues in production

## 📞 Getting Help

### Questions About Standards

- **Check Examples** - Each standard document includes ✅ Good and ❌ Bad examples
- **Ask the Team** - Post in team chat or discuss in standup
- **Review Existing Code** - Look for similar patterns in the codebase
- **Consult Official Docs** - Microsoft documentation for .NET, TypeScript handbook, etc.

### Disagreements About Standards

- **Discuss Respectfully** - Different perspectives can improve standards
- **Provide Rationale** - Explain why alternative approaches might be better
- **Consider Context** - Some rules have exceptions for specific scenarios
- **Escalate if Needed** - For unresolved disagreements, involve tech leads

## 📚 Additional Resources

### Official Documentation

- [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [XAML Overview](https://docs.microsoft.com/en-us/windows/uwp/xaml-platform/xaml-overview)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/intro.html)
- [SQL Server Best Practices](https://docs.microsoft.com/en-us/sql/relational-databases/best-practices/)

### Books

- **C#:** "C# in Depth" by Jon Skeet
- **Design Patterns:** "Design Patterns: Elements of Reusable Object-Oriented Software"
- **Clean Code:** "Clean Code: A Handbook of Agile Software Craftsmanship" by Robert C. Martin
- **SQL:** "SQL Performance Explained" by Markus Winand

### Online Courses

- Microsoft Learn (free courses on .NET, XAML, SQL Server)
- Pluralsight (comprehensive programming courses)
- Frontend Masters (TypeScript and JavaScript courses)

## 📄 License

These coding standards are part of the UniGetUI project and are subject to the same license as the project.

---

**Remember:** Standards are guidelines to help us write better code together. When in doubt, prioritize code clarity, maintainability, and team consensus over strict adherence to any single rule.
