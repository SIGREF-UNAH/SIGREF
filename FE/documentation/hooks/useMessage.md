## 🔗 Navegación  

⬅️ [Volver Atrás](./index.md)  
🏠 [Volver al Inicio](/documentation/index.md)  

# 📢 useMessage

Hook personalizado para mostrar **mensajes (`message`)** y **notificaciones (`notification`)** de **Ant Design**.  
Permite manejar fácilmente mensajes globales con íconos personalizados, duraciones configurables y callbacks, unificando la API de mensajes y notificaciones bajo una sola interfaz.

---

## 1. 🔧 API
```ts
const msg = useMessage()
```

| Método                                             | Descripción                                       | Tipo de componente |
| -------------------------------------------------- | ------------------------------------------------- | ------------------ |
| `msg.success(content, duration?, options?)`        | Muestra un mensaje de éxito                       | `message`          |
| `msg.error(content, duration?, options?)`          | Muestra un mensaje de error                       | `message`          |
| `msg.warning(content, duration?, options?)`        | Muestra una advertencia                           | `message`          |
| `msg.info(content, duration?, options?)`           | Muestra un mensaje informativo                    | `message`          |
| `msg.loading(content, duration?, options?)`        | Muestra un mensaje de carga                       | `message`          |
| `msg.notifySuccess(title, description?, options?)` | Notificación de éxito                             | `notification`     |
| `msg.notifyError(title, description?, options?)`   | Notificación de error                             | `notification`     |
| `msg.notifyWarning(title, description?, options?)` | Notificación de advertencia                       | `notification`     |
| `msg.notifyInfo(title, description?, options?)`    | Notificación informativa                          | `notification`     |
| `msg.notify(options)`                              | Notificación personalizada (sin tipo predefinido) | `notification`     |
| `msg.destroy(key?)`                                | Cierra un mensaje o notificación específicos      | Ambos              |
| `msg.destroyAll()`                                 | Cierra todos los mensajes y notificaciones        | Ambos              |
| `msg.raw.message`                                  | Acceso directo a `App.useApp().message`           | -                  |
| `msg.raw.notification`                             | Acceso directo a `App.useApp().notification`      | -                  |

---

## 2. ⚙️ Tipos

### MessageType
```ts
type MessageType = "success" | "error" | "warning" | "info" | "loading"; 
```

### MessageOptions
```ts
interface MessageOptions {
  content: string | ReactNode;  
  duration?: number; 
  icon?: ReactNode;
  key?: string;
  style?: React.CSSProperties;
  className?: string;
  onClose?: () => void;
}
```

### NotificationOptions
```ts
interface NotificationOptions {
  title: string;
  description?: string | ReactNode;
  duration?: number;
  icon?: ReactNode;
  placement?: "topLeft" | "topRight" | "bottomLeft" | "bottomRight";
  onClick?: () => void;
  onClose?: () => void;
  btn?: ReactNode;
  key?: string;
  className?: string;
  style?: React.CSSProperties;
}
```

---

## 💡 Ejemplo Básico
```tsx
import { App } from "antd";
import { useMessage } from "../hooks/useMessage";

function ExamplePage() {
  const msg = useMessage();

  const handleSave = () => {
    msg.success("Guardado correctamente");
  };

  const handleError = () => {
    msg.error("Ocurrió un error al guardar");
  };

  return (
    <App>
      <button onClick={handleSave}>Guardar</button>
      <button onClick={handleError}>Error</button>
    </App>
  );
}
```

> 🧩 **Importante:** Para que `message` y `notification` funcionen, el hook debe usarse dentro de un `<App>` de Ant Design.

---

## 🔔 Ejemplo con Notificaciones
```tsx
function NotificationsExample() {
  const msg = useMessage();

  const handleNotify = () => {
    msg.notifySuccess("Operación Exitosa", "El registro fue creado correctamente");
  };

  const handleWarning = () => {
    msg.notifyWarning("Advertencia", "Verifica los datos ingresados");
  };

  return (
    <div>
      <button onClick={handleNotify}>Mostrar Notificación</button>
      <button onClick={handleWarning}>Mostrar Advertencia</button>
    </div>
  );
}
```

---

## 🧠 Ejemplo Avanzado: Cierre Manual
```tsx
function CustomExample() {
  const msg = useMessage();

  const showPersistentMessage = () => {
    msg.loading("Procesando solicitud...", 0, { key: "loading" });

    setTimeout(() => {
      msg.destroy("loading");
      msg.success("Operación completada");
    }, 3000);
  };

  return <button onClick={showPersistentMessage}>Ejecutar proceso</button>;
}
```

---

## 🎨 Personalización

Cada método acepta propiedades adicionales para ajustar el estilo:
```tsx
msg.success("Listo", 3, {
  style: { background: "#f6ffed", border: "1px solid #b7eb8f" },
  className: "custom-message",
});
```

También puedes cambiar el ícono:
```tsx
msg.info("Información con ícono personalizado", 3, {
  icon: <InfoCircleOutlined style={{ color: "purple" }} />,
});
```

---

## 🧹 Limpieza Global

Cierra todos los mensajes y notificaciones abiertos:
```tsx
msg.destroyAll();
```

O uno específico por clave:
```tsx
msg.destroy("loading");
```

---

## 🧩 Mejores Prácticas

### ✅ DO

- Usa claves (`key`) únicas cuando muestres mensajes persistentes.
- Mantén las notificaciones breves y con propósito.
- Utiliza `msg.loading` seguido de `msg.success` o `msg.error` para procesos asíncronos.
- Agrupa mensajes similares para evitar spam visual.

### ❌ DON'T

- No uses mensajes para reemplazar validaciones de formulario.
- No muestres notificaciones largas o con mucho texto.
- No llames `message.open()` directamente si ya tienes `useMessage`.

---

## 🧰 Casos Comunes

### 1. Mensaje al guardar datos
```tsx
try {
  await saveData();
  msg.success("Datos guardados con éxito");
} catch {
  msg.error("Error al guardar los datos");
}
```

### 2. Proceso de carga
```tsx
msg.loading("Cargando...");
setTimeout(() => {
  msg.destroy();
  msg.success("Completado");
}, 2000);
```

### 3. Notificación al abrir página
```tsx
useEffect(() => {
  msg.notifyInfo("Bienvenido", "Explora las opciones del panel");
}, []);
```