# Database Migration Strategy and Versioning

## Overview

This guide covers database migration strategies, versioning approaches, and deployment best practices for Windows applications using SQL Server and Entity Framework Core.

## Table of Contents

1. [Migration Fundamentals](#migration-fundamentals)
2. [Entity Framework Core Migrations](#entity-framework-core-migrations)
3. [Version Control Strategies](#version-control-strategies)
4. [Deployment Approaches](#deployment-approaches)
5. [Rollback Strategies](#rollback-strategies)
6. [Zero-Downtime Migrations](#zero-downtime-migrations)
7. [Data Migration](#data-migration)
8. [Best Practices](#best-practices)

## Migration Fundamentals

### What is a Database Migration?

A database migration is a controlled, versioned change to the database schema or data. Migrations allow you to:

- Track schema changes over time
- Apply changes consistently across environments
- Rollback changes if needed
- Collaborate with team members on schema changes

### Migration Types

#### 1. Schema Migrations

Changes to database structure:
- Creating/dropping tables
- Adding/removing columns
- Modifying column types
- Creating/dropping indexes
- Adding/removing constraints

#### 2. Data Migrations

Changes to database data:
- Seed data insertion
- Data transformation
- Data cleanup
- Default value population

#### 3. Reversible Migrations

Migrations that can be rolled back (Up and Down methods)

#### 4. Irreversible Migrations

One-way migrations (data deletion, destructive changes)

## Entity Framework Core Migrations

### Setup

```bash
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Or update existing tools
dotnet tool update --global dotnet-ef

# Verify installation
dotnet ef --version
```

### Creating Migrations

#### Add Migration Command

```bash
# Create a new migration
dotnet ef migrations add InitialCreate

# With specific DbContext (if multiple contexts exist)
dotnet ef migrations add InitialCreate --context ApplicationDbContext

# With specific startup project
dotnet ef migrations add InitialCreate --project MyApp.Data --startup-project MyApp.Web

# With specific output directory
dotnet ef migrations add InitialCreate --output-dir Data/Migrations
```

#### Migration File Structure

```csharp
// 20240105120000_InitialCreate.cs
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Forward migration - apply changes
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                UserId = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Username = table.Column<string>(maxLength: 50, nullable: false),
                Email = table.Column<string>(maxLength: 255, nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.UserId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Users_Username",
            table: "Users",
            column: "Username",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Reverse migration - undo changes
        migrationBuilder.DropTable(name: "Users");
    }
}

// 20240105120000_InitialCreate.Designer.cs
[DbContext(typeof(ApplicationDbContext))]
[Migration("20240105120000_InitialCreate")]
partial class InitialCreate
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        // Snapshot of model at this migration point
    }
}
```

### Applying Migrations

#### Update Database Command

```bash
# Apply all pending migrations
dotnet ef database update

# Apply up to a specific migration
dotnet ef database update AddUserEmail

# Rollback to a specific migration
dotnet ef database update PreviousMigration

# Rollback all migrations (empty database)
dotnet ef database update 0

# Apply to a specific connection string
dotnet ef database update --connection "Server=...;Database=..."
```

#### Apply Migrations in Code

```csharp
// Startup.cs or Program.cs
public static async Task Main(string[] args)
{
    var host = CreateHostBuilder(args).Build();
    
    // Apply migrations on application startup
    using (var scope = host.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            // This will apply pending migrations
            await context.Database.MigrateAsync();
            
            // Optionally seed data
            await SeedDataAsync(context);
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }
    
    await host.RunAsync();
}

// Check if migrations are pending
public async Task<bool> HasPendingMigrationsAsync(ApplicationDbContext context)
{
    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
    return pendingMigrations.Any();
}

// Get applied migrations
public async Task<IEnumerable<string>> GetAppliedMigrationsAsync(ApplicationDbContext context)
{
    return await context.Database.GetAppliedMigrationsAsync();
}
```

### Generating SQL Scripts

```bash
# Generate SQL for all migrations
dotnet ef migrations script

# Generate SQL for specific range
dotnet ef migrations script FromMigration ToMigration

# Generate SQL for pending migrations
dotnet ef migrations script --idempotent

# Output to file
dotnet ef migrations script --output migration.sql

# Generate SQL from specific migration to latest
dotnet ef migrations script AddUserEmail

# Idempotent scripts (safe to run multiple times)
dotnet ef migrations script --idempotent --output migration.sql
```

### Removing Migrations

```bash
# Remove the last migration (if not applied to database)
dotnet ef migrations remove

# Force remove (if already applied)
dotnet ef migrations remove --force
```

### Custom Migration Code

```csharp
public partial class AddCustomIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Execute raw SQL
        migrationBuilder.Sql(@"
            CREATE NONCLUSTERED INDEX IX_Users_Email_CreatedAt
            ON Users(Email, CreatedAt)
            INCLUDE (Username)
            WHERE IsDeleted = 0");

        // Create stored procedure
        migrationBuilder.Sql(@"
            CREATE PROCEDURE sp_GetActiveUsers
            AS
            BEGIN
                SELECT * FROM Users WHERE IsDeleted = 0
            END");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IX_Users_Email_CreatedAt ON Users");
        migrationBuilder.Sql("DROP PROCEDURE sp_GetActiveUsers");
    }
}

// Data migration example
public partial class SeedDefaultRoles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "Roles",
            columns: new[] { "RoleId", "RoleName" },
            values: new object[,]
            {
                { 1, "Admin" },
                { 2, "User" },
                { 3, "Guest" }
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "Roles",
            keyColumn: "RoleId",
            keyValues: new object[] { 1, 2, 3 });
    }
}

// Complex data transformation
public partial class MigrateUserData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add new column
        migrationBuilder.AddColumn<string>(
            name: "FullName",
            table: "Users",
            maxLength: 200,
            nullable: true);

        // Migrate data
        migrationBuilder.Sql(@"
            UPDATE Users 
            SET FullName = CONCAT(FirstName, ' ', LastName)
            WHERE FirstName IS NOT NULL AND LastName IS NOT NULL");

        // Make column required
        migrationBuilder.AlterColumn<string>(
            name: "FullName",
            table: "Users",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldMaxLength: 200,
            oldNullable: true);

        // Drop old columns
        migrationBuilder.DropColumn(name: "FirstName", table: "Users");
        migrationBuilder.DropColumn(name: "LastName", table: "Users");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Restore old columns
        migrationBuilder.AddColumn<string>(
            name: "FirstName",
            table: "Users",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "LastName",
            table: "Users",
            maxLength: 50,
            nullable: true);

        // Migrate data back (best effort)
        migrationBuilder.Sql(@"
            UPDATE Users 
            SET FirstName = SUBSTRING(FullName, 1, CHARINDEX(' ', FullName + ' ') - 1),
                LastName = SUBSTRING(FullName, CHARINDEX(' ', FullName + ' ') + 1, LEN(FullName))
            WHERE FullName IS NOT NULL");

        // Drop new column
        migrationBuilder.DropColumn(name: "FullName", table: "Users");
    }
}
```

## Version Control Strategies

### Migration Naming Convention

```bash
# Use descriptive names
dotnet ef migrations add AddUserEmailColumn
dotnet ef migrations add CreateOrdersTable
dotnet ef migrations add AddIndexOnUserEmail
dotnet ef migrations add SeedDefaultRoles

# Avoid vague names
dotnet ef migrations add Update1  # Bad
dotnet ef migrations add Fix      # Bad
dotnet ef migrations add New      # Bad
```

### Versioning Approaches

#### 1. Sequential Versioning

```plaintext
Migrations/
├── 20240101120000_InitialCreate.cs
├── 20240102130000_AddUserEmail.cs
├── 20240103140000_CreateOrdersTable.cs
└── 20240104150000_AddOrderIndex.cs
```

**Pros:**
- Clear chronological order
- Built into EF Core
- Easy to track history

**Cons:**
- Timestamp conflicts in team environments

#### 2. Semantic Versioning

```csharp
// Track version in database
CREATE TABLE SchemaVersion (
    Version VARCHAR(20) PRIMARY KEY,
    Description NVARCHAR(500),
    AppliedDate DATETIME2 DEFAULT GETUTCDATE()
);

// Example versions
v1.0.0 - Initial schema
v1.1.0 - Added email column
v2.0.0 - Breaking change - restructured orders
v2.1.0 - Added indexes
```

#### 3. Feature Branch Migrations

```plaintext
# Main branch migrations
Migrations/
├── 20240101120000_InitialCreate.cs
├── 20240102130000_AddUserEmail.cs

# Feature branch creates new migration
feature/orders
├── 20240105160000_CreateOrdersTable.cs  # Timestamp after main branch
```

**Handling merge conflicts:**
1. Rebase feature branch on main
2. Regenerate migration with new timestamp
3. Test migration on clean database

### Git Workflow

```bash
# Always commit migrations with code changes
git add Migrations/
git add Models/
git commit -m "Add User email column"

# Tag releases with schema versions
git tag v1.2.0 -m "Added user email and order indexes"

# Migration checklist for pull requests
- [ ] Migration file reviewed
- [ ] Up and Down methods tested
- [ ] Database updated successfully in dev environment
- [ ] SQL script generated and reviewed
- [ ] Data migration tested with production-like data
```

### Schema Version Table

```csharp
// Custom version tracking (alternative to EF Core's __EFMigrationsHistory)
public class DatabaseVersionService
{
    private readonly ApplicationDbContext _context;

    public DatabaseVersionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetCurrentVersionAsync()
    {
        var version = await _context.Database
            .ExecuteSqlRawAsync("SELECT MAX(Version) FROM SchemaVersion");
        return version.ToString();
    }

    public async Task RecordVersionAsync(string version, string description)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO SchemaVersion (Version, Description, AppliedDate)
            VALUES ({version}, {description}, GETUTCDATE())");
    }

    public async Task<bool> IsVersionAppliedAsync(string version)
    {
        var exists = await _context.Database
            .ExecuteSqlInterpolatedAsync($@"
                SELECT COUNT(*) FROM SchemaVersion WHERE Version = {version}");
        return exists > 0;
    }
}
```

## Deployment Approaches

### 1. Automatic Migration on Startup

**Best for:** Development, staging, small applications

```csharp
// Program.cs
public static async Task Main(string[] args)
{
    var host = CreateHostBuilder(args).Build();
    
    using (var scope = host.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            // Decide: throw to prevent startup, or continue
            throw;
        }
    }
    
    await host.RunAsync();
}
```

**Pros:**
- Simple, automatic
- Works well in development
- No manual steps

**Cons:**
- Can cause startup delays
- Multiple instances might conflict
- Risk of failed migrations preventing startup

### 2. Separate Migration Application

**Best for:** Production, enterprise applications

```csharp
// DbMigrator/Program.cs
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddCommandLine(args)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var context = new ApplicationDbContext(optionsBuilder.Options);
        
        try
        {
            Console.WriteLine("Checking for pending migrations...");
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            
            if (!pendingMigrations.Any())
            {
                Console.WriteLine("Database is up to date.");
                return 0;
            }

            Console.WriteLine($"Found {pendingMigrations.Count()} pending migrations:");
            foreach (var migration in pendingMigrations)
            {
                Console.WriteLine($"  - {migration}");
            }

            Console.WriteLine("Applying migrations...");
            await context.Database.MigrateAsync();
            
            Console.WriteLine("Migrations applied successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying migrations: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return 1;
        }
    }
}
```

**Usage:**
```bash
# Run before deploying application
dotnet run --project DbMigrator -- --connection "Server=prod;Database=mydb;..."

# Or as part of deployment script
./DbMigrator.exe --connection "..."
if %ERRORLEVEL% NEQ 0 (
    echo Migration failed!
    exit /b 1
)
```

### 3. SQL Script Deployment

**Best for:** Highly controlled environments, regulated industries

```bash
# Generate idempotent SQL script
dotnet ef migrations script --idempotent --output migration_v1.2.0.sql

# Review script
code migration_v1.2.0.sql

# Apply manually or via deployment tool
sqlcmd -S server -d database -i migration_v1.2.0.sql
```

**Deployment script example:**

```sql
-- migration_v1.2.0.sql
-- Generated by Entity Framework Core

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240105120000_AddUserEmail')
BEGIN
    ALTER TABLE [Users] ADD [Email] nvarchar(255) NOT NULL DEFAULT N'';
    
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240105120000_AddUserEmail', N'8.0.0');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240106130000_CreateOrdersTable')
BEGIN
    CREATE TABLE [Orders] (
        [OrderId] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [OrderDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([OrderId]),
        CONSTRAINT [FK_Orders_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
    );
    
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240106130000_CreateOrdersTable', N'8.0.0');
END;
GO
```

### 4. Blue-Green Deployment

**Best for:** Zero-downtime requirements

```plaintext
1. Deploy new version to "green" environment
2. Run migrations on green database
3. Test green environment
4. Switch traffic to green
5. Keep blue as rollback option
```

## Rollback Strategies

### 1. Migration Rollback (Development)

```bash
# Rollback to previous migration
dotnet ef database update PreviousMigration

# Rollback to specific migration
dotnet ef database update 20240101120000_InitialCreate

# Rollback all migrations
dotnet ef database update 0
```

### 2. Database Backup and Restore

```sql
-- Before migration
BACKUP DATABASE MyAppDb 
TO DISK = 'C:\Backups\MyAppDb_PreMigration_v1.2.0.bak'
WITH FORMAT, INIT, COMPRESSION;

-- If migration fails
RESTORE DATABASE MyAppDb 
FROM DISK = 'C:\Backups\MyAppDb_PreMigration_v1.2.0.bak'
WITH REPLACE, RECOVERY;
```

### 3. Versioned Rollback Scripts

```csharp
// Migrations/Rollback/
// v1.2.0_to_v1.1.0.sql
BEGIN TRANSACTION;

-- Rollback AddUserEmail migration
ALTER TABLE Users DROP COLUMN Email;

-- Update version
DELETE FROM SchemaVersion WHERE Version = '1.2.0';

COMMIT TRANSACTION;
```

### 4. Point-in-Time Recovery

```sql
-- Restore to point before migration
RESTORE DATABASE MyAppDb
FROM DISK = 'C:\Backups\MyAppDb_Full.bak'
WITH NORECOVERY;

RESTORE LOG MyAppDb
FROM DISK = 'C:\Backups\MyAppDb_Log.trn'
WITH RECOVERY, STOPAT = '2024-01-05 11:55:00';
```

### Rollback Best Practices

```csharp
// 1. Always implement Down() method
public partial class AddUserEmail : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Email",
            table: "Users",
            nullable: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Email",
            table: "Users");
    }
}

// 2. For irreversible migrations, throw exception
public partial class DeleteOldData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM Users WHERE CreatedAt < '2020-01-01'");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException("This migration cannot be rolled back - data has been deleted");
    }
}

// 3. Test rollback before deployment
[Test]
public async Task TestMigrationRollback()
{
    // Apply migration
    await _context.Database.MigrateAsync();
    
    // Rollback
    await _context.Database.MigrateAsync("PreviousMigration");
    
    // Verify database state
    var columns = await GetTableColumnsAsync("Users");
    Assert.DoesNotContain("Email", columns);
}
```

## Zero-Downtime Migrations

### Expand-Contract Pattern

#### Phase 1: Expand (Add New Column)

```csharp
// Migration 1: Add new column (nullable)
public partial class AddUserEmailColumn : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add as nullable to allow existing rows
        migrationBuilder.AddColumn<string>(
            name: "Email",
            table: "Users",
            maxLength: 255,
            nullable: true);
    }
}

// Application code supports both old and new columns
public class User
{
    public string Username { get; set; }
    public string? Email { get; set; }  // New, optional
}
```

#### Phase 2: Migrate Data

```csharp
// Migration 2: Populate new column
public partial class MigrateUserEmailData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Backfill data in batches
        migrationBuilder.Sql(@"
            UPDATE Users 
            SET Email = Username + '@example.com'
            WHERE Email IS NULL");
    }
}
```

#### Phase 3: Contract (Remove Old Column)

```csharp
// Migration 3: Make column required, drop old column
public partial class MakeEmailRequired : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Make email required
        migrationBuilder.AlterColumn<string>(
            name: "Email",
            table: "Users",
            maxLength: 255,
            nullable: false,
            oldClrType: typeof(string),
            oldMaxLength: 255,
            oldNullable: true);

        // If replacing old column, drop it here
        // migrationBuilder.DropColumn(name: "OldColumn", table: "Users");
    }
}
```

### Backward-Compatible Changes

```csharp
// ✅ Safe: Add new table
migrationBuilder.CreateTable(name: "NewTable", ...);

// ✅ Safe: Add new column (nullable)
migrationBuilder.AddColumn<string>(name: "NewColumn", nullable: true);

// ✅ Safe: Add new index
migrationBuilder.CreateIndex(name: "IX_NewIndex", ...);

// ⚠️ Risky: Rename column (old code breaks)
// Instead: Add new column, migrate data, deprecate old column

// ⚠️ Risky: Change column type
// Instead: Add new column, migrate data, switch code, drop old column

// ❌ Unsafe: Drop column (old code breaks immediately)
// Instead: Deprecate, stop using in code, then drop in later migration

// ❌ Unsafe: Drop table (old code breaks immediately)
```

### Multi-Step Deployment Example

```plaintext
Version 1.0 (Current)
- User table has Username column

Step 1: Deploy v1.1 (Expand)
- Migration adds Email column (nullable)
- Code writes to both Username and Email
- Old instances (v1.0) still work

Step 2: Data Migration
- Background job populates Email for existing users
- Verify all rows have Email populated

Step 3: Deploy v1.2 (Transition)
- Code reads from Email, writes to both
- Can still rollback to v1.1

Step 4: Deploy v1.3 (Contract)
- Migration makes Email required
- Code only uses Email
- Migration drops Username column
```

## Data Migration

### Seed Data

```csharp
// In OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Role>().HasData(
        new Role { RoleId = 1, RoleName = "Admin", CreatedAt = DateTime.UtcNow },
        new Role { RoleId = 2, RoleName = "User", CreatedAt = DateTime.UtcNow },
        new Role { RoleId = 3, RoleName = "Guest", CreatedAt = DateTime.UtcNow }
    );

    // Or read from JSON
    var rolesJson = File.ReadAllText("SeedData/roles.json");
    var roles = JsonSerializer.Deserialize<List<Role>>(rolesJson);
    modelBuilder.Entity<Role>().HasData(roles);
}
```

### Large Data Migrations

```csharp
public partial class MigrateUserData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Process in batches to avoid timeout
        migrationBuilder.Sql(@"
            DECLARE @BatchSize INT = 1000;
            DECLARE @RowsAffected INT = @BatchSize;
            
            WHILE @RowsAffected = @BatchSize
            BEGIN
                UPDATE TOP (@BatchSize) Users
                SET Email = Username + '@migrated.com'
                WHERE Email IS NULL;
                
                SET @RowsAffected = @@ROWCOUNT;
                
                WAITFOR DELAY '00:00:01';  -- Throttle to avoid locking
            END");
    }
}

// Or use custom migration operation
public class DataMigrationOperation : MigrationOperation
{
    public string TableName { get; set; }
    public int BatchSize { get; set; } = 1000;
}

// Execute in application code
public async Task MigrateDataAsync()
{
    var batchSize = 1000;
    var processed = 0;

    while (true)
    {
        var batch = await _context.Users
            .Where(u => u.Email == null)
            .Take(batchSize)
            .ToListAsync();

        if (!batch.Any()) break;

        foreach (var user in batch)
        {
            user.Email = $"{user.Username}@migrated.com";
        }

        await _context.SaveChangesAsync();
        processed += batch.Count;

        Console.WriteLine($"Processed {processed} users...");
        await Task.Delay(100);  // Throttle
    }
}
```

## Best Practices

### 1. Test Migrations Thoroughly

```csharp
[TestClass]
public class MigrationTests
{
    [TestMethod]
    public async Task TestMigrationCanBeApplied()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        // Act
        using var context = new ApplicationDbContext(options);
        await context.Database.MigrateAsync();

        // Assert
        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        Assert.IsTrue(appliedMigrations.Any());
    }

    [TestMethod]
    public async Task TestMigrationCanBeRolledBack()
    {
        // Test Down() method
        using var context = new ApplicationDbContext(options);
        
        await context.Database.MigrateAsync();
        var table = await GetTableInfoAsync("Users");
        Assert.IsTrue(table.Columns.Contains("Email"));

        await context.Database.MigrateAsync("PreviousMigration");
        table = await GetTableInfoAsync("Users");
        Assert.IsFalse(table.Columns.Contains("Email"));
    }
}
```

### 2. Version Your Database

```sql
-- Always include version tracking
CREATE TABLE __SchemaVersion (
    Version VARCHAR(20) PRIMARY KEY,
    Description NVARCHAR(500),
    AppliedBy NVARCHAR(100),
    AppliedDate DATETIME2 DEFAULT GETUTCDATE()
);
```

### 3. Backup Before Migration

```powershell
# PowerShell deployment script
param(
    [string]$Server,
    [string]$Database
)

# Backup database
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFile = "C:\Backups\${Database}_PreMigration_${timestamp}.bak"

Write-Host "Creating backup: $backupFile"
Invoke-Sqlcmd -Query "BACKUP DATABASE [$Database] TO DISK = '$backupFile'" -ServerInstance $Server

# Apply migrations
Write-Host "Applying migrations..."
dotnet ef database update --connection "Server=$Server;Database=$Database;..."

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migration completed successfully"
} else {
    Write-Host "Migration failed! Restore backup with:"
    Write-Host "RESTORE DATABASE [$Database] FROM DISK = '$backupFile' WITH REPLACE"
    exit 1
}
```

### 4. Use Transactions

```csharp
// EF Core migrations are automatically wrapped in transactions
// For custom SQL, ensure proper transaction handling
public partial class CustomMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            BEGIN TRANSACTION;
            
            -- Multiple operations
            UPDATE Users SET Email = Username + '@example.com' WHERE Email IS NULL;
            ALTER TABLE Users ALTER COLUMN Email NVARCHAR(255) NOT NULL;
            
            COMMIT TRANSACTION;
        ");
    }
}
```

### 5. Document Breaking Changes

```csharp
/// <summary>
/// BREAKING CHANGE: This migration removes the Username column.
/// Ensure all applications are updated to use Email column before applying.
/// 
/// Rollback: Run 'dotnet ef database update PreviousMigration'
/// </summary>
public partial class RemoveUsernameColumn : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Drop Username column
        migrationBuilder.DropColumn(name: "Username", table: "Users");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Restore Username column
        migrationBuilder.AddColumn<string>(
            name: "Username",
            table: "Users",
            maxLength: 50,
            nullable: false,
            defaultValue: "");
    }
}
```

### 6. Avoid Data Loss

```csharp
// ✅ Good: Preserve data when renaming
public partial class RenameColumn : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Use sp_rename or create new column and copy data
        migrationBuilder.Sql("sp_rename 'Users.OldName', 'NewName', 'COLUMN'");
    }
}

// ❌ Bad: Drop and recreate (data loss!)
public partial class RenameColumnBad : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "OldName", table: "Users");
        migrationBuilder.AddColumn<string>(name: "NewName", table: "Users");
    }
}
```

### 7. Monitor Migration Performance

```csharp
public class TimedMigrationOperation
{
    public static async Task<TimeSpan> MeasureMigrationTimeAsync(ApplicationDbContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        await context.Database.MigrateAsync();
        
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }
}

// Log slow migrations
if (migrationTime > TimeSpan.FromMinutes(5))
{
    Logger.Warning($"Migration took {migrationTime.TotalMinutes:F2} minutes - consider optimization");
}
```

## Tools and Resources

### Migration Tools

- **Entity Framework Core CLI**: `dotnet ef` commands
- **Package Manager Console**: PowerShell commands in Visual Studio
- **FluentMigrator**: Alternative migration framework
- **DbUp**: Lightweight migration library
- **Roundhouse**: Database deployment system

### Deployment Tools

- **Azure DevOps**: CI/CD pipelines with migration steps
- **Octopus Deploy**: Deployment automation
- **Redgate SQL Change Automation**: Professional database deployment
- **DbUp**: Simple .NET-based migrations
- **Flyway**: Database migration tool (Java-based)

### Best Practices Resources

- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Database Reliability Engineering](https://www.oreilly.com/library/view/database-reliability-engineering/9781491925935/)
- [Evolutionary Database Design (Martin Fowler)](https://martinfowler.com/articles/evodb.html)

## Related Documents

- [Schema Design](./schema-design.md)
- [ORM Patterns](./orm-patterns.md)
- [Performance Optimization](./performance-optimization.md)
