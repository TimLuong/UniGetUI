# Code Review Checklist

This document provides a comprehensive checklist for code reviewers to ensure code quality, maintainability, and adherence to standards.

## Table of Contents

1. [General Guidelines](#general-guidelines)
2. [Code Quality](#code-quality)
3. [Architecture & Design](#architecture--design)
4. [Security](#security)
5. [Performance](#performance)
6. [Testing](#testing)
7. [Documentation](#documentation)
8. [C# Specific](#c-specific)
9. [XAML Specific](#xaml-specific)
10. [TypeScript/JavaScript Specific](#typescriptjavascript-specific)
11. [SQL Specific](#sql-specific)

## General Guidelines

### Before You Begin

- [ ] Understand the purpose of the change (review issue/ticket description)
- [ ] Check that the PR description clearly explains what and why
- [ ] Verify that the changes address the stated problem
- [ ] Ensure the scope of changes is reasonable (not too large)
- [ ] Check that tests are included and passing

### Communication

- [ ] Be constructive and respectful in comments
- [ ] Explain the "why" behind suggestions, not just "what" to change
- [ ] Distinguish between blocking issues and suggestions
- [ ] Ask questions when code intent is unclear
- [ ] Acknowledge good practices and improvements

## Code Quality

### Readability

- [ ] Code is easy to read and understand
- [ ] Variable and method names are descriptive and meaningful
- [ ] Code follows consistent naming conventions
- [ ] Complex logic has explanatory comments
- [ ] No commented-out code (unless temporarily needed with explanation)
- [ ] No obvious typos in names or comments
- [ ] Line length is reasonable (prefer < 120 characters)

### Maintainability

- [ ] Code follows DRY principle (Don't Repeat Yourself)
- [ ] Functions/methods have single, clear responsibilities
- [ ] No magic numbers or strings (use constants)
- [ ] Code is modular and well-organized
- [ ] No God classes or methods (too many responsibilities)
- [ ] Dependencies are clear and minimal
- [ ] Code can be easily modified or extended

### Standards Compliance

- [ ] Code follows project coding standards
- [ ] Formatting is consistent with `.editorconfig`
- [ ] Naming conventions match language-specific standards
- [ ] No linting errors or warnings (justified if suppressed)
- [ ] Import/using statements are organized correctly

## Architecture & Design

### Design Patterns

- [ ] Appropriate design patterns are used
- [ ] SOLID principles are followed
- [ ] Separation of concerns is maintained
- [ ] No tight coupling between components
- [ ] Interfaces are used appropriately
- [ ] Dependency injection is used where appropriate

### Code Organization

- [ ] Files are in the correct directories
- [ ] One class per file (unless nested/helper classes)
- [ ] Related functionality is grouped together
- [ ] Public API surface is minimal and well-defined
- [ ] Internal implementation details are hidden

### Reusability

- [ ] Code reuses existing functionality instead of duplicating
- [ ] New utility functions are placed in appropriate shared locations
- [ ] Generic/reusable code is not hardcoded to specific use cases

## Security

### Input Validation

- [ ] All user inputs are validated
- [ ] Input validation happens on the server-side (not just client)
- [ ] Appropriate data type validation is in place
- [ ] Length/size limits are enforced
- [ ] Special characters are handled correctly

### Authentication & Authorization

- [ ] Authentication is required where needed
- [ ] Authorization checks are in place for sensitive operations
- [ ] User permissions are verified before granting access
- [ ] Session management is secure

### Data Protection

- [ ] Sensitive data is encrypted at rest and in transit
- [ ] Passwords are hashed (never stored in plain text)
- [ ] No sensitive information in logs or error messages
- [ ] Secrets are not hardcoded (use configuration/environment variables)
- [ ] No credentials in source code or comments

### SQL Injection & XSS

- [ ] SQL queries use parameterized statements (no string concatenation)
- [ ] User input is sanitized before display
- [ ] HTML/JavaScript is properly encoded
- [ ] No dynamic SQL from untrusted input

### Dependencies

- [ ] No known vulnerabilities in dependencies
- [ ] Dependencies are from trusted sources
- [ ] Dependency versions are pinned or constrained
- [ ] License compliance is ensured

## Performance

### Efficiency

- [ ] Algorithms have appropriate time/space complexity
- [ ] No unnecessary loops or iterations
- [ ] Collections are used appropriately (List vs Array vs HashSet, etc.)
- [ ] Lazy loading is used where appropriate
- [ ] Resources are cached when beneficial

### Database

- [ ] Database queries are optimized
- [ ] Appropriate indexes are in place
- [ ] No N+1 query problems
- [ ] Large result sets use pagination
- [ ] Bulk operations are used instead of loops where possible

### Memory Management

- [ ] No obvious memory leaks
- [ ] Disposable objects are properly disposed
- [ ] Large objects are not kept in memory unnecessarily
- [ ] Event handlers are unsubscribed when needed

### Async Operations

- [ ] Async operations are used appropriately
- [ ] No blocking async calls (`Task.Result`, `Task.Wait()`)
- [ ] CancellationToken is passed through for long operations
- [ ] Parallel operations are used when beneficial

## Testing

### Test Coverage

- [ ] New code has appropriate test coverage
- [ ] Edge cases are tested
- [ ] Error scenarios are tested
- [ ] Tests are meaningful (not just for coverage numbers)
- [ ] Tests follow AAA pattern (Arrange, Act, Assert)

### Test Quality

- [ ] Tests are readable and maintainable
- [ ] Test names clearly describe what is being tested
- [ ] Tests are isolated and don't depend on each other
- [ ] No flaky tests
- [ ] Mock objects are used appropriately
- [ ] Tests run quickly

### Integration Tests

- [ ] Integration points are tested
- [ ] External dependencies are properly mocked/stubbed
- [ ] Database tests use appropriate test data
- [ ] Tests clean up after themselves

## Documentation

### Code Comments

- [ ] Complex logic has explanatory comments
- [ ] Public APIs have XML documentation comments (C#)
- [ ] TODOs are documented with context and assignee
- [ ] Comments are accurate and up-to-date
- [ ] No redundant comments (explaining obvious code)

### README & Docs

- [ ] README is updated if needed
- [ ] API changes are documented
- [ ] Breaking changes are clearly noted
- [ ] Setup/installation instructions are current
- [ ] Examples are provided for new features

### Changelog

- [ ] CHANGELOG is updated for user-facing changes
- [ ] Migration guide is provided for breaking changes
- [ ] Version numbers follow semantic versioning

## C# Specific

### Language Features

- [ ] Modern C# features are used appropriately (pattern matching, records, etc.)
- [ ] Nullable reference types are used correctly
- [ ] LINQ is used for collection operations
- [ ] `async`/`await` is used correctly (not `async void` except event handlers)
- [ ] `ConfigureAwait(false)` is used in library code

### Error Handling

- [ ] Specific exceptions are caught (not `Exception`)
- [ ] Exceptions are not swallowed silently
- [ ] `throw;` is used instead of `throw ex;` to preserve stack trace
- [ ] Custom exceptions inherit from appropriate base classes
- [ ] Try-catch blocks are used judiciously (not for flow control)

### Disposal

- [ ] IDisposable is implemented for types managing resources
- [ ] `using` statements are used for disposable objects
- [ ] Finalizers are only used when necessary
- [ ] `GC.SuppressFinalize()` is called in Dispose

### Types & Nullability

- [ ] Value types vs reference types are used appropriately
- [ ] Nullable types (`int?`, `string?`) are used correctly
- [ ] Null checks are in place where needed
- [ ] Null-coalescing operators (`??`, `??=`) are used

## XAML Specific

### Data Binding

- [ ] `x:Bind` is preferred over `Binding` for performance
- [ ] Binding modes are explicit (OneWay, TwoWay, etc.)
- [ ] `x:DataType` is specified in DataTemplates
- [ ] FallbackValue/TargetNullValue are used for null scenarios
- [ ] No business logic in code-behind (use MVVM)

### Naming & Organization

- [ ] Named elements use descriptive PascalCase names
- [ ] Resources have meaningful names
- [ ] XAML is properly indented and formatted
- [ ] Attributes are ordered consistently
- [ ] Self-closing tags are used where appropriate

### Performance

- [ ] `x:Load` is used instead of `Visibility` for conditional UI
- [ ] Virtualization is used for large lists
- [ ] Images have appropriate DecodePixelWidth/Height
- [ ] Visual tree depth is minimized
- [ ] Animations are efficient

### Accessibility

- [ ] AutomationProperties are set for interactive elements
- [ ] Keyboard navigation works correctly
- [ ] Screen reader support is in place
- [ ] Color contrast meets accessibility standards

## TypeScript/JavaScript Specific

### Type Safety

- [ ] TypeScript is used for all new code
- [ ] `any` type is avoided (use `unknown` or specific types)
- [ ] Interfaces/types are defined for data structures
- [ ] Return types are specified for functions
- [ ] Null/undefined are handled appropriately

### Modern JavaScript

- [ ] `const` and `let` are used (never `var`)
- [ ] Arrow functions are used appropriately
- [ ] Destructuring is used for objects and arrays
- [ ] Template literals are used for string interpolation
- [ ] Async/await is used instead of promise chains

### Functions

- [ ] Functions are small and focused
- [ ] Pure functions are preferred where possible
- [ ] Side effects are minimized and documented
- [ ] Higher-order functions are used appropriately

### Error Handling

- [ ] Errors are caught and handled appropriately
- [ ] Promises have `.catch()` or try/catch in async functions
- [ ] Custom error classes are used for domain errors
- [ ] Error messages are helpful for debugging

## SQL Specific

### Query Quality

- [ ] Explicit column lists (no `SELECT *`)
- [ ] WHERE clauses use indexed columns
- [ ] JOINs are explicit (no implicit joins)
- [ ] CTEs are used for complex queries
- [ ] Queries are formatted and readable

### Performance

- [ ] Appropriate indexes exist for queries
- [ ] No N+1 query problems
- [ ] Set-based operations are used (no cursors)
- [ ] Result sets are limited appropriately (TOP, OFFSET/FETCH)
- [ ] Functions on indexed columns are avoided in WHERE clauses

### Security

- [ ] Parameterized queries are used (no string concatenation)
- [ ] Least privilege principle is followed
- [ ] No SQL injection vulnerabilities
- [ ] Sensitive data is properly protected

### Transactions

- [ ] Transactions are used for multi-statement operations
- [ ] Error handling is in place (TRY/CATCH)
- [ ] Transactions are as short as possible
- [ ] Appropriate isolation levels are used

## Final Checks

### Before Approval

- [ ] All CI/CD checks are passing
- [ ] Code builds successfully
- [ ] All tests pass
- [ ] No merge conflicts
- [ ] Breaking changes are approved by team
- [ ] Security scan passes (if applicable)

### Quality Gate

- [ ] Code meets minimum quality standards
- [ ] No critical or high-severity issues
- [ ] Technical debt is minimized
- [ ] Code is production-ready
- [ ] Performance is acceptable

## Review Outcome

After completing the review, choose one:

- **✅ Approve**: Code meets all standards, ready to merge
- **💬 Comment**: Minor suggestions, approval once addressed
- **🔄 Request Changes**: Issues that must be fixed before merge
- **❌ Reject**: Code does not meet standards, major rework needed

## Tips for Effective Reviews

1. **Start with the big picture**: Review architecture before diving into details
2. **Use the 20-minute rule**: Take breaks every 20 minutes to maintain focus
3. **Review < 400 lines at a time**: Large PRs should be split up
4. **Test the code**: Don't just read it, run it locally when possible
5. **Be thorough but not pedantic**: Focus on important issues
6. **Suggest alternatives**: If you criticize, offer solutions
7. **Learn from reviews**: Both reviewers and authors learn during reviews
8. **Automate what you can**: Use linters, formatters, and static analysis tools

## Additional Resources

- [C# Coding Standards](./coding-standards/csharp-standards.md)
- [XAML Best Practices](./coding-standards/xaml-standards.md)
- [TypeScript Standards](./coding-standards/typescript-standards.md)
- [SQL Standards](./coding-standards/sql-standards.md)
- [EditorConfig File](../.editorconfig)
