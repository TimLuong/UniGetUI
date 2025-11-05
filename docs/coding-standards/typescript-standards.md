# TypeScript/JavaScript Coding Standards

This document outlines TypeScript and JavaScript coding standards for hybrid Windows applications in the UniGetUI project.

## Table of Contents

1. [General Principles](#general-principles)
2. [TypeScript vs JavaScript](#typescript-vs-javascript)
3. [Naming Conventions](#naming-conventions)
4. [Formatting Standards](#formatting-standards)
5. [Type Definitions](#type-definitions)
6. [Functions and Methods](#functions-and-methods)
7. [Async/Await Patterns](#asyncawait-patterns)
8. [Error Handling](#error-handling)
9. [Module Organization](#module-organization)
10. [Anti-Patterns to Avoid](#anti-patterns-to-avoid)
11. [Common Pitfalls](#common-pitfalls)

## General Principles

### Prefer TypeScript

- **Always use TypeScript** for new code
- JavaScript is acceptable for:
  - Legacy code maintenance
  - Simple scripts
  - Configuration files

### Modern JavaScript/TypeScript

- Use ES6+ features (const, let, arrow functions, destructuring, etc.)
- Target modern browsers (ES2020+)
- Use async/await over callbacks and promises chains

## TypeScript vs JavaScript

### When to Use TypeScript

```typescript
// ✅ Good - TypeScript for application code
interface Package {
    id: string;
    name: string;
    version: string;
    isInstalled: boolean;
}

class PackageManager {
    async installPackage(pkg: Package): Promise<boolean> {
        // Implementation
    }
}
```

### When JavaScript is Acceptable

```javascript
// ✅ Acceptable - Simple configuration
module.exports = {
    entry: './src/index.ts',
    output: {
        filename: 'bundle.js'
    }
};
```

## Naming Conventions

### Variables and Constants

```typescript
// ✅ Good - camelCase for variables
const packageName = "UniGetUI";
let retryCount = 0;
const maxRetries = 3;

// ✅ Good - UPPER_SNAKE_CASE for true constants
const API_BASE_URL = "https://api.example.com";
const MAX_RETRY_ATTEMPTS = 3;

// ❌ Bad
const PackageName = "UniGetUI"; // Should be camelCase
const max_retries = 3; // Should be UPPER_SNAKE_CASE or camelCase
```

### Functions and Methods

```typescript
// ✅ Good - camelCase, verb-based names
function installPackage(packageId: string): Promise<void> { }
async function fetchPackageData(id: string): Promise<Package> { }
function isPackageInstalled(packageId: string): boolean { }

// ❌ Bad
function InstallPackage(packageId: string) { } // Should be camelCase
function package_install(id: string) { } // Should be camelCase
```

### Classes and Interfaces

```typescript
// ✅ Good - PascalCase
class PackageManager {
    // Implementation
}

interface PackageRepository {
    getPackage(id: string): Promise<Package>;
}

type PackageStatus = "pending" | "installed" | "failed";

// ❌ Bad
class packageManager { } // Should be PascalCase
interface packageRepository { } // Should be PascalCase
```

### Type Names and Generics

```typescript
// ✅ Good
type PackageList = Package[];
type PackageMap = Map<string, Package>;

// Generic type parameters
class Repository<T> {
    items: T[];
}

interface Result<TData, TError> {
    data?: TData;
    error?: TError;
}

// ❌ Bad
type packageList = Package[]; // Should be PascalCase
class repository<t> { } // Type parameter should be uppercase
```

### File Names

```typescript
// ✅ Good
// package-manager.ts
// package.interface.ts
// install-service.ts

// ❌ Bad
// PackageManager.ts (use kebab-case, not PascalCase)
// package_manager.ts (use kebab-case, not snake_case)
```

## Formatting Standards

### Indentation and Spacing

```typescript
// ✅ Good - 2 or 4 spaces (consistent across project)
function installPackage(packageId: string): void {
  if (!packageId) {
    throw new Error("Package ID is required");
  }
  
  const package = findPackage(packageId);
  install(package);
}

// ❌ Bad
function installPackage(packageId: string): void {
if (!packageId) {
throw new Error("Package ID is required");
}
const package = findPackage(packageId);
install(package);
}
```

### Semicolons

Use semicolons consistently:

```typescript
// ✅ Good - With semicolons (recommended)
const name = "UniGetUI";
const version = "1.0.0";

function install() {
  console.log("Installing");
}

// ✅ Also acceptable - Without semicolons (if consistent)
const name = "UniGetUI"
const version = "1.0.0"

function install() {
  console.log("Installing")
}
```

### Quotes

Use single or double quotes consistently:

```typescript
// ✅ Good - Consistent single quotes
const message = 'Hello, World!';
const name = 'UniGetUI';

// ✅ Also good - Consistent double quotes
const message = "Hello, World!";
const name = "UniGetUI";

// ✅ Template literals for interpolation
const greeting = `Hello, ${name}!`;

// ❌ Bad - Inconsistent
const message = 'Hello, World!';
const name = "UniGetUI";
```

### Line Length

```typescript
// ✅ Good - Break long lines
const package = await packageRepository.getPackageById(
  packageId,
  { includeMetadata: true, includeDependencies: true }
);

// ❌ Bad - Line too long
const package = await packageRepository.getPackageById(packageId, { includeMetadata: true, includeDependencies: true, includeVersionHistory: true });
```

## Type Definitions

### Basic Types

```typescript
// ✅ Good - Explicit types
const name: string = "UniGetUI";
const version: number = 1.0;
const isInstalled: boolean = true;
const packages: Package[] = [];

// ✅ Also good - Type inference
const name = "UniGetUI"; // Inferred as string
const version = 1.0; // Inferred as number
const isInstalled = true; // Inferred as boolean
```

### Interfaces vs Types

```typescript
// ✅ Good - Interface for objects
interface Package {
  id: string;
  name: string;
  version: string;
  dependencies?: Dependency[];
}

// ✅ Good - Type for unions, intersections, primitives
type PackageStatus = "pending" | "installed" | "failed";
type PackageId = string;
type PackageOrNull = Package | null;

// ✅ Good - Type for complex unions
type Result<T> = 
  | { success: true; data: T }
  | { success: false; error: Error };
```

### Optional and Required Properties

```typescript
// ✅ Good
interface Package {
  id: string;           // Required
  name: string;         // Required
  version: string;      // Required
  description?: string; // Optional
  icon?: string;        // Optional
}

// ✅ Good - Making all properties optional
type PartialPackage = Partial<Package>;

// ✅ Good - Making all properties required
type RequiredPackage = Required<Package>;
```

### Generics

```typescript
// ✅ Good
interface Repository<T> {
  getById(id: string): Promise<T | null>;
  getAll(): Promise<T[]>;
  save(item: T): Promise<void>;
}

class PackageRepository implements Repository<Package> {
  async getById(id: string): Promise<Package | null> {
    // Implementation
  }
  
  async getAll(): Promise<Package[]> {
    // Implementation
  }
  
  async save(item: Package): Promise<void> {
    // Implementation
  }
}

// ❌ Bad - Using 'any'
interface Repository {
  getById(id: string): Promise<any>;
  getAll(): Promise<any[]>;
}
```

### Avoid 'any'

```typescript
// ❌ Bad
function processData(data: any): any {
  return data.value;
}

// ✅ Good - Use specific types
function processData(data: Package): string {
  return data.name;
}

// ✅ Good - Use generics when type is unknown
function processData<T>(data: T): T {
  return data;
}

// ✅ Good - Use 'unknown' when truly unknown
function processData(data: unknown): string {
  if (typeof data === 'object' && data !== null && 'name' in data) {
    return (data as Package).name;
  }
  return 'Unknown';
}
```

## Functions and Methods

### Function Declarations

```typescript
// ✅ Good - Function declaration
function installPackage(packageId: string): Promise<boolean> {
  // Implementation
}

// ✅ Good - Arrow function for callbacks
const handleClick = (event: MouseEvent): void => {
  console.log(event.target);
};

// ✅ Good - Arrow function in class
class PackageManager {
  installPackage = async (packageId: string): Promise<boolean> => {
    // Implementation preserves 'this' context
  };
}
```

### Parameter Types

```typescript
// ✅ Good - Type all parameters
function createPackage(
  name: string,
  version: string,
  options?: PackageOptions
): Package {
  // Implementation
}

// ✅ Good - Destructuring with types
function createPackage({
  name,
  version,
  description = 'No description'
}: PackageInput): Package {
  // Implementation
}

// ❌ Bad - No parameter types
function createPackage(name, version, options) {
  // Implementation
}
```

### Return Types

```typescript
// ✅ Good - Explicit return type
function getPackageCount(): number {
  return packages.length;
}

async function fetchPackage(id: string): Promise<Package> {
  const response = await fetch(`/api/packages/${id}`);
  return response.json();
}

// ✅ Acceptable - Inferred return type for simple functions
function add(a: number, b: number) {
  return a + b; // Inferred as number
}
```

### Optional Parameters

```typescript
// ✅ Good - Optional parameters at the end
function installPackage(
  packageId: string,
  force?: boolean,
  silent?: boolean
): Promise<void> {
  // Implementation
}

// ✅ Good - Default values
function installPackage(
  packageId: string,
  force: boolean = false,
  silent: boolean = false
): Promise<void> {
  // Implementation
}

// ❌ Bad - Required parameter after optional
function installPackage(
  packageId: string,
  force?: boolean,
  version: string
): Promise<void> {
  // Implementation
}
```

## Async/Await Patterns

### Prefer Async/Await

```typescript
// ✅ Good - Async/await
async function installPackage(packageId: string): Promise<boolean> {
  try {
    const package = await fetchPackage(packageId);
    await downloadPackage(package);
    await installPackageFiles(package);
    return true;
  } catch (error) {
    console.error('Installation failed:', error);
    return false;
  }
}

// ❌ Bad - Promise chains
function installPackage(packageId: string): Promise<boolean> {
  return fetchPackage(packageId)
    .then(package => downloadPackage(package))
    .then(package => installPackageFiles(package))
    .then(() => true)
    .catch(error => {
      console.error('Installation failed:', error);
      return false;
    });
}
```

### Parallel Async Operations

```typescript
// ✅ Good - Run in parallel with Promise.all
async function getMultiplePackages(
  packageIds: string[]
): Promise<Package[]> {
  const packages = await Promise.all(
    packageIds.map(id => fetchPackage(id))
  );
  return packages;
}

// ✅ Good - Handle errors individually with Promise.allSettled
async function getMultiplePackages(
  packageIds: string[]
): Promise<(Package | null)[]> {
  const results = await Promise.allSettled(
    packageIds.map(id => fetchPackage(id))
  );
  
  return results.map(result => 
    result.status === 'fulfilled' ? result.value : null
  );
}

// ❌ Bad - Sequential when parallel is possible
async function getMultiplePackages(
  packageIds: string[]
): Promise<Package[]> {
  const packages: Package[] = [];
  for (const id of packageIds) {
    packages.push(await fetchPackage(id));
  }
  return packages;
}
```

### Error Handling

```typescript
// ✅ Good - Specific error handling
async function installPackage(packageId: string): Promise<void> {
  try {
    const package = await fetchPackage(packageId);
    await install(package);
  } catch (error) {
    if (error instanceof NetworkError) {
      throw new InstallError('Network error during installation', error);
    } else if (error instanceof ValidationError) {
      throw new InstallError('Invalid package data', error);
    } else {
      throw new InstallError('Unexpected error during installation', error);
    }
  }
}

// ❌ Bad - Swallowing errors
async function installPackage(packageId: string): Promise<void> {
  try {
    const package = await fetchPackage(packageId);
    await install(package);
  } catch (error) {
    // Silent failure
  }
}
```

## Error Handling

### Custom Error Classes

```typescript
// ✅ Good - Custom error classes
class PackageError extends Error {
  constructor(
    message: string,
    public packageId: string,
    public cause?: Error
  ) {
    super(message);
    this.name = 'PackageError';
  }
}

class NetworkError extends Error {
  constructor(message: string, public statusCode: number) {
    super(message);
    this.name = 'NetworkError';
  }
}

// Usage
throw new PackageError('Package not found', packageId);
```

### Error Handling Patterns

```typescript
// ✅ Good - Result type pattern
type Result<T, E = Error> = 
  | { ok: true; value: T }
  | { ok: false; error: E };

async function fetchPackage(id: string): Promise<Result<Package>> {
  try {
    const response = await fetch(`/api/packages/${id}`);
    const data = await response.json();
    return { ok: true, value: data };
  } catch (error) {
    return { ok: false, error: error as Error };
  }
}

// Usage
const result = await fetchPackage('package-id');
if (result.ok) {
  console.log(result.value);
} else {
  console.error(result.error);
}
```

## Module Organization

### Import/Export

```typescript
// ✅ Good - Named exports
export interface Package {
  id: string;
  name: string;
}

export class PackageManager {
  // Implementation
}

export function installPackage(pkg: Package): Promise<void> {
  // Implementation
}

// ✅ Good - Default export for main class
export default class PackageManager {
  // Implementation
}

// ❌ Bad - Mixing default and named exports carelessly
export default class PackageManager { }
export const install = () => { }; // Confusing
```

### Import Organization

```typescript
// ✅ Good - Organized imports
// 1. Node/external modules
import { readFile } from 'fs/promises';
import axios from 'axios';

// 2. Internal modules
import { Package } from './models/package';
import { PackageRepository } from './repositories/package-repository';

// 3. Types
import type { PackageOptions } from './types';
```

### Module Structure

```typescript
// ✅ Good - Organized module
// models/package.ts
export interface Package {
  id: string;
  name: string;
  version: string;
}

export interface PackageOptions {
  force?: boolean;
  silent?: boolean;
}

// services/package-service.ts
import { Package, PackageOptions } from '../models/package';

export class PackageService {
  async install(pkg: Package, options?: PackageOptions): Promise<void> {
    // Implementation
  }
}
```

## Anti-Patterns to Avoid

### 1. Using 'var'

```typescript
// ❌ Bad
var packageName = "UniGetUI";

// ✅ Good
const packageName = "UniGetUI";
let retryCount = 0;
```

### 2. Not Using Strict Mode

```typescript
// ❌ Bad - tsconfig.json
{
  "compilerOptions": {
    "strict": false
  }
}

// ✅ Good - tsconfig.json
{
  "compilerOptions": {
    "strict": true,
    "noImplicitAny": true,
    "strictNullChecks": true
  }
}
```

### 3. Callback Hell

```typescript
// ❌ Bad
function installPackages(packageIds, callback) {
  fetchPackage(packageIds[0], (package1) => {
    install(package1, (result1) => {
      fetchPackage(packageIds[1], (package2) => {
        install(package2, (result2) => {
          callback([result1, result2]);
        });
      });
    });
  });
}

// ✅ Good
async function installPackages(packageIds: string[]): Promise<boolean[]> {
  const results = await Promise.all(
    packageIds.map(async id => {
      const package = await fetchPackage(id);
      return await install(package);
    })
  );
  return results;
}
```

### 4. Mutating Parameters

```typescript
// ❌ Bad
function addPackage(packages: Package[], newPackage: Package): Package[] {
  packages.push(newPackage); // Mutates original array
  return packages;
}

// ✅ Good
function addPackage(packages: Package[], newPackage: Package): Package[] {
  return [...packages, newPackage]; // Returns new array
}
```

### 5. Using == Instead of ===

```typescript
// ❌ Bad
if (value == null) { } // Matches both null and undefined
if (count == '5') { }  // Type coercion

// ✅ Good
if (value === null || value === undefined) { }
if (count === 5) { }
```

## Common Pitfalls

### 1. Forgetting to Await

```typescript
// ❌ Bad
async function installPackage(id: string) {
  const pkg = fetchPackage(id); // Missing await
  install(pkg); // pkg is a Promise, not a Package!
}

// ✅ Good
async function installPackage(id: string) {
  const pkg = await fetchPackage(id);
  await install(pkg);
}
```

### 2. Not Handling Null/Undefined

```typescript
// ❌ Bad
function getPackageName(pkg: Package | null): string {
  return pkg.name; // Runtime error if pkg is null
}

// ✅ Good
function getPackageName(pkg: Package | null): string {
  return pkg?.name ?? 'Unknown';
}
```

### 3. Array Mutation

```typescript
// ❌ Bad
const packages = getPackages();
packages.sort(); // Mutates original array

// ✅ Good
const packages = getPackages();
const sortedPackages = [...packages].sort();
```

### 4. Floating Promises

```typescript
// ❌ Bad
async function processPackages() {
  packages.forEach(pkg => {
    installPackage(pkg); // Promise not awaited!
  });
}

// ✅ Good
async function processPackages() {
  await Promise.all(
    packages.map(pkg => installPackage(pkg))
  );
}
```

### 5. Incorrect 'this' Context

```typescript
// ❌ Bad
class PackageManager {
  packageName = "UniGetUI";
  
  install() {
    setTimeout(function() {
      console.log(this.packageName); // 'this' is undefined or window
    }, 1000);
  }
}

// ✅ Good - Arrow function
class PackageManager {
  packageName = "UniGetUI";
  
  install() {
    setTimeout(() => {
      console.log(this.packageName); // 'this' is PackageManager
    }, 1000);
  }
}
```

## Best Practices

### 1. Use TypeScript Utility Types

```typescript
// ✅ Good
type PackageUpdate = Partial<Package>; // All properties optional
type RequiredPackage = Required<Package>; // All properties required
type PackageWithoutId = Omit<Package, 'id'>; // Exclude id
type PackageIdAndName = Pick<Package, 'id' | 'name'>; // Only id and name
type ReadonlyPackage = Readonly<Package>; // All properties readonly
```

### 2. Use Destructuring

```typescript
// ✅ Good
const { id, name, version } = package;
const [first, second, ...rest] = packages;

// Function parameters
function createPackage({ name, version }: PackageInput) {
  // Implementation
}
```

### 3. Use Template Literals

```typescript
// ✅ Good
const message = `Installing ${packageName} version ${version}`;
const url = `https://api.example.com/packages/${packageId}`;

// ❌ Bad
const message = 'Installing ' + packageName + ' version ' + version;
```

### 4. Use Optional Chaining and Nullish Coalescing

```typescript
// ✅ Good
const name = package?.metadata?.name ?? 'Unknown';
const count = package?.dependencies?.length ?? 0;

// ❌ Bad
const name = package && package.metadata && package.metadata.name 
  ? package.metadata.name 
  : 'Unknown';
```

## ESLint Configuration

Recommended ESLint rules for TypeScript:

```json
{
  "extends": [
    "eslint:recommended",
    "plugin:@typescript-eslint/recommended"
  ],
  "rules": {
    "@typescript-eslint/explicit-function-return-type": "warn",
    "@typescript-eslint/no-explicit-any": "error",
    "@typescript-eslint/no-unused-vars": "error",
    "prefer-const": "error",
    "no-var": "error",
    "eqeqeq": ["error", "always"]
  }
}
```

## Additional Resources

- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/intro.html)
- [TypeScript Deep Dive](https://basarat.gitbook.io/typescript/)
- [ESLint Rules](https://eslint.org/docs/rules/)
- [Airbnb JavaScript Style Guide](https://github.com/airbnb/javascript)
