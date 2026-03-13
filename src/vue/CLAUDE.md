# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Tech Stack

1. Vue 3 + TypeScript + Vite monorepo project based on **pnpm workspaces** + **Turborepo**.
2. Supports multiple UI component library variants (Ant Design Vue, Element Plus, Naive UI, TDesign), all sharing a core UI framework built with TailwindCSS + shadcn-vue.
3. Requires Node ≥ 20.19.0 and pnpm ≥ 10.
4. Uses **prettier** + **eslint** + **stylelint** for code linting and formatting.
5. Uses **vitest** for unit testing.
6. Uses **commitlint** for commit message conventions.
7. Uses **czg** for interactive commit prompts.
8. Uses **lefthook** for Git hooks.
9. Uses **vsh** for code linting and formatting utilities.
10. Uses **turbo** for building.
11. Uses **vite** for development.
12. Uses **vue-tsc** for type checking.

```bash
# Additional checks
pnpm check:circular # Circular dependency scan
pnpm check:dep      # depcheck dependency audit
pnpm check:cspell   # Spell check

# Cleanup
pnpm clean          # Remove dist, node_modules, and other build artifacts
pnpm reinstall      # clean + reinstall

# Interactive conventional commit
pnpm commit         # czg commit wizard
```

Turbo tasks cascade via `dependsOn: ["^build"]`, so building an app automatically builds all its dependency packages first.

## Monorepo Directory Structure

```text
apps/
  web-antd/         # Ant Design Vue application

packages/
  @core/            # Framework core (no dependency on specific UI libraries)
    base/           # Shared utilities, caching, color handling, type definitions
    composables/    # Core Vue composables
    preferences/    # PreferenceManager class (reactive, persistent configuration)
    ui-kit/         # UI component fragments: form-ui, layout-ui, menu-ui, popup-ui, shadcn-ui, tabs-ui
  effects/          # Higher-level modules that may depend on @core and UI libraries
    access/         # Route/menu generation and permission directives
    common-ui/      # Common UI components (ApiComponent, IconPicker, VCropper, Tippy, etc.)
    hooks/          # useAppConfig and others
    layouts/        # BasicLayout, login page, various widgets
    plugins/        # Motion and other plugins
    request/        # RequestClient (axios wrapper + interceptor system)
  constants/        # Global constants (LOGIN_PATH, etc.)
  icons/            # Iconify icon wrappers
  locales/          # vue-i18n initialization, loadLocalesMap utility
  preferences/      # Public API exposing @core/preferences
  stores/           # Pinia global stores: useAccessStore, useUserStore, useTabbarStore
  styles/           # Global CSS / TailwindCSS base styles
  types/            # Shared TypeScript types
  utils/            # Shared utility functions (mergeRouteModules, mapTree, etc.)

internal/
  lint-configs/     # ESLint, Prettier, Stylelint, commitlint config packages
  node-utils/       # Build-time Node utilities
  tailwind-config/  # Shared Tailwind configuration
  tsconfig/         # Base tsconfig
  vite-config/      # Shared Vite config factory + plugin collection

scripts/
  vsh/              # CLI tools (lint, check-dep, check-circular, publint)
  turbo-run/        # Interactive turbo runner

playground/         # Component playground
docs/               # VitePress documentation
```

## Core Architecture

### Application Bootstrap Flow

Each app's `src/main.ts` calls `bootstrap(namespace)` (defined in `src/bootstrap.ts`), which executes in order:

1. Initializes the **component adapter** (`src/adapter/component/index.ts`) — maps generic form component names to concrete UI library components.
2. Calls `initSetupVbenForm()` (`src/adapter/form.ts`) to configure the generic form system.
3. Sequentially initializes i18n, Pinia stores, permission directives, Tippy, router, and MotionPlugin, then mounts to `#app`.

### Preferences System

`@vben/preferences` exports a singleton `preferences` (`PreferenceManager`). It is reactive, automatically persisted to localStorage (prefixed with the app namespace), and drives theme CSS variable updates. Each app calls `defineOverridesPreferences()` in `src/preferences.ts` to override defaults without modifying core code.

### Permission / Access System

`@vben/access` (`packages/effects/access`) supports three access modes:

- **frontend**: Filters static routes based on user roles.
- **backend**: Fetches menus from an API (`getAllMenusApi`) and dynamically registers routes.
- **mixed**: Uses both approaches simultaneously.

The route guard (`src/router/guard.ts`) calls `generateAccess()` on the first post-login navigation, stores the result in `useAccessStore`, then redirects to the target page. The `v-access` directive and `<AccessControl>` component control UI element visibility by permission code or role.

### Request Client

`@vben/request` wraps Axios as `RequestClient`. Each app creates its own instance in `src/api/request.ts` with the following interceptors:

- **Request interceptor**: Automatically attaches Bearer Token and Accept-Language headers.
- **`defaultResponseInterceptor`**: Unwraps the `{ code, data, message }` response format.
- **`authenticateResponseInterceptor`**: Handles 401 responses by auto-refreshing the token or redirecting to login.
- **`errorMessageResponseInterceptor`**: Calls `message.error()` to display error messages.

In API files, import `requestClient` (auto-unwraps response) or `baseRequestClient` (raw response) from `#/api/request`.

### Route Organization

- `src/router/routes/modules/*.ts`: Dynamic routes requiring authentication.
- `src/router/routes/core/`: Always-accessible routes (login page, 404, etc.).
- `mergeRouteModules(import.meta.glob(...))` aggregates route module files.
- Dynamic routes are registered at runtime by the permission system.

### Adapter Pattern

Each UI library app provides adapters under `src/adapter/` to bridge `@vben/common-ui` generic form/modal/drawer components to the specific component library. This is the primary difference between `web-antd`, `web-ele`, and other apps.

### Global Pinia Stores

- `useAccessStore`: Token, routes, menus, lock screen, and login expiry state.
- `useUserStore`: User info, roles, homePath.
- `useTabbarStore`: Open tabs management.

All stores are initialized together via `initStores(app, { namespace })` from `@vben/stores`.

### Mock Backend

`apps/backend-mock` is a Nitro server that can be started independently: `pnpm -F @vben/backend-mock start`. The Vite dev server proxies API requests to it via `vite.config.ts` (in `internal/vite-config`).

## Development Conventions

- **Path alias**: `#/*` points to each app's `./src/*` (defined in `package.json#imports`).
- **Dependency version management**: Internal packages use `workspace:*`; third-party packages use `catalog:` (versions centrally managed in `pnpm-workspace.yaml#catalog`).
- **Commit conventions**: Follow Conventional Commits (`feat`, `fix`, `chore`, `docs`, `refactor`, `perf`, `test`, `ci`, `style`, `types`, `revert`), enforced by lefthook + commitlint.
- **Pre-commit hook** (lefthook): Automatically runs prettier + eslint + stylelint on staged files. Use `pnpm commit` (czg) for guided commits.
- **Adding new pages**: Create a `.vue` file under `src/views/`, add a route module under `src/router/routes/modules/`; if using backend mode, also ensure the backend API returns the corresponding menu data.
- **Internationalization**: Use `$t('key')` throughout; locale files are in `packages/locales/`, and app-level i18n files are in `src/locales/langs`.
