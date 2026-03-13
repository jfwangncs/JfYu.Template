---
name: dotnet-backend-feature
description: "Implement a complete backend feature module for the ASP.NET Core 10 WebApi project. Use when adding a new entity, CRUD endpoints, service, or any module in src/dotnet. Covers: DB schema design with user review, Entity + AppDbContext, Request/Response DTOs, FluentValidation, ErrorCode assignment (4000+ range), Options config, Service interface + implementation (JfYu.Data), DI registration, and Controller (CustomController). Follow project conventions end-to-end."
argument-hint: 'Feature module name, e.g. "Product" or "Order management"'
---

# Implement a Backend Feature Module

## Overview

When the user asks to implement a feature in the dotnet backend, follow this workflow **in order**, pausing for user review at marked checkpoints.

---

## Step 1 — Analyze Database Requirements

1. Determine if a **new table** or **modification to an existing table** is needed.
2. List the proposed fields with:
   - Field name (PascalCase)
   - C# type (string, int, decimal, DateTime, bool, enum, …)
   - Constraints: `[Required]`, `[MaxLength(n)]`, nullable `?`, default value
   - Relationships: navigation properties and FK conventions
3. **Note**: `BaseEntity` already provides `Id` (int), `Status`, `CreatedTime`, `UpdatedTime` — do NOT redeclare these.

> **⏸ CHECKPOINT — Present fields to the user for review before coding.**
> Ask: "Are these fields correct? Any changes, additions, or removals?"

---

## Step 2 — Create / Modify Entity

**New entity** → create `src/dotnet/WebApi/Entity/<Name>.cs`:

```csharp
using JfYu.Data.Model;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Entity
{
    public class Product : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        // navigation properties as needed
    }
}
```

**Register in AppDbContext** → add `DbSet<T>` to `Entity/AppDbContext.cs`:

```csharp
public DbSet<Product> Products { get; set; }
```

**Add EF Core migration** (remind the user to run after review):

```bash
dotnet ef migrations add Add<Name> --project src/dotnet/WebApi
dotnet ef database update --project src/dotnet/WebApi
```

---

## Step 3 — Create DTOs

Create files in `src/dotnet/WebApi/Model/`:

| File                              | Purpose                                       |
| --------------------------------- | --------------------------------------------- |
| `Request/Create<Name>Request.cs`  | Fields for creation                           |
| `Request/Update<Name>Request.cs`  | Fields for update (Id + changeable fields)    |
| `Request/Feature<Name>Request.cs` | Fields for business logic                     |
| `Response/<Name>Response.cs`      | Fields returned to client (no sensitive data) |

Use plain C# classes — no inheritance needed. Nullable optional fields.

---

## Step 4 — Create Validations

Create Validations for Requests `src/dotnet/WebApi/Validations/Create<Name>RequestValidation.cs` and `Update<Name>RequestValidation.cs` and `Request/Feature<Name>Request.cs`:

```csharp
using FluentValidation;
using WebApi.Model.Request;

namespace WebApi.Validations
{
    public class CreateProductRequestValidation : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidation()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Price).GreaterThan(0);
        }
    }
}
```

Validators are **auto-discovered** — no manual registration needed.

---

## Step 5 — Assign Error Codes

Open `src/dotnet/WebApi/Constants/ErrorCode.cs`.

**Current module ranges** (4000+ space, 50–100 codes per module):

| Range     | Module                |
| --------- | --------------------- |
| 4000–4049 | General client errors |
| 4100–4149 | Auth                  |
| 4150–4199 | User                  |
| 4200–4249 | Role                  |
| 4300+     | Other / unassigned    |

**Decision logic**:

- Error clearly belongs to an **existing module** → add under that module's `#region`, continuing its sequential values.If it overflows, let me know.
- Error is for a **new module** → start a new `#region` at the next available 50-step boundary (e.g., 4250, 4300 if free) and comment the range.
- Ambiguous → assign to the closest existing module.

**Pattern**:

```csharp
#region Product
[Description("Product not found.")]
ProductNotFound = 4250,

[Description("Duplicate product name.")]
DuplicateProduct,
#endregion
```

---

## Step 6 — Options Configuration (if needed)

Only when the feature requires settings stored in `appsettings.json` (no secrets/passwords).

1. Create `src/dotnet/WebApi/Options/<Name>Settings.cs`:

```csharp
namespace WebApi.Options
{
    public class ProductSettings
    {
        public int MaxItemsPerPage { get; set; } = 50;
    }
}
```

2. Register in `Extensions/OptionsExtension.cs`:

```csharp
services.Configure<ProductSettings>(configuration.GetSection("ProductSettings"));
```

3. Inject via `IOptions<ProductSettings>` in services/controllers.

---

## Step 7 — Create Service Interface + Implementation

### Interface: `Services/Interfaces/I<Name>Service.cs`

- **With DB access** → inherit `IService<TEntity, AppDbContext>`:

```csharp
public interface IProductService : IService<Product, AppDbContext>
{
    Task<PagedData<ProductResponse>> GetPagedAsync(QueryRequest query);
}
```

- **Without DB access** → plain interface, no inheritance.

### Implementation: `Services/<Name>Service.cs`

- **With DB access** → inherit `Service<TEntity, AppDbContext>`:

```csharp
public class ProductService(AppDbContext context, ReadonlyDBContext<AppDbContext> readonlyDBContext)
    : Service<Product, AppDbContext>(context, readonlyDBContext), IProductService
{
    // AddAsync, UpdateAsync, DeleteAsync, GetOneAsync, GetAllAsync are inherited — do NOT re-implement
    // Only add custom business methods
}
```

- **Without DB access** → plain class implementing the interface.

**Business errors** → throw `BusinessException(ErrorCode.XxxError)` — the global handler converts these to structured `BadRequest` responses automatically.
every business error need to mapping to one error code.

---

## Step 8 — Register DI

Add to `Extensions/InjectionExtension.cs` inside `AddCustomInjection()`:

```csharp
//#if (EnableRBAC)   ← wrap in feature flag if DB-related
services.AddScoped<IProductService, ProductService>();
//#endif
```

---

## Step 9 — Create Controller

Create `Controllers/<Name>Controller.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Constants;
using WebApi.Model.Request;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController(IProductService productService) : CustomController
    {
        private readonly IProductService _productService = productService;

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] QueryRequest query)
        {
            var result = await _productService.GetPagedAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var item = await _productService.GetOneAsync(q => q.Id == id);
            if (item == null)
                return BadRequest(ErrorCode.ProductNotFound);
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateProductRequest request)
        {
            // check duplicates, adapt, save
            await _productService.AddAsync(item);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateProductRequest request)
        {
            var item = await _productService.GetOneAsync(q => q.Id == id);
            if (item == null)
                return BadRequest(ErrorCode.ProductNotFound);
            // apply changes
            await _productService.UpdateAsync(item);
            return Ok();
        }
    }
}
```

**Conventions**:

- Always return `IActionResult` — never `ActionResult<T>` or raw objects.
- Use `Ok<T>(data)` / `Ok()` / `BadRequest(ErrorCode)` from `CustomController`.
- Add `[Authorize(Roles = "Admin")]` for admin-only actions.
- All methods must be async — no `.Result` or `.Wait()`.

---

## Completion Checklist

- [ ] DB fields reviewed and approved by user
- [ ] Entity created / modified, registered in `AppDbContext`
- [ ] Migration command reminder given
- [ ] `Create<Name>Request`, `Update<Name>Request`, `<Name>Response` created
- [ ] FluentValidation validators created
- [ ] Error codes added to `ErrorCode.cs` in the correct range/region
- [ ] Options class + `OptionsExtension` registration added (if needed)
- [ ] Service interface + implementation created
- [ ] DI registered in `InjectionExtension`
- [ ] Controller created, inheriting `CustomController`
