# UniGetUI Governance

## Overview

This document outlines the governance structure for the UniGetUI project and its CodingKit framework. It defines decision-making processes, roles and responsibilities, and how the project evolves over time.

## Project Mission

UniGetUI aims to provide an intuitive graphical interface for managing Windows package managers, with a focus on:
- User-friendly design and experience
- Reliability and stability
- Support for multiple package managers
- Community-driven development
- High-quality code and documentation standards

The **CodingKit** framework supports this mission by establishing coding standards, design patterns, and best practices that ensure consistency, maintainability, and quality across the codebase.

## Governance Principles

### Transparency

- All decisions are made in the open
- Discussions happen in public forums (GitHub Issues, Discussions, PRs)
- Meeting notes and decisions are documented
- Rationale for decisions is clearly communicated

### Meritocracy

- Contributions are valued based on quality and impact
- Anyone can contribute and earn recognition
- Path to maintainership is open and merit-based
- Expertise and consistent contribution are recognized

### Consensus-Seeking

- Decisions aim for broad consensus
- Community input is valued and considered
- When consensus isn't possible, maintainers make final decisions
- Dissenting opinions are respected and documented

### Inclusivity

- Everyone is welcome to participate
- Code of Conduct is enforced
- Multiple perspectives are valued
- Barriers to contribution are minimized

## Organizational Structure

### Roles and Responsibilities

#### Project Owner/Lead Maintainer

**Current:** @marticliment

**Responsibilities:**
- Final decision authority on project direction
- Resolve disputes when consensus cannot be reached
- Manage project infrastructure and resources
- Represent the project externally
- Appoint maintainers

**Authority:**
- Veto power on major decisions (used sparingly)
- Access to all project resources
- Final say on maintainer appointments

#### Maintainers

**Responsibilities:**
- Review and merge pull requests
- Approve RFCs for CodingKit changes
- Triage and prioritize issues
- Guide project roadmap
- Mentor contributors
- Enforce Code of Conduct
- Make day-to-day project decisions

**Authority:**
- Merge pull requests
- Approve/reject RFCs
- Close issues
- Manage labels and milestones
- Grant contributor access

**How to Become a Maintainer:**
1. Demonstrate consistent, high-quality contributions
2. Show understanding of project goals and architecture
3. Participate actively in reviews and discussions
4. Support and mentor other contributors
5. Nominated by existing maintainer
6. Approved by project lead

#### CodingKit Reviewers

**Responsibilities:**
- Review CodingKit contributions (patterns, standards, documentation)
- Provide expert feedback on proposed changes
- Ensure consistency with existing standards
- Help evaluate RFCs

**Authority:**
- Provide binding feedback on CodingKit changes
- Request changes to contributions
- Participate in RFC approval process

**How to Become a CodingKit Reviewer:**
- Expertise in software architecture and design patterns
- Deep understanding of CodingKit principles
- Multiple accepted contributions to CodingKit
- Invited by maintainers

#### Contributors

**Who:** Anyone who contributes to the project

**Types of Contributions:**
- Code (features, bug fixes, improvements)
- Documentation
- Bug reports
- Feature requests
- Design and UX feedback
- Testing
- Community support

**Recognition:**
- Listed in contributors section
- Mentioned in release notes
- Eligible for maintainer role

#### Community Members

**Who:** Users, supporters, and participants

**Participation:**
- Use and test the software
- Report bugs and issues
- Suggest features and improvements
- Participate in discussions
- Help other users
- Promote the project

## Decision-Making Process

### Types of Decisions

#### Routine Decisions
**Examples:** Bug fixes, documentation updates, minor improvements

**Process:**
- PR submitted
- Reviewed by one or more maintainers
- Merged when approved

**Timeline:** 1-7 days

#### Standard Decisions
**Examples:** New features, refactoring, dependency updates

**Process:**
- Issue or discussion opened
- Community input gathered
- PR submitted with implementation
- Reviewed by maintainers
- Merged when approved

**Timeline:** 1-4 weeks

#### Significant Decisions
**Examples:** Major features, architectural changes, breaking API changes

**Process:**
1. GitHub Discussion opened for initial feedback
2. RFC created using CodingKit RFC template
3. Community review period (14+ days)
4. Maintainers review and discuss
5. Approval requires consensus of maintainers
6. Implementation PR created
7. Final review and merge

**Timeline:** 4-8 weeks

#### CodingKit Changes
**Examples:** New patterns, standard updates, governance changes

**Process:**
- Follow [CodingKit Maintenance Process](./docs/governance/maintenance-process.md)
- Minor changes: Standard PR process
- Major changes: RFC required with extended review

**Timeline:** Varies by change type (see maintenance process)

#### Strategic Decisions
**Examples:** Project direction, major partnerships, funding decisions

**Process:**
- Proposed by project lead or maintainers
- Extended community discussion
- Maintainer consensus required
- Project lead makes final decision if needed

**Timeline:** Varies (2-12 weeks)

### Consensus and Voting

#### Consensus-Seeking

Most decisions are made by consensus:
- **Consensus:** General agreement with no strong objections
- **Rough consensus:** Most agree, minority concerns are heard and addressed
- **No consensus:** Maintainers vote or project lead decides

#### When Voting Is Needed

Voting is used when:
- Consensus cannot be reached after good-faith discussion
- Timeline requires a decision
- Significant disagreement exists

**Voting Process:**
- Only maintainers vote
- Simple majority needed for standard decisions
- Supermajority (2/3) needed for major changes
- Project lead can break ties

**Voting Timeline:**
- Voting announced with at least 7 days notice
- Voting period: 7 days
- Results announced publicly

### Vetoes

The project lead may veto decisions that:
- Compromise project security
- Violate licensing or legal requirements
- Fundamentally contradict project mission
- Risk project sustainability

Vetoes are rare and include:
- Clear explanation of reasoning
- Alternative path forward
- Community discussion of concerns

## CodingKit Governance

### Purpose

CodingKit establishes standards for code quality, architecture, and best practices across UniGetUI. It requires special governance to ensure consistency and thoughtful evolution.

### CodingKit Scope

CodingKit includes:
- Coding standards and conventions
- Design patterns and architectural guidelines
- Documentation standards
- Testing practices
- Review processes
- Maintenance procedures

### Change Management

Changes to CodingKit follow the [Maintenance Process](./docs/governance/maintenance-process.md):

| Change Type | Approval Process | Timeline |
|-------------|-----------------|----------|
| Patch (docs fixes) | 1 maintainer | 1-3 days |
| Minor (new patterns) | 2 maintainers | 3-7 days |
| Major (breaking changes) | All maintainers + community | 14-30 days |

### RFC Process

Significant CodingKit changes require an RFC:

1. **Draft:** Author creates RFC using [template](./docs/governance/rfc-template.md)
2. **Review:** RFC opened as PR, community provides feedback
3. **Revision:** Author addresses feedback and revises
4. **Approval:** Maintainers approve when ready
5. **Implementation:** Approved RFC implemented via standard PR process

See [RFC Template](./docs/governance/rfc-template.md) for details.

### Versioning

CodingKit follows [Semantic Versioning](https://semver.org/):
- **Major:** Breaking changes to standards or patterns
- **Minor:** New patterns or significant additions
- **Patch:** Documentation fixes and clarifications

### Deprecation

Features deprecated from CodingKit follow a structured process:
- **Announce:** Mark as deprecated, provide migration path (1 major version)
- **Warn:** Upgrade to error state (1 major version)
- **Remove:** Remove in next major version

Minimum timeline: 2 major versions or 6 months, whichever is longer.

See [Maintenance Process](./docs/governance/maintenance-process.md) for details.

## Communication Channels

### GitHub

Primary communication platform:

**Issues:**
- Bug reports
- Feature requests
- Task tracking
- Questions

**Pull Requests:**
- Code contributions
- Documentation updates
- RFC proposals

**Discussions:**
- General discussion
- Ideas and brainstorming
- Q&A
- Announcements

### Other Channels

- **Release Notes:** Communicated via GitHub Releases
- **Security:** Reported via [disclosure program](https://whitehub.net/programs/unigetui)
- **Social Media:** Announcements and updates

## Contribution Process

### General Contributions

See [CONTRIBUTING.md](./CONTRIBUTING.md) for:
- Code contribution guidelines
- PR requirements
- Commit message format
- Review process

### CodingKit Contributions

See [CodingKit Contribution Guidelines](./docs/governance/contribution-guidelines.md) for:
- Types of contributions
- Standards and requirements
- RFC process
- Review criteria

### Code of Conduct

All participants must follow the [Code of Conduct](./CODE_OF_CONDUCT.md):
- Be respectful and professional
- Welcome diversity and inclusion
- Give and receive constructive feedback
- Focus on what's best for the community

Violations are handled by maintainers according to the enforcement guidelines.

## Release Process

### Release Types

#### Patch Releases (1.0.x)
- Bug fixes
- Documentation updates
- Security patches

**Frequency:** As needed (typically bi-weekly)

#### Minor Releases (1.x.0)
- New features
- Non-breaking improvements
- CodingKit minor updates

**Frequency:** Monthly or when features accumulate

#### Major Releases (x.0.0)
- Breaking changes
- Major new features
- CodingKit major updates

**Frequency:** Quarterly to semi-annually

### Release Process

1. **Planning:** Milestone created with target date
2. **Development:** Features and fixes implemented
3. **Testing:** Release candidate tested
4. **Documentation:** Release notes prepared
5. **Release:** Tagged and published
6. **Announcement:** Communicated via GitHub and other channels

### Version Support

- **Current major version:** Fully supported
- **Previous major version:** Security fixes for 6 months after new major release
- **Older versions:** Not supported

## Review and Updates

### Governance Review

This governance document is reviewed:
- **Quarterly:** Quick review for any issues
- **Annually:** Comprehensive review and updates
- **As Needed:** When significant issues arise

### Proposing Changes

Changes to governance require:
1. GitHub Discussion proposing changes
2. Community feedback period (14+ days)
3. Maintainer consensus
4. Project lead approval
5. PR with updates

### Amendment History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-11-05 | Initial governance document |

## Resources

### Documentation

- [Contributing Guidelines](./CONTRIBUTING.md) - How to contribute code
- [CodingKit Contribution Guidelines](./docs/governance/contribution-guidelines.md) - CodingKit-specific contributions
- [Maintenance Process](./docs/governance/maintenance-process.md) - How CodingKit is maintained
- [RFC Template](./docs/governance/rfc-template.md) - Template for proposals
- [Code of Conduct](./CODE_OF_CONDUCT.md) - Community standards
- [Security Policy](./SECURITY.md) - Security reporting

### External Resources

- [Semantic Versioning](https://semver.org/)
- [Contributor Covenant](https://www.contributor-covenant.org/)
- [GitHub Community Guidelines](https://docs.github.com/en/site-policy/github-terms/github-community-guidelines)

## Contact

### Maintainers

- Project Lead: @marticliment

### Reporting Issues

- **Bugs:** GitHub Issues
- **Security:** [Disclosure program](https://whitehub.net/programs/unigetui)
- **Code of Conduct violations:** Contact maintainers directly
- **General questions:** GitHub Discussions

## Acknowledgments

Thank you to all contributors, maintainers, and community members who help make UniGetUI a success!

This governance structure is inspired by successful open source projects and adapted to UniGetUI's specific needs.

---

**Last Updated:** 2025-11-05  
**Version:** 1.0.0
