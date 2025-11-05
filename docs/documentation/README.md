# Documentation Guide

This directory contains comprehensive documentation standards and guidelines for the UniGetUI project.

## Overview

The documentation in this directory establishes the standards and best practices for all documentation within the UniGetUI project, ensuring consistency, quality, and maintainability.

## Contents

### 📋 [standards.md](./standards.md)

**The comprehensive documentation standards guide** that covers all aspects of documentation for the project.

#### What's Inside:

1. **General Documentation Principles**
   - Purpose and audience consideration
   - Document structure guidelines
   - Language and style conventions

2. **Markdown Standards**
   - File naming conventions
   - Header hierarchy
   - Formatting guidelines (bold, italic, code)
   - Lists and tables
   - Code blocks with syntax highlighting
   - Links and images
   - Blockquotes and GitHub alerts

3. **Code Comment Standards**
   - C# XML documentation guidelines
   - Required tags for public APIs
   - Inline comment best practices
   - When to comment (and when not to)

4. **Changelog Format**
   - Keep a Changelog standard
   - Change categories (Added, Changed, Fixed, etc.)
   - Best practices for maintaining changelogs

5. **Wiki Structure and Organization**
   - Recommended wiki structure
   - Page naming conventions
   - Cross-referencing guidelines
   - Maintenance procedures

6. **User Guide and Help Documentation**
   - User-centric documentation principles
   - Task-oriented structure
   - Writing tips for user documentation
   - Screenshot and image guidelines

7. **API Documentation**
   - API documentation requirements
   - Endpoint documentation format
   - Request/response examples

8. **Architecture Decision Records (ADRs)**
   - ADR standards and conventions
   - When to create an ADR
   - Numbering and naming guidelines

## Quick Reference

### For New Contributors

If you're new to the project and need to write documentation:

1. **Start here**: Read [standards.md](./standards.md) to understand our documentation conventions
2. **Use templates**: Check out the [templates directory](../../templates/) for ready-to-use templates
3. **Follow examples**: Look at existing documentation in the project for reference
4. **Ask questions**: Don't hesitate to ask maintainers if you're unsure

### For Documentation Writers

#### Creating New Documentation

```bash
# 1. Choose appropriate template from templates/
# 2. Copy template to appropriate location
# 3. Fill in the content following standards.md guidelines
# 4. Review and validate before committing
```

#### Updating Existing Documentation

```bash
# 1. Review current content
# 2. Make necessary updates
# 3. Ensure formatting follows standards.md
# 4. Update "Last Updated" date
# 5. Commit with descriptive message
```

### For Code Reviewers

When reviewing pull requests with documentation changes:

- ✅ Check adherence to standards.md guidelines
- ✅ Verify markdown formatting is correct
- ✅ Ensure code examples are accurate and tested
- ✅ Validate all links work properly
- ✅ Check for spelling and grammar errors
- ✅ Verify screenshots are current (if applicable)

## Documentation Standards Summary

### Markdown Files

- Use `.md` extension
- Name files with lowercase and hyphens: `my-document.md`
- Include table of contents for long documents
- Use proper heading hierarchy (H1 → H2 → H3)

### Code Comments

For C#, use XML documentation for public APIs:

```csharp
/// <summary>
/// Brief description of the method.
/// </summary>
/// <param name="parameter">Parameter description</param>
/// <returns>Return value description</returns>
public ReturnType MethodName(ParameterType parameter)
{
    // Implementation
}
```

### Changelog Format

Follow Keep a Changelog format:

```markdown
## [Version] - YYYY-MM-DD

### Added
- New feature

### Fixed
- Bug fix
```

### Architecture Decision Records

Use the ADR template and sequential numbering:
- `ADR-0001-title-of-decision.md`
- `ADR-0002-another-decision.md`

## Templates Available

All templates are located in the [templates directory](../../templates/):

| Template | Purpose | Use Case |
|----------|---------|----------|
| README-template.md | Project overview and documentation | New projects, modules, components |
| CONTRIBUTING-template.md | Contribution guidelines | Setting up contribution workflow |
| API-DOC-template.md | API documentation | REST APIs, library interfaces |
| ADR-template.md | Architecture decisions | Significant technical decisions |

## Best Practices

### Do's ✅

- **Do** follow the standards in standards.md
- **Do** use templates when available
- **Do** include code examples
- **Do** add screenshots for UI-related docs
- **Do** test all code examples before committing
- **Do** keep documentation up to date
- **Do** use clear, concise language
- **Do** spell check and grammar check

### Don'ts ❌

- **Don't** commit without reviewing documentation standards
- **Don't** use inconsistent formatting
- **Don't** leave placeholder text in final docs
- **Don't** include untested code examples
- **Don't** use outdated screenshots
- **Don't** commit broken links
- **Don't** assume readers know technical jargon

## Documentation Review Checklist

Before committing documentation:

- [ ] Content is accurate and up to date
- [ ] Follows standards.md guidelines
- [ ] Markdown formatting is correct
- [ ] All links are valid and working
- [ ] Code examples are tested and working
- [ ] Screenshots are current (if applicable)
- [ ] Spelling and grammar checked
- [ ] Table of contents updated (if applicable)
- [ ] "Last Updated" date is current

## Tools and Resources

### Recommended Tools

- **Markdown Editors**: 
  - VS Code with Markdown extensions
  - Typora
  - MarkText
  
- **Spell Checkers**:
  - VS Code spell checker extensions
  - Grammarly
  
- **Link Checkers**:
  - markdown-link-check
  - linkchecker

### External Resources

- [Markdown Guide](https://www.markdownguide.org/)
- [GitHub Flavored Markdown Spec](https://github.github.com/gfm/)
- [Keep a Changelog](https://keepachangelog.com/)
- [C# XML Documentation Comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)
- [Architecture Decision Records](https://adr.github.io/)

## Contributing to Documentation

Documentation improvements are always welcome! To contribute:

1. **Small fixes** (typos, grammar): Create a PR directly
2. **Larger changes**: Open an issue first to discuss
3. **New sections**: Propose in an issue before implementing
4. **Template updates**: Discuss with maintainers first

See [CONTRIBUTING.md](../../CONTRIBUTING.md) for full contribution guidelines.

## Questions and Support

If you have questions about documentation:

- 📖 Read [standards.md](./standards.md) thoroughly
- 💬 Ask in GitHub Discussions
- 🐛 Report documentation issues in the issue tracker
- 📧 Contact documentation maintainers

## Maintenance

### Regular Reviews

Documentation should be reviewed:
- **Quarterly**: Check for outdated information
- **With each release**: Update version-specific docs
- **On major changes**: Update affected documentation

### Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2024-11-05 | Initial documentation standards and templates |

---

**Last Updated**: 2024-11-05  
**Maintained By**: UniGetUI Contributors  
**Version**: 1.0.0
