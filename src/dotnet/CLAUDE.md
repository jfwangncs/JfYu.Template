# CLAUDE.md

This file provides guidance to AI coding assistants when working with the `src/dotnet` project.

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

## Project Structure

```text
WebApi/
  Program.cs                     # App entry point; registers all extensions
  WebApi.csproj                  # Package references (feature-gated via <!--#if-->)
  appsettings.json               # Base config (feature-gated sections via //#if)
  nlog.config                    # NLog logging configuration

  Authorization/
    PermissionHandler.cs         # Custom AuthorizationHandler (currently scaffolded/commented)

  Constants/
    ResponseCode.cs              # Enum: Success, Failed, ...
    ErrorCode.cs                 # Enum for business error codes (has [Description] attributes)
    Gender.cs                    # Gender enum
    PlatformEnum.cs              # Login platform enum (Normal, Wechat)

  Controllers/
    CustomController.cs          # Base controller — wraps Ok()/BadRequest() in BaseResponse<T>
    AuthController.cs            # Login endpoint
    UserController.cs            # User CRUD (Admin-only)
    RoleController.cs            # Role CRUD

  Entity/
    AppDbContext.cs              # EF Core DbContext (Users, Roles, Permissions)
    User.cs / Role.cs / Permission.cs

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
    Request/                     # Request DTOs (CreateXxxRequest, UpdateXxxRequest, QueryRequest)
    Response/                    # Response DTOs (XxxResponse)

  Options/
    JwtSettings.cs               # Strongly-typed JWT config

  Services/
    Interfaces/                  # Service interfaces (IUserService, IRoleService, etc.)
    UserService.cs               # Implements Service<User, AppDbContext> from JfYu.Data
    RoleService.cs
    PermissionService.cs
    JwtService.cs                # JWT token generation/validation
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

- `AddAsync`, `UpdateAsync`, `RemoveAsync(predicate)` (soft delete), `HardRemoveAsync(predicate)` (hard delete)
- `GetOneAsync(expression)`, `GetPagedAsync(query)`, `GetAllAsync(expression)`
- Automatic read/write DB context splitting when `ReadonlyDBContext<T>` is registered

### Adding a New Feature

1. **Entity** — add to `Entity/` and register `DbSet<T>` in `AppDbContext`.
2. **Migration** — run `dotnet ef migrations add <Name>` then `dotnet ef database update`.
3. **DTOs** — add `Request/CreateXxxRequest.cs`, `Response/XxxResponse.cs` in `Model/`.
4. **Validation** — add `Validations/CreateXxxRequestValidation.cs` implementing `AbstractValidator<CreateXxxRequest>`.
5. **Service** — add interface in `Services/Interfaces/`, implement in `Services/`.
6. **DI registration** — register the service in `InjectionExtension.AddCustomInjection()`.
7. **Controller** — inherit `CustomController`, inject service, use `Ok<T>()` / `BadRequest(ErrorCode)`.

### Object Mapping

Use Mapster's `Adapt<T>()` for DTO ↔ entity conversion. Custom mapping configurations go in `MapsterExtension.cs`.

### Validation

FluentValidation validators are auto-discovered and registered via `AddCustomFluentValidation()`. Model validation errors are automatically returned as `BaseResponse<Dictionary<string, ModelErrorCollection>>` with `ErrorCode.ValidationError`.

### Authentication & Authorization

- JWT tokens are issued by `JwtService` and validated by `AddCustomAuthentication()`.
- Use `[Authorize]` on controllers/actions; use `[Authorize(Roles = "Admin")]` for role-based access.
- `ICurrentUser` is injectable to get the currently authenticated user's claims.

## Build & Run Commands

```bash
# Restore dependencies
dotnet restore src/dotnet/WebApi/WebApi.csproj

# Build
dotnet build src/dotnet/WebApi/WebApi.csproj

# Run (development)
dotnet run --project src/dotnet/WebApi/WebApi.csproj

# Run unit tests
dotnet test src/dotnet/WebApi.UnitTests/WebApi.UnitTests.csproj

# EF Core migrations
dotnet ef migrations add <MigrationName> --project src/dotnet/WebApi
dotnet ef database update --project src/dotnet/WebApi
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
dotnet user-secrets set "JwtSettings:SecretKey" "<value>" --project src/dotnet/WebApi
```

## Key Conventions

- Enum values should carry `[Description("...")]` attributes; use `.GetDescription()` extension for display text.
- Throw `BusinessException(ErrorCode.XxxError)` from service layer for domain errors; the global exception handler converts these to structured `BadRequest` responses.
- All controller actions return `IActionResult`; never return raw objects or use `ActionResult<T>`.
- Async/await throughout — no `.Result` or `.Wait()`.
- Nullable reference types are enabled (`<Nullable>enable</Nullable>`).
