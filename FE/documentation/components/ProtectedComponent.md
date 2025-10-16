## 🔗 Navegación  

⬅️ [Volver Atrás](./index.md)  
🏠 [Volver al Inicio](/documentation/index.md)  

# 📌 ProtectedComponent

Componente de React para **proteger contenido según los roles de usuario** obtenidos desde **Keycloak**.
Permite renderizar o ocultar secciones de la interfaz basándose en los permisos definidos en el token JWT del usuario.

## ⚙️ Props

| Propiedad            | Tipo              | Requerido | Descripción                                                                                                                                     |
| -------------------- | ----------------- | --------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| `allowedRoles`       | `UserRole[]`      | ✅         | Lista de roles permitidos para visualizar el contenido.                                                                                         |
| `children`           | `React.ReactNode` | ✅         | Contenido protegido que se mostrará solo si el usuario tiene permiso.                                                                           |
| `fallback`           | `React.ReactNode` | ❌         | Elemento alternativo que se muestra cuando el usuario no tiene los roles necesarios. (Por defecto: `null`)                                      |
| `hideIfUnauthorized` | `boolean`         | ❌         | Si es `true`, el componente no renderiza nada cuando el usuario no tiene permisos. Si es `false`, muestra el `fallback`. (Por defecto: `false`) |

## 🔐 Roles válidos

Los roles disponibles se definen en el módulo `../../auth` y son los siguientes:

```ts
// Roles válidos del sistema
admin | cashier | ti | auditor
```

## 🧠 Lógica interna

El componente utiliza un **hook personalizado** `useUserRoles()` para:

1. Verificar si el usuario está autenticado en Keycloak.
2. Obtener todos los roles del token JWT.
3. Filtrar únicamente los roles válidos definidos en `validRoles`.
4. Comparar los roles del usuario con los roles requeridos (`allowedRoles`).

Si el usuario cumple con al menos uno de los roles permitidos, el contenido se renderiza.

## 💡 Ejemplo de uso

```tsx
<ProtectedComponent allowedRoles={['admin', 'ti']}>
  <AdminPanel />
</ProtectedComponent>
```

➡️ En este ejemplo, **solo los usuarios con rol `admin` o `ti`** podrán ver el componente `<AdminPanel />`.

## ⚠️ Ejemplo con `fallback` y `hideIfUnauthorized`

```tsx
<ProtectedComponent
  allowedRoles={['cashier']}
  fallback={<p>No tienes permisos para acceder a esta sección.</p>}
  hideIfUnauthorized={false}
>
  <CashierDashboard />
</ProtectedComponent>
```

* Si el usuario **no tiene** el rol `cashier`:

  * Se mostrará el mensaje de fallback si `hideIfUnauthorized = false`.
  * No se mostrará nada si `hideIfUnauthorized = true`.

## 🧩 Dependencias

* [`@react-keycloak/web`](https://www.npmjs.com/package/@react-keycloak/web): para obtener el contexto de autenticación.
* Funciones auxiliares de `../../auth`:

  * `getRolesFromToken(keycloak)` → Extrae los roles del JWT.
  * `validRoles` → Define los roles válidos del sistema.

