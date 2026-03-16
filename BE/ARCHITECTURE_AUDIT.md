# Architecture Audit — SIGREF
**Date:** 16 de marzo de 2026  
**Auditor:** GitHub Copilot (Senior Architect Mode)  
**Scope:** Backend only

---

## 1. Executive Summary
The SIGREF backend follows a Clean Architecture pattern with Core (domain), Infrastructure, and API layers, using .NET 9, EF Core, PostgreSQL, Keycloak for auth, and Hangfire for background jobs. The codebase shows solid separation of concerns with proper dependency inversion. However, critical security issues exist with hardcoded secrets in appsettings.json, domain models are anemic with no business logic, and there's no test coverage. Biggest risks are production security breaches and lack of resilience for external integrations. Immediate priority: move secrets to environment variables and implement domain modeling.

## 2. Solution Map
- **SIGREF.Core**: Domain layer (entities, no business logic)
- **SIGREF.Common**: Shared utilities and types
- **SIGREF.API**: Presentation layer (controllers, services with business logic)
- **SIGREF.API.ServiceDefaults**: Aspire service defaults (logging, resilience)
- **SIGREF.API.AppHost**: Aspire orchestration
- **SIGREF.Hangfire.Worker**: Background job processing
- **SIGREF.Infrastructure.Keycloak**: Keycloak integration
- **SIGREF.Infrastructure.Persistence**: EF Core context and configs
- **SIGREF.Infrastructure.Reporting**: Report generation (QuestPDF, Hangfire)

Dependency direction: Infrastructure → Core ← API → Infrastructure

No circular dependencies detected.

## 3. Layer-by-Layer Analysis

### Domain Layer (SIGREF.Core)
- Entities inherit from BaseEntity with audit fields (good for consistency).
- Models are anemic: ReportHistoryEntity has only properties, no behavior.
- No aggregates, value objects, or domain events.
- Extensions folder exists but minimal.

### Application Layer (SIGREF.API Services)
- Controllers are thin, delegate to services (good).
- Services contain complex business logic (e.g., ReportQueryService has EF queries with grouping).
- Uses CQRS partially (queries vs commands not separated).
- No interfaces for services in application layer.

### Infrastructure Layer
- Persistence: EF Core with proper configurations in separate files.
- Keycloak: Uses IHttpClientFactory, token caching (good).
- Reporting: Integrates QuestPDF, Hangfire jobs.
- No visible N+1 problems or raw SQL misuse.

### Presentation Layer (SIGREF.API)
- Controllers thin, use ResponseDto pattern.
- Middleware pipeline: CORS, Auth, Audit (custom middleware).
- Authentication: Keycloak JWT with role mapping.
- No global exception handler visible.
- API versioning not implemented.

### Background Jobs (Hangfire)
- Jobs use scoped services (SIGREFContext), registered in Worker.
- Retry policy: 2 attempts with delays.
- No culture settings handling (potential CultureNotFoundException risk).

## 4. Findings Registry

| # | Severity | Layer | File/Class | Issue | Recommendation |
|---|----------|-------|------------|-------|----------------|
| 1 | 🔴 CRITICAL | Security | SIGREF.API/appsettings.json | AdminPassword and AdminClientSecret hardcoded | Move to environment variables or Azure Key Vault |
| 2 | 🔴 CRITICAL | Security | N/A | No appsettings.Production.json | Create production config with secure secrets |
| 3 | 🟠 HIGH | Domain | SIGREF.Core/Entity/* | Anemic domain models, no business logic | Implement domain services, aggregates, value objects |
| 4 | 🟠 HIGH | Application | SIGREF.API/Services/* | Business logic in services, not domain | Move logic to domain layer |
| 5 | 🟡 MEDIUM | Testing | N/A | No unit/integration tests | Add xUnit tests for domain and services |
| 6 | 🟡 MEDIUM | Resilience | Infrastructure | No Polly policies for external calls | Add retry/circuit breakers for Keycloak, FHIR |
| 7 | 🟢 LOW | CQRS | Application | Partial CQRS (queries only) | Implement full CQRS with MediatR |
| 8 | 🟢 LOW | Validation | N/A | No centralized validation | Add FluentValidation |

## 5. Architecture Strengths
- Clean separation of layers with dependency inversion.
- Proper use of EF Core configurations in infrastructure.
- HttpClientFactory usage in Keycloak integration.
- Aspire for orchestration and service discovery.
- Background job isolation in separate worker.

## 6. Architecture Weaknesses
- Domain layer lacks behavior, leading to fat services.
- Business logic scattered in application services.
- No automated tests, risking regressions.
- Secrets exposed in config files.
- No resilience patterns for external dependencies.

## 7. Improvement Roadmap

### Quick Wins (< 1 day each)
- Move Keycloak secrets to environment variables.
- Create appsettings.Production.json.
- Add global exception handler.

### Short Term (1–2 weeks)
- Implement domain services for report logic.
- Add unit tests for core entities.
- Integrate FluentValidation.

### Long Term / Refactors (1+ month)
- Refactor to full DDD with aggregates.
- Implement CQRS with MediatR.
- Add Polly resilience policies.
- Introduce API versioning.

## 8. Open Questions
- Environment variable definitions for production deployment.
- Docker-compose environment file (.env) contents.
- FHIR server configuration and resilience.
- Audit middleware implementation details.
- Seeding logic and data validation.</content>
<parameter name="filePath">c:\Users\hecto\Documents\SIGREF\DEVELOPMENT\BE\ARCHITECTURE_AUDIT.md