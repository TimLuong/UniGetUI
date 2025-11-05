# Architectural Decisions (ADRs)

> **Architecture Decision Records** document key architectural choices, their context, and rationale.

## Overview

This document captures important architectural decisions made during the development of UniGetUI. Each decision includes:
- **Context**: Why the decision was needed
- **Decision**: What was chosen
- **Rationale**: Why this approach was selected
- **Consequences**: Trade-offs and implications
- **Status**: Current state (Active, Superseded, Deprecated)

## Decision Records

### ADR-001: Use Layered Architecture Pattern

**Status**: Active  
**Date**: Project Inception  
**Context**: Need clear separation between UI, business logic, and external integrations

**Decision**: Implement a strict layered architecture with:
- UI Layer (WinUI 3)
- Core Services Layer (Settings, Logging, Language)
- Package Engine Layer (Unified package management)
- Package Manager Adapters (Individual manager implementations)

**Rationale**:
- Promotes separation of concerns
- Enables independent testing of layers
- Allows UI changes without affecting business logic
- Facilitates code reuse across the application

**Consequences**:
- ✅ Improved testability and maintainability
- ✅ Clear code organization
- ⚠️ More complex initial structure
- ⚠️ Requires discipline to maintain boundaries

**Related Documentation**: [Layered Architecture](./layered-architecture.md)

---

### ADR-002: WinUI 3 for User Interface

**Status**: Active  
**Date**: Project Inception  
**Context**: Need modern Windows desktop UI framework

**Decision**: Use WinUI 3 (Windows App SDK) instead of WPF or Windows Forms

**Rationale**:
- Modern, native Windows 11/10 look and feel
- Better performance than WPF for complex UIs
- Access to latest Windows features (e.g., Mica, Acrylic)
- Microsoft's recommended framework for new Windows apps
- Strong community support via CommunityToolkit.WinUI

**Consequences**:
- ✅ Modern UI with native Windows styling
- ✅ Good performance with large package lists
- ✅ Future-proof technology choice
- ⚠️ Windows-only (acceptable for Windows package managers)
- ⚠️ Breaking changes in early versions (now stabilized)

---

### ADR-003: CLI-Based Package Manager Integration

**Status**: Active  
**Date**: Project Inception  
**Context**: Need to integrate with multiple package managers

**Decision**: Integrate with package managers via their CLI interfaces (with WinGet optionally using COM API)

**Rationale**:
- CLI interfaces are stable public contracts
- Works with all package managers consistently
- No dependency on undocumented APIs
- Simpler implementation and debugging
- Easier to maintain as CLIs change less frequently than internal APIs

**Consequences**:
- ✅ Universal compatibility
- ✅ Stable integration surface
- ✅ Easy to debug (can run commands manually)
- ⚠️ Performance overhead of process spawning
- ⚠️ Output parsing complexity
- ✅ WinGet COM API available for better performance when needed

**Related Documentation**: [Package Managers](../09-integration/package-managers.md)

---

### ADR-004: Use IPackageManager Interface

**Status**: Active  
**Date**: Project Inception  
**Context**: Need consistent API across different package managers

**Decision**: Define `IPackageManager` interface that all package managers must implement

**Rationale**:
- Provides uniform API for UI and business logic
- Enables plugin-style architecture
- Allows adding new package managers without changing core code
- Facilitates testing with mock implementations

**Consequences**:
- ✅ Easy to add new package managers
- ✅ Consistent behavior across managers
- ✅ Testability via mocking
- ⚠️ Interface must accommodate all manager capabilities
- ⚠️ Least common denominator approach for some features

**Related Documentation**: [Design Patterns](../03-development-standards/design-patterns.md#pattern-4-strategy-pattern)

---

### ADR-005: Centralized Logging via Core.Logger

**Status**: Active  
**Date**: Project Inception  
**Context**: Need consistent logging across all components

**Decision**: All components log through `UniGetUI.Core.Logger`

**Rationale**:
- Unified log format and storage
- Easy debugging and troubleshooting
- Consistent error tracking
- Single place to manage log levels
- Simplified log viewing in UI

**Consequences**:
- ✅ Consistent logging across codebase
- ✅ Easy to aggregate and analyze logs
- ✅ Single configuration point
- ⚠️ Must remember to use Logger instead of Console.WriteLine
- ⚠️ Coupling to logging framework (acceptable)

**Related Documentation**: [Logging & Monitoring](../10-operations/logging-monitoring.md)

---

### ADR-006: Async-First Design

**Status**: Active  
**Date**: Project Inception  
**Context**: Package operations can be long-running

**Decision**: All package operations are asynchronous using async/await

**Rationale**:
- Package manager CLI commands can take seconds or minutes
- UI must remain responsive during operations
- Multiple operations can execute concurrently
- Better user experience with progress feedback
- Modern C# best practice

**Consequences**:
- ✅ Responsive UI
- ✅ Better concurrency
- ✅ Improved user experience
- ⚠️ More complex state management
- ⚠️ Requires careful exception handling

**Related Documentation**: [Async Patterns](../03-development-standards/async-patterns.md)

---

### ADR-007: PEInterface Facade

**Status**: Active  
**Date**: Project Inception  
**Context**: Need simple access point to package management functionality

**Decision**: Create `PEInterface` static class as facade to Package Engine

**Rationale**:
- Single entry point for all package operations
- Hides complexity of package engine initialization
- Makes package managers easily discoverable
- Simplifies UI code

**Consequences**:
- ✅ Simple, intuitive API for UI developers
- ✅ Easy discovery (`PEInterface.WinGet`, `PEInterface.Scoop`, etc.)
- ✅ Manages initialization complexity
- ⚠️ Static class (acceptable for singleton facade)

**Related Documentation**: [Package Engine](../04-core-systems/package-engine.md)

---

### ADR-008: TaskRecycler for Performance

**Status**: Active  
**Date**: Core Development  
**Context**: CPU-intensive operations called concurrently causing performance issues

**Decision**: Implement `TaskRecycler<T>` pattern to deduplicate concurrent operations

**Rationale**:
- Multiple UI components often request same data simultaneously
- Expensive operations (e.g., querying installed packages) were running multiple times
- Caching results reduces CPU and I/O load
- Improves responsiveness

**Consequences**:
- ✅ Significant performance improvement
- ✅ Reduced CPU usage
- ✅ Better user experience
- ⚠️ Shared mutable objects can cause issues
- ⚠️ Requires understanding when to use

**Related Documentation**: [Task Recycler](../04-core-systems/task-recycler.md)

---

### ADR-009: Local-First with Optional Cloud

**Status**: Active  
**Date**: Project Inception  
**Context**: Privacy concerns and offline capability requirements

**Decision**: Core functionality works entirely locally; cloud features optional

**Rationale**:
- User privacy and control prioritized
- No required internet connectivity (after package managers installed)
- Optional GitHub integration for backup/sync
- Optional telemetry for improving application
- Users opt-in to cloud features

**Consequences**:
- ✅ Privacy-focused design
- ✅ Works offline
- ✅ No vendor lock-in
- ⚠️ Limited sync capabilities
- ⚠️ Each feature must work locally

---

### ADR-010: File-Based Settings Storage

**Status**: Active  
**Date**: Project Inception  
**Context**: Need simple, reliable configuration storage

**Decision**: Use file-based settings storage in `%LOCALAPPDATA%\UniGetUI\`

**Rationale**:
- Simple implementation
- No database dependencies
- Easy to backup and transfer
- Transparent to users
- Portable mode support via relative paths

**Consequences**:
- ✅ Simple, reliable
- ✅ Easy backup and recovery
- ✅ Supports portable installations
- ⚠️ Not suitable for large data volumes
- ⚠️ No built-in query capabilities

**Related Documentation**: [Settings Management](../04-core-systems/settings-management.md)

---

### ADR-011: Multi-Language Support via LanguageEngine

**Status**: Active  
**Date**: Project Inception  
**Context**: Need to support international users

**Decision**: Implement `LanguageEngine` with translation files for 50+ languages

**Rationale**:
- Makes application accessible worldwide
- Community can contribute translations
- Uses industry-standard approaches
- Fallback to English when translation missing

**Consequences**:
- ✅ Wide international reach
- ✅ Community contribution opportunities
- ✅ Graceful fallback
- ⚠️ Requires translation maintenance
- ⚠️ Testing across languages is complex

**Related Documentation**: [Language Engine](../04-core-systems/language-engine.md)

---

### ADR-012: Background API for Widget Integration

**Status**: Active  
**Date**: Widget Development  
**Context**: Need external access for Windows Widgets integration

**Decision**: Expose REST API on `localhost:7058` with token authentication

**Rationale**:
- Enables widgets to query package update status
- Allows external tools to integrate with UniGetUI
- Token-based security prevents unauthorized access
- Local-only prevents external attacks

**Consequences**:
- ✅ Enables widget and tool integrations
- ✅ Secure with token authentication
- ✅ Local-only reduces attack surface
- ⚠️ Requires API maintenance
- ⚠️ Token must be securely managed

**Related Documentation**: [Background API](../09-integration/background-api.md)

---

## Decision Process

### When to Create an ADR

Create an ADR when making decisions that:
- Affect the overall structure or architecture
- Have long-term implications
- Are difficult or expensive to change later
- Impact multiple components or developers
- Involve significant trade-offs

### ADR Template

```markdown
### ADR-XXX: [Decision Title]

**Status**: [Active | Superseded | Deprecated]
**Date**: [YYYY-MM-DD]
**Context**: [What situation led to this decision?]

**Decision**: [What was decided?]

**Rationale**:
- [Why this approach?]
- [What alternatives were considered?]

**Consequences**:
- ✅ [Positive consequence]
- ⚠️ [Trade-off or limitation]

**Related Documentation**: [Links to relevant docs]
```

## Superseded Decisions

### ADR-XXX: [Old Decision Title]

**Status**: Superseded by ADR-YYY  
**Date**: [Original Date]

[Brief description of what changed and why]

---

## Reviewing and Updating ADRs

- ADRs are immutable records once created
- To change a decision, create a new ADR that supersedes the old one
- Update the status of the old ADR to "Superseded by ADR-XXX"
- Keep old ADRs for historical context

## Related Documentation

- [System Architecture](./system-architecture.md) - Overall architecture
- [Layered Architecture](./layered-architecture.md) - Layer details
- [Design Patterns](../03-development-standards/design-patterns.md) - Implementation patterns
