# CLAUDE.md

This file provides guidance to Claude Code when working in this repository.

---

## Build & Run Commands

### Backend

```bash
# Build the solution
dotnet build

# Run the API
dotnet run --project src/Api

# Run tests (no rebuild)
dotnet test --no-build

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# EF Core migrations (always run from solution root)
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Api
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

### Frontend

```bash
# Install dependencies
npm install

# Start dev server (with SSR)
ng serve

# Build for production (SSR enabled by default)
ng build --configuration production

# Run unit tests
npx vitest

# Lint
ng lint

# i18n — extract translatable strings
ng extract-i18n

# i18n — build for a specific locale
ng build --configuration production --localize
```

---

## Project Structure

### Backend

```
src/
├── Api/                  # Entry point, endpoints, middleware, DI composition root
├── Application/          # Commands, queries, handlers, DTOs, validators, pipeline behaviors
├── Domain/               # Entities, value objects, domain events, domain exceptions
├── Infrastructure/       # EF Core, Identity, external integrations, repository implementations
tests/
├── Unit/
└── Integration/
```

### Frontend

```
src/
├── app/
│   ├── core/             # Singleton services, guards, interceptors, app-level providers
│   ├── shared/           # Reusable standalone components, pipes, directives
│   ├── features/         # Lazy-loaded feature folders (one per domain feature)
│   └── app.routes.ts     # Root route configuration
├── i18n/                 # Translation files: messages.fr.xlf, messages.en.xlf
├── environments/
└── assets/
```

---

## Architecture

### Philosophy — Clean Architecture with CQRS

- **Domain** has zero external dependencies — pure business logic and invariants only.
- **Application** orchestrates use cases via Mediator handlers. Depends only on Domain.
- **Infrastructure** implements interfaces defined in Application. Never referenced by Application directly.
- **Api** is thin — maps HTTP to Mediator requests, no business logic whatsoever.

**Why CQRS?** Read and write models have different performance and shape requirements. Keeping them separate makes both simpler.
**Why Mediator?** Decouples handlers from the HTTP layer, enables pipeline behaviors (validation, logging, performance). This project uses the Mediator pattern via the dispatcher, no external librairy like MediaR.

### Dependency Direction

```
Domain ← Application ← Infrastructure
                     ← Api
```

Never reference an outer layer from an inner one. Never reference Infrastructure from Api directly.

---

## Backend Standards (C# 14 / .NET 10)

> **.NET 10** is an LTS release (November 2025). Use it for all new projects.
> **Entity Framework Core 10** ships alongside .NET 10 — use it exclusively. Do not mix EF Core versions.

### Tech Stack

- .NET 10, ASP.NET Core REST APIs (prefer APIs with controllers with API versionning)
- Entity Framework Core 10 with SQL Server (or PostgreSQL via Npgsql.EFCore)
- Generate Mediator Pattern for CQRS — **not** MediatR
- FluentValidation for request validation
- Scalar for API documentation (OpenAPI 3.1)
- ASP.NET Core Identity with JWT Bearer tokens

### C# Style

- Use **primary constructors** for dependency injection everywhere — no `private readonly` field + constructor boilerplate.
- Use **file-scoped namespaces** in every `.cs` file.
- Prefer `is null` / `is not null` over `== null` / `!= null`.
- Prefer `IReadOnlyList<T>` or `IEnumerable<T>` over `List<T>` in return types.
- Use **`record`** types for DTOs, commands, and queries — they are immutable and have value-based equality by default.
- Apply `sealed` to records and DTOs where inheritance is not intended.
- No magic strings — use `const` or `static readonly` for role names, claim types, policy names, and route segments.
- XML doc comments on all public Domain types and Application interfaces.
- Prefer **`Result<T>`** pattern (e.g. `ErrorOr`) over throwing exceptions for expected failure paths in the Application layer.

### CQRS & Mediator

- **Commands** mutate state and return minimal data (new `Guid` or `Unit`).
- **Queries** return data and never mutate state.
- Name requests explicitly: `CreateProjectCommand`, `GetProjectBySlugQuery`.
- Pipeline behaviors own all cross-cutting concerns (validation, logging, performance) — never duplicate this inside handlers.
- Validators live in the same folder as their command or query.

### Validation

- All validation lives in **FluentValidation** validators in the Application layer — no exceptions.
- `ValidationBehavior` (Mediator pipeline) throws `ValidationException` before the handler is ever reached.
- **Never** check `ModelState` manually — it is redundant by design.
- Validators are registered automatically via assembly scanning at startup.

### Security & Authentication

- All API endpoints use **`RequireAuthorization()`** by default — secure by default.
- Public endpoints explicitly opt out with **`.AllowAnonymous()`** — always a conscious decision.
- Admin-only endpoints use `.RequireAuthorization(Roles.Admin)`.
- Role name constants live in a single `Roles` static class.
- Sensitive config (`Jwt:Key`, connection strings) must never be committed. Use `dotnet user-secrets` locally and environment variables (Azure Key Vault in production).

### Error Handling

- Throw **domain-specific exceptions** from handlers and domain services (`NotFoundException`, `ConflictException`, `ForbiddenException`).
- Global exception middleware maps exceptions to RFC 9457 problem-details responses with correct HTTP status codes.
- **No try/catch in controllers or endpoint handlers** — they dispatch to Mediator and return the result.
- Never expose internal exception details in API responses.

### Domain Rules

- **Never modify `Domain/`** without explicit approval — changes there have cascading effects everywhere.
- Use `IDateTimeProvider` (or equivalent abstraction) instead of `DateTime.UtcNow` directly — keeps domain logic testable.
- Domain entities enforce their own invariants — invalid state must be impossible to construct.

### EF Core Patterns

- Define all entity configurations in **`IEntityTypeConfiguration<T>`** classes in `Infrastructure/Persistence/Configurations/` — never use Data Annotations on domain entities.
- Keep `DbContext` lean: no business logic, no queries. It is an infrastructure concern.
- Use the **Repository Pattern** with interfaces defined in `Application/` and implemented in `Infrastructure/`. Repositories return domain entities, not EF proxies.
- Queries that are read-only must call **`.AsNoTracking()`** — never track entities you won't modify.
- Use **LINQ** for all queries — no raw SQL unless absolutely necessary for performance. If raw SQL is needed, use `FromSqlRaw` with parameterized inputs only (never string interpolation).
- Seed reference data via `IEntityTypeConfiguration<T>.Configure()` using `builder.HasData(...)` — not via migration Up() methods.
- Always add **indexes** on foreign keys and frequently filtered columns in the entity configuration.

```csharp
// Example entity configuration
public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Slug).HasMaxLength(100).IsRequired();
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.Property(p => p.Title).HasMaxLength(200).IsRequired();
    }
}
```

### Logging

- Use **`ILogger<T>`** injected via the primary constructor — never use a static logger.
- Prefer **structured logging** with named parameters: `_logger.LogInformation("Project {ProjectId} created", id)` — not string interpolation.
- Log at `Information` for business events, `Warning` for recoverable errors, `Error` for unhandled exceptions.
- Use **Serilog** as the logging provider, configured in `Program.cs` with sinks for Console (dev) and Application Insights or Azure Monitor (production).
- Never log sensitive data (passwords, tokens, PII).

### API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/v1/accessibilityaudits` | Admin | Create a new audit session |
| GET | `/api/v1/accessibilityaudits` | Admin | List all audits |
| GET/POST/PUT/DELETE | `/api/v1/bugs/*` | Admin | Bug report management |

### API Versioning & Pagination

- Version all API routes from day one: `/api/v1/...`. Use `Asp.Versioning.Http` for APIs.
- Queries returning collections must support **cursor-based or offset pagination**. Never return unbounded lists.
- Paginated responses use a consistent envelope:

```json
{
  "items": [...],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20,
  "hasNextPage": true
}
```

### CORS

- Configure CORS in `Program.cs` using a named policy. Never use `AllowAnyOrigin()` in production.
- Development: allow `http://localhost:4200`.
- Production: allow only the Vercel deployment URL (set via environment variable `ALLOWED_ORIGINS`).

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(builder.Configuration["AllowedOrigins"]!.Split(','))
              .AllowAnyHeader()
              .AllowAnyMethod());
});
```

---

## Backend Testing Standards

### Unit Tests (tests/Unit/)

- Test **one behavior per test method** — one assertion (or a logically grouped set) per test.
- Follow **AAA**: Arrange → Act → Assert. Never nest Arrange inside loops.
- Name tests with the pattern: `MethodName_Scenario_ExpectedResult` — e.g. `Handle_WhenProjectNotFound_ThrowsNotFoundException`.
- Use **xUnit** as the test framework. Use **NSubstitute** for mocking (not Moq).
- Never test EF Core or the database in unit tests — mock all repositories and services.
- Domain entities are tested directly without mocks — they have no external dependencies.

```csharp
// Example unit test
public sealed class CreateProjectHandlerTests
{
    private readonly IProjectRepository _repository = Substitute.For<IProjectRepository>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ThrowsConflictException()
    {
        // Arrange
        _repository.ExistsBySlugAsync("my-project", Arg.Any<CancellationToken>()).Returns(true);
        var handler = new CreateProjectHandler(_repository, _clock);
        var command = new CreateProjectCommand("My Project", "my-project");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(command, default));
    }
}
```

### Integration Tests (tests/Integration/)

- Use **`WebApplicationFactory<Program>`** to test the full HTTP stack.
- Use **Testcontainers** (`Testcontainers.MsSql` or `.PostgreSql`) to spin up a real database per test run — never a SQLite in-memory substitute.
- Reset database state between test classes using `IAsyncLifetime`.
- Integration tests cover the full pipeline: HTTP request → Mediator → Handler → Repository → DB → HTTP response.
- Never mock anything in integration tests — that defeats their purpose.

---

## Frontend Standards (Angular 21 / TypeScript)

> **Angular 21** is the current stable major version (released ~November 2025).
> Verify the exact version in `package.json` before making assumptions about available APIs.
> Angular follows a predictable 6-month release cycle; always target the latest stable major.

### Tech Stack

- Angular 21+ (standalone components, SSR enabled via `@angular/ssr`)
- TypeScript 5.7+ with strict mode enabled
- Signals for state management
- Vitest + Angular Testing Library for unit tests
- Angular i18n (`@angular/localize`) for bilingual FR/EN support

### TypeScript

- Enable **strict mode** in `tsconfig.json` — no exceptions.
- Prefer type inference when the type is obvious.
- Never use `any` — use `unknown` when the type is genuinely uncertain, then narrow it.
- Use `readonly` on properties that should not be mutated after construction.

### Angular

- **Never set `standalone: true`** inside decorators — it is the default in Angular v17+ and is redundant noise.
- Always use **`inject()`** instead of constructor injection.
- Implement **lazy loading** for all feature routes.
- Use **`NgOptimizedImage`** for all `<img>` tags that reference static assets (`NgOptimizedImage` does not support inline base64 or dynamically computed URLs).
- Never use `@HostBinding` or `@HostListener` — put host bindings in the `host` object of `@Component` or `@Directive`.
- Use **`@defer`** blocks to defer non-critical UI below the fold — improves LCP and INP scores.

### Components

- Set **`changeDetection: ChangeDetectionStrategy.OnPush`** on every component — no exceptions.
- Use **`input()`**, **`output()`**, and **`model()`** functions instead of `@Input()` / `@Output()` decorators. Use `model()` for two-way bound values.
- Use **`computed()`** for all derived state.
- Keep components small and focused on a single responsibility.
- Prefer inline templates for small components; use file-relative paths for external templates and styles.
- Prefer **Reactive Forms** over Template-driven forms.
- Never use `ngClass` — use `class` bindings instead.
- Never use `ngStyle` — use `style` bindings instead.

### State Management

- Use **signals** for local component state.
- Use **`computed()`** for derived state — never duplicate derived values.
- Keep state transformations pure and predictable — no side effects inside computed.
- Never use `mutate()` on signals — it no longer exists. Use `update()` or `set()`.

### Templates

- Keep templates simple — no complex logic or expressions.
- Use **native control flow** (`@if`, `@for`, `@switch`, `@defer`) — never `*ngIf`, `*ngFor`, `*ngSwitch`.
- Use the **`async` pipe** to handle observables in templates.
- Never assume globals like `new Date()` are available in templates.

### HTTP & API Communication

- Use **typed `HttpClient`** with strongly typed return types. Avoid `.pipe(map(...))` for simple JSON — let the generic handle it.
- Create **dedicated service classes** per domain entity. Never call `HttpClient` directly from components.
- Handle HTTP errors in services using `catchError` — never in components.
- Use **HTTP interceptors** for auth headers, global error handling, and loading state.
- For repeated calls with the same URL, use `shareReplay(1)` to avoid duplicate requests.
- Use the **`resource()` / `rxResource()` API** (Angular 19+) for signal-based data fetching that integrates with the component lifecycle:

```typescript
// Prefer resource() over manual subscribe for component-scoped data
readonly project = resource({
  request: () => ({ slug: this.slug() }),
  loader: ({ request }) => this.projectService.getBySlug(request.slug),
});
// Access: this.project.value(), this.project.isLoading(), this.project.error()
```

### Global Error Handling

- Provide a custom **`ErrorHandler`** in `core/` that catches unhandled Angular errors and reports them (console in dev, monitoring service in prod).
- Register a global **`HttpInterceptor`** that maps 4xx/5xx responses to user-friendly messages and triggers a notification signal.
- Never show raw error objects or stack traces to the user.

```typescript
// core/interceptors/error.interceptor.ts
export const errorInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      inject(NotificationService).showError(mapHttpError(error));
      return throwError(() => error);
    })
  );
```

### Internationalization (i18n)

- All user-facing strings must be marked with `i18n` attributes or the `$localize` tagged template literal.
- Extract strings with `ng extract-i18n` — translation files live in `src/i18n/`.
- Supported locales: `fr` (default) and `en`.
- Build separate bundles per locale with `ng build --localize` for production.
- Language-switch UI must update the locale via navigation, not runtime string swapping.

### Accessibility (WCAG 2.2 AA — mandatory)

This portfolio is also a demonstration of accessibility expertise. Every component must meet WCAG 2.2 AA.

- All components **must pass axe DevTools and Lighthouse Accessibility at 100%** — zero violations.
- Follow **WCAG AA** minimums: focus management, color contrast ≥ 4.5:1 (normal text), ≥ 3:1 (large text).
- Interactive elements must be keyboard navigable (tab order, focus ring visible).
- Images must have meaningful `alt` text (or `alt=""` if decorative).
- Use semantic HTML elements (`<nav>`, `<main>`, `<section>`, `<article>`, `<button>`) — never `<div>` for interactive elements.
- Form fields must have associated `<label>` elements (or `aria-label` / `aria-labelledby`).
- Modals and dialogs must trap focus and return focus on close.
- Avoid `tabindex` values greater than 0.
- Test with NVDA + Firefox and VoiceOver + Safari before every release.
- Use `aria-live` regions for dynamic content updates (search results, notifications).

### Services

- Design services around a **single responsibility**.
- Always use **`providedIn: 'root'`** for singleton services.
- Use **`inject()`** instead of constructor injection.

### Environment Configuration

- Never hardcode API URLs or feature flags — always read from the `environment.ts` object.
- `environment.ts` (dev) and `environment.prod.ts` (prod) are the only files that differ between builds.
- Access via `inject(ENVIRONMENT)` using an `InjectionToken<Environment>` provided in `app.config.ts` — never import the file directly in services.

```typescript
// core/tokens/environment.token.ts
export const ENVIRONMENT = new InjectionToken<Environment>('environment');

// app.config.ts
providers: [{ provide: ENVIRONMENT, useValue: environment }]

// In a service
readonly #env = inject(ENVIRONMENT);
readonly apiUrl = this.#env.apiUrl;
```

### SEO & Meta Tags (SSR)

- Every routed page must set its `<title>` and `<meta name="description">` using Angular's **`Title`** and **`Meta`** services — critical for SSR-rendered pages.
- Use a base `SeoService` in `core/` called from each feature component's `constructor` or `ngOnInit`.
- Open Graph tags (`og:title`, `og:description`, `og:image`) must be set for portfolio pages shared on LinkedIn.
- Use `<link rel="canonical">` on each page to avoid duplicate content from locale-prefixed URLs.

```typescript
@Injectable({ providedIn: 'root' })
export class SeoService {
  readonly #title = inject(Title);
  readonly #meta = inject(Meta);

  setPage(title: string, description: string): void {
    this.#title.setTitle(`${title} | Philippe Charron`);
    this.#meta.updateTag({ name: 'description', content: description });
    this.#meta.updateTag({ property: 'og:title', content: title });
  }
}
```

### Animations

- Use **Angular animations** (`@angular/animations`) for route transitions and entrance effects — not CSS-only for complex stateful transitions.
- Always wrap animations in **`@media (prefers-reduced-motion: reduce)`** via the `matchMedia` API or Angular CDK's `ANIMATION_MODULE_TYPE` — respect the user's OS accessibility setting.
- Keep animations under 300ms for UI feedback, 500ms for page transitions. Nothing longer.
- Define animations in a shared `animations.ts` file, not inline in component decorators.

### Frontend Testing (Vitest + Angular Testing Library)

- Test **component behavior**, not implementation — query by role/label/text, not by CSS class or internal signal values.
- Use **Angular Testing Library** (`@testing-library/angular`) for component tests — it enforces accessibility-first querying.
- Use **Vitest** as the test runner (replaces Jest). Configure via `vitest.config.ts` at the project root.
- Mock `HttpClient` using `provideHttpClientTesting()` — never create real HTTP calls in unit tests.
- Test signals indirectly via their effect on the rendered DOM, not by reading signal values directly.

```typescript
// Example component test
it('shows error message when form submitted with empty name', async () => {
  const { getByRole, getByText } = await render(ContactFormComponent);

  await userEvent.click(getByRole('button', { name: /envoyer/i }));

  expect(getByText(/le nom est requis/i)).toBeInTheDocument();
});
```

- Run all tests before every push: `npx vitest run`.
- Aim for 80%+ coverage on services and 100% on pure utility functions.

### Server-Side Rendering (SSR / Vercel)

- Angular 21 ships SSR via `@angular/ssr` — enable it at project creation with `--ssr`.
- Use **`isPlatformBrowser()`** to guard browser-only APIs (`localStorage`, `window`, `document`) from running on the server.
- Vercel auto-detects Angular SSR projects — build command: `ng build --configuration production`, output directory: `dist/<project-name>/browser`.
- Avoid `document.querySelector` in components — use Angular's `ElementRef` or the `viewChild()` / `contentChild()` signal queries instead.
- Pre-render static routes at build time with `prerender: true` in `angular.json` — improves Lighthouse scores and SEO.

---

## Styling & Theming (SCSS)

- Use **SCSS** with a design token layer (`_tokens.scss`) for colors, spacing, typography, and breakpoints — never hardcode raw values in component styles.
- Support **dark mode** via `prefers-color-scheme` media query on CSS custom properties defined on `:root`. No JavaScript toggle required.
- Never use `::ng-deep` except as a last resort for third-party component overrides — always add a comment explaining why.
- Use **BEM naming** for class names: `.block__element--modifier`.
- Keep component `styles.scss` short — extract reusable patterns into `src/shared/styles/`.
- Define breakpoints as SCSS mixins, not raw `@media` queries repeated across files:

```scss
// src/shared/styles/_breakpoints.scss
@mixin tablet  { @media (min-width: 768px)  { @content; } }
@mixin desktop { @media (min-width: 1024px) { @content; } }
```

- Typography scale, color tokens, and spacing scale live in `src/shared/styles/_tokens.scss` and are imported globally in `styles.scss`.

---

## Git Conventions

### Branch Naming

| Type | Pattern | Example |
|------|---------|---------|
| Feature | `feat/<short-description>` | `feat/hero-section` |
| Bug fix | `fix/<short-description>` | `fix/contrast-ratio-nav` |
| Accessibility | `a11y/<short-description>` | `a11y/focus-trap-modal` |
| Refactor | `refactor/<short-description>` | `refactor/project-service` |
| Chore | `chore/<short-description>` | `chore/update-angular-21-1` |

### Commit Messages

Follow **Conventional Commits**: `<type>(<scope>): <imperative verb> <what>`

```
feat(hero): add bilingual language toggle
fix(contact-form): restore focus after submission
a11y(nav): add skip-to-content link
refactor(project): extract SeoService to core
chore(deps): update @angular/core to 21.1.0
```

- Scope is the feature folder name or layer (`hero`, `api`, `domain`, `infra`).
- Subject line ≤72 characters. No period at the end.
- Breaking changes: `feat(api)!: change pagination envelope`.

### Pull Request Rules

- One logical change per PR — split large features into stacked PRs if needed.
- PR description must include: what changed, why, and how to test it manually.
- Link the Azure DevOps work item in the PR description.
- All checks (tests, lint, axe) must be green before requesting review.
- Squash merge to `main` — keep the history linear.

---

## CI/CD Pipeline (GitHub Actions)

### Frontend CI (runs on every pull request)

```yaml
# .github/workflows/frontend.yml
name: Frontend CI
on:
  pull_request:
    branches: [main]
jobs:
  ci:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with: { node-version: '22' }
      - run: npm ci
      - run: npx vitest run
      - run: ng lint
      - run: ng build --configuration production
```

Vercel handles the actual deployment on merge to `main` — no deploy step needed in this workflow.

### Backend CI/CD (runs on push to `main`)

```yaml
# .github/workflows/backend.yml
name: Backend CI/CD
on:
  push:
    branches: [main]
jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '10.x' }
      - run: dotnet restore
      - run: dotnet build --no-restore -c Release
      - run: dotnet test --no-build -c Release
      - run: dotnet publish src/Api -c Release -o ./publish
      - uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ secrets.AZURE_APP_NAME }}
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
          package: ./publish
```

All secrets (`AZURE_APP_NAME`, `AZURE_PUBLISH_PROFILE`) live in GitHub repository secrets — never in source code.

---

## SOLID Principles Checklist

Before submitting a PR, verify each principle:

- **S** — Single Responsibility: does each class/component do exactly one thing?
- **O** — Open/Closed: new behavior is added via extension (new handler, new strategy), not by modifying existing code.
- **L** — Liskov Substitution: any implementation of an interface must be safely substitutable.
- **I** — Interface Segregation: no interface forces implementors to depend on methods they don't use.
- **D** — Dependency Inversion: high-level modules (Application) depend on abstractions, not on Infrastructure concretions.

---

## Workflow Rules

- **Always create a feature branch** before making any changes — never commit directly to `main`.
- **Run tests after every implementation** — `dotnet test` for backend, `npx vitest` for frontend.
- **Run `ng lint` and `npx vitest` before every PR** — CI will block on failures.
- **Never modify `Domain/`** without explicit discussion and approval.
- **Keep commits atomic** — one logical change per commit, written in imperative mood (`Add`, `Fix`, `Refactor`).
- **No commented-out code** — delete it, version control has the history.
- **No TODOs without a tracking issue** — either fix it now or open a ticket in Azure DevOps.
- Pull requests must have a passing test suite and zero axe violations before merge.

---

## Deployment

### Frontend → Vercel

| Setting | Value |
|---|---|
| Build command | `ng build --configuration production` |
| Output directory | `dist/<project-name>/browser` |
| Install command | `npm install` |
| Node version | 22 LTS |

Set environment variables in Vercel dashboard — never in committed files.

### Backend → Azure App Service

| Setting | Value |
|---|---|
| Runtime | .NET 10 |
| Publish | GitHub Actions → Azure App Service |
| Secrets | Azure Key Vault (never appsettings.json) |
| Environment | `ASPNETCORE_ENVIRONMENT=Production` |

Use Azure DevOps pipelines or GitHub Actions for CI/CD. Tag production deployments with the sprint number.
