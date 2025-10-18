## 🔗 Navegación

⬅️ [Volver Atrás](./index.md)
🏠 [Volver al Inicio](/documentation/index.md)

# 🛡️ CASL & Keycloak – Control de Permisos

Sistema de control de acceso basado en **CASL** y **Keycloak** para proteger rutas, componentes y acciones dentro de la aplicación según el rol del usuario.

---

## ⚙️ Archivos principales

| Archivo              | Descripción                                                     |
| -------------------- | --------------------------------------------------------------- |
| `abilities.ts`       | Define qué acciones puede realizar cada rol (permisos base).    |
| `abilityProvider.ts` | Crea el contexto global de CASL para React.                     |
| `ProtectedRoute.tsx` | Protege páginas completas.                                      |

---

## ⚙️ Etiquetas principales
| Etiqueta             | Descripción                                                     |
| `Can`                | Protege componentes, botones o secciones dentro de la interfaz. |

---

## 🔐 Roles válidos

Los roles disponibles se definen en `roles.ts`:

```ts
admin | cashier | ti | auditor
```

---

## 🧩 Definición de permisos (`abilities.ts`)

Ejemplo de cómo se definen las habilidades por rol:

```ts
if (roles.includes('admin')) {
  can('manage', 'all');
}

if (roles.includes('cashier')) {
  can(['create', 'read', 'update'], 'Fondos');
  can(['create', 'read', 'update'], 'Pacientes');
}

if (roles.includes('auditor')) {
  can('read', 'Servicios');
  can('read', 'Eventos');
}
```

| Acción   | Descripción                      |
| -------- | -------------------------------- |
| `read`   | Puede ver el contenido.          |
| `create` | Puede crear registros.           |
| `update` | Puede modificar registros.       |
| `delete` | Puede eliminar registros.        |
| `manage` | Puede realizar cualquier acción. |

---

## 🧱 Protección de Rutas (`ProtectedRoute`)

Permite restringir el acceso a **páginas completas** según permisos.

```tsx
<ProtectedRoute action="read" subject="Empleados">
  <EmployeesPage />
</ProtectedRoute>
```

➡️ Solo usuarios con permiso `read` sobre `Empleados` podrán ver la página.

---

## 🔒 Protección de Componentes (`Can`)

Componente para proteger secciones específicas, botones o ítems.

```tsx
<Can I="read" a="Pacientes" ability={ability}>
  <Button type="primary">Nuevo Paciente</Button>
</Can>
```

➡️ Solo usuarios con permiso `create` sobre `Pacientes` verán este botón.

---

## 🧭 Ejemplo en la Página Principal

```tsx
<Can I="read" a="Empleados" ability={ability}>
  <ModuleCard
    title="Gestión de Empleados"
    description={"Lleve a cabo las tareas de gestión de los empleados del hospital"}
    icon={<TeamOutlined />}
    shortcut="Ctrl + E"
    path="/practitioners/list"
  />
</Can>
```

➡️ El módulo solo será visible si el usuario puede `read` sobre `Empleados`.

---

