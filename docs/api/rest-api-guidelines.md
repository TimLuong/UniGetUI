# REST API Design Guidelines

This document outlines REST API design principles and best practices for UniGetUI and related projects. These guidelines are based on industry standards, Microsoft REST API Guidelines, and lessons learned from existing integrations in the codebase.

## Table of Contents

- [Core Principles](#core-principles)
- [Resource Naming](#resource-naming)
- [HTTP Methods](#http-methods)
- [Status Codes](#status-codes)
- [Versioning Strategies](#versioning-strategies)
- [Request and Response Formats](#request-and-response-formats)
- [Error Handling](#error-handling)
- [Authentication and Authorization](#authentication-and-authorization)
- [Rate Limiting and Throttling](#rate-limiting-and-throttling)
- [Caching](#caching)
- [Pagination](#pagination)
- [Filtering and Searching](#filtering-and-searching)
- [API Documentation](#api-documentation)

---

## Core Principles

### 1. RESTful Architecture

REST APIs should follow these architectural constraints:

- **Client-Server**: Separation of concerns between client and server
- **Stateless**: Each request contains all information needed to process it
- **Cacheable**: Responses must define themselves as cacheable or not
- **Uniform Interface**: Consistent resource identification and manipulation
- **Layered System**: Client cannot tell if connected directly to end server

### 2. Resource-Oriented Design

APIs should be designed around resources (nouns) rather than actions (verbs):

**Good:**
```
GET /api/v1/packages
GET /api/v1/packages/{id}
POST /api/v1/packages
PUT /api/v1/packages/{id}
DELETE /api/v1/packages/{id}
```

**Avoid:**
```
GET /api/v1/getPackages
POST /api/v1/createPackage
POST /api/v1/deletePackageById
```

### 3. Consistency

Maintain consistency across your API:
- Naming conventions (use consistent casing)
- Response structures
- Error formats
- HTTP status codes
- Authentication mechanisms

---

## Resource Naming

### URL Structure

Follow these conventions for resource URLs:

1. **Use nouns, not verbs**
   ```
   ✓ /packages
   ✗ /getPackages
   ```

2. **Use plural nouns for collections**
   ```
   ✓ /packages
   ✗ /package
   ```

3. **Use lowercase and hyphens for multi-word resources**
   ```
   ✓ /package-managers
   ✗ /PackageManagers
   ✗ /package_managers
   ```

4. **Use forward slashes to indicate hierarchical relationships**
   ```
   ✓ /packages/{id}/versions
   ✓ /packages/{id}/versions/{version}
   ```

5. **Avoid file extensions**
   ```
   ✓ /packages/123
   ✗ /packages/123.json
   ```

### Path Parameters vs Query Parameters

**Path Parameters**: For resource identification
```
GET /packages/{id}/versions/{version}
```

**Query Parameters**: For filtering, sorting, pagination
```
GET /packages?search=chrome&sort=name&limit=50
```

---

## HTTP Methods

Use standard HTTP methods according to their semantics:

### GET - Retrieve Resources

- **Purpose**: Retrieve resource(s)
- **Idempotent**: Yes
- **Safe**: Yes (no side effects)
- **Request Body**: No

**Examples:**
```http
GET /api/v1/packages           # Get all packages
GET /api/v1/packages/chrome    # Get specific package
GET /api/v1/packages/chrome/versions  # Get package versions
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": "chrome",
  "name": "Google Chrome",
  "version": "120.0.0",
  "description": "Web browser"
}
```

### POST - Create Resources

- **Purpose**: Create new resource
- **Idempotent**: No
- **Safe**: No
- **Request Body**: Yes

**Example:**
```http
POST /api/v1/packages
Content-Type: application/json

{
  "name": "My Package",
  "version": "1.0.0",
  "description": "Package description"
}
```

**Response:**
```http
HTTP/1.1 201 Created
Location: /api/v1/packages/my-package
Content-Type: application/json

{
  "id": "my-package",
  "name": "My Package",
  "version": "1.0.0",
  "createdAt": "2024-11-05T15:45:00Z"
}
```

### PUT - Update/Replace Resources

- **Purpose**: Replace entire resource
- **Idempotent**: Yes
- **Safe**: No
- **Request Body**: Yes

**Example:**
```http
PUT /api/v1/packages/my-package
Content-Type: application/json

{
  "name": "My Updated Package",
  "version": "2.0.0",
  "description": "Updated description"
}
```

### PATCH - Partial Update

- **Purpose**: Partially update resource
- **Idempotent**: Yes (typically)
- **Safe**: No
- **Request Body**: Yes

**Example:**
```http
PATCH /api/v1/packages/my-package
Content-Type: application/json

{
  "version": "2.0.1"
}
```

### DELETE - Remove Resources

- **Purpose**: Delete resource
- **Idempotent**: Yes
- **Safe**: No
- **Request Body**: No

**Example:**
```http
DELETE /api/v1/packages/my-package
```

**Response:**
```http
HTTP/1.1 204 No Content
```

---

## Status Codes

Use appropriate HTTP status codes:

### Success Codes (2xx)

| Code | Name | Usage |
|------|------|-------|
| 200 | OK | Successful GET, PUT, PATCH, or DELETE |
| 201 | Created | Successful POST that creates a resource |
| 202 | Accepted | Request accepted for async processing |
| 204 | No Content | Successful request with no response body (DELETE) |

### Redirection Codes (3xx)

| Code | Name | Usage |
|------|------|-------|
| 301 | Moved Permanently | Resource permanently moved to new URL |
| 302 | Found | Temporary redirect |
| 304 | Not Modified | Resource not modified (caching) |

### Client Error Codes (4xx)

| Code | Name | Usage |
|------|------|-------|
| 400 | Bad Request | Invalid request format or parameters |
| 401 | Unauthorized | Authentication required or failed |
| 403 | Forbidden | Authenticated but not authorized |
| 404 | Not Found | Resource does not exist |
| 405 | Method Not Allowed | HTTP method not supported for resource |
| 409 | Conflict | Request conflicts with current state |
| 422 | Unprocessable Entity | Validation errors |
| 429 | Too Many Requests | Rate limit exceeded |

### Server Error Codes (5xx)

| Code | Name | Usage |
|------|------|-------|
| 500 | Internal Server Error | Generic server error |
| 501 | Not Implemented | Functionality not implemented |
| 502 | Bad Gateway | Invalid response from upstream server |
| 503 | Service Unavailable | Server temporarily unavailable |
| 504 | Gateway Timeout | Upstream server timeout |

---

## Versioning Strategies

API versioning ensures backward compatibility while allowing evolution.

### URL Path Versioning (Recommended)

Include version in the URL path:

```
https://api.example.com/v1/packages
https://api.example.com/v2/packages
```

**Pros:**
- Clear and explicit
- Easy to route and cache
- Simple to understand

**Cons:**
- Less flexible for minor changes
- Multiple versions require maintenance

**Implementation Example (from UniGetUI):**
```csharp
// Crates.io API
public const string ApiUrl = "https://crates.io/api/v1";

var manifestUrl = new Uri($"{ApiUrl}/crates/{packageId}");
```

### Header Versioning

Specify version in request header:

```http
GET /api/packages
Accept: application/vnd.api+json; version=1
```

**Pros:**
- URL remains clean
- More flexible for content negotiation

**Cons:**
- Less visible
- Harder to test in browser
- More complex caching

### Query Parameter Versioning

Include version as query parameter:

```
https://api.example.com/packages?version=1
```

**Pros:**
- Simple to implement
- Easy to test

**Cons:**
- Can be overlooked
- Pollutes query parameters

### Semantic Versioning

Use semantic versioning (MAJOR.MINOR.PATCH):

- **MAJOR**: Breaking changes
- **MINOR**: Backward-compatible features
- **PATCH**: Backward-compatible bug fixes

**Example:**
```
v1.0.0 -> v1.1.0  # Added feature, backward compatible
v1.1.0 -> v2.0.0  # Breaking change
```

### Version Deprecation

When deprecating API versions:

1. **Announce**: Communicate deprecation timeline
2. **Warn**: Include deprecation warnings in responses
3. **Support**: Maintain for reasonable period (6-12 months)
4. **Remove**: Finally remove after transition period

**Example Response Header:**
```http
X-API-Warn: API v1 is deprecated and will be removed on 2025-12-31
Link: <https://api.example.com/v2/packages>; rel="successor-version"
```

---

## Request and Response Formats

### Content Types

Use standard content types:

- **JSON** (Recommended): `application/json`
- **XML**: `application/xml`
- **Form Data**: `application/x-www-form-urlencoded`
- **Multipart**: `multipart/form-data`

### JSON Naming Conventions

**camelCase** (Recommended for JavaScript compatibility):
```json
{
  "packageId": "chrome",
  "packageName": "Google Chrome",
  "installedVersion": "120.0.0"
}
```

**snake_case** (Common in Python APIs):
```json
{
  "package_id": "chrome",
  "package_name": "Google Chrome",
  "installed_version": "120.0.0"
}
```

**Choose one and be consistent across your entire API.**

### Request Format

**POST/PUT/PATCH Request:**
```http
POST /api/v1/packages
Content-Type: application/json
Accept: application/json
Authorization: Bearer {token}

{
  "name": "Example Package",
  "version": "1.0.0",
  "description": "Package description"
}
```

### Response Format

**Standard Success Response:**
```json
{
  "id": "example-package",
  "name": "Example Package",
  "version": "1.0.0",
  "description": "Package description",
  "createdAt": "2024-11-05T15:45:00Z",
  "updatedAt": "2024-11-05T15:45:00Z"
}
```

**Collection Response:**
```json
{
  "data": [
    {
      "id": "package1",
      "name": "Package 1"
    },
    {
      "id": "package2",
      "name": "Package 2"
    }
  ],
  "pagination": {
    "total": 100,
    "page": 1,
    "pageSize": 20,
    "totalPages": 5
  },
  "links": {
    "self": "/api/v1/packages?page=1",
    "next": "/api/v1/packages?page=2",
    "last": "/api/v1/packages?page=5"
  }
}
```

### Date and Time Format

Use **ISO 8601** format (UTC):

```json
{
  "createdAt": "2024-11-05T15:45:00Z",
  "updatedAt": "2024-11-05T15:45:00.123Z"
}
```

---

## Error Handling

### Standard Error Response Format

Provide consistent error responses:

```json
{
  "error": {
    "code": "PACKAGE_NOT_FOUND",
    "message": "The requested package could not be found",
    "details": {
      "packageId": "nonexistent-package",
      "searchedSources": ["winget", "chocolatey"]
    },
    "timestamp": "2024-11-05T15:45:00Z",
    "requestId": "abc-123-def-456"
  }
}
```

### Error Code Categories

**Structure**: `CATEGORY_SPECIFIC_ERROR`

Examples:
- `VALIDATION_INVALID_FIELD`
- `AUTHENTICATION_TOKEN_EXPIRED`
- `RATE_LIMIT_EXCEEDED`
- `PACKAGE_NOT_FOUND`
- `PACKAGE_VERSION_CONFLICT`

### Field Validation Errors

For multiple validation errors:

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Request validation failed",
    "errors": [
      {
        "field": "name",
        "message": "Package name is required",
        "code": "REQUIRED_FIELD"
      },
      {
        "field": "version",
        "message": "Version must follow semantic versioning",
        "code": "INVALID_FORMAT"
      }
    ]
  }
}
```

### HTTP Status Code to Error Code Mapping

| HTTP Status | Error Code Example | Scenario |
|-------------|-------------------|----------|
| 400 | VALIDATION_ERROR | Invalid input |
| 401 | AUTHENTICATION_REQUIRED | Missing auth |
| 403 | PERMISSION_DENIED | No access rights |
| 404 | RESOURCE_NOT_FOUND | Resource missing |
| 409 | CONFLICT | Duplicate resource |
| 429 | RATE_LIMIT_EXCEEDED | Too many requests |
| 500 | INTERNAL_ERROR | Server error |
| 503 | SERVICE_UNAVAILABLE | Maintenance mode |

---

## Authentication and Authorization

### Authentication Methods

#### 1. API Keys

Simple but less secure:

```http
GET /api/v1/packages
X-API-Key: your-api-key-here
```

#### 2. Bearer Tokens (Recommended)

OAuth 2.0 Bearer tokens:

```http
GET /api/v1/packages
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### 3. Basic Authentication

For simple scenarios:

```http
GET /api/v1/packages
Authorization: Basic dXNlcm5hbWU6cGFzc3dvcmQ=
```

**Note**: Always use HTTPS with authentication.

#### 4. OAuth 2.0

Industry standard for authorization:

**Authorization Flow:**
```
1. Client requests authorization
2. User grants permission
3. Client receives authorization code
4. Client exchanges code for access token
5. Client uses access token for API requests
```

### Authorization Patterns

#### Role-Based Access Control (RBAC)

```csharp
[Authorize(Roles = "Admin,PackageManager")]
public IActionResult UpdatePackage(string id)
{
    // Only users with Admin or PackageManager role can access
}
```

#### Scope-Based Authorization

```http
GET /api/v1/packages
Authorization: Bearer {token}

# Token includes scopes:
{
  "scopes": ["packages:read", "packages:write"]
}
```

#### Resource-Level Authorization

```csharp
public IActionResult GetPackage(string id)
{
    var package = _repository.Get(id);
    if (!User.CanAccess(package))
    {
        return Forbid();
    }
    return Ok(package);
}
```

### Security Best Practices

1. **Always use HTTPS** for API communications
2. **Never expose credentials** in URLs or logs
3. **Implement token expiration** and refresh mechanisms
4. **Use short-lived tokens** (15-60 minutes)
5. **Validate tokens** on every request
6. **Implement rate limiting** per user/token
7. **Log authentication failures** for security monitoring

**Example from UniGetUI (Telemetry API):**
```csharp
request.Headers.Add("clientId", ID);
request.Headers.Add("clientVersion", CoreData.VersionName);
// Simple client identification without exposing sensitive data
```

---

## Rate Limiting and Throttling

Rate limiting protects your API from abuse and ensures fair usage.

### Rate Limit Headers

Include rate limit information in response headers:

```http
HTTP/1.1 200 OK
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 987
X-RateLimit-Reset: 1604447200
X-RateLimit-Retry-After: 3600
```

### Rate Limit Response

When limit is exceeded:

```http
HTTP/1.1 429 Too Many Requests
Retry-After: 3600
Content-Type: application/json

{
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "API rate limit exceeded",
    "details": {
      "limit": 1000,
      "remaining": 0,
      "resetAt": "2024-11-05T16:45:00Z"
    }
  }
}
```

### Rate Limiting Strategies

#### 1. Fixed Window

Reset counter at fixed intervals (e.g., every hour):

```
Time:     00:00  00:30  01:00  01:30  02:00
Requests: 1000   1000   1000   1000   1000
Reset:    -------|-------|-------|-------
```

**Pros**: Simple to implement
**Cons**: Burst at window boundaries

#### 2. Sliding Window

Count requests in last N seconds:

```
Current time: 00:45
Count requests from 00:00 to 00:45
```

**Pros**: Smoother, prevents boundary bursts
**Cons**: More complex

#### 3. Token Bucket

Tokens added at fixed rate, consumed per request:

```
Bucket capacity: 100 tokens
Refill rate: 10 tokens/second
Cost per request: 1 token
```

**Pros**: Allows bursts, smooth long-term
**Cons**: More complex to implement

#### 4. Leaky Bucket

Requests processed at constant rate:

```
Queue capacity: 100 requests
Processing rate: 10 requests/second
```

**Pros**: Smooth output rate
**Cons**: May delay requests

### Rate Limit Tiers

Different limits for different users:

| Tier | Requests/Hour | Burst |
|------|--------------|-------|
| Anonymous | 100 | 10 |
| Free | 1,000 | 50 |
| Pro | 10,000 | 200 |
| Enterprise | 100,000 | 1,000 |

### Implementation Example

```csharp
public class RateLimitMiddleware
{
    private readonly Dictionary<string, RateLimitInfo> _clients = new();
    
    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientId(context);
        var limit = GetLimitForClient(clientId);
        
        if (IsRateLimitExceeded(clientId, limit))
        {
            context.Response.StatusCode = 429;
            context.Response.Headers.Add("Retry-After", "3600");
            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = "RATE_LIMIT_EXCEEDED",
                    message = "API rate limit exceeded"
                }
            });
            return;
        }
        
        IncrementRequestCount(clientId);
        await _next(context);
    }
}
```

---

## Caching

Caching improves performance and reduces server load.

### Cache Control Headers

```http
# Cache for 1 hour
Cache-Control: public, max-age=3600

# No caching
Cache-Control: no-store

# Cache but revalidate
Cache-Control: must-revalidate, max-age=3600

# Private (user-specific)
Cache-Control: private, max-age=1800
```

### ETag and Conditional Requests

**First Request:**
```http
GET /api/v1/packages/chrome
```

**Response:**
```http
HTTP/1.1 200 OK
ETag: "33a64df551425fcc55e4d42a148795d9f25f89d4"
Cache-Control: max-age=3600

{...}
```

**Subsequent Request:**
```http
GET /api/v1/packages/chrome
If-None-Match: "33a64df551425fcc55e4d42a148795d9f25f89d4"
```

**Response (Not Modified):**
```http
HTTP/1.1 304 Not Modified
ETag: "33a64df551425fcc55e4d42a148795d9f25f89d4"
```

### Last-Modified

```http
GET /api/v1/packages/chrome
If-Modified-Since: Wed, 05 Nov 2024 15:00:00 GMT
```

**Response:**
```http
HTTP/1.1 304 Not Modified
Last-Modified: Wed, 05 Nov 2024 15:00:00 GMT
```

### Caching Strategies

1. **Static Resources**: Long cache (days/months)
2. **User-Specific Data**: Private cache (minutes)
3. **Real-Time Data**: No cache or very short (seconds)
4. **Lists/Collections**: Medium cache with revalidation

---

## Pagination

For large datasets, implement pagination.

### Offset-Based Pagination

```http
GET /api/v1/packages?offset=0&limit=20
```

**Response:**
```json
{
  "data": [...],
  "pagination": {
    "offset": 0,
    "limit": 20,
    "total": 1000
  },
  "links": {
    "next": "/api/v1/packages?offset=20&limit=20"
  }
}
```

**Pros**: Simple, allows jumping to specific page
**Cons**: Performance issues with large offsets, inconsistent with live data

### Cursor-Based Pagination (Recommended)

```http
GET /api/v1/packages?cursor=xyz123&limit=20
```

**Response:**
```json
{
  "data": [...],
  "pagination": {
    "limit": 20,
    "nextCursor": "abc456",
    "hasMore": true
  },
  "links": {
    "next": "/api/v1/packages?cursor=abc456&limit=20"
  }
}
```

**Pros**: Consistent with live data, better performance
**Cons**: Can't jump to specific page

### Page-Based Pagination

```http
GET /api/v1/packages?page=1&pageSize=20
```

**Response:**
```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "total": 1000,
    "totalPages": 50
  },
  "links": {
    "first": "/api/v1/packages?page=1&pageSize=20",
    "prev": null,
    "self": "/api/v1/packages?page=1&pageSize=20",
    "next": "/api/v1/packages?page=2&pageSize=20",
    "last": "/api/v1/packages?page=50&pageSize=20"
  }
}
```

### Example from UniGetUI (NuGet API)

```csharp
// NuGet OData API uses $skip and $top for pagination
Uri SearchUrl = new($"{source.Url}/Search()" +
    $"?$filter=IsLatestVersion" +
    $"&$skip=0" +
    $"&$top=50");
```

---

## Filtering and Searching

### Query Parameters for Filtering

```http
GET /api/v1/packages?status=active&category=browsers&minVersion=100
```

### Search Parameter

```http
GET /api/v1/packages?search=chrome
```

### Sorting

```http
GET /api/v1/packages?sort=name:asc
GET /api/v1/packages?sort=downloads:desc
GET /api/v1/packages?sort=name:asc,version:desc
```

### Field Selection (Sparse Fields)

Reduce response size by selecting specific fields:

```http
GET /api/v1/packages?fields=id,name,version
```

### Complex Filtering

For complex queries, consider:

**OData-style:**
```http
GET /api/v1/packages?$filter=downloads gt 1000 and category eq 'browsers'
```

**GraphQL-style (for complex queries):**
```graphql
query {
  packages(
    where: {
      downloads: { gt: 1000 }
      category: { eq: "browsers" }
    }
  ) {
    id
    name
    downloads
  }
}
```

---

## API Documentation

### OpenAPI/Swagger

Use OpenAPI Specification (OAS) for API documentation.

**Example `openapi.yaml`:**
```yaml
openapi: 3.0.0
info:
  title: Package Manager API
  version: 1.0.0
  description: API for managing software packages
  
servers:
  - url: https://api.example.com/v1
    description: Production server
    
paths:
  /packages:
    get:
      summary: List packages
      description: Retrieve a paginated list of packages
      parameters:
        - name: page
          in: query
          description: Page number
          schema:
            type: integer
            default: 1
        - name: pageSize
          in: query
          description: Number of items per page
          schema:
            type: integer
            default: 20
            maximum: 100
      responses:
        '200':
          description: Successful response
          content:
            application/json:
              schema:
                type: object
                properties:
                  data:
                    type: array
                    items:
                      $ref: '#/components/schemas/Package'
                  pagination:
                    $ref: '#/components/schemas/Pagination'
        '400':
          description: Bad request
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Error'
                
components:
  schemas:
    Package:
      type: object
      required:
        - id
        - name
        - version
      properties:
        id:
          type: string
          example: chrome
        name:
          type: string
          example: Google Chrome
        version:
          type: string
          example: 120.0.0
        description:
          type: string
          example: Web browser
          
    Pagination:
      type: object
      properties:
        page:
          type: integer
        pageSize:
          type: integer
        total:
          type: integer
        totalPages:
          type: integer
          
    Error:
      type: object
      properties:
        error:
          type: object
          properties:
            code:
              type: string
            message:
              type: string
```

### Documentation Best Practices

1. **Keep it Updated**: Documentation should match implementation
2. **Include Examples**: Provide request/response examples
3. **Error Scenarios**: Document all possible error responses
4. **Authentication**: Clearly explain auth requirements
5. **Rate Limits**: Document rate limiting policies
6. **Changelog**: Maintain version changelog
7. **Getting Started**: Include quick start guide
8. **SDKs**: Provide client libraries when possible

### Interactive Documentation

Use tools like:
- **Swagger UI**: Interactive API exploration
- **ReDoc**: Clean, responsive documentation
- **Postman Collections**: Shareable API collections

---

## Additional Best Practices

### 1. Use HTTPS Everywhere

Always use HTTPS for API endpoints to ensure:
- Data encryption in transit
- Authentication security
- Data integrity

### 2. Implement Proper Logging

Log important events without exposing sensitive data:

```csharp
// Good
Logger.Info($"Package search completed: query='{query}', results={count}");

// Bad - exposes sensitive data
Logger.Info($"User API key: {apiKey}");
```

### 3. Validate Input

Always validate and sanitize input:

```csharp
public IActionResult GetPackage(string id)
{
    if (string.IsNullOrWhiteSpace(id))
    {
        return BadRequest("Package ID is required");
    }
    
    if (!IsValidPackageId(id))
    {
        return BadRequest("Invalid package ID format");
    }
    
    // Process request
}
```

### 4. Use Asynchronous Operations

For I/O-bound operations:

```csharp
public async Task<IActionResult> GetPackagesAsync()
{
    var packages = await _repository.GetPackagesAsync();
    return Ok(packages);
}
```

### 5. Health Check Endpoints

Provide health check endpoints for monitoring:

```http
GET /health
```

**Response:**
```json
{
  "status": "healthy",
  "version": "1.0.0",
  "timestamp": "2024-11-05T15:45:00Z",
  "checks": {
    "database": "healthy",
    "cache": "healthy",
    "externalApi": "degraded"
  }
}
```

### 6. CORS Configuration

Configure CORS appropriately:

```csharp
services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins("https://example.com")
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});
```

### 7. Request/Response Compression

Enable compression for better performance:

```csharp
services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});
```

---

## References

- [Microsoft REST API Guidelines](https://github.com/microsoft/api-guidelines)
- [RESTful Web Services](https://restfulapi.net/)
- [OpenAPI Specification](https://swagger.io/specification/)
- [HTTP Status Code Definitions](https://httpstatuses.com/)
- [OAuth 2.0](https://oauth.net/2/)
- [JSON API Specification](https://jsonapi.org/)

---

## Related Documentation

- [Integration Patterns](./integration-patterns.md)
- [HTTP Client Best Practices](./http-client-best-practices.md)
- [External API Integrations](../codebase-analysis/05-integration/external-apis.md)
