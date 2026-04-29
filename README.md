# JfYu.Template

A full-stack application template combining a production-ready **ASP.NET Core 10** Web API backend with a **Vue 3 + TypeScript + Vite** monorepo frontend.

## Repository Layout

```text
src/
  dotnet/                  # .NET 10 backend (see src/dotnet/README.md)
    content/
      JfYu.WebApi.Template/        # Main API project
      JfYu.WebApi.Template.UnitTests/
      JfYu.WebApi.Template.slnx
  vue/                     # Vue 3 monorepo frontend (see src/vue/README.md)
    apps/web-antd/         # Ant Design Vue application
    packages/              # Shared packages (@core, effects, stores, etc.)
    internal/              # Internal tooling (lint, tsconfig, vite-config)
```

## Sub-projects

| Folder       | Stack                                                 | Docs                                         |
| ------------ | ----------------------------------------------------- | -------------------------------------------- |
| `src/dotnet` | ASP.NET Core 10, EF Core, JWT, RBAC, FluentValidation | [src/dotnet/README.md](src/dotnet/README.md) |
| `src/vue`    | Vue 3, TypeScript, Vite, Vben Admin, Ant Design Vue   | [src/vue/README.md](src/vue/README.md)       |

Each sub-project is independent and can be built/run/tested on its own.

## Quick Start

### Backend

```bash
cd src/dotnet/content
dotnet restore
dotnet run --project JfYu.WebApi.Template
```

Open `https://localhost:{port}/scalar/v1` to explore the API.

### Frontend

```bash
cd src/vue
pnpm install
pnpm --filter @vben/web-antd dev
```

Open the URL printed by Vite (default `http://localhost:5666`).

## Working Across the Stack

When a feature spans both frontend and backend:

1. Define the API contract (request/response models) in the dotnet project first.
2. Implement the backend endpoint with validation, service logic, and `BaseResponse<T>` envelope.
3. Add the corresponding API call and UI in the Vue project under `apps/web-antd/src/`.

Refer to the per-stack README and `CLAUDE.md` files for conventions, commands, and modification boundaries.

## Quality Gates

CI runs the following on every PR (see `.github/workflows/gate.yml`):

- **Backend**: `dotnet build`, `dotnet test` with coverage (≥ 90% line coverage), SonarCloud analysis.
- **Frontend**: `pnpm lint`, `pnpm test:coverage` (≥ 90% line coverage on `api/system/**` and `views/system/**/data.ts`), SonarCloud analysis.

## Security

Secrets (JWT keys, DB connection strings, WeChat credentials, Redis passwords) **must never be committed**. Use environment variables or `dotnet user-secrets` in development.

## License

MIT — see [LICENSE](LICENSE).
