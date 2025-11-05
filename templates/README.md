# Documentation Templates

This directory contains reusable documentation templates for the UniGetUI project.

## Available Templates

### 📄 [README-template.md](./README-template.md)
**Purpose**: Create comprehensive README files for projects, repositories, or modules.

**Use for**:
- Project repositories
- Sub-modules or components
- Package documentation
- Library documentation

**Key Sections**:
- Project overview and features
- Installation instructions
- Usage examples
- Configuration options
- Contributing guidelines
- License and support information

---

### 🤝 [CONTRIBUTING-template.md](./CONTRIBUTING-template.md)
**Purpose**: Provide clear guidelines for contributing to the project.

**Use for**:
- Main project contribution guidelines
- Component-specific contribution rules
- Setting contributor expectations

**Key Sections**:
- Code of conduct
- Development setup
- Coding standards
- Commit guidelines
- Pull request process
- Issue guidelines

---

### 🔌 [API-DOC-template.md](./API-DOC-template.md)
**Purpose**: Document RESTful APIs, libraries, or service interfaces.

**Use for**:
- REST API documentation
- Library API reference
- Service interfaces
- Integration guides

**Key Sections**:
- Authentication methods
- Rate limiting
- Endpoint documentation
- Request/response formats
- Error handling
- Code examples in multiple languages

---

### 🏗️ [ADR-template.md](./ADR-template.md)
**Purpose**: Record important architectural decisions with context and rationale.

**Use for**:
- Significant architectural changes
- Technology stack decisions
- Design pattern choices
- Trade-off decisions

**Key Sections**:
- Context and background
- Decision rationale
- Options considered
- Consequences and risks
- Implementation plan

---

## How to Use These Templates

### 1. Choose the Right Template
Select the template that best fits your documentation needs.

### 2. Copy the Template
```bash
# Example: Creating a new README for a component
cp templates/README-template.md src/MyComponent/README.md

# Example: Creating an Architecture Decision Record
cp templates/ADR-template.md docs/architecture/decisions/ADR-0001-choose-database.md
```

### 3. Fill in the Placeholders
- Replace `[Project Name]` with your actual project name
- Replace `[Your API Name]` with your API name
- Fill in all bracketed placeholders: `[...]`
- Remove sections that don't apply to your use case
- Add additional sections as needed

### 4. Follow the Standards
Refer to [Documentation Standards](../docs/documentation/standards.md) for detailed guidelines on:
- Markdown formatting
- Code comment standards
- Changelog format
- Wiki structure
- User guide best practices

### 5. Review and Validate
Before committing your documentation:
- ✅ Spell check
- ✅ Grammar check
- ✅ Link validation
- ✅ Code example testing
- ✅ Screenshot verification (if applicable)

## Quick Start Examples

### Example 1: Creating a README for a New Module

```bash
# Navigate to your module directory
cd src/NewModule

# Copy the README template
cp ../../templates/README-template.md ./README.md

# Edit the README
# - Replace [Project Name] with "NewModule"
# - Fill in the description, features, and usage
# - Add code examples specific to your module
# - Update installation instructions
```

### Example 2: Documenting an API

```bash
# Copy the API documentation template
cp templates/API-DOC-template.md docs/api/user-service-api.md

# Edit the API documentation
# - Replace [Your API Name] with "User Service API"
# - Document all endpoints
# - Add authentication details
# - Include request/response examples
# - Add code samples for different languages
```

### Example 3: Creating an Architecture Decision Record

```bash
# Determine the next ADR number (e.g., 0005)
# Copy the template with sequential numbering
cp templates/ADR-template.md docs/architecture/decisions/ADR-0005-migrate-to-sqlite.md

# Edit the ADR
# - Replace [NUMBER] with "0005"
# - Replace [Title of Decision] with "Migrate to SQLite Database"
# - Fill in the context, decision rationale, and options
# - Document consequences and implementation plan
```

## Template Customization

You can customize these templates for your specific needs:

1. **Add Organization-Specific Sections**: Include sections relevant to your organization
2. **Modify Structure**: Reorganize sections to match your workflow
3. **Update Examples**: Replace generic examples with project-specific ones
4. **Adjust Formatting**: Modify formatting to match your style guide

## Documentation Standards

For comprehensive documentation guidelines, see:

📖 [Documentation Standards](../docs/documentation/standards.md)

This document covers:
- General documentation principles
- Markdown standards and best practices
- C# XML documentation guidelines
- Changelog format (Keep a Changelog)
- Wiki structure and organization
- User guide and help documentation
- API documentation standards
- Architecture Decision Records

## Additional Resources

- [Markdown Guide](https://www.markdownguide.org/)
- [Keep a Changelog](https://keepachangelog.com/)
- [Semantic Versioning](https://semver.org/)
- [C# XML Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)
- [GitHub Flavored Markdown](https://github.github.com/gfm/)
- [Architecture Decision Records](https://adr.github.io/)

## Feedback and Improvements

If you have suggestions for improving these templates:

1. Open an issue describing the improvement
2. Submit a pull request with your changes
3. Discuss in the project's discussions area

---

**Version**: 1.0.0  
**Last Updated**: 2024-11-05  
**Maintained By**: UniGetUI Contributors
