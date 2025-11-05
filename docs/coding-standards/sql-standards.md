# SQL Coding Standards and Query Optimization Guidelines

This document outlines SQL coding standards and best practices for database interactions in the UniGetUI project.

## Table of Contents

1. [General Principles](#general-principles)
2. [Naming Conventions](#naming-conventions)
3. [Formatting Standards](#formatting-standards)
4. [Query Writing](#query-writing)
5. [Stored Procedures and Functions](#stored-procedures-and-functions)
6. [Performance Optimization](#performance-optimization)
7. [Indexing Strategy](#indexing-strategy)
8. [Transaction Management](#transaction-management)
9. [Security Best Practices](#security-best-practices)
10. [Anti-Patterns to Avoid](#anti-patterns-to-avoid)
11. [Common Pitfalls](#common-pitfalls)

## General Principles

### Code Clarity

- Write self-documenting SQL
- Use meaningful names for tables, columns, and procedures
- Add comments for complex logic
- Break complex queries into CTEs or temp tables

### Performance First

- Always consider query performance
- Use appropriate indexes
- Avoid unnecessary data retrieval
- Test queries with realistic data volumes

### Consistency

- Follow consistent naming conventions
- Use consistent formatting
- Apply the same patterns across the codebase

## Naming Conventions

### Tables

```sql
-- ✅ Good - PascalCase, singular noun
CREATE TABLE Package (
    PackageId INT PRIMARY KEY,
    PackageName NVARCHAR(100),
    Version NVARCHAR(50)
);

CREATE TABLE User (
    UserId INT PRIMARY KEY,
    Username NVARCHAR(50),
    Email NVARCHAR(100)
);

-- ❌ Bad
CREATE TABLE packages ( -- Should be singular and PascalCase
    package_id INT PRIMARY KEY, -- Should be PascalCase
    name NVARCHAR(100) -- Ambiguous name
);
```

### Columns

```sql
-- ✅ Good - PascalCase, descriptive names
CREATE TABLE Package (
    PackageId INT PRIMARY KEY,
    PackageName NVARCHAR(100) NOT NULL,
    Version NVARCHAR(50) NOT NULL,
    Description NVARCHAR(MAX),
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastModifiedDate DATETIME2,
    IsActive BIT NOT NULL DEFAULT 1
);

-- ❌ Bad
CREATE TABLE Package (
    id INT PRIMARY KEY, -- Missing table prefix
    name NVARCHAR(100), -- Ambiguous
    ver NVARCHAR(50), -- Abbreviated
    desc NVARCHAR(MAX), -- Reserved keyword
    created DATETIME, -- Wrong data type for UTC
    flag BIT -- Unclear meaning
);
```

### Primary Keys

```sql
-- ✅ Good - TableName + "Id"
PackageId
UserId
InstallationId

-- ❌ Bad
Id -- Too generic
PK_Package -- Includes prefix
package_id -- Wrong case
```

### Foreign Keys

```sql
-- ✅ Good - References table name with Id suffix
CREATE TABLE Installation (
    InstallationId INT PRIMARY KEY,
    PackageId INT NOT NULL, -- References Package.PackageId
    UserId INT NOT NULL, -- References User.UserId
    CONSTRAINT FK_Installation_Package FOREIGN KEY (PackageId) 
        REFERENCES Package(PackageId),
    CONSTRAINT FK_Installation_User FOREIGN KEY (UserId) 
        REFERENCES User(UserId)
);

-- ❌ Bad
CREATE TABLE Installation (
    Id INT PRIMARY KEY,
    Package INT, -- Unclear
    User_Id INT -- Inconsistent case
);
```

### Indexes

```sql
-- ✅ Good - IX_TableName_ColumnName(s)
CREATE INDEX IX_Package_Name ON Package(PackageName);
CREATE INDEX IX_Package_Version_CreatedDate ON Package(Version, CreatedDate);

-- ✅ Good - UIX for unique indexes
CREATE UNIQUE INDEX UIX_Package_Name_Version ON Package(PackageName, Version);

-- ❌ Bad
CREATE INDEX idx1 ON Package(PackageName); -- Non-descriptive
CREATE INDEX PackageNameIndex ON Package(PackageName); -- Inconsistent format
```

### Stored Procedures

```sql
-- ✅ Good - Verb + TableName + Description
CREATE PROCEDURE GetPackageById
    @PackageId INT
AS
BEGIN
    -- Implementation
END;

CREATE PROCEDURE InsertPackage
    @PackageName NVARCHAR(100),
    @Version NVARCHAR(50)
AS
BEGIN
    -- Implementation
END;

CREATE PROCEDURE UpdatePackageStatus
    @PackageId INT,
    @IsActive BIT
AS
BEGIN
    -- Implementation
END;

-- ❌ Bad
CREATE PROCEDURE sp_Package -- sp_ prefix reserved for system procedures
CREATE PROCEDURE GetPkg -- Abbreviated
CREATE PROCEDURE Proc1 -- Non-descriptive
```

### Views

```sql
-- ✅ Good - Descriptive names without "v" or "vw" prefix
CREATE VIEW ActivePackages AS
SELECT PackageId, PackageName, Version
FROM Package
WHERE IsActive = 1;

-- ❌ Bad
CREATE VIEW vPackages -- Prefix unnecessary
CREATE VIEW View1 -- Non-descriptive
```

### Functions

```sql
-- ✅ Good - Descriptive name with "fn" prefix (optional)
CREATE FUNCTION CalculatePackageSize
(
    @PackageId INT
)
RETURNS BIGINT
AS
BEGIN
    -- Implementation
END;

-- ❌ Bad
CREATE FUNCTION fn1 -- Non-descriptive
CREATE FUNCTION PackageSize -- Missing verb
```

## Formatting Standards

### Keyword Case

Use UPPERCASE for SQL keywords:

```sql
-- ✅ Good
SELECT 
    PackageId,
    PackageName,
    Version
FROM Package
WHERE IsActive = 1
ORDER BY PackageName;

-- ❌ Bad
select PackageId, PackageName, Version from Package where IsActive = 1 order by PackageName;
```

### Indentation

Use consistent indentation (4 spaces):

```sql
-- ✅ Good
SELECT 
    p.PackageId,
    p.PackageName,
    p.Version,
    u.Username AS InstalledBy,
    i.InstallationDate
FROM Package p
INNER JOIN Installation i ON p.PackageId = i.PackageId
INNER JOIN [User] u ON i.UserId = u.UserId
WHERE p.IsActive = 1
    AND i.InstallationDate >= DATEADD(DAY, -30, GETUTCDATE())
ORDER BY i.InstallationDate DESC;

-- ❌ Bad
SELECT p.PackageId,p.PackageName,p.Version,u.Username,i.InstallationDate
FROM Package p INNER JOIN Installation i ON p.PackageId=i.PackageId
INNER JOIN [User] u ON i.UserId=u.UserId WHERE p.IsActive=1
AND i.InstallationDate>=DATEADD(DAY,-30,GETUTCDATE())
ORDER BY i.InstallationDate DESC;
```

### Line Breaks

Break long queries into readable chunks:

```sql
-- ✅ Good
SELECT 
    p.PackageId,
    p.PackageName,
    p.Version,
    COUNT(i.InstallationId) AS InstallCount,
    MAX(i.InstallationDate) AS LastInstallDate
FROM Package p
LEFT JOIN Installation i ON p.PackageId = i.PackageId
WHERE p.IsActive = 1
GROUP BY 
    p.PackageId,
    p.PackageName,
    p.Version
HAVING COUNT(i.InstallationId) > 0
ORDER BY InstallCount DESC;
```

### Commas

Leading commas for better version control diffs:

```sql
-- ✅ Good - Leading commas
SELECT 
    PackageId
    , PackageName
    , Version
    , Description
FROM Package;

-- ✅ Also acceptable - Trailing commas
SELECT 
    PackageId,
    PackageName,
    Version,
    Description
FROM Package;
```

### Comments

Use comments to explain complex logic:

```sql
-- ✅ Good
-- Get packages that have been installed in the last 30 days
-- and have more than 10 installations
SELECT 
    p.PackageId,
    p.PackageName,
    COUNT(i.InstallationId) AS InstallCount
FROM Package p
INNER JOIN Installation i ON p.PackageId = i.PackageId
WHERE i.InstallationDate >= DATEADD(DAY, -30, GETUTCDATE())
GROUP BY p.PackageId, p.PackageName
HAVING COUNT(i.InstallationId) > 10;

/*
 * Multi-line comment for complex procedures
 * This procedure handles package cleanup
 * by removing inactive packages older than 1 year
 */
CREATE PROCEDURE CleanupInactivePackages
AS
BEGIN
    -- Implementation
END;
```

## Query Writing

### SELECT Statements

```sql
-- ✅ Good - Explicit column list
SELECT 
    PackageId,
    PackageName,
    Version
FROM Package;

-- ❌ Bad - SELECT *
SELECT * FROM Package;

-- ❌ Exception: COUNT(*) is acceptable
SELECT COUNT(*) FROM Package;
```

### WHERE Clause

```sql
-- ✅ Good - Clear and readable
SELECT 
    PackageId,
    PackageName,
    Version
FROM Package
WHERE IsActive = 1
    AND CreatedDate >= '2024-01-01'
    AND PackageName LIKE '%UniGetUI%';

-- ✅ Good - Using IN for multiple values
SELECT PackageId
FROM Package
WHERE PackageId IN (1, 2, 3, 4, 5);

-- ❌ Bad - Multiple OR conditions instead of IN
SELECT PackageId
FROM Package
WHERE PackageId = 1 OR PackageId = 2 OR PackageId = 3;
```

### JOINs

```sql
-- ✅ Good - Explicit JOIN syntax with table aliases
SELECT 
    p.PackageId,
    p.PackageName,
    u.Username
FROM Package p
INNER JOIN Installation i ON p.PackageId = i.PackageId
INNER JOIN [User] u ON i.UserId = u.UserId;

-- ❌ Bad - Implicit joins (old style)
SELECT 
    p.PackageId,
    p.PackageName,
    u.Username
FROM Package p, Installation i, [User] u
WHERE p.PackageId = i.PackageId
    AND i.UserId = u.UserId;

-- ✅ Good - LEFT JOIN when including optional data
SELECT 
    p.PackageId,
    p.PackageName,
    COUNT(i.InstallationId) AS InstallCount
FROM Package p
LEFT JOIN Installation i ON p.PackageId = i.PackageId
GROUP BY p.PackageId, p.PackageName;
```

### Common Table Expressions (CTEs)

```sql
-- ✅ Good - Use CTEs for complex queries
WITH RecentPackages AS (
    SELECT 
        PackageId,
        PackageName,
        Version
    FROM Package
    WHERE CreatedDate >= DATEADD(DAY, -30, GETUTCDATE())
),
PackageInstallCounts AS (
    SELECT 
        PackageId,
        COUNT(*) AS InstallCount
    FROM Installation
    GROUP BY PackageId
)
SELECT 
    rp.PackageId,
    rp.PackageName,
    rp.Version,
    ISNULL(pic.InstallCount, 0) AS InstallCount
FROM RecentPackages rp
LEFT JOIN PackageInstallCounts pic ON rp.PackageId = pic.PackageId
ORDER BY pic.InstallCount DESC;

-- ❌ Bad - Nested subqueries
SELECT 
    p.PackageId,
    p.PackageName,
    (SELECT COUNT(*) 
     FROM Installation i 
     WHERE i.PackageId = p.PackageId) AS InstallCount
FROM (
    SELECT PackageId, PackageName, Version
    FROM Package
    WHERE CreatedDate >= DATEADD(DAY, -30, GETUTCDATE())
) p;
```

## Stored Procedures and Functions

### Stored Procedure Structure

```sql
-- ✅ Good
CREATE PROCEDURE GetPackageById
    @PackageId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate input
    IF @PackageId IS NULL OR @PackageId <= 0
    BEGIN
        RAISERROR('Invalid PackageId', 16, 1);
        RETURN;
    END;
    
    -- Main query
    SELECT 
        PackageId,
        PackageName,
        Version,
        Description,
        CreatedDate
    FROM Package
    WHERE PackageId = @PackageId;
END;
GO

-- ❌ Bad
CREATE PROCEDURE GetPackageById @PackageId INT AS
SELECT * FROM Package WHERE PackageId = @PackageId;
```

### Error Handling

```sql
-- ✅ Good - Use TRY/CATCH
CREATE PROCEDURE InsertPackage
    @PackageName NVARCHAR(100),
    @Version NVARCHAR(50),
    @NewPackageId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        INSERT INTO Package (PackageName, Version, CreatedDate)
        VALUES (@PackageName, @Version, GETUTCDATE());
        
        SET @NewPackageId = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH;
END;
```

### Functions

```sql
-- ✅ Good - Scalar function
CREATE FUNCTION GetPackageInstallCount
(
    @PackageId INT
)
RETURNS INT
AS
BEGIN
    DECLARE @Count INT;
    
    SELECT @Count = COUNT(*)
    FROM Installation
    WHERE PackageId = @PackageId;
    
    RETURN ISNULL(@Count, 0);
END;

-- ✅ Good - Table-valued function
CREATE FUNCTION GetActivePackages()
RETURNS TABLE
AS
RETURN
(
    SELECT 
        PackageId,
        PackageName,
        Version
    FROM Package
    WHERE IsActive = 1
);
```

## Performance Optimization

### Use Appropriate Data Types

```sql
-- ✅ Good
CREATE TABLE Package (
    PackageId INT IDENTITY(1,1) PRIMARY KEY,
    PackageName NVARCHAR(100) NOT NULL, -- Unicode for international support
    Version NVARCHAR(50) NOT NULL,
    Size BIGINT NOT NULL, -- For large file sizes
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2(0) NOT NULL -- DATETIME2 more accurate than DATETIME
);

-- ❌ Bad
CREATE TABLE Package (
    PackageId NVARCHAR(50), -- Using string for ID is inefficient
    PackageName NVARCHAR(MAX), -- MAX when fixed length is better
    Size FLOAT, -- FLOAT for whole numbers
    IsActive INT, -- INT instead of BIT for boolean
    CreatedDate VARCHAR(50) -- String for date
);
```

### Avoid Functions on Indexed Columns

```sql
-- ❌ Bad - Function on indexed column prevents index usage
SELECT PackageId, PackageName
FROM Package
WHERE UPPER(PackageName) = 'UNIGETUI';

-- ✅ Good - Direct comparison (if case-insensitive collation)
SELECT PackageId, PackageName
FROM Package
WHERE PackageName = 'UniGetUI';

-- ✅ Good - Add computed column with index if needed
ALTER TABLE Package 
ADD PackageNameUpper AS UPPER(PackageName) PERSISTED;

CREATE INDEX IX_Package_NameUpper ON Package(PackageNameUpper);

SELECT PackageId, PackageName
FROM Package
WHERE PackageNameUpper = 'UNIGETUI';
```

### Limit Result Sets

```sql
-- ✅ Good - Use TOP or OFFSET/FETCH
SELECT TOP 100
    PackageId,
    PackageName,
    Version
FROM Package
ORDER BY CreatedDate DESC;

-- ✅ Good - Pagination with OFFSET/FETCH
SELECT 
    PackageId,
    PackageName,
    Version
FROM Package
ORDER BY CreatedDate DESC
OFFSET 100 ROWS
FETCH NEXT 50 ROWS ONLY;

-- ❌ Bad - Retrieving all rows when only a subset is needed
SELECT PackageId, PackageName, Version
FROM Package;
```

### Use EXISTS Instead of COUNT

```sql
-- ✅ Good - EXISTS stops at first match
IF EXISTS (SELECT 1 FROM Package WHERE PackageName = 'UniGetUI')
BEGIN
    PRINT 'Package exists';
END;

-- ❌ Bad - COUNT scans all matching rows
IF (SELECT COUNT(*) FROM Package WHERE PackageName = 'UniGetUI') > 0
BEGIN
    PRINT 'Package exists';
END;
```

### Avoid Cursors

```sql
-- ❌ Bad - Using cursor
DECLARE @PackageId INT;
DECLARE package_cursor CURSOR FOR
    SELECT PackageId FROM Package;

OPEN package_cursor;
FETCH NEXT FROM package_cursor INTO @PackageId;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Process each package
    EXEC ProcessPackage @PackageId;
    FETCH NEXT FROM package_cursor INTO @PackageId;
END;

CLOSE package_cursor;
DEALLOCATE package_cursor;

-- ✅ Good - Set-based operation
UPDATE Package
SET ProcessedDate = GETUTCDATE()
WHERE IsActive = 1;

-- ✅ Good - If cursor truly needed, use FAST_FORWARD
DECLARE package_cursor CURSOR FAST_FORWARD FOR
    SELECT PackageId FROM Package;
```

## Indexing Strategy

### When to Create Indexes

```sql
-- ✅ Good - Index on foreign key
CREATE INDEX IX_Installation_PackageId 
ON Installation(PackageId);

-- ✅ Good - Composite index for common queries
CREATE INDEX IX_Package_IsActive_CreatedDate 
ON Package(IsActive, CreatedDate);

-- ✅ Good - Covering index with INCLUDE
CREATE INDEX IX_Package_Name_INCLUDE_Version 
ON Package(PackageName) 
INCLUDE (Version, Description);

-- ❌ Bad - Over-indexing
CREATE INDEX IX_Package_Column1 ON Package(Column1);
CREATE INDEX IX_Package_Column2 ON Package(Column2);
CREATE INDEX IX_Package_Column3 ON Package(Column3);
-- Many indexes slow down INSERT/UPDATE/DELETE
```

### Index Maintenance

```sql
-- Check index fragmentation
SELECT 
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats(
    DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
INNER JOIN sys.indexes i ON ips.object_id = i.object_id 
    AND ips.index_id = i.index_id
WHERE ips.avg_fragmentation_in_percent > 10
ORDER BY ips.avg_fragmentation_in_percent DESC;

-- Rebuild highly fragmented indexes
ALTER INDEX IX_Package_Name ON Package REBUILD;

-- Reorganize moderately fragmented indexes
ALTER INDEX IX_Package_Name ON Package REORGANIZE;
```

## Transaction Management

### ACID Principles

```sql
-- ✅ Good - Proper transaction handling
BEGIN TRANSACTION;

BEGIN TRY
    -- Insert package
    INSERT INTO Package (PackageName, Version)
    VALUES ('UniGetUI', '1.0.0');
    
    DECLARE @PackageId INT = SCOPE_IDENTITY();
    
    -- Insert dependencies
    INSERT INTO Dependency (PackageId, DependencyName)
    VALUES (@PackageId, 'Dependency1');
    
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;

-- ❌ Bad - No error handling
BEGIN TRANSACTION;
INSERT INTO Package (PackageName, Version) VALUES ('UniGetUI', '1.0.0');
INSERT INTO Dependency (PackageId, DependencyName) VALUES (1, 'Dependency1');
COMMIT TRANSACTION;
```

### Isolation Levels

```sql
-- ✅ Good - Specify isolation level when needed
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

-- For read-heavy operations that don't need perfect accuracy
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

-- For operations requiring serializable reads
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
```

## Security Best Practices

### SQL Injection Prevention

```sql
-- ✅ Good - Parameterized query (in application code)
-- C# example:
-- var cmd = new SqlCommand("SELECT * FROM Package WHERE PackageId = @PackageId", conn);
-- cmd.Parameters.AddWithValue("@PackageId", packageId);

-- ❌ Bad - String concatenation (in application code)
-- var query = "SELECT * FROM Package WHERE PackageId = " + packageId;
```

### Principle of Least Privilege

```sql
-- ✅ Good - Grant specific permissions
CREATE USER AppUser WITHOUT LOGIN;

GRANT SELECT ON Package TO AppUser;
GRANT INSERT ON Package TO AppUser;
GRANT UPDATE ON Package TO AppUser;

GRANT EXECUTE ON GetPackageById TO AppUser;

-- ❌ Bad - Granting excessive permissions
GRANT db_owner TO AppUser;
```

### Sensitive Data

```sql
-- ✅ Good - Encrypt sensitive columns
CREATE TABLE [User] (
    UserId INT PRIMARY KEY,
    Username NVARCHAR(50),
    PasswordHash VARBINARY(64) NOT NULL, -- Store hash, not plain text
    Email NVARCHAR(100)
);

-- ✅ Good - Use Always Encrypted for highly sensitive data
CREATE TABLE [User] (
    UserId INT PRIMARY KEY,
    Username NVARCHAR(50),
    SSN NVARCHAR(11) ENCRYPTED WITH (
        COLUMN_ENCRYPTION_KEY = CEK1,
        ENCRYPTION_TYPE = RANDOMIZED,
        ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256'
    )
);
```

## Anti-Patterns to Avoid

### 1. SELECT *

```sql
-- ❌ Bad
SELECT * FROM Package;

-- ✅ Good
SELECT PackageId, PackageName, Version FROM Package;
```

### 2. NOLOCK Everywhere

```sql
-- ❌ Bad - Can lead to dirty reads
SELECT * FROM Package WITH (NOLOCK);

-- ✅ Good - Use appropriate isolation level
SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
SELECT PackageId, PackageName FROM Package;
```

### 3. Using OR in WHERE Clause

```sql
-- ❌ Bad - OR may prevent index usage
SELECT PackageId FROM Package
WHERE PackageName = 'UniGetUI' OR Version = '1.0.0';

-- ✅ Good - UNION or separate queries
SELECT PackageId FROM Package WHERE PackageName = 'UniGetUI'
UNION
SELECT PackageId FROM Package WHERE Version = '1.0.0';
```

### 4. Implicit Type Conversions

```sql
-- ❌ Bad - String compared to number
SELECT * FROM Package WHERE PackageId = '123'; -- Implicit conversion

-- ✅ Good
SELECT * FROM Package WHERE PackageId = 123;
```

## Common Pitfalls

### 1. Not Checking NULL Values

```sql
-- ❌ Bad
SELECT * FROM Package WHERE Description = NULL; -- Always returns 0 rows

-- ✅ Good
SELECT * FROM Package WHERE Description IS NULL;
```

### 2. Using DISTINCT Unnecessarily

```sql
-- ❌ Bad - DISTINCT as a crutch for bad joins
SELECT DISTINCT p.PackageName
FROM Package p
INNER JOIN Installation i ON p.PackageId = i.PackageId;

-- ✅ Good - Fix the query logic
SELECT p.PackageName
FROM Package p
WHERE EXISTS (SELECT 1 FROM Installation i WHERE i.PackageId = p.PackageId);
```

### 3. Not Using Transactions for Multiple Operations

```sql
-- ❌ Bad
DELETE FROM Dependency WHERE PackageId = 1;
DELETE FROM Package WHERE PackageId = 1;
-- What if second DELETE fails?

-- ✅ Good
BEGIN TRANSACTION;
DELETE FROM Dependency WHERE PackageId = 1;
DELETE FROM Package WHERE PackageId = 1;
COMMIT TRANSACTION;
```

## Query Optimization Checklist

- [ ] Use explicit column lists (no SELECT *)
- [ ] Add appropriate indexes for WHERE, JOIN, and ORDER BY clauses
- [ ] Avoid functions on indexed columns in WHERE clauses
- [ ] Use EXISTS instead of IN for subqueries
- [ ] Use CTEs for better readability of complex queries
- [ ] Limit result sets with TOP or OFFSET/FETCH
- [ ] Use covering indexes where appropriate
- [ ] Check execution plans for expensive operations
- [ ] Avoid cursors; use set-based operations
- [ ] Use appropriate data types
- [ ] Add proper error handling with TRY/CATCH
- [ ] Use transactions for multi-statement operations
- [ ] Test with realistic data volumes

## Additional Resources

- [SQL Server Best Practices](https://docs.microsoft.com/en-us/sql/relational-databases/best-practices/best-practices)
- [Query Performance Tuning](https://docs.microsoft.com/en-us/sql/relational-databases/performance/performance-tuning)
- [Index Design Guidelines](https://docs.microsoft.com/en-us/sql/relational-databases/sql-server-index-design-guide)
