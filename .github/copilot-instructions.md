# Instrucciones para GitHub Copilot

Copilot se usa como asistencia manual de desarrollo. No habilites la revisión automática de Copilot en este repositorio: CodeRabbit es el único revisor automático de Pull Requests para evitar duplicar comentarios y consumo.

- Revisa primero el contexto cercano y conserva el estilo ya usado por el proyecto.
- Prioriza bugs reales, regresiones, seguridad, tipos, concurrencia/estado y pruebas relevantes.
- No propongas refactors ajenos al cambio, renombres subjetivos ni comentarios de formato que cubran ESLint, Prettier o `dotnet format`.
- Omite código generado, `node_modules`, `dist`, `build`, `coverage`, `bin`, `obj` y lockfiles salvo incidencias de seguridad o integridad.
- No recomiendes saltarse autenticación, autorización, validación, tests o controles de seguridad.
