This file provides guidance to GitHub Copilot when working with this repository.

## Project Overview

This is a full-stack application template with two independent sub-projects:

- **`src/dotnet/`** — ASP.NET Core 10 Web API backend
- **`src/vue/`** — Vue 3 + TypeScript + Vite monorepo frontend

## Repository Layout

```text
src/
  dotnet/                  # .NET 10 backend
    WebApi/                # Main API project
    WebApi.UnitTests/      # Unit test project
    WebApi.slnx            # Solution file
  vue/                     # Vue 3 monorepo frontend
    apps/
      web-antd/            # Ant Design Vue application
    packages/              # Shared packages (@core, effects, stores, etc.)
    internal/              # Internal tooling (lint, tsconfig, vite-config)
    scripts/               # Build/utility scripts
```

## General Conventions

- **Language**: All code comments and identifiers are in English; user-facing content may be Chinese.
- **Feature flags**: The dotnet template uses `#if` conditional directives (e.g., `EnableJWT`, `EnableRBAC`) to enable/disable optional features. Do not remove these comments.
- **Security**: Secrets (JWT keys, DB connection strings, WeChat credentials, Redis passwords) must never be committed. Use environment variables or user-secrets in development.
- **Never edit** generated migration files manually (`src/dotnet/WebApi/Migrations/`).

## Working Across the Stack

When a feature spans both frontend and backend:

1. Define the API contract (request/response models) in the dotnet project first.
2. Implement the backend endpoint with proper validation, service logic, and response wrappers.
3. Add the corresponding API call and UI in the Vue project.
