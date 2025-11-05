# Best Practices Workshop

**Duration:** 3-4 hours (or split into multiple sessions)  
**Format:** Interactive workshop with hands-on exercises  
**Audience:** New and intermediate contributors

## 🎯 Workshop Objectives

By the end of this workshop, participants will:
- Understand and apply UniGetUI coding standards
- Write clean, maintainable code
- Follow established design patterns
- Conduct effective code reviews
- Contribute high-quality pull requests

## 📋 Workshop Agenda

### Part 1: Coding Standards (45 minutes)
1. Naming conventions
2. Code organization
3. Documentation practices
4. Common pitfalls

### Part 2: Design Patterns (60 minutes)
1. Patterns used in UniGetUI
2. When to use each pattern
3. Hands-on: Refactoring exercise

### Part 3: Testing Best Practices (45 minutes)
1. Writing effective unit tests
2. Test-driven development basics
3. Hands-on: Write tests for a feature

### Part 4: Code Review Skills (45 minutes)
1. What to look for in code reviews
2. Giving constructive feedback
3. Hands-on: Review sample PRs

### Part 5: Contributing Workflow (30 minutes)
1. Creating quality pull requests
2. Responding to feedback
3. Best practices for collaboration

---

## Part 1: Coding Standards

### 1.1 Naming Conventions

#### The Rules

**Variables and Parameters:** camelCase
```csharp
// ✅ Correct
int packageCount = 10;
string userName = "developer";
bool isInstalled = false;

// ❌ Incorrect
int PackageCount = 10;      // Should be camelCase
string user_name = "dev";   // No underscores (not Python!)
bool bIsInstalled = false;  // No Hungarian notation
```

**Methods and Properties:** PascalCase
```csharp
// ✅ Correct
public void InstallPackage() { }
public string PackageName { get; set; }

// ❌ Incorrect
public void installPackage() { }
public string packageName { get; set; }
```

**Private Fields:** _camelCase (with underscore prefix)
```csharp
// ✅ Correct
private readonly ILogger _logger;
private string _cachePath;

// ❌ Incorrect
private readonly ILogger logger;   // Missing underscore
private string m_cachePath;         // No Hungarian notation
```

**Constants:** PascalCase (not SCREAMING_CASE in C#)
```csharp
// ✅ Correct
private const int MaxRetryAttempts = 3;
public const string DefaultLocale = "en";

// ❌ Incorrect
private const int MAX_RETRY_ATTEMPTS = 3;  // Not C# style
```

**Classes and Interfaces:** PascalCase
```csharp
// ✅ Correct
public class PackageManager { }
public interface IPackageManager { }

// ❌ Incorrect
public class packageManager { }
public interface PackageManagerInterface { }  // Use 'I' prefix
```

#### Exercise 1.1: Fix the Naming (10 minutes)

Fix the naming issues in this code:

```csharp
public class package_installer
{
    private string CACHE_PATH;
    private readonly ILogger logger;
    
    public void install_package(string package_name)
    {
        int MAX_RETRIES = 3;
        bool bSuccess = false;
        
        // Implementation...
    }
}
```

<details>
<summary>Solution</summary>

```csharp
public class PackageInstaller
{
    private string _cachePath;
    private readonly ILogger _logger;
    
    public void InstallPackage(string packageName)
    {
        const int MaxRetries = 3;
        bool success = false;
        
        // Implementation...
    }
}
```
</details>

### 1.2 Code Organization

#### File Structure

Every C# file should follow this order:

```csharp
// 1. Using directives (sorted, System first)
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniGetUI.Core.Logging;
using UniGetUI.PackageEngine.Classes;

// 2. Namespace (file-scoped preferred in C# 10+)
namespace UniGetUI.PackageEngine.Managers.WinGet;

// 3. Class/Interface definition
public class WinGet : PackageManager
{
    // 4. Constants
    private const int DefaultTimeout = 300;
    
    // 5. Private fields
    private readonly ILogger _logger;
    private string _cachePath;
    
    // 6. Public properties
    public string Name { get; set; }
    public ManagerStatus Status { get; private set; }
    
    // 7. Constructors
    public WinGet()
    {
        _logger = Logger.GetLogger();
    }
    
    // 8. Public methods
    public async Task InstallPackage(Package package)
    {
        // Implementation
    }
    
    // 9. Private/Protected methods
    private async Task<string> ExecuteCommand(string command)
    {
        // Implementation
    }
}
```

#### Exercise 1.2: Reorganize Code (10 minutes)

Reorganize this poorly structured class:

```csharp
public class BadlyOrganized
{
    public void PublicMethod() { }
    private string _field;
    public BadlyOrganized() { }
    private void PrivateMethod() { }
    public string Property { get; set; }
    private const int CONSTANT = 10;
}
```

### 1.3 Comments and Documentation

#### XML Documentation

Always document public APIs:

```csharp
/// <summary>
/// Installs the specified package using the configured package manager.
/// </summary>
/// <param name="package">The package to install.</param>
/// <param name="options">Installation options such as version and location.</param>
/// <returns>True if installation succeeded, false otherwise.</returns>
/// <exception cref="ArgumentNullException">Thrown when package is null.</exception>
public async Task<bool> InstallPackage(Package package, InstallOptions options)
{
    // Implementation
}
```

#### Inline Comments

Comment the "why," not the "what":

```csharp
// ✅ Good: Explains reasoning
// Skip validation for local packages as they're already trusted
if (!package.IsLocal)
{
    ValidatePackage(package);
}

// ❌ Bad: States the obvious
// Check if package is not local
if (!package.IsLocal)
{
    // Validate the package
    ValidatePackage(package);
}
```

#### Exercise 1.3: Add Documentation (15 minutes)

Add XML documentation to this method:

```csharp
public async Task<List<Package>> SearchPackages(string query, int maxResults)
{
    // Implementation
}
```

---

## Part 2: Design Patterns

### 2.1 Factory Pattern

**When to use:** Creating objects with complex initialization

**Example in UniGetUI:**
```csharp
public class SourceFactory
{
    private readonly Dictionary<string, IManagerSource> _cache = new();
    
    public IManagerSource GetSourceOrDefault(string name)
    {
        if (_cache.TryGetValue(name, out var source))
            return source;
        
        var newSource = new ManagerSource(name, GetDefaultUrl(name));
        _cache[name] = newSource;
        return newSource;
    }
}
```

**Benefits:**
- Centralized object creation
- Caching and reuse
- Simplified client code

### 2.2 Strategy Pattern

**When to use:** Multiple algorithms/implementations for the same operation

**Example in UniGetUI:**
```csharp
public interface IPackageManager
{
    Task<List<Package>> FindPackages(string query);
    Task<bool> InstallPackage(Package package);
}

// Different implementations
public class WinGetManager : IPackageManager { }
public class ScoopManager : IPackageManager { }
public class ChocolateyManager : IPackageManager { }
```

**Benefits:**
- Swap implementations at runtime
- Easy to add new strategies
- Follows Open/Closed Principle

### 2.3 Observer Pattern

**When to use:** Notify multiple subscribers of state changes

**Example in UniGetUI:**
```csharp
public class ObservableQueue<T> : Queue<T>
{
    public event EventHandler<ItemEventArgs>? ItemEnqueued;
    
    public new void Enqueue(T item)
    {
        base.Enqueue(item);
        ItemEnqueued?.Invoke(this, new ItemEventArgs(item));
    }
}
```

**Benefits:**
- Loose coupling between objects
- Multiple subscribers
- Follows .NET event patterns

### 2.4 Helper Pattern (Composition)

**When to use:** Separate concerns and maintain single responsibility

**Example in UniGetUI:**
```csharp
public class PackageManager
{
    public IPackageDetailsHelper DetailsHelper { get; }
    public IPackageOperationHelper OperationHelper { get; }
    public IMultiSourceHelper SourcesHelper { get; }
    
    public PackageManager()
    {
        DetailsHelper = new DetailsHelper(this);
        OperationHelper = new OperationHelper(this);
        SourcesHelper = new SourceHelper(this);
    }
}
```

**Benefits:**
- Single Responsibility Principle
- Easier testing
- Better organization

#### Exercise 2.1: Identify Patterns (20 minutes)

Review these code snippets and identify the pattern:

1. **Code A:**
```csharp
public class TaskRecycler<T>
{
    private static readonly Dictionary<int, Task<T>> _tasks = new();
    
    public static Task<T> RunOrAttach(Func<T> method)
    {
        int hash = method.GetHashCode();
        if (_tasks.TryGetValue(hash, out var task))
            return task;
        
        var newTask = Task.Run(method);
        _tasks[hash] = newTask;
        return newTask;
    }
}
```

2. **Code B:**
```csharp
public interface ISerializer
{
    string Serialize(object obj);
}

public class JsonSerializer : ISerializer { }
public class XmlSerializer : ISerializer { }
public class YamlSerializer : ISerializer { }
```

#### Exercise 2.2: Refactor Using Patterns (40 minutes)

Refactor this monolithic class using appropriate patterns:

```csharp
public class PackageManager
{
    public List<Package> GetInstalledPackages()
    {
        // 50 lines of code
    }
    
    public PackageDetails GetPackageDetails(string id)
    {
        // 80 lines of code
    }
    
    public bool InstallPackage(Package package)
    {
        // 100 lines of code
    }
    
    public bool UninstallPackage(Package package)
    {
        // 60 lines of code
    }
    
    public List<Source> GetSources()
    {
        // 40 lines of code
    }
    
    public void AddSource(Source source)
    {
        // 30 lines of code
    }
}
```

**Hint:** Use the Helper pattern to separate concerns.

---

## Part 3: Testing Best Practices

### 3.1 The AAA Pattern

**Arrange-Act-Assert:**

```csharp
[Fact]
public async Task TestInstallPackage_SuccessfulInstallation()
{
    // Arrange: Set up test data and mocks
    var package = new Package { Name = "TestPkg", Version = "1.0" };
    var manager = new PackageManager();
    
    // Act: Execute the method being tested
    bool result = await manager.InstallPackage(package);
    
    // Assert: Verify the outcome
    Assert.True(result);
}
```

### 3.2 Test Naming

Be descriptive:

```csharp
// ✅ Good: Describes what's being tested
[Fact]
public void TestGetPackageDetails_WhenPackageExists_ReturnsDetails() { }

// ❌ Bad: Too vague
[Fact]
public void Test1() { }
```

### 3.3 Test One Thing

Each test should verify one behavior:

```csharp
// ✅ Good: Tests one specific behavior
[Fact]
public void TestValidatePackage_WhenNameIsEmpty_ThrowsException()
{
    var package = new Package { Name = "" };
    Assert.Throws<ArgumentException>(() => Validate(package));
}

// ❌ Bad: Tests multiple things
[Fact]
public void TestPackage()
{
    // Tests name validation
    // Tests version validation
    // Tests installation
    // Tests uninstallation
}
```

### 3.4 Use Meaningful Assertions

```csharp
// ✅ Good: Specific assertion
Assert.Equal(expectedPackageCount, actualPackageCount);

// ❌ Bad: Too broad
Assert.True(actualPackageCount > 0);  // What's the expected count?
```

#### Exercise 3.1: Write Tests (30 minutes)

Write unit tests for this method:

```csharp
public class PackageValidator
{
    public bool IsValid(Package package)
    {
        if (package == null)
            return false;
        
        if (string.IsNullOrWhiteSpace(package.Name))
            return false;
        
        if (string.IsNullOrWhiteSpace(package.Version))
            return false;
        
        return true;
    }
}
```

Write tests for:
1. Null package
2. Empty name
3. Empty version
4. Valid package

#### Exercise 3.2: Find Bugs Through Testing (15 minutes)

This method has bugs. Write tests to find them:

```csharp
public int CalculateDiscount(int price, int discountPercent)
{
    return price - (price * discountPercent / 100);
}
```

---

## Part 4: Code Review Skills

### 4.1 What to Look For

#### ✅ Checklist for Reviewers

**Code Quality:**
- [ ] Follows naming conventions
- [ ] Proper code organization
- [ ] No code duplication
- [ ] Appropriate use of design patterns

**Functionality:**
- [ ] Does what it's supposed to do
- [ ] Handles edge cases
- [ ] Proper error handling
- [ ] No obvious bugs

**Testing:**
- [ ] Has unit tests
- [ ] Tests cover edge cases
- [ ] Tests are meaningful

**Documentation:**
- [ ] Public APIs are documented
- [ ] Complex logic has comments
- [ ] README updated if needed

**Performance:**
- [ ] No obvious performance issues
- [ ] Efficient algorithms
- [ ] Proper async/await usage

### 4.2 Giving Constructive Feedback

#### ✅ Good Feedback

```
"Consider extracting this logic into a separate method for better 
reusability. For example:

    private bool ValidatePackage(Package package)
    {
        // validation logic
    }

This would also make the code easier to test."
```

**Why good:**
- Specific suggestion
- Shows example
- Explains benefit

#### ❌ Bad Feedback

```
"This code is bad."
```

**Why bad:**
- Not specific
- Not constructive
- No suggestion for improvement

### 4.3 Receiving Feedback

#### Do:
- ✅ Thank the reviewer
- ✅ Ask clarifying questions
- ✅ Discuss alternative approaches
- ✅ Make requested changes or explain why not

#### Don't:
- ❌ Take it personally
- ❌ Argue defensively
- ❌ Ignore feedback
- ❌ Make changes without understanding why

#### Exercise 4.1: Review Sample PRs (30 minutes)

Review these code samples and provide constructive feedback:

**Sample 1:**
```csharp
public class PackageManager
{
    public void Install(string p)
    {
        var cmd = "winget install " + p;
        System.Diagnostics.Process.Start("cmd", "/c " + cmd);
    }
}
```

**Sample 2:**
```csharp
public async Task<List<Package>> SearchPackages(string query)
{
    var packages = new List<Package>();
    var result = await ExecuteCommand($"search {query}");
    foreach (var line in result.Split('\n'))
    {
        if (line.StartsWith("Package:"))
        {
            packages.Add(ParsePackageLine(line));
        }
    }
    return packages;
}
```

#### Exercise 4.2: Respond to Feedback (15 minutes)

You receive this feedback on your PR:

> "The method name `DoStuff()` is not descriptive. Consider renaming 
> it to something more specific like `ProcessPackageQueue()`."

Write an appropriate response.

---

## Part 5: Contributing Workflow

### 5.1 Creating Quality Pull Requests

#### PR Title

```
✅ Good: Add support for Cargo package manager
❌ Bad: Update code
```

#### PR Description Template

```markdown
## Description
Brief summary of changes and motivation.

## Changes Made
- Added Cargo package manager implementation
- Updated documentation
- Added unit tests

## Testing
- [ ] Built successfully
- [ ] All tests pass
- [ ] Manually tested installation flow

## Related Issues
Fixes #123
Related to #456

## Screenshots (if applicable)
[Screenshot of new UI]
```

### 5.2 Commit Messages

```
✅ Good:
Add Cargo package manager support

Implemented IPackageManager interface for Rust's Cargo,
enabling installation and management of Rust crates.

Fixes #456

❌ Bad:
Updated files
```

### 5.3 Responding to Review Feedback

#### Example Flow

**Reviewer:**
> "Consider adding input validation for the package name."

**Your Response:**
> "Good catch! I've added validation in commit abc123. The method now 
> throws ArgumentException for null or empty package names, with a 
> corresponding unit test."

### 5.4 When to Split PRs

**Split into multiple PRs when:**
- Changes touch different features
- Can be reviewed/merged independently
- PR is too large (>500 lines)

**Example:** Instead of one PR:
- "Add Cargo support, update docs, refactor settings"

Split into:
1. "Add Cargo package manager support"
2. "Update documentation for new package managers"
3. "Refactor settings page layout"

#### Exercise 5.1: Write a PR Description (15 minutes)

You've added a new feature to display package installation history.

Write a complete PR description including:
- Description
- Changes made
- Testing done
- Related issues (make up an issue number)

#### Exercise 5.2: Review Your Own Code (15 minutes)

Before submitting a PR, review your own code:

1. Does it follow coding standards?
2. Is it well-tested?
3. Is it documented?
4. Are there any improvements you can make?

Practice this with your last commit/change.

---

## 🎓 Workshop Wrap-Up

### Key Takeaways

1. **Follow conventions** - Consistency makes code maintainable
2. **Use patterns wisely** - Patterns solve specific problems
3. **Test thoroughly** - Tests are documentation and safety net
4. **Review constructively** - We're all learning together
5. **Collaborate effectively** - Communication is key

### Action Items

After this workshop:
- [ ] Apply these practices to your next contribution
- [ ] Review an existing PR using new skills
- [ ] Refactor one piece of code you've written
- [ ] Help another contributor with code review
- [ ] Share what you learned

### Additional Resources

- [Coding Standards](../../codebase-analysis/07-best-practices/patterns-standards.md)
- [Adding Features Guide](../../codebase-analysis/08-extension/adding-features.md)
- [Contributing Guide](../../../CONTRIBUTING.md)

### Feedback

Help us improve this workshop! What would you add or change?

---

## 📚 Homework Assignments

### Assignment 1: Refactor Code (1-2 hours)
Find a class in the codebase that could be improved and refactor it following best practices.

### Assignment 2: Add Tests (1 hour)
Find untested code and write comprehensive unit tests.

### Assignment 3: Review PRs (30 minutes)
Review 2-3 open PRs and provide constructive feedback.

### Assignment 4: Document Code (30 minutes)
Add XML documentation to public APIs that lack it.

---

**Workshop Complete!** 🎉

You're now equipped with the skills to contribute high-quality code to UniGetUI!

**Questions?** Ask in [GitHub Discussions](https://github.com/marticliment/UniGetUI/discussions)
