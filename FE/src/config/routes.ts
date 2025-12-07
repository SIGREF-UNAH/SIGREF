import { validRoles } from "../auth";

export const IncomesRoutes = [
  { path: "/incomes/list", name: "Lista de Ingresos" },
  { path: "/incomes/create", name: "Generar Ingreso" },
  { path: "/incomes/close", name: "Cerrar Caja" },
  { path: "/incomes/history", name: "Historial de Cierres de Caja" },
];

export const HealthcaresRoutes = [
  { path: "/healthcares/list", name: "Lista de Servicios" },
  { path: "/healthcares/create", name: "Crear Servicio" },
  { path: "/service-groups/list", name: "Lista de Paquetes" },
  { path: "/service-groups/create", name: "Crear Paquete" },
];

export const LocationsRoutes = [
  { path: "/locations/list", name: "Lista de Ubicaciones" },
  { path: "/locations/create", name: "Crear Ubicación" },
];

export const PatientsRoutes = [
  { path: "/patients/list", name: "Lista de Pacientes" },
  { path: "/patients/create", name: "Crear Paciente" },
];

export const PractitionersRoutes = [
  { path: "/practitioners/list", name: "Lista de Empleados" },
  { path: "/practitioners/create", name: "Crear Empleado" },
];

export const ShiftsRoutes = [
  { path: "/shifts/list", name: "Lista de Turnos" },
];

export const EventsRoutes = [
  { path: "/events/list", name: "Listar Eventos" },
];

export const OrganizationsRoutes = [
  { path: "/organizations/list", name: "Lista de Organizaciones" },
  { path: "/organizations/create", name: "Crear Organización" },
];

export const ReportsRoutes = [
  { path: "/reports/list", name: "Control de Reportes" },
  { path: "/reports/create", name: "Generar Reporte" },
  { path: "/reports/history", name: "Historial de Reportes" },
];

// Rutas validas para cada rol
export const RoutesByRole: Record<
  string,
  {
    fondos?: any[];
    servicios?: any[];
    ubicaciones?: any[];
    pacientes?: any[];
    empleados?: any[];
    turnos?: any[];
    organizaciones?: any[];
    reportes?: any[];
    eventos?: any[];
  }
> = {
  [validRoles.admin]: {
    fondos: IncomesRoutes,
    servicios: HealthcaresRoutes,
    ubicaciones: LocationsRoutes,
    pacientes: PatientsRoutes,
    empleados: PractitionersRoutes,
    turnos: ShiftsRoutes,
    organizaciones: OrganizationsRoutes,
    reportes: ReportsRoutes,
    eventos: EventsRoutes,
  },
  [validRoles.auditor]: {
    fondos: IncomesRoutes,
    servicios: HealthcaresRoutes,
    ubicaciones: LocationsRoutes, 
    pacientes: PatientsRoutes,
    empleados: PractitionersRoutes,
    turnos: ShiftsRoutes,
    organizaciones: OrganizationsRoutes,
    reportes: ReportsRoutes,
    eventos: EventsRoutes,
  },
  [validRoles.cashier]: {
    fondos: IncomesRoutes,
    turnos: ShiftsRoutes,
    servicios: HealthcaresRoutes,
    pacientes: PatientsRoutes,
  },
  [validRoles.ti]: {
    servicios: HealthcaresRoutes,
    empleados: PractitionersRoutes,
    organizaciones: OrganizationsRoutes,
    eventos: EventsRoutes,
  },
};
