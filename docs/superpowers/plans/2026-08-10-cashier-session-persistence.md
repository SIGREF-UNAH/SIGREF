# Persistencia segura de sesiones de turno Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Hacer que el Frontend use `localStorage` solo como caché rápida y valide siempre la sesión de turno contra el Backend, evitando fugas entre cajeros y manteniendo abierto el turno remoto durante logout.

**Architecture:** El Backend expondrá un endpoint autenticado `/api/CashierSessions/active` que obtiene el usuario desde los claims y reutiliza el servicio existente. El Frontend guardará la sesión con su `userId`, ejecutará una validación remota por `sub` de Keycloak antes de activar guardas y limpiará la caché/token al logout sin cerrar la sesión remota.

**Tech Stack:** ASP.NET Core 9, C#, Entity Framework Core, React 19, TypeScript, Zustand persist, TanStack Query, Keycloak JS, Axios, Vite.

## Global Constraints

- El Backend es la única fuente de verdad para determinar si el cajero tiene un turno activo.
- `localStorage` solo puede acelerar la presentación; nunca puede autorizar una ruta protegida.
- Logout no invoca el cierre de caja; solo limpia autenticación y estado local.
- El endpoint activo no acepta `userId` desde el cliente.
- Los cambios deben mantenerse dentro de la rama `356-bug-persistencia-incorrecta-de-turno-en-localstorage-y-fuga-de-sesion-entre-cajeros`.

---

### Task 1: Preparar pruebas ejecutables para Backend y Frontend

**Files:**
- Create: `FE/vitest.config.ts`
- Modify: `FE/package.json`
- Modify: `FE/package-lock.json`
- Create: `BE/SIGREF.API.Tests/SIGREF.API.Tests.csproj`
- Create: `BE/SIGREF.API.Tests/CashierSessionsControllerTests.cs`
- Modify: `BE/SIGREF.API.sln`

**Interfaces:**
- Consumes: Existing FE TypeScript configuration and Backend API project.
- Produces: `npm test -- --run <file>` for pure Frontend tests and `dotnet test BE/SIGREF.API.sln --no-restore` for controller tests.

- [ ] **Step 1: Add the failing test harness and contract test**

  Add Vitest with a `test` script and a Node environment. Create an xUnit test project referencing `BE/SIGREF.API/SIGREF.API.csproj`, `Moq`, and `Microsoft.NET.Test.Sdk`. Add a controller test that constructs `CashierSessionsController` with a mocked service and asserts the future `GetActive` action calls `GetActiveSessionByUserAsync` with the authenticated user id and returns `OkObjectResult`.

- [ ] **Step 2: Run the new tests and verify the expected failures**

  Run `npm test -- --run` from `FE` and `dotnet test BE/SIGREF.API.sln --no-restore` from `BE`. The Frontend command should pass with zero tests only if the runner is configured; the Backend contract test must fail to compile because `GetActive` does not exist yet. Fix setup errors before proceeding.

- [ ] **Step 3: Commit the test harness only after it runs**

  ```powershell
  git add FE/vitest.config.ts FE/package.json FE/package-lock.json BE/SIGREF.API.Tests BE/SIGREF.API.sln
  git commit -m "test: preparar pruebas para persistencia de turnos"
  ```

### Task 2: Agregar la consulta Backend de sesión activa

**Files:**
- Modify: `BE/SIGREF.API/Controllers/Cashier/CashierSessionsController.cs`

**Interfaces:**
- Consumes: Existing `GetActiveSessionByUserAsync(Guid userId)` and `IUserContextService.GetUserId()`.
- Produces: `GET /api/CashierSessions/active`, returning `CashierSessionDto` with `200` or the existing `404 CASHIER_NO_ACTIVE_SESSION` response.

- [ ] **Step 1: Write the failing API contract check**

  Extend `BE/SIGREF.API.Tests/CashierSessionsControllerTests.cs` with a test that calls `GetActive()` without a user-id parameter and asserts the endpoint delegates to the authenticated user. Add a separate test for a service `NotFoundException` being propagated to the API middleware contract if the project already exposes that mapping.

- [ ] **Step 2: Run the check and verify it fails because the route is absent**

  Run from `BE`: `dotnet test SIGREF.API.sln --no-restore`. The new controller test must fail because the action is absent; it must not fail because of project references or missing packages.

- [ ] **Step 3: Add the authenticated current-user endpoint**

  Add a controller action equivalent to:

  ```csharp
  [HttpGet("active")]
  [EndpointName("GetActiveSession")]
  [Authorize(Roles = RolesConstants.cashier)]
  [ProducesResponseType(typeof(CashierSessionDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetActive()
  {
      var userId = _userContext.GetUserId();
      var result = await _cashierSessionService.GetActiveSessionByUserAsync(userId);
      return Ok(result);
  }
  ```

  Inject `IUserContextService` into the controller if the current controller does not already have it. Do not add a route accepting `userId`.

- [ ] **Step 4: Run the Backend check and verify the endpoint contract passes**

  Run `dotnet test SIGREF.API.sln --no-restore` from `BE` and then `dotnet build SIGREF.API.sln --no-restore`. Verify the focused controller tests pass and the API compiles.

- [ ] **Step 5: Commit the Backend endpoint**

  ```powershell
  git add BE/SIGREF.API/Controllers/Cashier/CashierSessionsController.cs BE/SIGREF.API/Services/Cashier
  git commit -m "feat: exponer turno activo del cajero autenticado"
  ```

### Task 3: Encapsular la caché local identificada por usuario

**Files:**
- Create: `FE/src/features/cashier-sessions/store/cashierSessionStorage.ts`
- Modify: `FE/src/features/cashier-sessions/store/useCashierSessionStore.ts`
- Modify: `FE/src/features/cashier-sessions/store/index.ts`
- Create: `FE/src/features/cashier-sessions/store/cashierSessionStorage.test.ts`
- Modify: `FE/package.json` and `FE/package-lock.json` to retain the Vitest test command established in Task 1

**Interfaces:**
- Consumes: Current `CashierSession` shape and Zustand persist middleware.
- Produces: Typed store operations `setSessionForUser(userId, session)`, `getSessionForUser(userId)`, `clearSession()`, `clearIfUserChanged(userId)`, `setValidationState(state)`, and selectors for `session`, `validatedUserId`, and validation state.

- [ ] **Step 1: Write failing storage tests**

  Add tests with these exact behaviors:

  ```ts
  it("does not expose cashier A session to cashier B", () => {
    const stored = { userId: "cashier-a", session: activeSession };
    expect(getSessionForUser(stored, "cashier-b")).toBeNull();
  });

  it("rejects legacy storage without an owner", () => {
    expect(parseStoredSession({ session: activeSession }, "cashier-a")).toBeNull();
  });

  it("returns the cached session only for its owner", () => {
    expect(getSessionForUser({ userId: "cashier-a", session: activeSession }, "cashier-a"))
      .toEqual(activeSession);
  });
  ```

- [ ] **Step 2: Run the focused Frontend test and verify it fails**

  Run `npm test -- --run src/features/cashier-sessions/store/cashierSessionStorage.test.ts` from `FE` after adding the repository's minimal test command, or run the configured equivalent. The failure must be caused by the missing storage functions, not by a test setup error.

- [ ] **Step 3: Implement the minimal owner-aware storage boundary**

  Keep the persisted value versioned and shaped as `{ userId, session }`. Parse defensively, return `null` for malformed/legacy data, and ensure `clearSession` removes the persisted value. Do not let callers read the raw persisted value to authorize routes.

- [ ] **Step 4: Run the focused tests and verify they pass**

  Run the same focused command and verify all storage tests pass with no warnings.

- [ ] **Step 5: Commit the storage boundary**

  ```powershell
  git add FE/src/features/cashier-sessions/store FE/package.json FE/package-lock.json
  git commit -m "fix: aislar persistencia de turnos por cajero"
  ```

### Task 4: Generar y consumir el endpoint de validación activa

**Files:**
- Modify: `FE/src/api/cashier-sessions/cashier-sessions.ts` through the repository's Orval generation flow
- Create: `FE/src/features/cashier-sessions/hooks/useValidateCashierSession.ts`
- Create: `FE/src/features/cashier-sessions/hooks/useValidateCashierSession.test.ts`
- Modify: `FE/src/features/cashier-sessions/hooks/index.ts`

**Interfaces:**
- Consumes: `GET /api/CashierSessions/active`, Keycloak `sub`, and the owner-aware store.
- Produces: `useValidateCashierSession(userId: string | undefined)` with `isValidating`, `isValidated`, `hasActiveSession`, and `error` state. A `404` means no active session; other errors do not validate the local cache.

- [ ] **Step 1: Write failing validation tests**

  Cover:

  ```ts
  it("replaces stale local data with the Backend session", async () => {
    // API returns cashier-a session; local cache contains an older session.
    // Assert store contains the API session after validation.
  });

  it("clears local data when Backend returns CASHIER_NO_ACTIVE_SESSION", async () => {
    // API returns 404; assert no active session and cleared cache.
  });

  it("does not authorize a stale cache when validation fails", async () => {
    // API returns 500/network failure; assert isValidated is false or error state.
  });
  ```

- [ ] **Step 2: Run the focused validation tests and verify they fail**

  Run `npm test -- --run src/features/cashier-sessions/hooks/useValidateCashierSession.test.ts` from `FE`; verify the failure is due to the absent hook/validation behavior.

- [ ] **Step 3: Regenerate the API client and implement the validation hook**

  Run the existing Orval command from `FE` (`npm run orval`) after the Backend route is represented in the API schema. Use the generated `useGetActiveSession` query with a query key scoped to the authenticated user. On success, map the Backend DTO into the display session and persist it with the current `userId`; on `404`, clear it; on other errors, set an unvalidated error state. The hook must not use an old local session as a successful validation result.

- [ ] **Step 4: Run focused tests and Frontend typecheck**

  Run the focused validation tests, then `npm run build` from `FE`. Verify both complete successfully.

- [ ] **Step 5: Commit the validation hook and generated client**

  ```powershell
  git add FE/src/api FE/src/features/cashier-sessions/hooks
  git commit -m "feat: validar turno activo contra backend"
  ```

### Task 5: Integrate validation into guards and user transitions

**Files:**
- Modify: `FE/src/features/cashier-sessions/components/CashierSessionChecker.tsx`
- Modify: `FE/src/features/cashier-sessions/components/CashierRouteGuard.tsx`
- Modify: `FE/src/App.tsx` to mount the validation-aware checker in the existing authenticated application tree
- Modify: `FE/src/auth/keycloak.ts` or `FE/src/main.tsx` for centralized token/logout cleanup
- Create: `FE/src/auth/sessionCleanup.ts`
- Create: `FE/src/auth/sessionCleanup.test.ts`

**Interfaces:**
- Consumes: Keycloak `authenticated`, `tokenParsed.sub`, validation hook state, and store clear operation.
- Produces: One transition-safe flow that clears prior user state, validates the new user, and delays active-session redirects until validation finishes.

- [ ] **Step 1: Write failing transition tests**

  Assert:

  ```ts
  it("clears local cashier session and kc_token on logout cleanup", () => {
    // Seed localStorage and store, call cleanup, assert both are absent.
  });

  it("does not call the cashier-session close endpoint during logout cleanup", () => {
    // Assert cleanup only removes client state and performs no close request.
  });
  ```

- [ ] **Step 2: Run the focused transition tests and verify they fail**

  Run `npm test -- --run src/auth/sessionCleanup.test.ts` from `FE` and confirm the expected missing cleanup behavior.

- [ ] **Step 3: Implement user-aware checker and cleanup**

  Replace the one-time `hasCheckedRef` logic with a validation keyed by the current `sub`. On `sub` change, clear old local state, invalidate the current query if needed, and start a new Backend validation. Redirect cashiers only once `isValidated` is true and no active session exists. For non-cashier users, do not call the active cashier endpoint.

  Register cleanup for Keycloak logout/token invalidation and remove `kc_token`. Do not call the close-session mutation. Ensure the cleanup is idempotent.

  Update `CashierRouteGuard` so `requiresActiveSession` treats validation-pending and validation-error as blocked/loading states, never as proof that no session exists. It may continue to use the validated store session after the checker succeeds.

- [ ] **Step 4: Run transition tests, build, and lint**

  Run the focused tests, `npm run build`, and `npm run lint` from `FE`. Confirm that a stale cached session cannot render a protected route before the active-session request resolves.

- [ ] **Step 5: Commit the authentication integration**

  ```powershell
  git add FE/src/App.tsx FE/src/auth FE/src/features/cashier-sessions/components FE/src/main.tsx
  git commit -m "fix: limpiar y revalidar turno al cambiar de usuario"
  ```

### Task 6: Update explicit open/close flows and perform end-to-end verification

**Files:**
- Modify: `FE/src/features/cashier-sessions/hooks/useOpenCashierSession.ts`
- Modify: `FE/src/features/incomes/hooks/useCashClosing.ts`
- Modify: `FE/src/features/cashier-sessions/store/useCashierSessionStore.ts` if selectors need to distinguish validated state
- Create or modify: `FE/src/features/cashier-sessions` focused tests for open/close state transitions

**Interfaces:**
- Consumes: Authenticated `sub`, owner-aware store, open/close mutations.
- Produces: Explicit open stores the current user's session; successful close clears only after the Backend close succeeds; logout remains independent from close.

- [ ] **Step 1: Write failing open/close transition tests**

  Cover that a successful open stores `{ userId, session }`, a successful close clears the store, and a failed close retains the session so the user can retry.

- [ ] **Step 2: Run the focused tests and verify they fail**

  Run the focused command from `FE` and verify the existing callbacks do not satisfy the owner-aware contract.

- [ ] **Step 3: Update open/close callbacks**

  In `useOpenCashierSession`, obtain `keycloak.tokenParsed.sub` and call `setSessionForUser` after open success. In `useCashClosing`, clear the session only in the successful explicit close completion path; preserve it on API errors. Do not add any logout-to-close behavior.

- [ ] **Step 4: Run the complete verification suite**

  Run from `FE`: `npm run build` and `npm run lint`, plus the complete configured test command. Run from `BE`: `dotnet build SIGREF.API.sln --no-restore` and any available API tests. Verify branch status and inspect the final diff for unintended generated/build artifacts.

- [ ] **Step 5: Commit the open/close integration**

  ```powershell
  git add FE/src/features/cashier-sessions FE/src/features/incomes
  git commit -m "fix: sincronizar caché de turno con apertura y cierre"
  ```

## Final review checklist

- [ ] The active endpoint derives the user from authentication claims.
- [ ] Local storage entries are owner-scoped and legacy/malformed entries are discarded.
- [ ] Backend validation completes before any active-session redirect or protected cashier access.
- [ ] A non-404 validation failure never authorizes from stale local data.
- [ ] Logout clears `kc_token` and local cashier state but does not close the remote turn.
- [ ] Explicit close clears local state only after a successful Backend response.
- [ ] Frontend and Backend verification commands have fresh successful output.
