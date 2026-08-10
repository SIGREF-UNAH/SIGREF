# SIGREF.API Scalar Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task with verification checkpoints.

**Goal:** Replace Swagger/Swashbuckle in SIGREF.API with Scalar while preserving the current Development-only API documentation behavior and endpoint descriptions.

**Architecture:** ASP.NET Core native OpenAPI generation will produce `/openapi/v1.json`; Scalar.AspNetCore will render that document at `/scalar`. Existing XML documentation and API metadata will be retained or migrated to framework-compatible metadata, while Swashbuckle-only filters and attributes are removed.

**Tech Stack:** .NET 9, ASP.NET Core OpenAPI, Scalar.AspNetCore, MVC controllers, XML documentation.

## Global Constraints

- Scalar is mapped only in `Development`, matching the existing Swagger exposure.
- The public documentation route is `/scalar`; `/swagger` routes are removed.
- Endpoint behavior, authentication, common error responses, and existing XML documentation must remain intact.
- No `Swashbuckle.*` package, namespace, attribute, filter, or runtime reference may remain in SIGREF.API.

---

### Task 1: Replace API documentation service registration

**Files:**
- Modify: `BE/SIGREF.API/SIGREF.API.csproj`
- Modify: `BE/SIGREF.API/Configuration/Startup.MvcSwagger.cs`
- Modify: `BE/SIGREF.API/SIGREF.API.csproj` XML documentation settings only if required by native OpenAPI
- Delete: `BE/SIGREF.API/Utils/EnumSchemaFilter.cs` if no remaining consumer exists

**Interfaces:**
- Produces `builder.Services.AddOpenApi()` registration consumed by the pipeline.
- Preserves `AddControllers`, `AddEndpointsApiExplorer`, `AddCommonErrorResponsesConvention`, XML documentation generation, and the API metadata required by Scalar.

- [ ] Remove all four Swashbuckle package references and add `Scalar.AspNetCore` plus the ASP.NET Core OpenAPI package compatible with `net9.0`.
- [ ] Replace `AddSwaggerGen` with native `AddOpenApi`, retaining the title, version, description, contact metadata, JWT bearer security scheme, XML comments, tag grouping, and endpoint ordering through native OpenAPI transformers or controller metadata.
- [ ] Remove `Microsoft.OpenApi` and `EnumSchemaFilter` only if the resulting native OpenAPI implementation no longer requires them.
- [ ] Remove the `using` directives and file-level references that only supported Swashbuckle.
- [ ] Run `dotnet build BE/SIGREF.API/SIGREF.API.csproj --no-restore` and record any API incompatibilities before proceeding.

### Task 2: Replace Swagger middleware and launch configuration

**Files:**
- Modify: `BE/SIGREF.API/Program.cs`
- Modify: `BE/SIGREF.API/Startup.cs`
- Modify: `BE/SIGREF.API/Properties/launchSettings.json`

**Interfaces:**
- Exposes `/openapi/v1.json` and `/scalar` only in Development.
- Keeps routing, HTTPS, authentication, authorization, CORS, audit, static files, Hangfire, and controller mapping unchanged.

- [ ] Add `using Scalar.AspNetCore;` where required and map `app.MapOpenApi()` followed by `app.MapScalarApiReference()` inside the existing Development branch.
- [ ] Remove the temporary Swagger exception middleware and all `UseSwagger`/`UseSwaggerUI` code.
- [ ] Change launch URLs from `swagger` to `scalar`.
- [ ] Preserve middleware ordering for authentication, authorization, audit, static files, and controllers.
- [ ] Build again and verify that the only new documentation endpoints are `/openapi/v1.json` and `/scalar`.

### Task 3: Remove Swashbuckle annotations without losing endpoint documentation

**Files:**
- Modify: all `BE/SIGREF.API/Controllers/**/*.cs` files containing `Swashbuckle.AspNetCore.Annotations`
- Modify: `BE/SIGREF.API/Audit/Types/DatabaseAction.cs`
- Modify: `BE/SIGREF.API/Dtos/Audit/AuditLogFilterDto.cs`

**Interfaces:**
- Existing controller routes and action signatures remain unchanged.
- Endpoint summaries, descriptions, tags, parameter descriptions, and response metadata remain available to native OpenAPI and Scalar.

- [ ] Remove `using Swashbuckle.AspNetCore.Annotations` from every affected source file.
- [ ] Replace `[SwaggerTag]` with controller-level `ApiExplorerSettings`/native metadata or XML documentation supported by the generated OpenAPI document.
- [ ] Replace `[SwaggerOperation]` with XML `<summary>`/`<remarks>` and native endpoint metadata where the generated document would otherwise lose a summary or description.
- [ ] Replace `[SwaggerParameter]` descriptions with XML `<param>` documentation or native parameter metadata.
- [ ] Replace `[SwaggerSchema]` with XML type/property documentation or native schema metadata.
- [ ] Keep existing `ProducesResponseType` attributes and common response conventions unchanged.
- [ ] Run a source search proving no `SwaggerOperation`, `SwaggerTag`, `SwaggerSchema`, `SwaggerParameter`, or Swashbuckle namespace remains.

### Task 4: Remove stale Swagger-only code and audit exclusions

**Files:**
- Modify: `BE/SIGREF.API/Audit/Middleware/AuditMiddleware.cs`
- Delete: `BE/SIGREF.API/Utils/EnumSchemaFilter.cs` if Task 1 confirms it is unused
- Rename or modify: `BE/SIGREF.API/Configuration/Startup.MvcSwagger.cs` to a neutral documentation configuration name if practical without unrelated churn

**Interfaces:**
- Audit behavior continues excluding health and media paths while no longer referring to a removed Swagger UI.

- [ ] Remove `/swagger` checks and Swagger-specific comments from audit middleware.
- [ ] Remove any stale debug or compatibility code referencing Swagger.
- [ ] Rename the configuration file/method only if the repository compiles cleanly with the neutral name; otherwise retain the file path but remove the obsolete terminology from its implementation.
- [ ] Search the backend source, project files, launch settings, and generated package manifests for `Swagger`, `Swashbuckle`, and `/swagger`.

### Task 5: Verify documentation and dependency cleanup

**Files:**
- Modify: generated dependency files only through the normal restore/build process if tracked by the repository

- [ ] Run `dotnet restore BE/SIGREF.API/SIGREF.API.csproj`.
- [ ] Run `dotnet build BE/SIGREF.API/SIGREF.API.csproj`.
- [ ] Run the repository's applicable backend tests from `BE` and the root test suite if their prerequisites are available.
- [ ] Start SIGREF.API in Development with its configured dependencies and request `/openapi/v1.json` and `/scalar`.
- [ ] Confirm the OpenAPI document contains controller operations, XML descriptions, response schemas, and bearer authentication.
- [ ] Confirm no Swagger/Swashbuckle references remain outside the approved migration spec/plan.
- [ ] Report any environment-dependent checks that cannot run instead of claiming completion.
