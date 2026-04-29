---
applyTo: "src/dotnet/**"
---

## Tech Stack

- **Runtime**: .NET 10 / ASP.NET Core 10
- **ORM**: Entity Framework Core via `JfYu.Data` (wraps EF Core with read/write context splitting)
- **Object mapping**: Mapster (`Adapt<T>()`)
- **Validation**: FluentValidation (`AbstractValidator<T>`)
- **Authentication**: JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Password hashing**: BCrypt.Net-Next (`BCrypt.Net.BCrypt.HashPassword / Verify`)
- **Logging**: NLog + `NLog.Web.AspNetCore` (config: `nlog.config`)
- **API docs**: Scalar + ASP.NET Core OpenAPI (`/scalar/v1` in development)
- **API versioning**: `Asp.Versioning.Mvc`
- **HTTP client**: `JfYu.Request`
- **Cache**: `JfYu.Redis` (optional)
- **WeChat Mini Program**: `JfYu.WeChat` (optional)
- **Observability**: OpenTelemetry (optional)

## Optional Feature Flags (Template Conditional Directives)

The project uses `#if` / `<!--#if-->` directives for template-time feature toggling:

| Flag              | Enables                                                           |
| ----------------- | ----------------------------------------------------------------- |
| `EnableJWT`       | JWT authentication, `IJwtService`, `JwtSettings` config           |
| `EnableJWTRedis`  | Redis-backed JWT token store (`JfYu.Redis`)                       |
| `EnableRBAC`      | Role-based access control, EF Core, User/Role/Permission entities |
| `EnableTelemetry` | OpenTelemetry traces, metrics, logs (OTLP exporter)               |
| `EnableWeChat`    | WeChat Mini Program login (`JfYu.WeChat`)                         |

Do **not** remove these directive comments — they are used by the `dotnet new` template engine.

### Template file exclusion rules

**Never wrap an entire file in `//#if ... //#endif`** — if a feature is disabled the file body becomes empty but the file still exists, which is confusing.

Instead, use **two complementary techniques**:

1. **Whole-file exclusion via `template.json` modifiers** — for files that only exist when a flag is enabled:
   - Add the file path to the `"exclude"` array of the appropriate `modifiers` entry in `src/dotnet/content/.template.config/template.json`.
   - This completely removes the file when the template is instantiated without that flag.

2. **Inline `//#if` blocks** — only for files that exist regardless of flags but contain some flag-specific code (e.g., `InjectionExtension.cs`, `Program.cs`, `appsettings.json`).

**Decision rule**: If a file has *no content at all* when a flag is disabled, it belongs in `template.json` excludes, not in an `#if` wrapper.

**Naming conflicts with StackExchange.Redis.RedisKey**: When the project's `Constants.RedisKey` and `StackExchange.Redis.RedisKey` are both in scope, add a using alias:
```csharp
using AppRedisKey = JfYu.WebApi.Template.Constants.RedisKey;
```

## Project Structure

```text
WebApi/
  Program.cs                     # App entry point; registers all extensions
  WebApi.csproj                  # Package references (feature-gated via <!--#if-->)
  appsettings.json               # Base config (feature-gated sections via //#if)
  nlog.config                    # NLog logging configuration

  Authorization/
    PermissionHandler.cs         # Custom AuthorizationHandler

  Constants/
    ResponseCode.cs              # Enum: Success, Failed, ...
    ErrorCode.cs                 # Enum for business error codes (has [Description] attributes)
    PermissionCodes.cs           # Permission code constants (EnableRBAC only)
    PermissionType.cs            # Permission type enum (EnableRBAC only)
    RedisKey.cs                  # Redis key constants (EnableJWTRedis only)
    Gender.cs                    # Gender enum
    PlatformEnum.cs              # Login platform enum (Normal, Wechat)

  Controllers/
    CustomController.cs          # Base controller — wraps Ok()/BadRequest() in BaseResponse<T>
    AuthController.cs            # Login endpoint
    UserController.cs            # User CRUD (Admin-only)
    RoleController.cs            # Role CRUD
    RedisCacheController.cs      # Redis key management (EnableJWTRedis only)

  Entity/
    AppDbContext.cs              # EF Core DbContext (Users, Roles, Permissions, etc.)
    User.cs / Role.cs / Permission.cs / DictType.cs / DictItem.cs / AuditLog.cs / LoginLog.cs

  Exceptions/
    BusinessException.cs         # Thrown by services for domain errors

  Extensions/
    ServicesExtension.cs         # All AddCustomXxx() extension methods
    InjectionExtension.cs        # AddCustomInjection() — service DI registrations
    OptionsExtension.cs          # AddCustomOptions() — binds IOptions<T>
    MapsterExtension.cs          # AddMapster() — Mapster configuration
    EnumExtensions.cs            # GetDescription() for enums

  Migrations/                    # EF Core migrations — do NOT edit manually

  Model/
    BaseResponse.cs              # Generic API response envelope
    PagedResult.cs               # Pagination wrapper
    <Feature>/                   # One subdirectory per feature module (e.g. Model/Product/)
      Create<Name>Request.cs
      Update<Name>Request.cs
      <Name>Response.cs

  Options/
    JwtSettings.cs               # Strongly-typed JWT config

  Services/
    Interfaces/                  # Service interfaces (IUserService, IRoleService, IRedisCacheService, etc.)
    UserService.cs               # Implements Service<User, AppDbContext> from JfYu.Data
    RoleService.cs
    PermissionService.cs
    JwtService.cs                # JWT token generation/validation
    RedisCacheService.cs         # Redis key SCAN + delete (EnableJWTRedis only)
    CurrentUser.cs               # ICurrentUser — resolves user from HttpContext claims

  Validations/
    CreateUserRequestValidation.cs
    UpdateUserRequestValidation.cs
```

## Architecture Patterns

### Response Envelope

All API responses are wrapped in `BaseResponse<T>`:

```csharp
public class BaseResponse<T>
{
    public ResponseCode Code { get; set; }   // Success | Failed
    public string Message { get; set; }
    public ErrorCode? ErrorCode { get; set; }
    public T? Data { get; set; }
}
```

Controllers inherit from `CustomController` (not `ControllerBase` directly) which provides `Ok<T>(data)` and `BadRequest(ErrorCode)` helpers that automatically wrap results.

### Service Layer

Services extend `Service<TEntity, TDbContext>` from `JfYu.Data` which provides:

- `AddAsync`, `UpdateAsync`, `DeleteAsync`
- `GetOneAsync(expression)`, `GetPagedAsync(query)`, `GetAllAsync(expression)`
- Automatic read/write DB context splitting when `ReadonlyDBContext<T>` is registered

### Redis Cache Invalidation

When a service operation changes data that is cached in Redis (e.g. user permissions), invalidate the cache immediately after the DB write. Inject `IRedisService` and call:
- `_redisService.RemoveAsync(key)` — single key
- `_redisService.RemoveAllAsync(List<string> keys)` — batch keys

Wrap the injection and calls in `//#if (EnableJWTRedis)` blocks. Example in `RoleService.AssignPermissionsAsync`:
```csharp
//#if (EnableJWTRedis)
var cacheKeys = affectedUserIds.Select(id => string.Format(AppRedisKey.UserPermission, id)).ToList();
await _redisService.RemoveAllAsync(cacheKeys);
//#endif
```

### Adding a New Feature

1. **Entity** — add to `Entity/` and register `DbSet<T>` in `AppDbContext`.
2. **Migration** — run `dotnet ef migrations add <Name>` then `dotnet ef database update`.
3. **DTOs** — create a `Model/<Name>/` subdirectory with `Create<Name>Request.cs`, `<Name>Response.cs`, etc.
4. **Validation** — add `Validations/Create<Name>RequestValidation.cs` implementing `AbstractValidator<CreateXxxRequest>`.
5. **Service** — add interface in `Services/Interfaces/`, implement in `Services/`.
6. **DI registration** — register the service in `InjectionExtension.AddCustomInjection()` inside the appropriate `#if` block.
7. **Controller** — inherit `CustomController`, inject service, use `Ok<T>()` / `BadRequest(ErrorCode)`.
8. **Permission codes** — add to `PermissionCodes.cs` inside `#if (EnableRBAC)` if the module is RBAC-gated.
9. **template.json** — add all new feature-only files to the appropriate `(!EnableXxx)` modifier's exclude list.

### Object Mapping

Use Mapster's `Adapt<T>()` for DTO ↔ entity conversion. Custom mapping configurations go in `MapsterExtension.cs`.

### Validation

FluentValidation validators are auto-discovered and registered via `AddCustomFluentValidation()`. Model validation errors are automatically returned as `BaseResponse<Dictionary<string, ModelErrorCollection>>` with `ErrorCode.ValidationError`.

### Authentication & Authorization

- JWT tokens are issued by `JwtService` and validated by `AddCustomAuthentication()`.
- Use `[Permission(PermissionCodes.Xxx)]` on controllers/actions — do **not** use `[Authorize]` directly.
- `ICurrentUser` is injectable to get the currently authenticated user's claims.
- `PermissionAttribute` checks permissions from Redis (if `EnableJWTRedis`) or from the JWT token claims.

## Build & Run Commands

```bash
# Restore dependencies
dotnet restore src/dotnet/content/JfYu.WebApi.Template/JfYu.WebApi.Template.csproj

# Build
dotnet build src/dotnet/content/JfYu.WebApi.Template/JfYu.WebApi.Template.csproj

# Run (development)
dotnet run --project src/dotnet/content/JfYu.WebApi.Template

# Run unit tests
dotnet test src/dotnet/content/JfYu.WebApi.Template.UnitTests/JfYu.WebApi.Template.UnitTests.csproj

# EF Core migrations
dotnet ef migrations add <MigrationName> --project src/dotnet/content/JfYu.WebApi.Template
dotnet ef database update --project src/dotnet/content/JfYu.WebApi.Template
```

## Configuration

`appsettings.json` (and `appsettings.Development.json` for overrides):

| Section                               | Purpose                                        |
| ------------------------------------- | ---------------------------------------------- |
| `JwtSettings`                         | SecretKey, Issuer, Audience, Expires (seconds) |
| `ConnectionStrings.DefaultConnection` | EF Core DB connection string                   |
| `Redis`                               | Host, Port, Password, DbIndex, Timeout, Ssl    |
| `MiniProgram`                         | WeChat AppId + Secret                          |

Sensitive values should use **user-secrets** in development:

```bash
dotnet user-secrets set "JwtSettings:SecretKey" "<value>" --project src/dotnet/content/JfYu.WebApi.Template
```

## Key Conventions

- Enum values should carry `[Description("...")]` attributes; use `.GetDescription()` extension for display text.
- Throw `BusinessException(ErrorCode.XxxError)` from service layer for domain errors; the global exception handler converts these to structured `BadRequest` responses.
- All controller actions return `IActionResult`; never return raw objects or use `ActionResult<T>`.
- Async/await throughout — no `.Result` or `.Wait()`.
- Nullable reference types are enabled (`<Nullable>enable</Nullable>`).
- Model DTOs go in `Model/<FeatureName>/` subdirectories — not in generic `Model/Request/` or `Model/Response/` folders.

