# 📌 Cómo Configurar Keycloak para el Frontend (FE)

Este documento describe paso a paso cómo configurar Keycloak para integrarlo con el frontend de tu aplicación.

---

## Paso 1 — Acceder a Keycloak

1. Asegúrate de que tu API esté levantada.  
2. En la terminal, se mostrará una URL del DashBord del BE. Ábrela en tu navegador.  
3. Se abrirá una pestaña llamada **SIGREF.API** donde verás un listado de servicios en ejecución.  
4. Busca el servicio **Keycloak Server** y haz **Ctrl + clic** para abrirlo.

---

## Paso 2 — Iniciar sesión y acceder a Clients

1. Inicia sesión con tus credenciales de administrador (`admin`).  
2. En el panel lateral izquierdo, selecciona la opción **Clients**.

---

## Paso 3 — Crear un Cliente

1. Dentro de **Clients**, haz clic en **Create Client**.  
2. Completa los siguientes campos:
   - **Client ID**: Puede ser cualquier nombre identificativo (por ejemplo: `frontend-app`).
   - **Client Name**: El nombre que prefieras.  
3. Haz clic en **Next**.  
4. En la configuración de autenticación que aparece, déjala tal como está y haz clic en **Next**.

---

## Paso 4 — Configurar URLs del Cliente

1. En la sección **Login Settings**, verás varios campos relacionados con URLs.  
2. El campo que nos interesa es **Valid Redirect URIs**.  
3. Coloca la URL correspondiente de tu frontend (FE): http://localhost:5173/*
4. El anterior campo es obligatorio, una vez hecho haz clic en **guardar**

---

## Paso 5 — Configuracion del .env

1. Para la variable de **Realm**, en caso de que no hayas creado uno, puedes usar el que se da por defecto que es **`master`**
2. En la variable de **client Id**, se debe de poner el client Id que creaste en keycloak escrito de la misma forma que lo hiciste en keycloak.

## Paso 6 — Crear un Usuario
1. Haz clic en **Users** del lado izquierdo.
2. clic en **Add User**, pones tu informacion y lo guardas.
3. Cuando lo creas veras que en la parte arriba de configuraciones sale la opciones de **credenciales**
4. Haz clic, esto te llevara a ponerle una clave a tu nuevo usuario.

Cuando tengas tu usuario listo ya podras usarlo para iniciar sesión.