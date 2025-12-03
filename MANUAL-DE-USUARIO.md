# 📖 Manual de Usuario - SIGREF
## Sistema de Gestión de la Receptoría de Fondos

---

## 📑 Tabla de Contenidos

1. [Introducción](#1-introducción)
2. [Requisitos del Sistema](#2-requisitos-del-sistema)
3. [Acceso al Sistema](#3-acceso-al-sistema)
4. [Roles y Permisos](#4-roles-y-permisos)
5. [Módulos del Sistema](#5-módulos-del-sistema)
6. [Guía de Uso por Rol](#6-guía-de-uso-por-rol)
7. [Funcionalidades Comunes](#7-funcionalidades-comunes)
8. [Preguntas Frecuentes](#8-preguntas-frecuentes)
9. [Soporte Técnico](#9-soporte-técnico)

---

## 1. Introducción

### ¿Qué es SIGREF?

SIGREF (Sistema de Gestión de la Receptoría de Fondos) es una plataforma web moderna diseñada para gestionar y controlar los ingresos en servicios médicos. El sistema permite:

- ✅ Registrar ingresos y pagos de servicios médicos
- ✅ Gestionar información de pacientes
- ✅ Administrar servicios y paquetes médicos
- ✅ Controlar sesiones de caja y turnos
- ✅ Generar reportes administrativos y financieros
- ✅ Auditar todas las operaciones del sistema
- ✅ Gestionar empleados, ubicaciones y organizaciones

### Características Principales

- **Interfaz Intuitiva**: Diseño moderno y fácil de usar
- **Seguridad**: Autenticación mediante Keycloak con roles y permisos
- **Estándares de Salud**: Cumple con estándares HL7 y FHIR R5
- **Auditoría Completa**: Registro de todas las operaciones
- **Reportes en Tiempo Real**: Generación de reportes administrativos
- **Multi-rol**: Diferentes perfiles de usuario con permisos específicos

---

## 2. Requisitos del Sistema

### Requisitos Mínimos del Navegador

- **Navegadores Compatibles**:
  - Google Chrome 90+
  - Mozilla Firefox 88+
  - Microsoft Edge 90+
  - Safari 14+

- **Resolución de Pantalla**: Mínimo 1366x768 píxeles
- **Conexión a Internet**: Estable (mínimo 2 Mbps)

### Requisitos de Hardware Recomendados

- **Procesador**: Intel Core i3 o equivalente
- **Memoria RAM**: 4 GB mínimo
- **Espacio en Disco**: 100 MB para caché del navegador

---

## 3. Acceso al Sistema

### 3.1 Inicio de Sesión

1. **Abrir el navegador** y acceder a la URL del sistema proporcionada por su administrador
   - Ejemplo: `https://sigref.unah.edu.hn`

2. **Pantalla de Inicio de Sesión**:
   - El sistema redirigirá automáticamente a la página de autenticación de Keycloak
   - Ingrese su **nombre de usuario** o **correo electrónico**
   - Ingrese su **contraseña**
   - Haga clic en el botón **"Iniciar Sesión"**

3. **Primer Acceso**:
   - Si es su primer acceso, es posible que deba cambiar su contraseña
   - Siga las instrucciones en pantalla para establecer una contraseña segura

### 3.2 Recuperación de Contraseña

1. En la pantalla de inicio de sesión, haga clic en **"¿Olvidó su contraseña?"**
2. Ingrese su correo electrónico registrado
3. Recibirá un enlace de recuperación en su correo
4. Siga las instrucciones del correo para restablecer su contraseña

### 3.3 Cerrar Sesión

1. Haga clic en su **nombre de usuario** en la esquina superior derecha
2. Seleccione **"Cerrar Sesión"**
3. El sistema cerrará su sesión de forma segura

---

## 4. Roles y Permisos

El sistema SIGREF cuenta con 4 roles principales, cada uno con permisos específicos:

### 4.1 Administrador (admin)

**Descripción**: Tiene acceso completo a todas las funcionalidades del sistema.

**Permisos**:
- ✅ Gestión completa de ingresos y caja
- ✅ Administración de servicios médicos
- ✅ Gestión de paquetes de servicios
- ✅ Administración de pacientes
- ✅ Gestión de empleados
- ✅ Administración de ubicaciones
- ✅ Gestión de organizaciones
- ✅ Generación y consulta de reportes
- ✅ Visualización de eventos del sistema

**Módulos Disponibles**:
- Fondos (Ingresos)
- Servicios
- Paquetes
- Pacientes
- Empleados
- Ubicaciones
- Organizaciones
- Reportes
- Eventos

### 4.2 Auxiliar de Caja (cashier)

**Descripción**: Encargado de registrar ingresos y gestionar la caja diaria.

**Permisos**:
- ✅ Generar ingresos
- ✅ Consultar lista de ingresos
- ✅ Cerrar caja
- ✅ Ver historial de cierres de caja
- ✅ Consultar servicios médicos
- ✅ Consultar paquetes de servicios
- ✅ Gestionar información de pacientes

**Módulos Disponibles**:
- Fondos (Ingresos)
- Servicios (solo consulta)
- Paquetes (solo consulta)
- Pacientes

### 4.3 Auditor (auditor)

**Descripción**: Supervisa y audita las operaciones financieras y administrativas.

**Permisos**:
- ✅ Consultar todos los ingresos
- ✅ Ver historial de cierres de caja
- ✅ Consultar servicios médicos
- ✅ Consultar paquetes de servicios
- ✅ Ver información de empleados
- ✅ Visualizar eventos del sistema

**Módulos Disponibles**:
- Fondos (solo consulta)
- Servicios (solo consulta)
- Paquetes (solo consulta)
- Empleados (solo consulta)
- Eventos

### 4.4 Técnico de Informática (ti)

**Descripción**: Administra la configuración técnica del sistema.

**Permisos**:
- ✅ Gestión de empleados y roles
- ✅ Administración de ubicaciones
- ✅ Gestión de organizaciones
- ✅ Visualización de eventos del sistema
- ✅ Acceso al dashboard de monitoreo (Aspire Dashboard)
- ✅ Configuración de propiedades del hospital

**Módulos Disponibles**:
- Empleados
- Ubicaciones
- Organizaciones
- Eventos
- Dashboard de Monitoreo

---

## 5. Módulos del Sistema

### 5.1 Módulo de Fondos (Ingresos)

**Propósito**: Gestionar los ingresos por servicios médicos prestados.

#### Funcionalidades:

**a) Lista de Ingresos**
- Visualizar todos los ingresos registrados
- Filtrar por fecha, paciente, servicio, método de pago
- Paginación de resultados
- Exportar listados

**b) Generar Ingreso**
- Seleccionar paciente (buscar o crear nuevo)
- Agregar servicios individuales o paquetes
- Especificar método de pago:
  - Efectivo
  - Tarjeta de crédito/débito
  - Transferencia bancaria
  - Cheque
- Aplicar descuentos (si aplica)
- Generar factura automáticamente
- Imprimir recibo

**c) Cerrar Caja**
- Realizar arqueo de caja al final del turno
- Registrar efectivo, tarjetas y otros métodos de pago
- Calcular diferencias (faltantes o sobrantes)
- Generar reporte de cierre
- Imprimir comprobante de cierre

**d) Historial de Cierres de Caja**
- Consultar cierres anteriores
- Filtrar por fecha, cajero, turno
- Ver detalles de cada cierre
- Reimprimir comprobantes

### 5.2 Módulo de Servicios

**Propósito**: Administrar el catálogo de servicios médicos disponibles.

#### Funcionalidades:

**a) Lista de Servicios**
- Ver todos los servicios registrados
- Buscar por nombre, código o categoría
- Filtrar por estado (activo/inactivo)
- Ordenar por diferentes criterios

**b) Crear Servicio**
- Nombre del servicio
- Código único
- Descripción
- Categoría (consulta, laboratorio, imagen, procedimiento, etc.)
- Precio
- Duración estimada
- Estado (activo/inactivo)

**c) Editar Servicio**
- Modificar información del servicio
- Actualizar precios
- Cambiar estado

**d) Eliminar Servicio**
- Desactivar servicios obsoletos
- Mantener historial de servicios eliminados

### 5.3 Módulo de Paquetes

**Propósito**: Crear y gestionar paquetes de servicios con precios especiales.

#### Funcionalidades:

**a) Lista de Paquetes**
- Ver todos los paquetes disponibles
- Buscar por nombre o código
- Filtrar por estado

**b) Crear Paquete**
- Nombre del paquete
- Código único
- Descripción
- Seleccionar servicios incluidos
- Precio del paquete (generalmente con descuento)
- Vigencia (fecha inicio y fin)
- Estado (activo/inactivo)

**c) Editar Paquete**
- Modificar servicios incluidos
- Actualizar precios
- Cambiar vigencia

**d) Eliminar Paquete**
- Desactivar paquetes vencidos u obsoletos

### 5.4 Módulo de Pacientes

**Propósito**: Gestionar la información de los pacientes.

#### Funcionalidades:

**a) Lista de Pacientes**
- Ver todos los pacientes registrados
- Buscar por nombre, identidad, expediente
- Filtrar por diferentes criterios
- Paginación de resultados

**b) Crear Paciente**
- **Información Personal**:
  - Nombres y apellidos
  - Número de identidad
  - Fecha de nacimiento
  - Género
  - Estado civil
- **Información de Contacto**:
  - Teléfono(s)
  - Correo electrónico
  - Dirección completa
- **Información Médica**:
  - Tipo de sangre
  - Alergias
  - Condiciones médicas
- **Información Adicional**:
  - Contacto de emergencia
  - Aseguradora (si aplica)

**c) Editar Paciente**
- Actualizar información personal
- Modificar datos de contacto
- Agregar información médica

**d) Ver Historial**
- Consultar historial de servicios
- Ver facturas anteriores
- Revisar pagos realizados

### 5.5 Módulo de Empleados

**Propósito**: Administrar la información del personal del hospital.

#### Funcionalidades:

**a) Lista de Empleados**
- Ver todos los empleados registrados
- Buscar por nombre, identidad, especialidad
- Filtrar por rol, departamento, estado

**b) Crear Empleado**
- **Información Personal**:
  - Nombres y apellidos
  - Número de identidad
  - Fecha de nacimiento
  - Género
- **Información Laboral**:
  - Cargo/Rol
  - Especialidad (si aplica)
  - Departamento
  - Fecha de ingreso
  - Número de colegiación (si aplica)
- **Información de Contacto**:
  - Teléfono(s)
  - Correo electrónico
  - Dirección
- **Acceso al Sistema**:
  - Usuario de Keycloak
  - Rol en el sistema
  - Permisos específicos

**c) Editar Empleado**
- Actualizar información personal
- Modificar datos laborales
- Cambiar roles y permisos

**d) Gestionar Roles**
- Asignar roles del sistema
- Configurar permisos específicos
- Activar/desactivar acceso

### 5.6 Módulo de Ubicaciones

**Propósito**: Gestionar las ubicaciones físicas del hospital.

#### Funcionalidades:

**a) Lista de Ubicaciones**
- Ver todas las ubicaciones registradas
- Buscar por nombre o código
- Filtrar por tipo, estado

**b) Crear Ubicación**
- Nombre de la ubicación
- Código único
- Tipo (edificio, piso, sala, consultorio, etc.)
- Ubicación padre (jerarquía)
- Descripción
- Capacidad
- Estado (activo/inactivo)

**c) Editar Ubicación**
- Modificar información
- Cambiar jerarquía
- Actualizar estado

**d) Gestionar Jerarquía**
- Organizar ubicaciones en estructura de árbol
- Asignar ubicaciones padre-hijo

### 5.7 Módulo de Organizaciones

**Propósito**: Administrar organizaciones relacionadas (aseguradoras, proveedores, etc.).

#### Funcionalidades:

**a) Lista de Organizaciones**
- Ver todas las organizaciones registradas
- Buscar por nombre o identificador
- Filtrar por tipo

**b) Crear Organización**
- Nombre de la organización
- Identificador fiscal
- Tipo (aseguradora, proveedor, gobierno, etc.)
- Información de contacto
- Dirección
- Persona de contacto
- Estado (activo/inactivo)

**c) Editar Organización**
- Actualizar información
- Modificar contactos
- Cambiar estado

### 5.8 Módulo de Reportes

**Propósito**: Generar reportes administrativos y financieros.

#### Funcionalidades:

**a) Control de Reportes**
- Ver reportes disponibles
- Filtrar por tipo, fecha

**b) Generar Reporte**
- **Tipos de Reportes**:
  - Ingresos diarios
  - Ingresos por período
  - Servicios más solicitados
  - Ingresos por método de pago
  - Ingresos por cajero
  - Ingresos por servicio
  - Pacientes atendidos
  - Cierres de caja
- **Parámetros**:
  - Rango de fechas
  - Filtros específicos
  - Formato (PDF, Excel)

**c) Historial de Reportes**
- Ver reportes generados anteriormente
- Descargar reportes guardados
- Regenerar reportes

### 5.9 Módulo de Eventos

**Propósito**: Auditar y monitorear las operaciones del sistema.

#### Funcionalidades:

**a) Lista de Eventos**
- Ver todos los eventos registrados
- Filtrar por:
  - Tipo de evento (creación, modificación, eliminación)
  - Usuario
  - Módulo
  - Fecha y hora
  - Nivel (información, advertencia, error)
- Buscar eventos específicos
- Exportar logs

**b) Detalles de Evento**
- Ver información completa del evento
- Usuario que realizó la acción
- Fecha y hora exacta
- Datos antes y después del cambio
- Dirección IP
- Navegador utilizado

---

## 6. Guía de Uso por Rol

### 6.1 Guía para Auxiliar de Caja

#### Flujo de Trabajo Diario

**1. Inicio del Turno**
- Iniciar sesión en el sistema
- Verificar que su sesión de caja esté activa
- Si no hay sesión activa, solicitar al administrador que la cree

**2. Registrar un Ingreso**

a) **Buscar o Crear Paciente**:
   - Ir a **Fondos > Generar Ingreso**
   - Buscar paciente por nombre o identidad
   - Si el paciente no existe, hacer clic en **"Crear Nuevo Paciente"**
   - Completar formulario con datos del paciente
   - Guardar información

b) **Seleccionar Servicios**:
   - Buscar servicios en el catálogo
   - Agregar servicios individuales o paquetes
   - Verificar precios
   - Aplicar descuentos si corresponde

c) **Registrar Pago**:
   - Seleccionar método de pago:
     - **Efectivo**: Ingresar monto recibido, el sistema calcula el cambio
   - Confirmar el pago
   - El sistema genera automáticamente la factura

d) **Imprimir Documentos**:
   - Imprimir recibo para el paciente
   - Entregar documentos al paciente

**3. Consultar Ingresos del Día**
- Ir a **Fondos > Lista de Ingresos**
- Filtrar por fecha actual
- Revisar todos los ingresos registrados
- Verificar métodos de pago

**4. Cierre de Caja**

a) **Preparar el Cierre**:
   - Contar todo el efectivo en caja
   - Separar por denominaciones
   - Contar vouchers de tarjetas
   - Revisar comprobantes de los recibos

b) **Registrar el Cierre**:
   - Ir a **Fondos > Cerrar Caja**
   - El sistema muestra el total esperado
   - Ingresar los montos contados:
     - Efectivo por denominación
   - El sistema calcula automáticamente diferencias

c) **Finalizar el Cierre**:
   - Revisar el resumen del cierre
   - Agregar observaciones si hay diferencias
   - Confirmar el cierre
   - Imprimir comprobante de cierre
   - Entregar efectivo y comprobante al supervisor

**5. Fin del Turno**
- Verificar que el cierre esté completo
- Cerrar sesión del sistema

#### Consejos para Auxiliares de Caja

- ✅ Verificar siempre la identidad del paciente
- ✅ Confirmar los servicios antes de generar el ingreso
- ✅ Entregar siempre el recibo al paciente
- ✅ Contar el efectivo frente al paciente
- ✅ Mantener organizada la caja durante el turno
- ✅ Reportar inmediatamente cualquier problema técnico

### 6.2 Guía para Administrador

#### Responsabilidades Principales

**1. Gestión de Servicios y Paquetes**

a) **Crear Nuevo Servicio**:
   - Ir a **Servicios > Crear Servicio**
   - Completar información:
     - Nombre descriptivo
     - Código único
     - Categoría
     - Precio
     - Descripción detallada
   - Activar el servicio
   - Guardar

b) **Actualizar Precios**:
   - Ir a **Servicios > Lista de Servicios**
   - Buscar el servicio
   - Hacer clic en **"Editar"**
   - Actualizar precio
   - Guardar cambios

c) **Crear Paquete Promocional**:
   - Ir a **Paquetes > Crear Paquete**
   - Definir nombre y código
   - Seleccionar servicios incluidos
   - Establecer precio del paquete
   - Definir vigencia
   - Activar y guardar

**2. Gestión de Personal**

a) **Registrar Nuevo Empleado**:
   - Ir a **Empleados > Crear Empleado**
   - Completar información personal
   - Asignar cargo y departamento
   - Crear usuario en Keycloak
   - Asignar rol del sistema
   - Guardar

b) **Modificar Roles y Permisos**:
   - Ir a **Empleados > Lista de Empleados**
   - Buscar empleado
   - Hacer clic en **"Gestionar Roles"**
   - Seleccionar nuevo rol
   - Confirmar cambios

**3. Supervisión de Caja**

a) **Revisar Cierres de Caja**:
   - Ir a **Fondos > Historial de Cierres**
   - Filtrar por fecha o cajero
   - Revisar diferencias
   - Verificar observaciones
   - Aprobar o solicitar aclaraciones

b) **Gestionar Sesiones de Caja**:
   - Abrir sesiones para cajeros al inicio del turno
   - Cerrar sesiones al final del día
   - Resolver problemas de sesiones

**4. Generación de Reportes**

a) **Reporte de Ingresos Diarios**:
   - Ir a **Reportes > Generar Reporte**
   - Seleccionar tipo: "Ingresos Diarios"
   - Elegir fecha
   - Generar y descargar

b) **Reporte Mensual**:
   - Seleccionar tipo: "Ingresos por Período"
   - Definir rango de fechas (mes completo)
   - Aplicar filtros si es necesario
   - Generar en formato Excel o PDF

**5. Configuración del Sistema**

a) **Propiedades del Hospital**:
   - Configurar nombre del hospital
   - Subir logo institucional
   - Definir información de contacto
   - Configurar datos fiscales

b) **Gestión de Ubicaciones**:
   - Crear estructura de ubicaciones
   - Organizar jerarquía (edificios > pisos > salas)
   - Asignar servicios a ubicaciones

### 6.3 Guía para Auditor

#### Funciones de Auditoría

**1. Revisión de Ingresos**

a) **Auditoría Diaria**:
   - Ir a **Fondos > Lista de Ingresos**
   - Filtrar por fecha actual
   - Revisar todos los ingresos
   - Verificar métodos de pago
   - Identificar patrones inusuales

b) **Análisis de Cierres**:
   - Ir a **Fondos > Historial de Cierres**
   - Revisar diferencias en cierres
   - Analizar tendencias
   - Identificar discrepancias recurrentes

**2. Monitoreo de Eventos**

a) **Revisar Logs del Sistema**:
   - Ir a **Eventos > Lista de Eventos**
   - Filtrar por tipo de evento
   - Buscar eventos críticos
   - Revisar modificaciones importantes

b) **Auditoría de Usuarios**:
   - Filtrar eventos por usuario
   - Revisar acciones realizadas
   - Identificar comportamientos anómalos

**3. Análisis de Servicios**

a) **Servicios Más Utilizados**:
   - Generar reporte de servicios
   - Analizar frecuencia de uso
   - Identificar servicios rentables

b) **Análisis de Precios**:
   - Revisar historial de cambios de precios
   - Verificar aplicación correcta de tarifas

**4. Generación de Reportes de Auditoría**

a) **Reporte de Discrepancias**:
   - Identificar cierres con diferencias
   - Documentar hallazgos
   - Generar reporte para dirección

b) **Reporte de Cumplimiento**:
   - Verificar cumplimiento de procedimientos
   - Documentar desviaciones
   - Proponer mejoras

### 6.4 Guía para Técnico de Informática

#### Responsabilidades Técnicas

**1. Gestión de Usuarios y Accesos**

a) **Crear Usuario en Keycloak**:
   - Acceder al panel de administración de Keycloak
   - Crear nuevo usuario
   - Asignar credenciales temporales
   - Configurar roles
   - Notificar al usuario

b) **Gestionar Roles y Permisos**:
   - Revisar roles existentes
   - Crear roles personalizados si es necesario
   - Asignar permisos específicos
   - Auditar accesos

**2. Configuración de Ubicaciones**

a) **Estructura Organizacional**:
   - Crear edificios principales
   - Definir pisos y áreas
   - Crear consultorios y salas
   - Establecer jerarquías

**3. Gestión de Organizaciones**

a) **Registrar Aseguradoras**:
   - Ir a **Organizaciones > Crear Organización**
   - Tipo: Aseguradora
   - Completar información
   - Configurar convenios

b) **Registrar Proveedores**:
   - Tipo: Proveedor
   - Información de contacto
   - Términos de servicio

**4. Monitoreo del Sistema**

a) **Acceder al Dashboard de Aspire**:
   - URL: `/dashboard`
   - Revisar métricas en tiempo real
   - Monitorear rendimiento
   - Identificar cuellos de botella

b) **Revisar Logs en Seq**:
   - Acceder a Seq
   - Buscar errores
   - Analizar patrones
   - Configurar alertas

**5. Mantenimiento**

a) **Respaldos**:
   - Verificar respaldos automáticos
   - Realizar respaldos manuales cuando sea necesario
   - Probar restauración

b) **Actualizaciones**:
   - Revisar actualizaciones disponibles
   - Planificar ventanas de mantenimiento
   - Aplicar actualizaciones
   - Verificar funcionamiento

---

## 7. Funcionalidades Comunes

### 7.1 Navegación en el Sistema

#### Menú Principal

El menú principal se encuentra en la parte izquierda de la pantalla y contiene:

- **Dashboard**: Página de inicio con resumen de actividades
- **Fondos**: Gestión de ingresos y caja
- **Servicios**: Catálogo de servicios médicos
- **Paquetes**: Paquetes de servicios
- **Pacientes**: Gestión de pacientes
- **Empleados**: Administración de personal
- **Ubicaciones**: Gestión de ubicaciones físicas
- **Organizaciones**: Administración de organizaciones
- **Reportes**: Generación de reportes
- **Eventos**: Auditoría del sistema

**Nota**: Los módulos visibles dependen del rol del usuario.

#### Barra Superior

- **Búsqueda Global**: Buscar pacientes, servicios, empleados
- **Notificaciones**: Alertas y mensajes del sistema
- **Perfil de Usuario**: Acceso a configuración personal y cerrar sesión

### 7.2 Búsqueda y Filtros

#### Búsqueda Rápida

En la mayoría de los listados encontrará:

1. **Campo de Búsqueda**: 
   - Escriba el término a buscar
   - El sistema busca en tiempo real
   - Busca en múltiples campos (nombre, código, identidad, etc.)

2. **Filtros Avanzados**:
   - Haga clic en el icono de filtro
   - Seleccione criterios específicos:
     - Fechas
     - Estados
     - Categorías
     - Rangos de valores
   - Aplique los filtros
   - Limpie filtros cuando sea necesario

#### Ordenamiento

- Haga clic en los encabezados de las columnas para ordenar
- Primer clic: orden ascendente
- Segundo clic: orden descendente
- Tercer clic: orden original

### 7.3 Paginación

Los listados muestran un número limitado de registros por página:

- **Navegación**: Use los botones "Anterior" y "Siguiente"
- **Ir a Página**: Ingrese el número de página directamente
- **Registros por Página**: Seleccione cuántos registros mostrar (10, 25, 50, 100)
- **Total de Registros**: Se muestra el total de registros encontrados

### 7.4 Exportación de Datos

#### Exportar Listados

1. En cualquier listado, busque el botón **"Exportar"**
2. Seleccione el formato:
   - **Excel (.xlsx)**: Para análisis en hojas de cálculo
   - **PDF**: Para impresión o archivo
   - **CSV**: Para importar en otros sistemas
3. El archivo se descargará automáticamente

#### Exportar Reportes

1. Genere el reporte deseado
2. Seleccione formato de exportación
3. Descargue el archivo

### 7.5 Impresión

#### Imprimir Documentos

1. **Recibos de Pago**:
   - Se generan automáticamente al registrar un ingreso
   - Haga clic en **"Imprimir Recibo"**
   - Configure su impresora
   - Imprima

2. **Facturas**:
   - Se generan junto con el recibo
   - Incluyen información fiscal completa
   - Formato oficial

3. **Comprobantes de Cierre**:
   - Se generan al cerrar caja
   - Incluyen resumen detallado
   - Requieren firma del cajero

4. **Reportes**:
   - Todos los reportes son imprimibles
   - Use la opción "Imprimir" del navegador
   - O exporte a PDF primero

### 7.6 Formularios

#### Completar Formularios

**Campos Obligatorios**:
- Marcados con asterisco (*) rojo
- Deben completarse para guardar

**Validaciones**:
- El sistema valida los datos en tiempo real
- Mensajes de error aparecen bajo los campos
- Corrija los errores antes de guardar

**Autocompletado**:
- Algunos campos sugieren valores mientras escribe
- Seleccione de la lista o continúe escribiendo

**Campos de Fecha**:
- Haga clic en el campo para abrir el calendario
- Seleccione la fecha
- O escriba la fecha en formato DD/MM/AAAA

**Campos Numéricos**:
- Solo aceptan números
- Algunos tienen formato automático (moneda, porcentaje)

#### Guardar Cambios

1. Complete todos los campos obligatorios
2. Revise la información
3. Haga clic en **"Guardar"**
4. Espere la confirmación
5. El sistema mostrará un mensaje de éxito

#### Cancelar Cambios

- Haga clic en **"Cancelar"** para descartar cambios
- Si hay cambios sin guardar, el sistema pedirá confirmación

### 7.7 Mensajes del Sistema

#### Tipos de Mensajes

**Éxito (Verde)**:
- Operación completada correctamente
- Ejemplo: "Paciente creado exitosamente"

**Información (Azul)**:
- Información general
- Ejemplo: "Cargando datos..."

**Advertencia (Amarillo)**:
- Situaciones que requieren atención
- Ejemplo: "El paciente ya tiene un ingreso hoy"

**Error (Rojo)**:
- Problemas que impiden completar la operación
- Ejemplo: "Error al guardar. Intente nuevamente"

#### Notificaciones

- Aparecen en la esquina superior derecha
- Se cierran automáticamente después de unos segundos
- Puede cerrarlas manualmente haciendo clic en la X

### 7.8 Atajos de Teclado

Algunos atajos útiles:

- **Ctrl + S**: Guardar (en formularios)
- **Esc**: Cancelar/Cerrar modal
- **Ctrl + F**: Buscar en la página
- **Tab**: Navegar entre campos
- **Enter**: Confirmar/Buscar

### 7.9 Ayuda Contextual

#### Tooltips

- Pase el cursor sobre iconos de interrogación (?)
- Aparecerá información adicional
- Útil para entender campos específicos

#### Mensajes de Ayuda

- Algunos formularios incluyen textos de ayuda
- Leen atentamente para completar correctamente

---

## 8. Preguntas Frecuentes

### 8.1 Acceso y Seguridad

**P: ¿Qué hago si olvidé mi contraseña?**
R: En la pantalla de inicio de sesión, haga clic en "¿Olvidó su contraseña?" e ingrese su correo electrónico. Recibirá un enlace para restablecerla.

**P: ¿Cuánto tiempo puedo estar inactivo antes de que se cierre mi sesión?**
R: Por seguridad, la sesión se cierra automáticamente después de 30 minutos de inactividad.

**P: ¿Puedo acceder al sistema desde mi teléfono móvil?**
R: Sí, el sistema es responsive y se adapta a dispositivos móviles, aunque algunas funciones son más cómodas en computadora.

**P: ¿Puedo tener múltiples sesiones abiertas?**
R: No se recomienda. El sistema permite una sesión activa por usuario para evitar conflictos.

### 8.2 Gestión de Ingresos

**P: ¿Qué hago si me equivoqué al registrar un ingreso?**
R: Contacte inmediatamente a su supervisor o administrador. Solo ellos pueden anular o modificar ingresos registrados.

**P: ¿Puedo registrar un ingreso sin paciente?**
R: No, todos los ingresos deben estar asociados a un paciente registrado.

**P: ¿Cómo aplico un descuento?**
R: Al generar el ingreso, encontrará un campo para descuentos. Ingrese el porcentaje o monto. Algunos descuentos requieren autorización.

**P: ¿Puedo cerrar caja si hay diferencias?**
R: Sí, pero debe documentar la diferencia en las observaciones. Diferencias mayores requieren autorización del supervisor.

### 8.3 Gestión de Pacientes

**P: ¿Qué hago si un paciente no tiene número de identidad?**
R: Para extranjeros o casos especiales, use el número de pasaporte u otro documento oficial. Documente el tipo de documento.

**P: ¿Puedo modificar la información de un paciente?**
R: Sí, si tiene los permisos necesarios. Vaya a la lista de pacientes, busque el paciente y haga clic en "Editar".

**P: ¿Cómo busco el historial médico de un paciente?**
R: En la ficha del paciente, encontrará una pestaña de "Historial" con todos los servicios y pagos anteriores.

### 8.4 Servicios y Paquetes

**P: ¿Puedo crear un servicio personalizado?**
R: Solo los administradores pueden crear servicios. Si necesita un servicio nuevo, solicítelo a su administrador.

**P: ¿Los precios incluyen impuestos?**
R: Depende de la configuración del hospital. Generalmente los precios son finales e incluyen todos los cargos.

**P: ¿Qué pasa si un servicio del paquete ya no está disponible?**
R: El sistema alertará si un servicio no está activo. Contacte al administrador para actualizar el paquete.

### 8.5 Reportes

**P: ¿Cuánto tiempo tardan en generarse los reportes?**
R: Reportes simples se generan en segundos. Reportes complejos con muchos datos pueden tardar hasta un minuto.

**P: ¿Puedo programar reportes automáticos?**
R: Actualmente no, pero puede generar y descargar reportes cuando los necesite.

**P: ¿Los reportes muestran datos en tiempo real?**
R: Sí, los reportes siempre muestran los datos más actuales al momento de generarlos.

### 8.6 Problemas Técnicos

**P: ¿Qué hago si el sistema está lento?**
R: Verifique su conexión a internet. Si el problema persiste, contacte al área de TI.

**P: ¿Qué hago si aparece un error al guardar?**
R: Intente nuevamente. Si el error persiste, tome una captura de pantalla del mensaje y contacte a TI.

**P: ¿El sistema guarda automáticamente?**
R: No, debe hacer clic en "Guardar" para que los cambios se registren.

**P: ¿Qué navegador debo usar?**
R: Se recomienda Google Chrome o Microsoft Edge actualizados. Firefox y Safari también son compatibles.

---

## 9. Soporte Técnico

### 9.1 Canales de Soporte

#### Soporte de Primer Nivel (Usuario)

**Para problemas operativos**:
- Contacte a su supervisor inmediato
- Consulte este manual de usuario
- Revise las preguntas frecuentes

#### Soporte de Segundo Nivel (Administrador)

**Para problemas administrativos**:
- Contacte al administrador del sistema
- Problemas con permisos y accesos
- Configuración de servicios y precios
- Gestión de usuarios

#### Soporte de Tercer Nivel (TI)

**Para problemas técnicos**:
- Contacte al área de Tecnología de Información
- Problemas de conectividad
- Errores del sistema
- Problemas de rendimiento
- Respaldos y recuperación

### 9.2 Información para Reportar Problemas

Cuando reporte un problema, incluya:

1. **Información del Usuario**:
   - Nombre completo
   - Rol en el sistema
   - Usuario de acceso

2. **Descripción del Problema**:
   - ¿Qué estaba haciendo cuando ocurrió?
   - ¿Qué esperaba que sucediera?
   - ¿Qué sucedió en realidad?

3. **Información Técnica**:
   - Navegador y versión
   - Fecha y hora del problema
   - Mensaje de error (si aparece)
   - Captura de pantalla

4. **Pasos para Reproducir**:
   - Liste los pasos exactos que llevaron al problema
   - Esto ayuda a TI a identificar y resolver el problema

### 9.3 Horarios de Soporte

**Soporte en Línea**:
- Lunes a Viernes: 8:00 AM - 4:00 PM


### 9.4 Recursos Adicionales

#### Documentación Técnica

Para administradores y personal de TI:
- **README.MD**: Información general del proyecto
- **README-DEPLOYMENT.md**: Guía de despliegue
- **Documentación de API**: Disponible en `/api/swagger`

#### Capacitación

- Capacitación inicial para nuevos usuarios
- Sesiones de actualización periódicas
- Capacitación especializada por rol
- Materiales de capacitación en línea

#### Actualizaciones del Sistema

- El sistema se actualiza periódicamente
- Las actualizaciones se realizan en horarios de bajo uso
- Se notifica con anticipación sobre actualizaciones mayores
- Este manual se actualiza con cada versión

---

## 10. Mejores Prácticas

### 10.1 Seguridad

✅ **Contraseñas**:
- Use contraseñas seguras (mínimo 8 caracteres, mayúsculas, minúsculas, números)
- No comparta su contraseña con nadie
- Cambie su contraseña periódicamente
- No use la misma contraseña en otros sistemas

✅ **Sesiones**:
- Cierre sesión al terminar su turno
- No deje su sesión abierta sin supervisión
- Bloquee su computadora si se ausenta

✅ **Datos Sensibles**:
- Proteja la información de los pacientes
- No tome fotos de pantallas con datos personales
- No comparta información confidencial por medios no seguros

### 10.2 Eficiencia Operativa

✅ **Organización**:
- Mantenga su área de trabajo ordenada
- Organice documentos físicos y digitales
- Archive documentos procesados

✅ **Verificación**:
- Verifique siempre los datos antes de guardar
- Confirme información con el paciente
- Revise totales antes de cerrar caja

✅ **Comunicación**:
- Reporte problemas inmediatamente
- Documente situaciones inusuales
- Mantenga informado a su supervisor

### 10.3 Calidad de Datos

✅ **Precisión**:
- Ingrese datos completos y correctos
- Use mayúsculas y minúsculas apropiadamente
- Verifique ortografía en nombres

✅ **Consistencia**:
- Use formatos estándar para fechas y números
- Siga las convenciones establecidas
- Mantenga uniformidad en registros

✅ **Actualización**:
- Actualice información desactualizada
- Corrija errores cuando los detecte
- Mantenga registros al día

### 10.4 Respaldo de Información

✅ **Documentos Físicos**:
- Mantenga copias de documentos importantes
- Archive comprobantes de cierre de caja
- Guarde facturas y recibos según política

✅ **Documentos Digitales**:
- El sistema realiza respaldos automáticos
- No es necesario guardar copias locales
- Confíe en el sistema para recuperación

---

## 11. Glosario de Términos

**Arqueo de Caja**: Proceso de contar y verificar el efectivo y otros valores en caja al final del turno.

**Auditoría**: Revisión sistemática de operaciones y registros para verificar cumplimiento y detectar irregularidades.

**Cierre de Caja**: Proceso de finalizar las operaciones de caja, contar valores y generar reporte de cierre.

**Dashboard**: Tablero o panel de control que muestra resumen de información importante.

**Factura**: Documento fiscal que respalda una transacción comercial.

**FHIR (Fast Healthcare Interoperability Resources)**: Estándar para intercambio de información de salud.

**Filtro**: Herramienta para limitar resultados según criterios específicos.

**HL7**: Estándar internacional para intercambio de información en salud.

**Ingreso**: Registro de pago por servicios médicos prestados.

**Keycloak**: Sistema de gestión de identidad y acceso utilizado para autenticación.

**Método de Pago**: Forma en que el paciente realiza el pago (efectivo, tarjeta, etc.).

**Paginación**: División de resultados en múltiples páginas para facilitar navegación.

**Paquete**: Conjunto de servicios médicos agrupados con precio especial.

**Recibo**: Comprobante de pago entregado al paciente.

**Rol**: Conjunto de permisos asignados a un tipo de usuario.

**Sesión de Caja**: Período durante el cual un cajero opera la caja.

**Tooltip**: Mensaje de ayuda que aparece al pasar el cursor sobre un elemento.

**Turno**: Período de trabajo asignado a un empleado.

---

## 12. Información de Contacto

### Equipo de Desarrollo

Este sistema fue desarrollado por:

- **Carlos Ovidio Dubón Pineda** - Backend
- **Michael Andrey Galdamez Martinez** - Frontend
- **Ever Josue Garcia Leonor** - Backend
- **Danilo Isaac Vides Chicas** - Frontend
- **Anthony Edward Miranda Fuentes** - Backend
- **Hector Rene Martinez Vega** - Backend
- **Erick Marley Arita** - Frontend/Backend
- **Josue David Diaz Rodriguez** - Frontend/Backend

### Tecnologías Utilizadas

**Frontend**:
- React 18
- TypeScript
- Tailwind CSS
- Zustand (gestión de estado)
- TanStack Query
- Formik & Yup
- Orval (generación de cliente API)

**Backend**:
- .NET 8
- Entity Framework Core
- PostgreSQL
- MongoDB
- Keycloak (autenticación)
- HAPI FHIR Server

**Infraestructura**:
- Docker & Docker Compose
- Nginx (proxy reverso)
- OpenTelemetry (observabilidad)
- Seq (logs)
- Aspire Dashboard (monitoreo)

### Repositorio del Proyecto

- **GitHub**: [SIGREF Repository](https://github.com/Pineda04/SIGREF)
- **Licencia**: Ver archivo LICENSE.MD

---

## 13. Historial de Versiones

### Versión 1.0.0 (Diciembre 2024)

**Características Iniciales**:
- ✅ Gestión de ingresos y caja
- ✅ Administración de pacientes
- ✅ Catálogo de servicios y paquetes
- ✅ Gestión de empleados
- ✅ Control de ubicaciones y organizaciones
- ✅ Generación de reportes
- ✅ Auditoría de eventos
- ✅ Autenticación con Keycloak
- ✅ Integración con HAPI FHIR
- ✅ Monitoreo con OpenTelemetry

**Roles Implementados**:
- Administrador
- Auxiliar de Caja
- Auditor
- Técnico de Informática

---

## 14. Notas Finales

### Cumplimiento de Estándares

SIGREF es un **sistema FHIR-LIKE** que sigue los estándares **HL7 FHIR R5** para la gestión de información de salud. Esto significa que:

- Los datos se estructuran según recursos FHIR
- Se facilita la interoperabilidad con otros sistemas de salud
- Se mantiene compatibilidad con estándares internacionales
- No es un sistema FHIR completo, pero adopta sus mejores prácticas

### Privacidad y Protección de Datos

El sistema SIGREF cumple con:

- Protección de datos personales de pacientes
- Cifrado de información sensible
- Auditoría completa de accesos
- Control de permisos por rol
- Respaldos automáticos y seguros

### Mejora Continua

Este sistema está en constante evolución. Sus comentarios y sugerencias son valiosos para:

- Mejorar la experiencia de usuario
- Agregar nuevas funcionalidades
- Optimizar procesos
- Corregir problemas

**¿Tiene sugerencias?** Contacte a su administrador o al equipo de TI.

---

## 📞 Contacto de Soporte

**Soporte Técnico**: Contacte al área de TI de su institución

**Documentación Adicional**: 
- Manual Técnico: README.MD
- Guía de Despliegue: README-DEPLOYMENT.md
- Documentación de API: `/api/swagger`

---

**© 2024 SIGREF - Sistema de Gestión de la Receptoría de Fondos**

*Este manual está sujeto a actualizaciones. Versión: 1.0.0 - Diciembre 2024*

---

**¡Gracias por usar SIGREF!**

Este sistema fue diseñado para facilitar su trabajo diario y mejorar la gestión de ingresos en servicios médicos. Si tiene dudas o necesita ayuda, no dude en consultar este manual o contactar al equipo de soporte.
