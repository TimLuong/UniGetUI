# Database Schema Design Best Practices

## Overview

This guide establishes database schema design principles for Windows applications. While UniGetUI currently uses file-based JSON storage, these guidelines provide standards for projects requiring traditional relational databases.

## Table of Contents

1. [Schema Design Principles](#schema-design-principles)
2. [Normalization Guidelines](#normalization-guidelines)
3. [When to Denormalize](#when-to-denormalize)
4. [Naming Conventions](#naming-conventions)
5. [Data Types](#data-types)
6. [Constraints and Keys](#constraints-and-keys)
7. [Indexes](#indexes)
8. [Schema Versioning](#schema-versioning)

## Schema Design Principles

### 1. Design for the Business Domain

- **Model real-world entities**: Tables should represent clear business concepts (Users, Orders, Products)
- **Keep it simple**: Start with the simplest design that meets requirements
- **Plan for growth**: Consider future requirements without over-engineering

### 2. Consistency and Standards

```sql
-- Good: Consistent naming and structure
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL
);

CREATE TABLE Orders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    OrderDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TotalAmount DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
```

### 3. Single Source of Truth

- Each piece of data should have one authoritative source
- Use foreign keys to reference data rather than duplicating it
- Store computed values only when absolutely necessary for performance

### 4. Separate Concerns

- **Transactional data**: Current operational data (orders, transactions)
- **Historical data**: Archive tables for old records
- **Reference data**: Lookup tables (statuses, categories)
- **Configuration data**: Application settings

## Normalization Guidelines

### First Normal Form (1NF)

**Rule**: Eliminate repeating groups and ensure atomic values.

```sql
-- Bad: Repeating columns
CREATE TABLE Orders_Bad (
    OrderId INT PRIMARY KEY,
    Item1 NVARCHAR(100),
    Item2 NVARCHAR(100),
    Item3 NVARCHAR(100)
);

-- Good: Separate table for items
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    OrderDate DATETIME2 NOT NULL
);

CREATE TABLE OrderItems (
    OrderItemId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ProductName NVARCHAR(100) NOT NULL,
    Quantity INT NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId)
);
```

### Second Normal Form (2NF)

**Rule**: Remove partial dependencies (all non-key columns must depend on the entire primary key).

```sql
-- Bad: ProductName depends only on ProductId, not the composite key
CREATE TABLE OrderItems_Bad (
    OrderId INT,
    ProductId INT,
    ProductName NVARCHAR(100),  -- Partial dependency
    Quantity INT,
    PRIMARY KEY (OrderId, ProductId)
);

-- Good: Product information in separate table
CREATE TABLE Products (
    ProductId INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(100) NOT NULL,
    Price DECIMAL(10,2) NOT NULL
);

CREATE TABLE OrderItems (
    OrderId INT,
    ProductId INT,
    Quantity INT NOT NULL,
    PRIMARY KEY (OrderId, ProductId),
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);
```

### Third Normal Form (3NF)

**Rule**: Remove transitive dependencies (non-key columns should not depend on other non-key columns).

```sql
-- Bad: CityName and StateCode are transitively dependent on ZipCode
CREATE TABLE Customers_Bad (
    CustomerId INT PRIMARY KEY,
    Name NVARCHAR(100),
    ZipCode NVARCHAR(10),
    CityName NVARCHAR(50),
    StateCode NVARCHAR(2)
);

-- Good: Separate location data
CREATE TABLE ZipCodes (
    ZipCode NVARCHAR(10) PRIMARY KEY,
    CityName NVARCHAR(50) NOT NULL,
    StateCode NVARCHAR(2) NOT NULL
);

CREATE TABLE Customers (
    CustomerId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    ZipCode NVARCHAR(10) NOT NULL,
    CONSTRAINT FK_Customers_ZipCodes FOREIGN KEY (ZipCode) REFERENCES ZipCodes(ZipCode)
);
```

### Boyce-Codd Normal Form (BCNF)

**Rule**: Every determinant must be a candidate key.

Typically achieved when following 3NF, but watch for situations where a non-key column determines part of the key.

## When to Denormalize

### Valid Reasons for Denormalization

#### 1. Performance Optimization

```sql
-- Store calculated total to avoid joining order items every query
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY,
    CustomerId INT NOT NULL,
    OrderDate DATETIME2 NOT NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,  -- Denormalized
    ItemCount INT NOT NULL                -- Denormalized
);

-- Use triggers or application logic to maintain consistency
CREATE TRIGGER trg_UpdateOrderTotals
ON OrderItems
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    UPDATE Orders
    SET TotalAmount = (SELECT SUM(Quantity * UnitPrice) FROM OrderItems WHERE OrderId = Orders.OrderId),
        ItemCount = (SELECT COUNT(*) FROM OrderItems WHERE OrderId = Orders.OrderId)
    WHERE OrderId IN (SELECT DISTINCT OrderId FROM inserted UNION SELECT DISTINCT OrderId FROM deleted);
END;
```

#### 2. Read-Heavy Workloads

For data that is frequently read but rarely updated:

```sql
-- Cache user statistics
CREATE TABLE UserStatistics (
    UserId INT PRIMARY KEY,
    TotalOrders INT NOT NULL DEFAULT 0,
    TotalSpent DECIMAL(10,2) NOT NULL DEFAULT 0,
    LastOrderDate DATETIME2 NULL,
    LastUpdated DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
```

#### 3. Historical Snapshots

```sql
-- Store order snapshot with prices at time of order
CREATE TABLE OrderHistory (
    OrderId INT PRIMARY KEY,
    CustomerId INT NOT NULL,
    ProductName NVARCHAR(100) NOT NULL,    -- Snapshot
    PriceAtOrder DECIMAL(10,2) NOT NULL,   -- Snapshot
    OrderDate DATETIME2 NOT NULL
);
```

### Denormalization Guidelines

1. **Document the decision**: Clearly note why denormalization was necessary
2. **Maintain consistency**: Use triggers, stored procedures, or application logic
3. **Monitor performance**: Ensure the optimization actually improves performance
4. **Consider alternatives**: Indexed views, materialized views, or caching layers

## Naming Conventions

### Table Names

```sql
-- Use PascalCase for table names
CREATE TABLE Users (...);
CREATE TABLE OrderItems (...);
CREATE TABLE ProductCategories (...);

-- Use singular nouns (preferred)
CREATE TABLE User (...);        -- Preferred
CREATE TABLE Users (...);       -- Also acceptable

-- Be consistent project-wide
```

### Column Names

```sql
-- Use PascalCase for columns
CREATE TABLE Products (
    ProductId INT PRIMARY KEY,
    ProductName NVARCHAR(100),
    UnitPrice DECIMAL(10,2),
    CreatedAt DATETIME2
);

-- Avoid prefixing with table name (redundant)
-- Bad: Product_ProductName
-- Good: ProductName
```

### Primary Keys

```sql
-- Pattern 1: {TableName}Id (preferred)
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1)
);

-- Pattern 2: Id (acceptable)
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1)
);

-- Be consistent across the schema
```

### Foreign Keys

```sql
-- Name matches the referenced table's primary key
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY,
    UserId INT NOT NULL,  -- Matches Users.UserId
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Constraint naming: FK_{ChildTable}_{ParentTable}
```

### Indexes

```sql
-- Naming: IX_{TableName}_{ColumnName(s)}
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Orders_UserId_OrderDate ON Orders(UserId, OrderDate);

-- Unique indexes: UX_{TableName}_{ColumnName}
CREATE UNIQUE INDEX UX_Users_Username ON Users(Username);
```

## Data Types

### SQL Server Best Practices

```sql
-- String data
NVARCHAR(MAX)     -- Avoid unless necessary; use specific length
NVARCHAR(255)     -- Email addresses, URLs
NVARCHAR(50)      -- Usernames, codes
NVARCHAR(4000)    -- Descriptions
NCHAR(10)         -- Fixed-length codes (use sparingly)

-- Numeric data
INT               -- Integer values, IDs
BIGINT            -- Large integer values
DECIMAL(18,2)     -- Money, precise decimals
FLOAT             -- Scientific calculations (avoid for money)

-- Date/Time
DATETIME2         -- Preferred over DATETIME (more precision, smaller size)
DATE              -- Date only (no time component)
TIME              -- Time only (no date component)

-- Boolean
BIT               -- True/False values

-- Binary data
VARBINARY(MAX)    -- Files, images (consider file system storage)
UNIQUEIDENTIFIER  -- GUIDs (larger than INT, use when needed)
```

### Type Selection Guidelines

1. **Use the smallest appropriate type**: Saves space and improves performance
2. **Be specific with lengths**: `NVARCHAR(50)` not `NVARCHAR(MAX)`
3. **Use DATETIME2 over DATETIME**: Better precision and range
4. **Use DECIMAL for money**: Avoid FLOAT/REAL for financial data
5. **Consider future growth**: INT vs BIGINT for IDs

## Constraints and Keys

### Primary Keys

```sql
-- Identity columns for surrogate keys (most common)
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL
);

-- Natural keys when appropriate
CREATE TABLE Countries (
    CountryCode NCHAR(2) PRIMARY KEY,  -- ISO 3166-1 alpha-2
    CountryName NVARCHAR(100) NOT NULL
);

-- Composite keys when needed
CREATE TABLE OrderItems (
    OrderId INT,
    ProductId INT,
    Quantity INT NOT NULL,
    PRIMARY KEY (OrderId, ProductId)
);
```

### Foreign Keys

```sql
-- Always use foreign key constraints
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) 
        REFERENCES Users(UserId)
        ON DELETE CASCADE      -- Choose appropriate action
        ON UPDATE CASCADE
);

-- Delete options:
-- CASCADE: Delete child records when parent is deleted
-- NO ACTION: Prevent deletion if child records exist (default)
-- SET NULL: Set foreign key to NULL (column must be nullable)
-- SET DEFAULT: Set foreign key to default value
```

### Check Constraints

```sql
-- Enforce business rules at the database level
CREATE TABLE Products (
    ProductId INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(100) NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    CONSTRAINT CK_Products_UnitPrice CHECK (UnitPrice >= 0)
);

CREATE TABLE Orders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    OrderDate DATETIME2 NOT NULL,
    ShipDate DATETIME2 NULL,
    CONSTRAINT CK_Orders_ShipDate CHECK (ShipDate IS NULL OR ShipDate >= OrderDate)
);
```

### Unique Constraints

```sql
-- Ensure uniqueness for non-primary key columns
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);
```

### Default Constraints

```sql
-- Provide default values
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    OrderDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    IsActive BIT NOT NULL DEFAULT 1
);
```

## Indexes

### When to Create Indexes

1. **Primary keys**: Automatically indexed
2. **Foreign keys**: Should be indexed for join performance
3. **Frequently queried columns**: WHERE, JOIN, ORDER BY clauses
4. **Unique constraints**: Automatically create unique indexes

### Index Types

#### Clustered Index

```sql
-- One per table; determines physical order
-- Usually on primary key (automatic)
CREATE CLUSTERED INDEX IX_Orders_OrderDate ON Orders(OrderDate);
```

#### Non-Clustered Index

```sql
-- Multiple per table; separate structure from table
CREATE NONCLUSTERED INDEX IX_Users_Email ON Users(Email);

-- Composite index (column order matters!)
CREATE INDEX IX_Orders_UserId_OrderDate ON Orders(UserId, OrderDate);

-- Covering index (includes non-key columns)
CREATE INDEX IX_Orders_UserId 
ON Orders(UserId) 
INCLUDE (OrderDate, TotalAmount);
```

#### Filtered Index

```sql
-- Index only a subset of rows
CREATE INDEX IX_Orders_Active 
ON Orders(OrderDate)
WHERE Status = 'Active';
```

### Index Best Practices

1. **Don't over-index**: Each index has maintenance cost
2. **Column order matters**: Most selective columns first
3. **Monitor usage**: Remove unused indexes
4. **Consider covering indexes**: For frequently-run queries
5. **Index foreign keys**: Critical for join performance

```sql
-- Good: Foreign key indexed for join performance
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY,
    UserId INT NOT NULL,
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
CREATE INDEX IX_Orders_UserId ON Orders(UserId);

-- Good: Composite index for common query pattern
-- Query: SELECT * FROM Orders WHERE UserId = @UserId ORDER BY OrderDate DESC
CREATE INDEX IX_Orders_UserId_OrderDate ON Orders(UserId, OrderDate DESC);
```

## Schema Versioning

### Version Tracking

```sql
-- Track schema version in the database
CREATE TABLE SchemaVersion (
    Version INT PRIMARY KEY,
    Description NVARCHAR(500) NOT NULL,
    AppliedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    AppliedBy NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER
);

-- Insert initial version
INSERT INTO SchemaVersion (Version, Description) 
VALUES (1, 'Initial schema');
```

### Migration Scripts

```sql
-- V2_AddUserEmailColumn.sql
BEGIN TRANSACTION;

-- Add new column
ALTER TABLE Users ADD Email NVARCHAR(255) NULL;

-- Update version
INSERT INTO SchemaVersion (Version, Description) 
VALUES (2, 'Added Email column to Users table');

COMMIT TRANSACTION;
```

### Best Practices

1. **Never modify existing migrations**: Create new ones
2. **Make migrations reversible**: Include rollback scripts
3. **Test migrations**: On copies of production data
4. **Use transactions**: Ensure atomic changes
5. **Version numbering**: Sequential integers or timestamps

## Common Patterns

### Audit Columns

```sql
-- Standard audit columns for tracking changes
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    -- ... other columns ...
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
    UpdatedAt DATETIME2 NULL,
    UpdatedBy NVARCHAR(100) NULL
);

-- Trigger to automatically update audit columns
CREATE TRIGGER trg_Orders_UpdateAudit
ON Orders
AFTER UPDATE
AS
BEGIN
    UPDATE Orders
    SET UpdatedAt = GETUTCDATE(),
        UpdatedBy = SYSTEM_USER
    WHERE OrderId IN (SELECT OrderId FROM inserted);
END;
```

### Soft Delete

```sql
-- Mark records as deleted instead of actually deleting
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    DeletedBy NVARCHAR(100) NULL
);

-- Filtered index for active records
CREATE INDEX IX_Users_Active 
ON Users(Username) 
WHERE IsDeleted = 0;

-- View for active records
CREATE VIEW ActiveUsers AS
SELECT UserId, Username, Email
FROM Users
WHERE IsDeleted = 0;
```

### Lookup Tables

```sql
-- Reference data for status codes
CREATE TABLE OrderStatuses (
    StatusId INT PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL,
    DisplayOrder INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- Seed data
INSERT INTO OrderStatuses (StatusId, StatusName, DisplayOrder) VALUES
(1, 'Pending', 1),
(2, 'Processing', 2),
(3, 'Shipped', 3),
(4, 'Delivered', 4),
(5, 'Cancelled', 5);

-- Reference from main table
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    StatusId INT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Orders_OrderStatuses FOREIGN KEY (StatusId) 
        REFERENCES OrderStatuses(StatusId)
);
```

## Anti-Patterns to Avoid

### 1. Entity-Attribute-Value (EAV)

```sql
-- Bad: EAV pattern (hard to query, no type safety)
CREATE TABLE EntityAttributes (
    EntityId INT,
    AttributeName NVARCHAR(50),
    AttributeValue NVARCHAR(MAX)
);

-- Good: Proper columns
CREATE TABLE Products (
    ProductId INT PRIMARY KEY,
    ProductName NVARCHAR(100),
    Price DECIMAL(10,2),
    Color NVARCHAR(50)
);
```

### 2. Multi-Value Columns

```sql
-- Bad: Comma-separated values
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY,
    ProductIds NVARCHAR(MAX)  -- '1,2,3,4'
);

-- Good: Junction table
CREATE TABLE OrderProducts (
    OrderId INT,
    ProductId INT,
    PRIMARY KEY (OrderId, ProductId)
);
```

### 3. Polymorphic Associations

```sql
-- Bad: Generic foreign key
CREATE TABLE Comments (
    CommentId INT PRIMARY KEY,
    EntityType NVARCHAR(50),  -- 'Product', 'Order', etc.
    EntityId INT              -- Could reference any table
);

-- Good: Specific foreign keys
CREATE TABLE ProductComments (
    CommentId INT PRIMARY KEY,
    ProductId INT NOT NULL,
    CONSTRAINT FK_ProductComments_Products FOREIGN KEY (ProductId) 
        REFERENCES Products(ProductId)
);

CREATE TABLE OrderComments (
    CommentId INT PRIMARY KEY,
    OrderId INT NOT NULL,
    CONSTRAINT FK_OrderComments_Orders FOREIGN KEY (OrderId) 
        REFERENCES Orders(OrderId)
);
```

## Tools and Resources

### Schema Design Tools

- **SQL Server Management Studio (SSMS)**: Database diagrams
- **Azure Data Studio**: Cross-platform database tool
- **Entity Framework Core**: Code-first migrations
- **DbUp**: Database migration library
- **Redgate SQL Compare**: Schema comparison
- **ApexSQL**: Schema versioning and comparison

### Further Reading

- Microsoft SQL Server Documentation
- Database Design for Mere Mortals (Michael J. Hernandez)
- SQL Antipatterns (Bill Karwin)
- Entity Framework Core Documentation

## Related Documents

- [ORM Patterns](./orm-patterns.md)
- [Migration Strategy](./migration-strategy.md)
- [Performance Optimization](./performance-optimization.md)
