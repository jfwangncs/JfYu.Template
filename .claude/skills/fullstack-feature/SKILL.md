---
name: fullstack-feature
description: "Implement a complete full-stack feature module (backend + frontend). Use when adding a new entity, CRUD endpoints, service, or any module in src/dotnet AND the corresponding Vue frontend. Covers: DB schema design with user review, Entity + AppDbContext, Request/Response DTOs, FluentValidation, ErrorCode assignment (4000+ range), Options config, Service interface + implementation (JfYu.Data), DI registration, Controller (CustomController), Vue router, API file (requestClient), VxeGrid list page, drawer form, and i18n locales (zh-CN + en-US error/system/page JSON). Follow project conventions end-to-end."
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

Create a **module folder** under `src/dotnet/WebApi/Model/<Name>/` and place all DTOs there:

| File                                   | Purpose                                       |
| -------------------------------------- | --------------------------------------------- |
| `Model/<Name>/Create<Name>Request.cs`  | Fields for creation                           |
| `Model/<Name>/Update<Name>Request.cs`  | Fields for update (Id + changeable fields)    |
| `Model/<Name>/Feature<Name>Request.cs` | Fields for business logic (if needed)         |
| `Model/<Name>/<Name>Response.cs`       | Fields returned to client (no sensitive data) |

Example for a `Product` module:

```
Model/
  Product/
    CreateProductRequest.cs
    UpdateProductRequest.cs
    ProductResponse.cs
```

Namespace follows the folder: `WebApi.Model.Product`.

Use plain C# classes — no inheritance needed. Nullable optional fields.

---

## Step 4 — Create Validations

Create validation files in `src/dotnet/WebApi/Validations/` for each request DTO:

- `Create<Name>RequestValidation.cs`
- `Update<Name>RequestValidation.cs`
- `Feature<Name>RequestValidation.cs` (if applicable)

```csharp
using FluentValidation;
using WebApi.Model.Product;

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

Use the module namespace `WebApi.Model.<Name>` matching the DTO folder.

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
| 4250–4299 | Permission            |
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
    Task<PagedResult<ProductResponse>> GetPagedAsync(QueryRequest query);
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

    public async Task<PagedResult<ProductResponse>> GetPagedAsync(QueryRequest query)
    {
        var q = _readonlyContext.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SearchKey))
            q = q.Where(p => p.Name.Contains(query.SearchKey));

        if (query.Status.HasValue)
            q = q.Where(p => p.Status == query.Status.Value);

        if (query.StartTime.HasValue)
            q = q.Where(p => p.CreatedTime >= query.StartTime.Value);

        if (query.EndTime.HasValue)
            q = q.Where(p => p.CreatedTime <= query.EndTime.Value);

        var paged = await q.ToPagedAsync(q => q.Adapt<IEnumerable<ProductResponse>>(), query.PageIndex, query.PageSize);
        return new PagedResult<ProductResponse>
        {
            Items = paged.Data?.ToList() ?? [],
            Total = paged.TotalCount
        };
    }
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

## Step 9 — Define Permission Codes

Open `src/dotnet/WebApi/Constants/PermissionCodes.cs` and add constants for the new module.

**Convention**:

- Module entry (Menu) → `public const string <Name> = "<name>";`
- Each action → `public const string <Name><Action> = "<Name>:<action>";`

```csharp
public const string Product = "Product";
public const string ProductGet = "Product:get";
public const string ProductAdd = "Product:add";
public const string ProductEdit = "Product:edit";
public const string ProductDelete = "Product:delete";
```

**Parent code** determines where this module appears in the permission tree:

- Check existing top-level constants (e.g., `System = "system"`, `Dashboard = "dashboard"`).
- If the module clearly belongs to an existing parent → use it directly.
- If uncertain or no suitable parent exists → **ask the user** before proceeding.

> **⏸ CHECKPOINT — Confirm parent code with user if not obvious.**
> Ask: "This module's menu permission will be placed under `PermissionCodes.System`. Is that correct, or should it be under a different parent?"

---

## Step 10 — Create Controller

Create `Controllers/<Name>Controller.cs`.

- The **class** gets `[Permission(PermissionCodes.<Name>, PermissionType.Menu, parentCode: PermissionCodes.<Parent>)]` — this registers it as a menu entry.
- Each **action method** gets `[Permission(PermissionCodes.<Name><Action>)]` — type defaults to `Button`.

```csharp
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WebApi.Attributes;
using WebApi.Constants;
using WebApi.Model.Product;
using WebApi.Services.Interfaces;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Permission(PermissionCodes.Product, PermissionType.Menu, parentCode: PermissionCodes.System)]
    public class ProductController(IProductService productService) : CustomController
    {
        private readonly IProductService _productService = productService;

        [HttpGet]
        [Permission(PermissionCodes.ProductGet)]
        public async Task<IActionResult> GetAllAsync([FromQuery] QueryRequest query)
        {
            var result = await _productService.GetPagedAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Permission(PermissionCodes.ProductGet)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var item = await _productService.GetOneAsync(q => q.Id == id);
            if (item == null)
                return BadRequest(ErrorCode.ProductNotFound);
            return Ok(item);
        }

        [HttpPost]
        [Permission(PermissionCodes.ProductAdd)]
        public async Task<IActionResult> CreateAsync([FromBody][Required] CreateProductRequest request)
        {
            // check duplicates, adapt, save
            await _productService.AddAsync(item);
            return Ok();
        }

        [HttpPut("{id}")]
        [Permission(PermissionCodes.ProductEdit)]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody][Required] UpdateProductRequest request)
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
- Do NOT use `[Authorize]` directly — `[Permission]` handles authentication + authorization.
- All methods must be async — no `.Result` or `.Wait()`.
- Add `[Required]` on `[FromBody]` parameters.

---

## Step 11 — Frontend: Router

Check `src/vue/apps/web-antd/src/router/routes/modules/`.

- **Existing domain file** (e.g., `system.ts`) → add a child route entry.
- **New domain** → create `<domain>.ts` and add its default export to `src/router/routes/index.ts`.

Route pattern:

```ts
{
  name: 'ProductManagement',           // PascalCase, unique
  path: '/system/product',
  component: () => import('#/views/system/product/index.vue'),
  meta: {
    icon: 'lucide:package',            // Lucide icon name
    title: $t('page.system.product'),  // i18n key
  },
},
```

---

## Step 12 — Frontend: API File

Create `src/vue/apps/web-antd/src/api/system/<domain>.ts`:

> **Note**: API files for features live in `src/api/system/`, **not** `src/api/core/`.
> Export chain: `src/api/system/<domain>.ts` → `src/api/system/index.ts` → `src/api/core/index.ts` → `src/api/index.ts`

```ts
import { requestClient } from "#/api/request";

export namespace SystemProductApi {
  export interface SystemProduct {
    id: number;
    name: string;
    price: number;
    status: number;
    createdTime: string;
  }

  export interface CreateProductParams {
    name: string;
    price: number;
  }

  export interface UpdateProductParams {
    name?: string;
    price?: number;
    status?: number;
  }
}

export async function getProductList(params: Record<string, any>) {
  return requestClient.get<{
    items: SystemProductApi.SystemProduct[];
    total: number;
  }>("/product", { params });
}

export async function createProduct(
  data: SystemProductApi.CreateProductParams,
) {
  return requestClient.post("/product", data);
}

export async function updateProduct(
  id: number,
  data: SystemProductApi.UpdateProductParams,
) {
  return requestClient.put(`/product/${id}`, data);
}

export async function deleteProduct(id: number) {
  return requestClient.delete(`/product/${id}`);
}
```

Then re-export from `src/api/system/index.ts`:

```ts
export * from "./<domain>";
```

---

## Step 13 — Frontend: Views (based on Role module template)

Create three files mirroring the role module structure:

### `src/views/<domain>/<name>/data.ts`

Defines form schemas and column configs using `$t()` for all labels:

```ts
import type { VbenFormSchema } from "#/adapter/form";
import type { OnActionClickFn, VxeTableGridOptions } from "#/adapter/vxe-table";
import { $t } from "#/locales";

export function useFormSchema(): VbenFormSchema[] {
  return [
    {
      component: "Input",
      fieldName: "name",
      label: $t("system.product.name"),
      rules: "required",
    },
    // ...
  ];
}

export function useGridFormSchema(): VbenFormSchema[] {
  /* search bar schema */
}

export function useColumns(
  onActionClick: OnActionClickFn,
  onStatusChange: Function,
): VxeTableGridOptions["columns"] {
  /* table columns */
}
```

### `src/views/<domain>/<name>/modules/form.vue`

Drawer form for create/edit — uses `useVbenForm` + `useVbenDrawer`:

```vue
<script lang="ts" setup>
import { ref } from "vue";
import { useVbenForm, useVbenDrawer } from "@vben/common-ui";
import { createProduct, updateProduct } from "#/api";
import { useFormSchema } from "../data";

const emits = defineEmits(["success"]);
const id = ref<number>();

const [Form, formApi] = useVbenForm({
  schema: useFormSchema(),
  showDefaultActions: false,
});
const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (!valid) return;
    const values = await formApi.getValues();
    drawerApi.lock();
    (id.value ? updateProduct(id.value, values) : createProduct(values))
      .then(() => {
        emits("success");
        drawerApi.close();
      })
      .catch(() => drawerApi.unlock());
  },
  async onOpenChange(isOpen) {
    if (!isOpen) return;
    formApi.resetForm();
    const data = drawerApi.getData<SystemProductApi.SystemProduct>();
    id.value = data?.id;
    if (data) {
      await nextTick();
      formApi.setValues(data);
    }
  },
});
</script>
```

### `src/views/<domain>/<name>/index.vue`

Main list page — uses `useVbenVxeGrid` with proxy config pointing to the list API:

> **Critical**: Always use `<Page auto-content-height>` on list pages that use `height: 'auto'` on VxeGrid.
> Without `auto-content-height`, the grid has no fixed-height parent and enters an infinite ResizeObserver
> feedback loop, causing `vxe-table--row-expanded-wrapper` height to grow endlessly.

> **Icons**: Use named Lucide icon components from `@vben/icons` (e.g. `RotateCw`, `Plus`).
> Do NOT import from `@ant-design/icons-vue` — that package is not available in this project.

```vue
<script lang="ts" setup>
import { Page, useVbenDrawer } from "@vben/common-ui";
import { Plus } from "@vben/icons";
import { Button } from "ant-design-vue";
import { useVbenVxeGrid } from "#/adapter/vxe-table";
import { getProductList } from "#/api";
import { useColumns, useGridFormSchema } from "./data";
import Form from "./modules/form.vue";

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});
const [Grid, gridApi] = useVbenVxeGrid({
  gridOptions: {
    columns: useColumns(onActionClick, onStatusChange),
    height: "auto",
    keepSource: true,
    proxyConfig: {
      ajax: {
        query: async ({ page }, formValues) =>
          getProductList({
            pageIndex: page.currentPage,
            pageSize: page.pageSize,
            ...formValues,
          }),
      },
    },
    rowConfig: { keyField: "id" },
    toolbarConfig: {
      custom: true,
      export: false,
      refresh: true,
      search: true,
      zoom: false,
    },
  },
  formOptions: { schema: useGridFormSchema(), submitOnChange: true },
});

function onActionClick(e) {
  if (e.code === "edit") formDrawerApi.setData(e.row).open();
}
function onRefresh() {
  gridApi.query();
}
</script>

<template>
  <Page auto-content-height>
    <FormDrawer @success="onRefresh" />
    <Grid>
      <template #toolbar-actions>
        <Button type="primary" @click="formDrawerApi.open()">
          <Plus class="size-5" />
          {{ $t("common.create") }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
```

---

## Step 14 — Frontend: i18n Locales

Update **all four** locale files:

### Error codes — map each new `ErrorCode` enum value to its numeric int key

`src/locales/langs/zh-CN/error.json`:

```json
{
  "4250": "商品不存在",
  "4251": "商品名称已存在"
}
```

`src/locales/langs/en-US/error.json`:

```json
{
  "4250": "Product not found",
  "4251": "Duplicate product name"
}
```

### UI labels — field names, table headers, drawer titles

`src/locales/langs/zh-CN/system.json` — add module key under the domain object:

```json
{
  "product": {
    "title": "商品管理",
    "list": "商品列表",
    "name": "商品名称",
    "price": "价格",
    "status": "状态",
    "createdTime": "创建时间",
    "operation": "操作"
  }
}
```

`src/locales/langs/en-US/system.json` — English equivalent.

### Page/menu titles (only if new route was added)

`src/locales/langs/zh-CN/page.json` — add to corresponding domain:

```json
{
  "system": {
    "product": "商品管理"
  }
}
```

`src/locales/langs/en-US/page.json` — English equivalent.

---

## Completion Checklist

### Backend

- [ ] DB fields reviewed and approved by user
- [ ] Entity created / modified, registered in `AppDbContext`
- [ ] Migration command reminder given
- [ ] `Create<Name>Request`, `Update<Name>Request`, `<Name>Response` created
- [ ] FluentValidation validators created
- [ ] Error codes added to `ErrorCode.cs` in the correct range/region
- [ ] Options class + `OptionsExtension` registration added (if needed)
- [ ] Service interface + implementation created
- [ ] DI registered in `InjectionExtension`
- [ ] Permission codes added to `PermissionCodes.cs` (module + actions), parent confirmed with user
- [ ] Controller created, inheriting `CustomController`, with `[Permission]` on class and all actions

### Frontend

- [ ] Router entry added (new file or child route in existing domain file)
- [ ] API file created/extended in `src/api/core/`, exported from `index.ts`
- [ ] `data.ts` created with `useFormSchema`, `useGridFormSchema`, `useColumns`
- [ ] `modules/form.vue` created (drawer create/edit form)
- [ ] `index.vue` created (list page with VxeGrid)
- [ ] `error.json` updated in both `zh-CN` and `en-US` with all new error codes
- [ ] `system.json` updated in both `zh-CN` and `en-US` with all UI labels
- [ ] `page.json` updated in both locales if a new route/menu title was added
