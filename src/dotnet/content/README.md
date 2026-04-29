# JfYu WebAPI Template

A production-ready **ASP.NET Core 10** Web API template with modular feature flags for JWT authentication, RBAC (Role-Based Access Control), WeChat Mini Program login, OpenTelemetry observability, Redis caching, and unit testing scaffolding.

## Features

| Feature                               | Flag                | Default |
| ------------------------------------- | ------------------- | ------- |
| JWT authentication                    | `--JwtOption`       | `Basic` |
| Redis-backed JWT token store          | `--JwtOption Redis` | —       |
| Role-Based Access Control (RBAC)      | `--EnableRBAC`      | `false` |
| WeChat Mini Program login             | `--EnableWeChat`    | `true`  |
| OpenTelemetry (traces, metrics, logs) | `--EnableTelemetry` | `true`  |
| Unit test project                     | `--EnableUnitTest`  | `true`  |

### Always included

- API versioning (`Asp.Versioning.Mvc`)
- FluentValidation with auto-discovery
- Mapster object mapping
- NLog logging
- Scalar + ASP.NET Core OpenAPI (`/scalar/v1` in development)
- Global exception handler → structured `BaseResponse<T>` envelope
- BCrypt password hashing

---

## Quick Start for Users

### Step 1 — Install the template

```bash
dotnet new install JfYu.WebApi.Template
```

### Step 2 — Create a new project

**Minimal (no auth):**

```bash
dotnet new JfYuWebApi -n MyProject --JwtOption None --EnableRBAC false --EnableWeChat false --EnableTelemetry false --EnableUnitTest false
```

**JWT only (no user management):**

```bash
dotnet new JfYuWebApi -n MyProject --JwtOption Basic --EnableRBAC false --EnableWeChat false
```

**Full stack (JWT + RBAC + WeChat):**

```bash
dotnet new JfYuWebApi -n MyProject --JwtOption Basic --EnableRBAC true --EnableWeChat true
```

**Full stack with Redis JWT store:**

```bash
dotnet new JfYuWebApi -n MyProject --JwtOption Redis --EnableRBAC true --EnableWeChat true
```

### Step 3 — Restore dependencies

```bash
cd MyProject
dotnet restore
```

### Step 4 — Configure secrets

Never commit secrets. Use user-secrets in development:

```bash
# JWT secret key (required if JWT is enabled)
dotnet user-secrets set "JwtSettings:SecretKey" "your-secret-key-at-least-32-characters" --project WebApi

# Database connection (required if RBAC is enabled)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=MyDb;User=root;Password=secret;" --project WebApi

# Redis (required if JwtOption=Redis)
dotnet user-secrets set "Redis:Password" "your-redis-password" --project WebApi

# WeChat Mini Program (required if WeChat is enabled)
dotnet user-secrets set "MiniProgram:AppId" "wx..." --project WebApi
dotnet user-secrets set "MiniProgram:Secret" "..." --project WebApi
```

### Step 5 — Apply database migrations (RBAC only)

```bash
dotnet ef database update --project WebApi
```

### Step 6 — Run

```bash
dotnet run --project WebApi
```

Open `https://localhost:{port}/scalar/v1` in your browser to explore the API.

---

## Feature Flag Reference

### `--JwtOption`

| Value   | Description                                                   |
| ------- | ------------------------------------------------------------- |
| `None`  | No JWT authentication                                         |
| `Basic` | JWT with in-memory token validation                           |
| `Redis` | JWT with Redis-backed token store (blacklisting / revocation) |

### `--EnableRBAC true`

Enables full role-based access control:

- `User`, `Role`, `Permission` entities with EF Core migrations
- `UserService`, `RoleService`, `PermissionService`
- User CRUD, Role CRUD, Permission management endpoints
- Auto-sync of `[Permission]` attribute metadata to the database on startup
- `AppDbContext` (MySQL via `JfYu.Data`)

**Requires:** `--JwtOption Basic` or `--JwtOption Redis`

### `--EnableWeChat true`

Enables WeChat Mini Program login alongside standard username/password login:

- `JfYu.WeChat` package
- `MiniProgram` config section in `appsettings.json`
- WeChat OAuth flow in `UserService`

**Requires:** `--EnableRBAC true`

### `--EnableTelemetry true`

Adds OpenTelemetry instrumentation:

- ASP.NET Core, HTTP client, EF Core, Redis, RabbitMQ traces (OTLP exporter)
- Prometheus metrics endpoint on `:9464/metrics`
- Log export via OTLP

### `--EnableUnitTest true`

Includes the `WebApi.UnitTests` project with:

- xUnit + Moq + FluentAssertions
- `Microsoft.AspNetCore.Mvc.Testing` for integration tests

---

## Supported Feature Combinations

| JWT         | RBAC  | WeChat | Notes                                     |
| ----------- | ----- | ------ | ----------------------------------------- |
| None        | false | false  | Skeleton API with no auth                 |
| Basic/Redis | false | false  | API with JWT auth, no user management     |
| Basic/Redis | true  | true   | Full stack with WeChat Mini Program login |
| Basic/Redis | true  | false  | Full stack with web-only login            |

---

## Project Structure (generated)

```
WebApi/
  Program.cs                     # App entry point
  WebApi.csproj                  # Feature-gated package references
  appsettings.json               # Feature-gated configuration sections
  nlog.config                    # NLog logging configuration

  Attributes/
    PermissionAttribute.cs       # [Permission] attribute for RBAC endpoint metadata

  Constants/
    ResponseCode.cs              # Success / Failed / Error enum
    ErrorCode.cs                 # Business error codes with [Description]
    Gender.cs                    # Gender enum
    PlatformEnum.cs              # Web / Wechat login platform
    PermissionType.cs            # Menu / Button permission types  (RBAC)
    PermissionCodes.cs           # Constant permission code strings  (RBAC)

  Controllers/
    CustomController.cs          # Base controller — Ok<T>() / BadRequest(ErrorCode)
    AuthController.cs            # POST /auth/login, GET /auth/codes  (JWT + RBAC)
    UserController.cs            # GET/PUT /user  (JWT + RBAC)
    RoleController.cs            # Role CRUD  (JWT + RBAC)
    PermissionController.cs      # Permission CRUD + sync  (JWT + RBAC)

  Entity/
    AppDbContext.cs              # EF Core DbContext  (RBAC)
    User.cs / Role.cs / Permission.cs  (RBAC)

  Extensions/
    ServicesExtension.cs         # All AddCustomXxx() and UseXxx() extensions
    InjectionExtension.cs        # Service DI registrations
    OptionsExtension.cs          # IOptions<T> bindings
    MapsterExtension.cs          # Mapster mapping configuration
    EnumExtensions.cs            # GetDescription() for enums

  Migrations/                    # EF Core migrations  (RBAC)

  Model/
    BaseResponse.cs              # Generic API response envelope
    PagedResult.cs               # Pagination wrapper
    QueryRequest.cs              # Common paged query params
    User/                        # User-related DTOs
    Role/                        # Role-related DTOs  (RBAC)
    Permission/                  # Permission-related DTOs  (RBAC)

  Options/
    JwtSettings.cs               # Strongly-typed JWT config  (JWT)

  Services/
    CurrentUser.cs               # ICurrentUser — reads claims from HttpContext
    JwtService.cs                # JWT generation/validation  (JWT)
    UserService.cs               # User management + login  (RBAC)
    RoleService.cs               # Role management  (RBAC)
    PermissionService.cs         # Permission management + sync  (RBAC)
    Interfaces/                  # Service interfaces

  Validations/
    UpdateUserRequestValidation.cs     (RBAC)
    CreateRoleRequestValidation.cs     (RBAC)
    UpdateRoleRequestValidation.cs     (RBAC)
    CreatePermissionRequestValidation.cs  (RBAC)
    UpdatePermissionRequestValidation.cs  (RBAC)

WebApi.UnitTests/                (EnableUnitTest)
  WebApi.UnitTests.csproj
```

---

## For Template Authors — Publishing to NuGet

### Step 1 — Increment the version

Edit `src/dotnet/JfYu.WebApi.Template.csproj` and update `<PackageVersion>`.

### Step 2 — Pack

```bash
dotnet pack src/dotnet/JfYu.WebApi.Template.csproj -o ./artifacts
```

### Step 3 — Test locally before publishing

```bash
# Install from local nupkg
dotnet new install ./artifacts/JfYu.WebApi.Template.1.0.0.nupkg

# Create a test project
dotnet new JfYuWebApi -n TestProject --EnableRBAC true --EnableWeChat true -o ./test-output

# Verify it builds
dotnet build ./test-output/WebApi/WebApi.csproj

# Uninstall when done testing
dotnet new uninstall JfYu.WebApi.Template
```

### Step 4 — Push to NuGet.org

```bash
dotnet nuget push ./artifacts/JfYu.WebApi.Template.1.0.0.nupkg \
  --api-key <YOUR_NUGET_API_KEY> \
  --source https://api.nuget.org/v3/index.json
```

---

## Development Notes

- Secrets must never be committed — use `dotnet user-secrets` in development.
- Never edit migration files manually (in `WebApi/Migrations/`).
- The `#if` / `<!--#if-->` directive comments in source files are template engine markers — do not remove them.
- Enum values carry `[Description]` attributes; use `.GetDescription()` for display text.
- Throw `BusinessException(ErrorCode.XxxError)` from service layer; the global exception handler converts them to structured `BadRequest` responses.
