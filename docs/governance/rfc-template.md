# RFC-XXXX: [Title]

## Metadata

- **RFC Number:** XXXX
- **Title:** [Brief, descriptive title]
- **Author(s):** [Name(s) and GitHub handle(s)]
- **Status:** [Draft | Under Review | Accepted | Rejected | Implemented]
- **Created:** [YYYY-MM-DD]
- **Last Updated:** [YYYY-MM-DD]
- **Related Issues:** [Links to related issues]
- **Related PRs:** [Links to related pull requests]

## Summary

A brief 2-3 sentence summary of the proposed change. This should be understandable by someone skimming the document.

## Motivation

### Problem Statement

Clearly describe the problem this RFC aims to solve. Include:
- What limitations exist in the current system?
- What pain points do developers or users experience?
- What use cases are not adequately supported?

### Goals

What are the primary objectives of this proposal? List them as:
- Goal 1: [Description]
- Goal 2: [Description]
- Goal 3: [Description]

### Non-Goals

What is explicitly out of scope for this proposal?
- Non-Goal 1: [Description]
- Non-Goal 2: [Description]

## Proposal

### Overview

Provide a high-level description of the proposed solution. Focus on the "what" rather than the "how."

### Detailed Design

Describe the proposed changes in detail. Include:

#### Architecture Changes

- What components will be added, modified, or removed?
- How will components interact?
- Include diagrams if helpful (use Mermaid syntax or link to external diagrams)

#### API Changes

```csharp
// Show examples of new or modified APIs
public interface INewInterface
{
    Task<Result> NewMethodAsync(Parameters params);
}
```

#### Configuration Changes

- What new configuration options are introduced?
- What existing configurations are affected?
- Provide examples:

```json
{
  "newFeature": {
    "enabled": true,
    "settings": {}
  }
}
```

#### Migration Strategy

How will existing code/data be migrated to the new system?
- What breaking changes are introduced?
- What migration steps are required?
- Can migration be automated?

### Examples

Provide concrete examples of how the proposed feature would be used:

```csharp
// Example 1: Basic usage
var manager = new CodingKitManager();
await manager.ProcessAsync();

// Example 2: Advanced usage
var result = await manager.ProcessWithOptionsAsync(new Options
{
    EnableFeature = true
});
```

### Edge Cases

Discuss how the proposal handles edge cases and error conditions:
- Edge Case 1: [Description and handling]
- Edge Case 2: [Description and handling]

## Alternatives Considered

### Alternative 1: [Name]

**Description:** [How this alternative would work]

**Pros:**
- Pro 1
- Pro 2

**Cons:**
- Con 1
- Con 2

**Why not chosen:** [Explanation]

### Alternative 2: [Name]

**Description:** [How this alternative would work]

**Pros:**
- Pro 1

**Cons:**
- Con 1

**Why not chosen:** [Explanation]

## Impact Analysis

### Compatibility

- **Backward Compatibility:** Is this change backward compatible? If not, describe the breaking changes.
- **Forward Compatibility:** Are there considerations for future changes?

### Performance

- What is the expected impact on performance?
- Are there any benchmarks or profiling results?
- What are the resource requirements (CPU, memory, disk)?

### Security

- Are there any security implications?
- What threat models are considered?
- How are security concerns mitigated?

### Testing

- What testing strategy will be used?
- What test coverage is expected?
- Are there specific test scenarios that must be covered?

### Documentation

- What documentation needs to be created or updated?
- Are there user-facing documentation changes?
- Are there developer documentation changes?

### Dependencies

- What new dependencies are introduced?
- Are there changes to existing dependencies?
- What is the impact on the dependency graph?

## Implementation Plan

### Phases

Break down the implementation into phases:

#### Phase 1: [Name]
- **Duration:** [Estimated time]
- **Deliverables:**
  - Deliverable 1
  - Deliverable 2
- **Dependencies:** [What must be completed first]

#### Phase 2: [Name]
- **Duration:** [Estimated time]
- **Deliverables:**
  - Deliverable 1
- **Dependencies:** [What must be completed first]

### Milestones

- [ ] Milestone 1: [Description]
- [ ] Milestone 2: [Description]
- [ ] Milestone 3: [Description]

### Resources Required

- Personnel: [Who is needed]
- Tools: [What tools are needed]
- Infrastructure: [What infrastructure is needed]

## Risks and Mitigation

| Risk | Likelihood | Impact | Mitigation Strategy |
|------|------------|--------|-------------------|
| Risk 1 | Low/Medium/High | Low/Medium/High | [How to mitigate] |
| Risk 2 | Low/Medium/High | Low/Medium/High | [How to mitigate] |

## Success Metrics

How will we measure the success of this proposal?

- Metric 1: [Description and target value]
- Metric 2: [Description and target value]
- Metric 3: [Description and target value]

## Open Questions

List any open questions that need to be resolved:

1. Question 1: [Description]
2. Question 2: [Description]

## References

- [Link to relevant documentation]
- [Link to related RFCs]
- [Link to external resources]

## Revision History

| Date | Author | Changes |
|------|--------|---------|
| YYYY-MM-DD | [Name] | Initial draft |
| YYYY-MM-DD | [Name] | [Description of changes] |

## Appendix

### Appendix A: [Title]

Additional supporting information, detailed diagrams, or extensive code examples.

### Appendix B: [Title]

Further supplementary material.
