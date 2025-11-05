# CodingKit Contribution Guidelines

## Welcome Contributors! 🎉

Thank you for your interest in contributing to the CodingKit framework for Windows Applications! This document provides guidelines specifically for contributing to CodingKit standards, patterns, and governance documentation.

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
- [Types of Contributions](#types-of-contributions)
- [Contribution Workflow](#contribution-workflow)
- [Standards and Requirements](#standards-and-requirements)
- [Review Process](#review-process)
- [Community Guidelines](#community-guidelines)
- [Recognition](#recognition)

## Overview

### What is CodingKit?

CodingKit is a comprehensive framework of coding standards, design patterns, best practices, and governance processes for Windows application development. It includes:

- Coding standards and naming conventions
- Design patterns and architectural guidelines
- Documentation templates and standards
- Testing strategies and practices
- Governance and maintenance processes

### Who Can Contribute?

Everyone! We welcome contributions from:
- Experienced developers with expertise in specific areas
- Newcomers who spot issues or have questions
- Technical writers who can improve documentation
- Community members who provide feedback

## Getting Started

### Before You Contribute

1. **Read existing documentation:**
   - [Coding Standards](../codebase-analysis/07-best-practices/patterns-standards.md)
   - [Maintenance Process](./maintenance-process.md)
   - [Main Contributing Guidelines](../../CONTRIBUTING.md)

2. **Check existing issues and RFCs:**
   - Look for related discussions
   - Avoid duplicate work
   - Join ongoing conversations

3. **Understand the scope:**
   - CodingKit focuses on framework-level standards
   - Application-specific code goes through standard contribution process
   - Governance documents require broader consensus

### Setting Up Your Environment

```bash
# Clone the repository
git clone https://github.com/TimLuong/UniGetUI.git
cd UniGetUI

# Create a new branch for your contribution
git checkout -b feature/your-contribution-name

# Make your changes
# ... edit files ...

# Commit and push
git add .
git commit -m "Brief description of changes"
git push origin feature/your-contribution-name
```

## Types of Contributions

### 1. Documentation Improvements

**What:** Fixing typos, clarifying explanations, adding examples

**Process:**
- Create a pull request with your changes
- Reference related issues if applicable
- No RFC needed for minor documentation fixes

**Examples:**
- Fixing typos in governance documents
- Adding clarifying examples to coding standards
- Improving formatting and readability
- Adding missing references or links

**Requirements:**
- Follow existing documentation structure
- Maintain consistent terminology
- Ensure technical accuracy

### 2. New Patterns or Standards

**What:** Adding new coding patterns, design principles, or best practices

**Process:**
- Create an RFC if the pattern is significant
- Include rationale, examples, and trade-offs
- Provide code examples demonstrating the pattern
- Show how it complements existing patterns

**Examples:**
- New design pattern for handling state
- Best practice for error handling in specific scenarios
- New architectural pattern for component organization
- Optimization techniques

**Requirements:**
- Pattern must be proven in practice
- Include working code examples
- Document when to use (and not use) the pattern
- Explain relationship to existing patterns

### 3. Standard Updates

**What:** Updating or modifying existing standards

**Process:**
- Create an RFC describing the change and rationale
- Explain impact on existing code
- Provide migration guidance if breaking
- Allow community feedback period

**Examples:**
- Changing naming conventions
- Updating architectural patterns
- Modifying testing requirements
- Revising documentation standards

**Requirements:**
- Strong justification for changes
- Impact analysis on existing codebase
- Migration path for breaking changes
- Community consensus for major changes

### 4. Governance Improvements

**What:** Updates to governance processes and procedures

**Process:**
- Open a GitHub Discussion first to gauge interest
- Create an RFC for significant process changes
- Allow extended feedback period (14+ days)
- Require maintainer approval

**Examples:**
- Refining RFC process
- Updating review requirements
- Modifying release procedures
- Adding new feedback mechanisms

**Requirements:**
- Clear problem statement
- Evidence of current process issues
- Specific, actionable proposals
- Consideration of unintended consequences

### 5. Examples and Tutorials

**What:** Adding examples, tutorials, or how-to guides

**Process:**
- Can be contributed directly via PR
- Should reference existing standards
- Include context about when to use

**Examples:**
- Step-by-step tutorial for using a pattern
- Complete example application demonstrating standards
- Common scenarios and solutions
- Anti-patterns to avoid

**Requirements:**
- Code must compile and run
- Follow all CodingKit standards
- Include explanatory comments
- Provide context and use cases

## Contribution Workflow

### Step 1: Identify the Need

- **Bug or issue:** Check if it's already reported
- **Enhancement:** Search for related discussions or RFCs
- **Question:** Use GitHub Discussions first

### Step 2: Propose Your Contribution

#### For Minor Changes (Documentation fixes, small clarifications)

1. Create an issue describing the change
2. Fork the repository
3. Make your changes
4. Submit a pull request

#### For Significant Changes (New patterns, standard updates)

1. Open a GitHub Discussion to gather initial feedback
2. Create an RFC using the [RFC template](./rfc-template.md)
3. Submit RFC as a pull request
4. Engage in discussion and revise as needed
5. Wait for RFC approval
6. Implement the approved changes
7. Submit implementation PR

### Step 3: Create Your Contribution

#### Writing Documentation

Follow these guidelines:

**Structure:**
- Use clear, hierarchical headings
- Include table of contents for long documents
- Organize information logically
- Use consistent formatting

**Content:**
- Be clear and concise
- Define technical terms
- Include practical examples
- Provide context and rationale

**Style:**
- Write in present tense
- Use active voice
- Avoid jargon when possible
- Be inclusive and welcoming

**Formatting:**
```markdown
# Main Heading

## Section Heading

### Subsection Heading

**Bold for emphasis**
*Italic for terms*

- Bullet points for lists
- Keep items parallel

1. Numbered lists for procedures
2. Each step is clear and actionable

`inline code` for short code references

```csharp
// Code blocks for examples
public class Example
{
    public void Method() { }
}
```
```

#### Adding Code Examples

**Requirements:**
- Code must compile without errors
- Follow all CodingKit standards
- Include necessary using statements
- Add explanatory comments

**Template:**
```csharp
// Brief description of what this example demonstrates
using System;
using System.Threading.Tasks;

namespace UniGetUI.Examples
{
    /// <summary>
    /// Demonstrates [pattern/feature name]
    /// </summary>
    public class ExampleClass
    {
        // Clear, working example code
        public async Task<string> ExampleMethodAsync()
        {
            // Comments explaining key points
            return await Task.FromResult("Result");
        }
    }
}
```

### Step 4: Submit Your Contribution

#### Pull Request Guidelines

**Title:**
- Use clear, descriptive titles
- Prefix with category: `[Docs]`, `[RFC]`, `[Pattern]`, `[Governance]`
- Keep under 72 characters

**Description:**
Use this template:

```markdown
## Description
Brief summary of what this PR does and why.

## Type of Change
- [ ] Documentation fix/improvement
- [ ] New pattern or standard
- [ ] Standard update (breaking/non-breaking)
- [ ] Governance process change
- [ ] Example or tutorial
- [ ] Other: _____

## Related Issues/RFCs
Fixes #123
Related to RFC-001

## Changes Made
- Change 1
- Change 2
- Change 3

## Impact
- Who is affected by this change?
- What is the scope of impact?

## Testing/Validation
How was this validated?
- [ ] Reviewed by peers
- [ ] Tested with code examples
- [ ] Checked against existing standards

## Checklist
- [ ] Follows CodingKit standards
- [ ] Documentation is clear and complete
- [ ] Examples compile and run
- [ ] No typos or formatting issues
- [ ] Linked to related issues/RFCs
- [ ] Updated table of contents if needed
```

**Labels:**
Add appropriate labels:
- `codingkit` - All CodingKit contributions
- `documentation` - Documentation changes
- `RFC` - Request for Comments
- `breaking-change` - Breaking changes
- `help-wanted` - Need community input
- `good-first-issue` - Good for newcomers

### Step 5: Engage in Review

**Expectations:**
- Respond to feedback within 7 days
- Be open to suggestions and changes
- Explain your reasoning clearly
- Make requested changes promptly

**During Review:**
1. Address all reviewer comments
2. Make changes based on feedback
3. Mark conversations as resolved when addressed
4. Request re-review after updates

**If Changes Are Requested:**
1. Don't take it personally - all contributions go through revision
2. Ask questions if feedback is unclear
3. Discuss alternative approaches if you disagree
4. Be willing to compromise

### Step 6: Get Merged

Once approved:
- Maintainers will merge your PR
- Your contribution will be part of the next release
- You'll be added to the contributors list
- Thank you! 🎉

## Standards and Requirements

### Quality Standards

All contributions must meet these standards:

#### Technical Accuracy
- Information is correct and up-to-date
- Code examples work as described
- References are valid and accessible

#### Clarity
- Explanations are clear and understandable
- Terminology is consistent
- Examples are relevant and helpful

#### Completeness
- All necessary information is included
- Edge cases are addressed
- Related concepts are linked

#### Consistency
- Follows existing formatting and style
- Uses consistent terminology
- Aligns with CodingKit principles

### Code Example Requirements

Code examples must:

1. **Compile and Run:**
   ```csharp
   // All examples must be complete and functional
   using System;
   
   public class ValidExample
   {
       public void Method()
       {
           Console.WriteLine("This works!");
       }
   }
   ```

2. **Follow Standards:**
   - Use camelCase for variables
   - Use PascalCase for classes and methods
   - Include XML documentation comments
   - Handle errors appropriately

3. **Be Self-Contained:**
   - Include all necessary using statements
   - Don't reference non-existent code
   - Provide context

4. **Be Illustrative:**
   - Focus on demonstrating one concept clearly
   - Avoid unnecessary complexity
   - Include helpful comments

### Documentation Standards

#### Markdown Formatting

- Use ATX-style headers (`#` not `===`)
- Include blank lines around headers
- Use fenced code blocks with language tags
- Use tables for structured data

#### Links and References

```markdown
<!-- Internal links -->
[Link text](./relative-path.md)
[Section link](#section-heading)

<!-- External links -->
[External reference](https://example.com)

<!-- Reference-style links for frequently used URLs -->
[RFC 001][rfc-001]

[rfc-001]: ./rfcs/rfc-001.md
```

#### Code Blocks

```markdown
<!-- Always specify language -->
```csharp
public class Example { }
```

<!-- Use appropriate language tags -->
```json
{ "key": "value" }
```

```bash
echo "Shell commands"
```
```

## Review Process

### Review Timeline

| Contribution Type | Review Time | Approvals Needed |
|------------------|-------------|------------------|
| Documentation fixes | 1-3 days | 1 maintainer |
| New examples | 3-5 days | 1 maintainer |
| New patterns | 5-7 days | 2 maintainers |
| Standard updates | 7-14 days | 2 maintainers + community |
| Breaking changes | 14-30 days | All maintainers + community |
| Governance changes | 14-30 days | All maintainers + community |

### What Reviewers Look For

**Technical Review:**
- Correctness: Is the information accurate?
- Completeness: Is anything missing?
- Examples: Do code examples work?
- Standards: Does it follow CodingKit standards?

**Content Review:**
- Clarity: Is it easy to understand?
- Organization: Is it well-structured?
- Consistency: Does it fit with existing content?
- Value: Does it add value to CodingKit?

**Process Review:**
- Appropriate change type and process followed
- Necessary approvals obtained
- Community feedback addressed (if required)
- Documentation updated appropriately

### Addressing Review Comments

**Types of Feedback:**

1. **Must Fix:** Required changes before merge
   - Technical errors
   - Standards violations
   - Missing critical information

2. **Should Fix:** Strongly recommended changes
   - Clarity improvements
   - Better examples
   - Additional context

3. **Nice to Have:** Optional suggestions
   - Alternative approaches
   - Additional examples
   - Future enhancements

**How to Respond:**

```markdown
<!-- Agree and implement -->
> Reviewer: This example should include error handling

Good point! I've added a try-catch block in commit abc123.

<!-- Disagree with reasoning -->
> Reviewer: This pattern seems too complex

I understand the concern. However, this complexity is necessary 
because [reason]. I've added a comment explaining this in the code.

<!-- Ask for clarification -->
> Reviewer: This could be clearer

Could you help me understand what's unclear? Is it the explanation 
or the example that needs work?
```

## Community Guidelines

### Code of Conduct

All contributors must follow the [Code of Conduct](../../CODE_OF_CONDUCT.md). Key points:

- Be respectful and inclusive
- Welcome newcomers
- Give and accept constructive feedback gracefully
- Focus on what's best for the community
- Show empathy and kindness

### Communication Best Practices

**In Issues and PRs:**
- Be clear and specific
- Provide context
- Be patient
- Stay on topic
- Use reactions instead of "+1" comments

**In Discussions:**
- Listen to different perspectives
- Assume good intent
- Disagree respectfully
- Back up opinions with reasoning
- Know when to agree to disagree

**In Reviews:**
- Be constructive, not critical
- Explain the "why" behind suggestions
- Offer specific suggestions
- Praise good work
- Be timely with responses

### Getting Help

**Questions about CodingKit:**
- Use [GitHub Discussions](https://github.com/TimLuong/UniGetUI/discussions)
- Tag your question with `codingkit` label
- Check existing discussions first

**Questions about contribution process:**
- Comment on the relevant issue or RFC
- Ask in your pull request
- Reach out to maintainers

**Technical questions:**
- Use GitHub Discussions Q&A section
- Reference specific documentation
- Provide code examples when relevant

## Recognition

### Contributors

All contributors are recognized in:
- Contributors list in repository
- Release notes (for significant contributions)
- README.md contributors section
- Special recognition for major contributions

### Types of Contributions Recognized

- Code contributions (patterns, examples)
- Documentation improvements
- RFC authorship
- Review and feedback
- Community support
- Bug reports
- Feature suggestions

### Becoming a Maintainer

Active contributors may be invited to become maintainers. Criteria:

- Consistent, high-quality contributions
- Deep understanding of CodingKit principles
- Active participation in reviews
- Community support and mentorship
- Alignment with project values

Maintainers have additional responsibilities:
- Review and approve contributions
- Guide RFC process
- Make governance decisions
- Mentor new contributors
- Represent the project

## Additional Resources

### Related Documentation

- [Maintenance Process](./maintenance-process.md) - How CodingKit is maintained
- [RFC Template](./rfc-template.md) - Template for proposals
- [Governance Overview](../../GOVERNANCE.md) - Overall governance structure
- [Coding Standards](../codebase-analysis/07-best-practices/patterns-standards.md) - Current standards
- [Main Contributing Guidelines](../../CONTRIBUTING.md) - General contribution guidelines

### Tools and Resources

- [GitHub Markdown Guide](https://guides.github.com/features/mastering-markdown/)
- [Semantic Versioning](https://semver.org/)
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

### Examples

<!-- Note: Example RFCs and contributions will be added as the governance framework is used -->
<!-- Check back for real-world examples of good patterns and contributions -->

- [Good RFC Example] - Coming soon as RFCs are created
- [Good Pattern Documentation](../codebase-analysis/07-best-practices/patterns-standards.md)
- [Example Contributions](../../CONTRIBUTING.md)

## FAQ

### How do I know if my contribution needs an RFC?

- **Patch changes** (docs fixes, typos): No RFC needed
- **Minor changes** (new examples, small patterns): RFC optional but recommended
- **Major changes** (breaking changes, new paradigms): RFC required

When in doubt, ask in a GitHub Discussion first!

### Can I contribute if I'm new to the project?

Absolutely! We welcome contributions from everyone. Good starting points:
- Documentation improvements
- Adding examples
- Clarifying explanations
- Reporting issues

Look for issues tagged with `good-first-issue`.

### What if my RFC is rejected?

- Understand the reasons for rejection
- Consider revising and resubmitting
- Alternative approaches may be suggested
- Not all ideas can be accepted, and that's okay

### How long does review take?

Depends on the type of contribution:
- Simple docs fixes: 1-3 days
- New patterns: 5-7 days  
- Major changes: 14-30 days

If your PR hasn't been reviewed, politely ping after the expected timeframe.

### Can I work on multiple contributions at once?

Yes! But:
- Keep them in separate branches
- Submit separate PRs
- Focus on getting each one merged before starting many new ones

### What if I disagree with a reviewer?

- Explain your reasoning clearly
- Be open to discussion
- Understand that reviewers have context you might lack
- If unresolved, ask other maintainers for input
- Final decision rests with maintainers

### How can I help review contributions?

- Anyone can review and provide feedback
- Focus on clarity, accuracy, and helpfulness
- Be constructive and respectful
- Only maintainers can approve, but all feedback is valued

## Thank You!

Thank you for contributing to CodingKit! Your efforts help make Windows application development better for everyone.

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-11-05 | Initial version |
