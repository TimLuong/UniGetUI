# Documentation Standards

This document outlines the comprehensive documentation standards for the UniGetUI project. Following these standards ensures consistency, maintainability, and professionalism across all project documentation.

## Table of Contents

1. [General Documentation Principles](#general-documentation-principles)
2. [Markdown Standards](#markdown-standards)
3. [Code Comment Standards](#code-comment-standards)
4. [Changelog Format](#changelog-format)
5. [Wiki Structure and Organization](#wiki-structure-and-organization)
6. [User Guide and Help Documentation](#user-guide-and-help-documentation)
7. [API Documentation](#api-documentation)
8. [Architecture Decision Records](#architecture-decision-records)

---

## General Documentation Principles

### Purpose and Audience

- **Know your audience**: Write for developers, contributors, and end-users as appropriate
- **Be clear and concise**: Use simple language and avoid unnecessary jargon
- **Be comprehensive**: Cover all necessary information without overwhelming the reader
- **Keep it updated**: Documentation should evolve with the codebase

### Document Structure

- Use hierarchical headings (H1 → H6) to organize content
- Include a table of contents for documents longer than 3 sections
- Start with an overview or introduction
- Use consistent formatting throughout

### Language and Style

- Use active voice when possible
- Write in present tense
- Use gender-neutral language
- Be respectful and inclusive in all documentation
- Use American English spelling for consistency

---

## Markdown Standards

### File Naming

- Use lowercase with hyphens for file names: `my-document.md`
- Use descriptive names that reflect content
- Avoid special characters except hyphens and underscores

### Headers

```markdown
# H1 - Document Title (Only one per document)
## H2 - Major Sections
### H3 - Subsections
#### H4 - Sub-subsections
```

### Formatting

**Bold Text**: Use for emphasis and UI elements
- `**bold text**` or `__bold text__`

*Italic Text*: Use for variable names or slight emphasis
- `*italic*` or `_italic_`

`Code Inline`: Use for code, commands, file names, and technical terms
- `` `inline code` ``

### Lists

**Unordered Lists**:
```markdown
- Item 1
- Item 2
  - Sub-item 2.1
  - Sub-item 2.2
- Item 3
```

**Ordered Lists**:
```markdown
1. First item
2. Second item
3. Third item
```

**Task Lists**:
```markdown
- [x] Completed task
- [ ] Pending task
```

### Code Blocks

Use fenced code blocks with language identifiers:

````markdown
```csharp
public class Example
{
    public void Method() { }
}
```

```bash
npm install package-name
```

```json
{
  "key": "value"
}
```
````

### Links

**External Links**:
```markdown
[Link Text](https://example.com)
```

**Internal Links**:
```markdown
[Section Name](#section-name)
[Other Document](./path/to/document.md)
```

### Images

```markdown
![Alt Text](path/to/image.png)
![Alt Text with Title](path/to/image.png "Image Title")
```

### Tables

```markdown
| Column 1 | Column 2 | Column 3 |
|----------|----------|----------|
| Data 1   | Data 2   | Data 3   |
| Data 4   | Data 5   | Data 6   |
```

### Blockquotes and Alerts

**Standard Blockquote**:
```markdown
> This is a blockquote
```

**GitHub Alerts** (for important information):
```markdown
> [!NOTE]
> Useful information that users should know

> [!TIP]
> Helpful advice for doing things better

> [!IMPORTANT]
> Key information users need to know

> [!WARNING]
> Urgent info that needs immediate attention

> [!CAUTION]
> Advises about risks or negative outcomes
```

---

## Code Comment Standards

### C# XML Documentation

UniGetUI is primarily a C# project. All public APIs should have XML documentation comments.

#### Basic Structure

```csharp
/// <summary>
/// Brief description of what the class/method/property does.
/// </summary>
/// <remarks>
/// Optional: More detailed explanation, usage notes, or additional context.
/// </remarks>
public class ExampleClass
{
    /// <summary>
    /// Gets or sets the name of the item.
    /// </summary>
    /// <value>A string containing the item name.</value>
    public string Name { get; set; }

    /// <summary>
    /// Calculates the sum of two numbers.
    /// </summary>
    /// <param name="a">The first number to add.</param>
    /// <param name="b">The second number to add.</param>
    /// <returns>The sum of <paramref name="a"/> and <paramref name="b"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when inputs are invalid.</exception>
    /// <example>
    /// <code>
    /// int result = Add(5, 3);
    /// // result will be 8
    /// </code>
    /// </example>
    public int Add(int a, int b)
    {
        return a + b;
    }
}
```

#### XML Documentation Tags

**Required for Public APIs**:
- `<summary>`: Brief description (1-2 sentences)
- `<param>`: Description of each parameter
- `<returns>`: Description of return value
- `<exception>`: Document expected exceptions

**Optional but Recommended**:
- `<remarks>`: Detailed information, usage notes
- `<example>`: Code examples showing usage
- `<value>`: Description of property values
- `<see>` and `<seealso>`: Cross-references to related types
- `<typeparam>`: Generic type parameter descriptions

#### Documentation Best Practices

1. **Be Concise**: Summaries should be brief but informative
2. **Use Third Person**: "Gets the value" not "Get the value"
3. **Document Behavior**: Explain what the method does, not how it does it
4. **Include Examples**: For complex APIs, provide usage examples
5. **Document Exceptions**: List all exceptions that can be thrown
6. **Keep Updated**: Update documentation when changing code

#### Inline Comments

```csharp
// Single-line comments for brief explanations
// Use for clarifying non-obvious code

/* Multi-line comments for longer explanations
 * that require multiple lines
 */

// TODO: Description of work to be done
// HACK: Description of temporary workaround
// NOTE: Important information
// FIXME: Description of known issue
```

**When to Use Inline Comments**:
- Complex algorithms or business logic
- Non-obvious workarounds or hacks
- Clarification of intent
- Explanation of "why" not "what"

**When NOT to Use Inline Comments**:
- Stating the obvious
- Explaining simple code that speaks for itself
- Compensating for poor variable/method names

---

## Changelog Format

We follow the [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) format, based on [Semantic Versioning](https://semver.org/).

### Changelog Structure

```markdown
# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- New features that have been added

### Changed
- Changes in existing functionality

### Deprecated
- Features that will be removed in upcoming releases

### Removed
- Features that have been removed

### Fixed
- Bug fixes

### Security
- Vulnerability fixes and security improvements

## [1.0.0] - 2024-01-15

### Added
- Initial release
- Feature A
- Feature B

### Fixed
- Bug #123

[Unreleased]: https://github.com/user/project/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/user/project/releases/tag/v1.0.0
```

### Change Categories

1. **Added**: New features
2. **Changed**: Changes in existing functionality
3. **Deprecated**: Features that will be removed soon
4. **Removed**: Removed features
5. **Fixed**: Bug fixes
6. **Security**: Security-related changes

### Best Practices

- Keep an `[Unreleased]` section at the top
- List changes in reverse chronological order (newest first)
- Group changes by category
- Include dates in YYYY-MM-DD format
- Link to release tags
- Reference issue/PR numbers when applicable
- Write for humans, not machines
- Don't include every single commit

### Entry Format

```markdown
- Brief description of change [#123](link-to-issue)
- Added support for new package manager [#456](link-to-pr)
```

---

## Wiki Structure and Organization

### Recommended Wiki Structure

```
Home
├── Getting Started
│   ├── Installation
│   ├── Quick Start Guide
│   └── Configuration
├── User Guide
│   ├── Basic Usage
│   ├── Advanced Features
│   ├── Package Managers
│   └── Troubleshooting
├── Developer Documentation
│   ├── Contributing
│   ├── Development Setup
│   ├── Architecture
│   ├── API Reference
│   └── Testing
├── FAQ
└── Release Notes
```

### Wiki Page Standards

#### Page Naming
- Use Title Case for page names
- Be descriptive and specific
- Avoid abbreviations unless widely known
- Keep names concise (under 50 characters)

#### Page Structure
1. **Title**: Clear, descriptive H1 heading
2. **Introduction**: Brief overview of the page content
3. **Navigation**: Links to related pages
4. **Content**: Main content organized with headers
5. **See Also**: Links to related resources

#### Cross-Referencing
- Link to related wiki pages
- Link to relevant GitHub issues and PRs
- Link to external documentation when helpful
- Use descriptive link text (not "click here")

### Wiki Maintenance

- Review and update quarterly
- Mark outdated content with notices
- Archive obsolete pages instead of deleting
- Keep navigation up to date
- Maintain a consistent style across all pages

---

## User Guide and Help Documentation

### User Documentation Principles

1. **User-Centric**: Focus on user goals and tasks
2. **Progressive Disclosure**: Start simple, add complexity gradually
3. **Task-Oriented**: Organize by what users want to accomplish
4. **Searchable**: Use clear headings and keywords
5. **Visual**: Include screenshots and diagrams

### User Guide Structure

```markdown
# [Feature Name] User Guide

## Overview
Brief introduction to the feature and its purpose.

## Prerequisites
What users need before starting.

## Getting Started
Step-by-step guide for basic usage.

## Common Tasks

### Task 1: [Task Name]
1. Step one
2. Step two
3. Step three

### Task 2: [Task Name]
1. Step one
2. Step two

## Advanced Usage
More complex scenarios and configurations.

## Troubleshooting

### Problem 1
**Symptom**: Description of the problem
**Solution**: Steps to resolve

### Problem 2
**Symptom**: Description of the problem
**Solution**: Steps to resolve

## FAQ

### Question 1?
Answer 1

### Question 2?
Answer 2

## See Also
- [Related Guide](link)
- [Related Feature](link)
```

### Writing Tips for User Documentation

- **Use Second Person**: Address the user as "you"
- **Be Action-Oriented**: Start with verbs (Click, Select, Enter)
- **Be Specific**: Use exact button names, menu items, etc.
- **Include Screenshots**: Show UI elements being described
- **Provide Context**: Explain why users would do something
- **Test Instructions**: Verify steps work as written
- **Keep Updated**: Update when UI changes

### Screenshots and Images

- Use clear, high-resolution screenshots
- Highlight important UI elements with arrows or boxes
- Include alt text for accessibility
- Keep screenshots up to date with current UI
- Use consistent screenshot style (same theme, size)

---

## API Documentation

### API Documentation Structure

For detailed API documentation, see the [API Documentation Template](../../templates/API-DOC-template.md).

### API Documentation Requirements

1. **Overview**: Purpose and scope of the API
2. **Authentication**: How to authenticate (if applicable)
3. **Endpoints**: All available endpoints
4. **Request/Response**: Format and examples
5. **Error Codes**: Common errors and handling
6. **Rate Limiting**: Any limitations (if applicable)
7. **Examples**: Real-world usage examples
8. **Changelog**: API version history

### API Endpoint Documentation Format

```markdown
### GET /api/resource/{id}

Retrieves a specific resource by ID.

**Parameters**:
- `id` (string, required): The unique identifier of the resource

**Response**:
```json
{
  "id": "123",
  "name": "Resource Name",
  "status": "active"
}
```

**Status Codes**:
- `200 OK`: Success
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

**Example Request**:
```bash
curl -X GET https://api.example.com/api/resource/123
```
```

---

## Architecture Decision Records

For creating Architecture Decision Records, use the [ADR Template](../../templates/ADR-template.md).

### ADR Standards

1. **Numbering**: Sequential numbers (0001, 0002, etc.)
2. **Naming**: `NNNN-title-with-dashes.md`
3. **Status**: Proposed, Accepted, Deprecated, or Superseded
4. **Storage**: Keep all ADRs in `/docs/architecture/decisions/`
5. **Immutability**: Don't edit accepted ADRs; create new ones instead

### When to Create an ADR

- Significant architectural changes
- Technology stack decisions
- Design pattern choices
- Trade-off decisions
- Changes affecting multiple components
- Decisions that need rationale documentation

---

## Documentation Review Process

### Before Committing Documentation

1. **Spell Check**: Use a spell checker
2. **Grammar Check**: Review for grammatical errors
3. **Link Check**: Verify all links work
4. **Format Check**: Ensure consistent formatting
5. **Code Examples**: Test all code examples
6. **Screenshots**: Verify screenshots are current
7. **Peer Review**: Have someone else review

### Documentation Maintenance

- Review documentation quarterly
- Update with each release
- Mark deprecated content
- Remove outdated information
- Keep examples current
- Update screenshots as needed

---

## Additional Resources

- [Markdown Guide](https://www.markdownguide.org/)
- [Keep a Changelog](https://keepachangelog.com/)
- [Semantic Versioning](https://semver.org/)
- [C# XML Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)
- [GitHub Flavored Markdown](https://github.github.com/gfm/)

---

**Version**: 1.0.0  
**Last Updated**: 2024-11-05  
**Maintained By**: UniGetUI Contributors
