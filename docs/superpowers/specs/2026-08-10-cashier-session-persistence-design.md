# Persistencia segura de sesiones de turno — Diseño

## Objetivo

Evitar que un cajero herede en el Frontend el turno persistido de otro cajero que utiliza el mismo navegador, manteniendo `localStorage` como una caché rápida pero haciendo que el Backend sea siempre la fuente de verdad.

El logout no cerrará automáticamente el turno de caja. Si el cajero no lo cerró explícitamente, el Backend lo conservará abierto para que pueda retomarlo después.

## Alcance

- Validar la sesión activa del cajero autenticado contra el Backend al iniciar o cambiar de usuario.
- Asociar la caché local al usuario autenticado.
- Limpiar la caché local al logout, expiración o cambio de usuario.
- Recuperar desde Backend el turno válido y actualizar la caché local.
- Mantener el cierre de caja como la única operación que cierra la sesión remota.
- Cubrir el comportamiento con pruebas de regresión.

No se cambiará la política de negocio del Backend para cerrar turnos durante logout.

## Arquitectura y flujo

### Backend como fuente de verdad

Se expondrá `GET /api/CashierSessions/active`, protegido por autenticación y rol `cashier`.

- El identificador del usuario se obtiene exclusivamente de `IUserContextService`.
- El cliente no enviará `userId`; así no podrá consultar ni reutilizar la sesión de otro usuario.
- Si existe una sesión abierta, responderá `200` con el detalle de la sesión.
- Si no existe, responderá `404` con el código `CASHIER_NO_ACTIVE_SESSION`.

El método de servicio existente `GetActiveSessionByUserAsync` se reutilizará para evitar duplicar la consulta y sus reglas.

### Caché local por usuario

El store de Zustand mantendrá el turno persistido con el propietario del dato, por ejemplo:

```ts
{
  userId: string;
  session: CashierSession;
}
```

El store tendrá operaciones explícitas para:

- establecer la sesión validada para un usuario;
- obtener la caché solo si pertenece al usuario actual;
- limpiar la sesión y su persistencia;
- marcar si la validación remota de la sesión actual está pendiente o finalizada.

Un valor local sin `userId`, con un `userId` diferente o con una estructura inválida se descartará. La caché no concederá acceso por sí sola.

### Validación al autenticar

`CashierSessionChecker` ejecutará una validación por usuario autenticado:

1. Esperar a que Keycloak esté inicializado y autenticado.
2. Obtener el identificador estable del usuario desde el token (`sub`).
3. Eliminar cualquier estado local perteneciente a otro usuario.
4. Consultar `GET /api/CashierSessions/active`.
5. Si la respuesta es `200`, actualizar Zustand con la sesión devuelta por Backend.
6. Si la respuesta es `404`, limpiar la caché y considerar que no hay turno activo.
7. Si ocurre un error distinto de 404, no tratar la caché como válida para autorizar; mostrar/retener un estado de validación y evitar redirecciones prematuras.
8. Solo después de terminar la validación, redirigir a apertura de turno si el cajero no tiene sesión activa.

La consulta se repetirá cuando cambie el `sub` del token. No se usará un `useRef` global que impida validar a un segundo usuario en la misma pestaña.

### Logout y cambio de usuario

El flujo de logout limpiará:

- la sesión del store y su entrada persistida;
- el token `kc_token` utilizado por el interceptor HTTP;
- cualquier marcador de usuario validado.

No invocará `POST /api/CashierSessions/{id}/close`.

Si Keycloak cambia de usuario sin recargar completamente la aplicación, el cambio de `sub` tendrá el mismo efecto de limpieza y validación como un logout/login.

### Apertura y cierre explícitos

- Tras abrir correctamente un turno, el Frontend guardará la respuesta contextualizada con el usuario autenticado.
- Tras cerrar correctamente un turno, el Frontend limpiará el store y la caché local.
- Las rutas protegidas por turno activo dependerán del estado validado, no del contenido bruto de `localStorage`.

## Manejo de errores

- `404 CASHIER_NO_ACTIVE_SESSION`: estado normal; se limpia la caché.
- `401/403`: se limpia la caché y se deja que el flujo de autenticación/autorización actúe.
- Error de red o `5xx`: se conserva la caché solo como información visual opcional, pero no se considera validada ni se permite usarla para pasar una guarda que exige turno. Se mostrará el estado de carga/error definido por la aplicación.
- JSON corrupto o esquema local antiguo: se elimina de forma segura mediante migración/versionado del persistido.

## Pruebas

### Store

- Una sesión guardada para Cajero A no aparece como activa para Cajero B.
- Un dato legado sin propietario se descarta.
- `clearSession` elimina tanto el estado en memoria como la persistencia.

### Validación

- Una sesión válida devuelta por Backend reemplaza la caché local.
- Un `404` elimina la caché y redirige a apertura solo después de finalizar la validación.
- Un error de Backend no permite que una caché local antigua habilite rutas protegidas.
- Cambiar de `sub` ejecuta una nueva validación y no conserva el resultado del usuario anterior.

### Autenticación

- Logout limpia el token y la sesión local.
- Logout no invoca el endpoint de cierre.
- Abrir y cerrar explícitamente actualizan el store correctamente.

## Criterios de aceptación

- Cajero B nunca ve ni puede usar el turno local de Cajero A.
- Al iniciar sesión, el turno mostrado coincide con la respuesta actual del Backend.
- Si el turno remoto fue cerrado mientras el navegador estaba cerrado, el Frontend elimina la caché al volver a validar.
- Si el cajero cierra logout con un turno abierto, el Backend conserva el turno abierto y la caché local se elimina.
- Las guardas no autorizan operaciones de caja basándose únicamente en `localStorage`.
