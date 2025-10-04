export const FondosMenu = [
  { path: "/", name: "Generar Ingreso" },
  { path: "/", name: "Reportar Error de Ingreso" },
  { path: "/", name: "Cerrar Caja" },
  { path: "/", name: "Historial de Cierres de Caja" },
];

export const ServiciosMenu = [
  { path: "/", name: "Listar Servicios" },
  { path: "/", name: "Crear Servicio" },
  { path: "/", name: "Editar Servicio" },
];

export const PacientesMenu = [
  { path: "/", name: "Listar Pacientes" },
  { path: "/", name: "Crear Paciente" },
  { path: "/", name: "Editar Paciente" },
];

export const EmpleadosMenu = [
  { path: "/", name: "Listar Empleados" },
  { path: "/", name: "Crear Empleado" },
  { path: "/", name: "Modificar Empleado" },
];

export const EmpleadosListMenu = [
  { path: "/", name: "Listar Empleados" },
];

export const ServiciosListMenu = [
  { path: "/", name: "Listar Servicios" },
];

export const EventosMenu = [
  { path: "/", name: "Listar Eventos" },
];

export const EmpresaMenu = [
  { path: "/", name: "Datos de Empresa" },
  { path: "/", name: "Listar Turnos" },
  { path: "/", name: "Crear Turno" },
  { path: "/", name: "Modificar Turno" },
];

export const MenusPorRol: Record<
  string,
  {
    fondos?: any[];
    servicios?: any[];
    pacientes?: any[];
    empleados?: any[];
    eventos?: any[];
    empresa?: any[];
  }
> = {
  cashier_assistant: {
    fondos: FondosMenu,
    servicios: ServiciosMenu,
    pacientes: PacientesMenu,
  },
  auditor: {
    servicios: ServiciosListMenu,
    empleados: EmpleadosListMenu,
    eventos: EventosMenu,
  },
  admin_1: {
    fondos: FondosMenu,
    servicios: ServiciosMenu,
    pacientes: PacientesMenu,
    empleados: EmpleadosMenu,
    empresa: EmpresaMenu,
  },
  ti: {
    empleados: EmpleadosMenu,
    eventos: EventosMenu,
    empresa: EmpresaMenu,
  },
};