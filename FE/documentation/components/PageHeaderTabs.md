## 🔗 Navegación  

⬅️ [Volver Atrás](./index.md)  
🏠 [Volver al Inicio](/documentation/index.md)  

# 📌 PageHeaderTabs

`PageHeaderTabs` es un componente reutilizable de encabezado que incluye un título y pestañas de navegación. Está diseñado para trabajar con React Router y proporciona una navegación intuitiva entre diferentes secciones de una página.

## Props

| Prop | Tipo | Requerido | Descripción | Valor por defecto |
|------|------|-----------|-------------|-------------------|
| `title` | `string` | Sí | Título principal del encabezado | - |
| `tabs` | `TabItem[]` | Sí | Array de objetos que definen las pestañas | - |
| `defaultActive` | `string` | No | Clave de la pestaña activa por defecto | Primera pestaña |

### Interface TabItem

```tsx
interface TabItem {
  key: string;      // Identificador único de la pestaña
  label: string;    // Texto que se muestra en la pestaña
  path: string;     // Ruta de navegación (React Router)
}
```

## Uso Básico

```tsx
import { BrowserRouter as Router } from 'react-router-dom';

const App = () => {
  const tabs = [
    { key: 'overview', label: 'Resumen', path: '/dashboard/overview' },
    { key: 'analytics', label: 'Analíticas', path: '/dashboard/analytics' },
    { key: 'reports', label: 'Reportes', path: '/dashboard/reports' },
  ];

  return (
    <Router>
      <PageHeaderTabs
        title="Panel de Control"
        tabs={tabs}
        defaultActive="overview"
      />
    </Router>
  );
};
```

## Ejemplo Completo con React Router

```tsx
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';

const Dashboard = () => {
  const dashboardTabs = [
    { key: 'home', label: 'Inicio', path: '/dashboard' },
    { key: 'users', label: 'Usuarios', path: '/dashboard/users' },
    { key: 'settings', label: 'Configuración', path: '/dashboard/settings' },
    { key: 'billing', label: 'Facturación', path: '/dashboard/billing' },
  ];

  return (
    <div>
      <PageHeaderTabs
        title="Dashboard Principal"
        tabs={dashboardTabs}
        defaultActive="home"
      />
      
      <Routes>
        <Route path="/dashboard" element={<DashboardHome />} />
        <Route path="/dashboard/users" element={<UsersPage />} />
        <Route path="/dashboard/settings" element={<SettingsPage />} />
        <Route path="/dashboard/billing" element={<BillingPage />} />
      </Routes>
    </div>
  );
};

const App = () => {
  return (
    <Router>
      <Routes>
        <Route path="/dashboard/*" element={<Dashboard />} />
      </Routes>
    </Router>
  );
};
```

## Comportamiento

### Navegación
- Las pestañas funcionan como enlaces de React Router
- Al hacer clic en una pestaña, se navega a la ruta especificada y se marca como activa
- La pestaña activa se sincroniza automáticamente con la ruta actual

### Estados Visuales
- **Pestaña activa**: Borde gris en los lados y superior, sin borde inferior, texto secundario
- **Pestaña inactiva**: Sin bordes, texto primario
- **Línea inferior**: Línea gris continua debajo de todas las pestañas

## Personalización de Estilos

El componente utiliza clases de Tailwind CSS. Puedes personalizar los estilos modificando las clases en el componente:

### Colores
- `text-general-primary`: Color del título
- `text-secondary`: Color del texto de pestaña activa
- `text-primary`: Color del texto de pestaña inactiva
- `bg-gray-300`: Color de la línea inferior y bordes

### Espaciado y Tamaños
- `px-4 py-2`: Padding de las pestañas
- `text-3xl`: Tamaño del título
- `gap-3`: Espacio entre pestañas
- `rounded-t-md`: Bordes redondeados en la parte superior

## Ejemplo con Datos Dinámicos

```tsx
const UserProfile = ({ userId }) => {
  const userTabs = [
    { key: 'profile', label: 'Perfil', path: `/users/${userId}/profile` },
    { key: 'activity', label: 'Actividad', path: `/users/${userId}/activity` },
    { key: 'permissions', label: 'Permisos', path: `/users/${userId}/permissions` },
  ];

  return (
    <PageHeaderTabs
      title={`Usuario #${userId}`}
      tabs={userTabs}
      defaultActive="profile"
    />
  );
};
```

## Consideraciones

1. **React Router**: El componente requiere que esté dentro de un `Router` de React Router
2. **Rutas coincidentes**: Asegúrate de que las rutas en las pestañas coincidan con las configuradas en tu router
3. **Claves únicas**: Cada pestaña debe tener una `key` única
4. **Responsive**: Considera agregar estilos responsive si trabajas con muchas pestañas

## Solución de Problemas

### La pestaña activa no se resalta
- Verifica que la ruta actual coincida con alguna de las rutas en las pestañas
- Asegúrate de que el componente esté dentro de un `Router`

### Error de navegación
- Confirma que las rutas en las pestañas estén configuradas en tu router principal
- Verifica que los paths sean correctos

### Estilos no se aplican
- Revisa que las clases de Tailwind CSS estén disponibles en tu proyecto
- Verifica que no haya conflictos con otros estilos