# Política de revisión automática

Los sistemas automáticos revisan solo el diff del Pull Request y el contexto estrictamente necesario para entenderlo. No deben convertir deuda histórica, código no modificado o preferencias subjetivas en comentarios nuevos.

## Prioridad de hallazgos

1. Correctness, bugs reales y regresiones.
2. Seguridad.
3. Errores de lógica, contratos, tipos, concurrencia o estado.
4. Regresiones significativas de rendimiento.
5. Tests faltantes cuando el comportamiento modificado lo requiera.

## Exclusiones

No se revisan archivos generados, dependencias vendorizadas, outputs de compilación, cobertura, minificados ni lockfiles, excepto por un problema concreto de integridad o seguridad. En particular se excluyen `FE/src/api/generated/**`, las migraciones de Entity Framework y los artefactos `bin`, `obj`, `node_modules`, `dist`, `build` y `coverage`.

Los comentarios de formato ya resueltos por Prettier, ESLint o `dotnet format`, refactors fuera de alcance, cambios de nombres sin beneficio claro y recomendaciones subjetivas se omiten.

## Responsabilidades

| Sistema | Responsabilidad |
| --- | --- |
| GitHub Actions | Build, lint, typecheck y tests. |
| CodeRabbit | Lógica y riesgos del diff de un PR no borrador. |
| SonarQube Cloud | Calidad, mantenibilidad, bugs, code smells y duplicación. |
| CodeQL | Seguridad del código fuente. |
| Codecov | Cobertura de código modificado. |
| Renovate | Actualizaciones normales de dependencias. |
| Dependabot | Alertas y actualizaciones de seguridad. |

Cada sistema evita repetir findings ya cubiertos por otro, salvo un error grave. Copilot queda disponible para asistencia manual y no como segundo revisor automático.

Snyk no se habilita: no hay infraestructura como código en el repositorio que requiera un escáner adicional, mientras CodeQL cubre el código fuente y Renovate/Dependabot cubren dependencias. Codacy tampoco se habilita porque SonarQube Cloud ya cubre calidad, mantenibilidad, duplicación y code smells.
