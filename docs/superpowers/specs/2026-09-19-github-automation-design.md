# Diseño de automatización de GitHub

## Objetivo

Configurar una automatización de GitHub por responsabilidades y rutas reales del repositorio, minimizando ejecuciones de GitHub Actions y comentarios duplicados. Las áreas de código son `BE/` (.NET 9) y `FE/` (React, Vite y TypeScript). La documentación está en `docs/`, `FE/docs/` y `FE/documentation/`.

## Principios de ejecución

- Los workflows de código usan filtros de ruta: cambios de `BE/**` no ejecutan frontend y cambios de `FE/**` no ejecutan backend.
- Los cambios exclusivos de documentación no ejecutan build, tests, SonarQube Cloud, CodeQL ni cobertura.
- Los trabajos costosos de los PR —CI completa, SonarQube Cloud, CodeQL y cobertura— se omiten mientras el PR sea borrador. Se ejecutan con `ready_for_review` y en los pushes posteriores cuando el PR ya no es borrador.
- El validador de mensajes de commit y espacios finales se mantiene como comprobación ligera para todos los PR.
- Cada workflow usa concurrencia por workflow y PR/ref con `cancel-in-progress: true`, caché de dependencias y un límite de tiempo explícito.

## Código generado y artefactos excluidos

Las herramientas de análisis y revisión excluyen las siguientes rutas o patrones:

- `FE/src/api/generated/**`: cliente TypeScript generado por Orval.
- `BE/SIGREF.Infrastructure.Persistence/Migrations/**`: migraciones y snapshot generados por Entity Framework.
- `**/*.Designer.cs`, `**/*.g.cs`, `**/bin/**`, `**/obj/**`.
- `**/node_modules/**`, `**/dist/**`, `**/build/**`, `**/coverage/**`, `**/*.min.js`, `**/*.min.css` y lockfiles para revisión estática o IA.

Los archivos fuente del generador continúan dentro del alcance: `FE/orval.config.ts`, `FE/src/api/mutator/**` y cambios de contrato/API en backend.

## Workflows

### Frontend CI

Archivo: `.github/workflows/frontend-ci.yml`.

- Eventos: `pull_request` no borrador y `push` a `main`.
- Rutas: `FE/**`, excepto `FE/docs/**` y `FE/documentation/**` cuando son cambios exclusivos de documentación.
- Comandos existentes: `npm ci`, `npm run lint`, `npm run typecheck`, `npm run test:run` y `npm run build` desde `FE/`.
- Caché: npm con `FE/package-lock.json`.
- No ejecuta generación de Orval: esta requiere `ORVAL_API_URL` y no es parte de la validación reproducible del cambio.

### Backend CI

Archivo: `.github/workflows/backend-ci.yml`.

- Eventos: `pull_request` no borrador y `push` a `main`.
- Rutas: `BE/**`.
- Comandos: `dotnet restore`, `dotnet build` y `dotnet test` sobre `BE/SIGREF.API.sln`.
- Caché: NuGet.

### CodeQL

Archivo: `.github/workflows/codeql.yml`, con configuración en `.github/codeql/config.yml`.

- Eventos: PR no borrador con código relevante, `push` a `main` con código relevante y ejecución semanal.
- Jobs independientes y condicionados por rutas para `csharp` (`BE/**`) y `javascript-typescript` (`FE/**`).
- C# usa `build-mode: none`, que evita una compilación duplicada y admite exclusiones por ruta; TypeScript no requiere compilación para el análisis.
- La ejecución semanal analiza ambas áreas, aunque no haya cambios, para detectar nuevas reglas o vulnerabilidades.

### SonarQube Cloud

Archivo: `.github/workflows/sonar.yml`, con propiedades independientes para backend y frontend.

- Eventos: PR no borrador con cambios de código relevante y `push` a `main`.
- Dos proyectos de SonarQube Cloud: uno para `BE/` y otro para `FE/`. Esto impide que un cambio en una área fuerce el análisis de la otra.
- El análisis de frontend usa la configuración de fuentes, pruebas y exclusiones de `FE/`. El backend usa SonarScanner for .NET y construye solo la solución de backend requerida por el escáner.
- Sonar no ejecuta pruebas ni sube cobertura; cobertura se gestiona en el workflow de Codecov para evitar trabajo duplicado.

### Cobertura y Codecov

Archivo: `.github/workflows/coverage.yml`, configuración en `codecov.yml`.

- Eventos: PR no borrador y `push` a `main`, filtrados por área.
- Frontend: `npm run test:run -- --coverage`, que usa el proveedor V8 y genera LCOV conforme a `FE/vitest.config.ts`.
- Backend: `dotnet test` con colector Cobertura. Para habilitarlo se agrega `coverlet.collector` como dependencia de desarrollo del proyecto de pruebas.
- Los reportes se suben con flags separados `frontend` y `backend`.
- Los estados de cobertura son informativos y el comentario de Codecov se limita al patch; aparece solo ante caída de cobertura o líneas nuevas sin cubrir. No se bloquea por deuda histórica.

### Reglas de commits

Se conserva `.github/workflows/commit-rules.yml` como workflow ligero para todos los PR. Valida títulos de commit menores de 200 caracteres y ausencia de espacios finales en mensajes y archivos modificados.

## Revisión automatizada

### CodeRabbit

Archivo: `.coderabbit.yaml`.

- Es el único revisor automático de IA para PR; GitHub Copilot puede conservarse para revisiones manuales, pero no debe habilitarse como revisor automático para evitar duplicación.
- Revisa únicamente PR no borrador contra `main`.
- Excluye documentación, código generado, binarios, artefactos de compilación, lockfiles y dependencias vendorizadas.
- Prioriza correctness, regresiones, seguridad, tipos, concurrencia/estado, rendimiento relevante y pruebas faltantes del comportamiento cambiado.
- Omite formato, estilo, cambios de nombres sin impacto, deuda histórica y hallazgos que ya cubren ESLint, Prettier, SonarQube Cloud, CodeQL o Codecov salvo impacto grave.

### Política global

Archivo: `.github/REVIEW_POLICY.md`.

Define la responsabilidad de cada herramienta y el criterio de relevancia para evitar análisis o comentarios fuera del diff y del área afectada.

## Dependencias y seguridad de la cadena de suministro

### Renovate

Archivo: `renovate.json`.

- Gestiona actualizaciones normales de npm (`FE/`), NuGet (`BE/`), GitHub Actions y las imágenes Docker/Compose detectadas.
- Límite de 5 PR abiertas y 2 PR nuevas por hora.
- Ventana semanal en `America/Tegucigalpa`.
- Agrupa minor/patch de devDependencies y ecosistemas compatibles: ESLint, TypeScript, pruebas, Vite/build y React/TanStack.
- Majors son PR individuales, requieren revisión humana y no tienen automerge.

### Dependabot

Archivo: `.github/dependabot.yml`.

- Cubre npm, NuGet, GitHub Actions y Docker/Compose detectados.
- Define `open-pull-requests-limit: 0` para desactivar actualizaciones de versión normales y conservar únicamente las actualizaciones de seguridad.

### Snyk y Codacy

No se agregan. SonarQube Cloud cubre calidad, mantenibilidad, bugs, code smells y duplicación; CodeQL cubre seguridad de código; Dependabot y Renovate cubren dependencias y bases de imagen. Añadir Codacy duplicaría SonarQube Cloud, y Snyk añadiría análisis redundante para el alcance actual sin IaC adicional.

## Servicios externos y configuración manual

- Instalar y autorizar la GitHub App de CodeRabbit.
- Activar Renovate para el repositorio.
- Activar Dependabot alerts y Dependabot security updates.
- Crear/importar los dos proyectos de SonarQube Cloud, enlazados a este repositorio, y configurar sus project keys.
- Activar GitHub Code Security/CodeQL si el repositorio privado o el plan de la organización lo requiere.
- Activar Codecov y, si el repositorio es privado, configurar token de subida.

Secrets y variables:

- `SONAR_TOKEN` (secret), `SONAR_ORGANIZATION`, `SONAR_BACKEND_PROJECT_KEY` y `SONAR_FRONTEND_PROJECT_KEY` (variables de repositorio).
- `CODECOV_TOKEN` (secret cuando Codecov lo requiera; puede ser opcional para repositorios públicos).

## Protección de ramas

Los workflows que usan filtros de ruta pueden no crear un check cuando el área no cambió. Por ello no deben marcarse como required checks tradicionales sin una regla de aplicación por rutas, ya que un PR de documentación quedaría esperando un check inexistente.

Se recomienda exigir `Validar reglas de commits / Commits y espacios finales` y, cuando GitHub Rulesets lo permita por rutas, requerir CI frontend para `FE/**` y CI backend para `BE/**`. CodeQL, SonarQube Cloud y Codecov deben comenzar como checks informativos y pasar a required solo después de confirmar que sus integraciones y filtros entregan estados consistentes para cada área.
