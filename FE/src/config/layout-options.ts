import { type Subjects } from "../auth";
import { USER_ROLE_OPTIONS } from "../shared/constants";

// Definir las rutas con información de permisos
export interface RouteItem {
  path: string;
  name: string;
  subject: Subjects;
  action: "read" | "create" | "update" | "delete";
}

// Gestión de Fondos
export const IncomesRoutes: RouteItem[] = [
  {
    path: "/incomes/list",
    name: "Lista de Ingresos",
    subject: "incomes" as Subjects,
    action: "read",
  },
  {
    path: "/incomes/create",
    name: "Generar Ingreso",
    subject: "incomes" as Subjects,
    action: "create",
  },
  {
    path: "/incomes/close",
    name: "Cerrar Caja",
    subject: "incomes" as Subjects,
    action: "update",
  },
  {
    path: "/incomes/history",
    name: "Historial de Cierres de Caja",
    subject: "incomes" as Subjects,
    action: "read",
  },
];

// Gestión de Servicios
export const HealthcaresRoutes: RouteItem[] = [
  {
    path: "/healthcares/list",
    name: "Lista de Servicios",
    subject: "healthcares" as Subjects,
    action: "read",
  },
  {
    path: "/healthcares/create",
    name: "Crear Servicio",
    subject: "healthcares" as Subjects,
    action: "create",
  },
];

// Gestión de Paquetes
export const ServiceGroupsRoutes: RouteItem[] = [
  {
    path: "/service-groups/list",
    name: "Lista de Paquetes",
    subject: "service-groups" as Subjects,
    action: "read",
  },
  {
    path: "/service-groups/create",
    name: "Crear Paquete",
    subject: "service-groups" as Subjects,
    action: "create",
  },
];

// Gestión de Ubicaciones
export const LocationsRoutes: RouteItem[] = [
  {
    path: "/locations/list",
    name: "Lista de Ubicaciones",
    subject: "locations" as Subjects,
    action: "read",
  },
  {
    path: "/locations/create",
    name: "Crear Ubicación",
    subject: "locations" as Subjects,
    action: "create",
  },
];

// Gestión de Turnos
export const ShiftsRoutes: RouteItem[] = [
  {
    path: "/shifts/list",
    name: "Lista de Turnos",
    subject: "shifts" as Subjects,
    action: "read",
  },
  {
    path: "/cashier/open-session",
    name: "Iniciar Turno",
    subject: "cashier-sessions" as Subjects,
    action: "create",
  },
];

// Gestión de Pacientes
export const PatientsRoutes: RouteItem[] = [
  {
    path: "/patients/list",
    name: "Lista de Pacientes",
    subject: "patients" as Subjects,
    action: "read",
  },
  {
    path: "/patients/create",
    name: "Crear Paciente",
    subject: "patients" as Subjects,
    action: "create",
  },
];

// Gestión de Empleados
export const PractitionersRoutes: RouteItem[] = [
  {
    path: "/practitioners/list",
    name: "Lista de Empleados",
    subject: "practitioners" as Subjects,
    action: "read",
  },
  {
    path: "/practitioners/create",
    name: "Crear Empleado",
    subject: "practitioners" as Subjects,
    action: "create",
  }
];

// Gestión de Usuarios
export const UsersRoutes: RouteItem[] = [
  {
    path: "/users/list",
    name: "Lista de Usuarios",
    subject: "users" as Subjects,
    action: "read",
  },
  {
    path: "/users/create",
    name: "Crear Usuario",
    subject: "users" as Subjects,
    action: "create",
  },
];

// Gestión de Organizaciones
export const OrganizationsRoutes: RouteItem[] = [
  {
    path: "/organizations/list",
    name: "Lista de Organizaciones",
    subject: "organizations" as Subjects,
    action: "read",
  },
  {
    path: "/organizations/create",
    name: "Crear Organización",
    subject: "organizations" as Subjects,
    action: "create",
  },
];

// Gestión de Reportes
export const ReportsRoutes: RouteItem[] = [
  {
    path: "/reports/list",
    name: "Control de Reportes",
    subject: "reports" as Subjects,
    action: "read",
  },
  {
    path: "/reports/create",
    name: "Generar Reporte",
    subject: "reports" as Subjects,
    action: "create",
  },
  {
    path: "/reports/history",
    name: "Historial de Reportes",
    subject: "reports" as Subjects,
    action: "read",
  },
];

// Gestión de Eventos/Logs
export const EventsRoutes: RouteItem[] = [
  {
    path: "/events/list",
    name: "Lista de Eventos",
    subject: "events" as Subjects,
    action: "read",
  },
];

// Gestión de Series
export const SeriesRoutes: RouteItem[] = [
  {
    path: "/series/list",
    name: "Lista de Series",
    subject: "serie" as Subjects,
    action: "read",
  },
  {
    path: "/series/create",
    name: "Crear Serie",
    subject: "serie" as Subjects,
    action: "create",
  },
];

// ================================================================

// Rutas por rol
export const RoutesByRole: Record<
  string,
  {
    fondos?: RouteItem[];
    servicios?: RouteItem[];
    pacientes?: RouteItem[];
    empleados?: RouteItem[];
    usuarios?: RouteItem[];
    turnos?: RouteItem[];
    ubicaciones?: RouteItem[];
    organizaciones?: RouteItem[];
    reportes?: RouteItem[];
    eventos?: RouteItem[];
    series?: RouteItem[];
  }
> = {
  [USER_ROLE_OPTIONS[0].label]: {
    // Administrador
    fondos: IncomesRoutes.filter((route) => route.action === "read"),
    series: SeriesRoutes,
    servicios: [...HealthcaresRoutes, ...ServiceGroupsRoutes],
    pacientes: PatientsRoutes,
    empleados: PractitionersRoutes,
    usuarios: UsersRoutes,
    turnos: ShiftsRoutes.filter((route) => route.action === "read"),
    ubicaciones: LocationsRoutes,
    organizaciones: OrganizationsRoutes,
    reportes: ReportsRoutes,
  },
  [USER_ROLE_OPTIONS[1].label]: {
    // Auditoria
    fondos: IncomesRoutes.filter((route) => route.action === "read"),
    servicios: [...HealthcaresRoutes, ...ServiceGroupsRoutes].filter(
      (route) => route.action === "read",
    ),
    pacientes: PatientsRoutes.filter((route) => route.action === "read"),
    empleados: PractitionersRoutes.filter((route) => route.action === "read"),
    turnos: ShiftsRoutes.filter((route) => route.action === "read"),
    ubicaciones: LocationsRoutes.filter((route) => route.action === "read"),
    organizaciones: OrganizationsRoutes.filter(
      (route) => route.action === "read",
    ),
    reportes: ReportsRoutes.filter((route) => route.action === "read"),
    eventos: EventsRoutes,
  },
  [USER_ROLE_OPTIONS[2].label]: {
    // Auxiliar de Caja
    fondos: IncomesRoutes.filter(
      (route) => route.action === "create" || route.action === "update",
    ),
    pacientes: PatientsRoutes,
    turnos: ShiftsRoutes,
    ubicaciones: LocationsRoutes.filter(route => route.action === "read"),
  },
  [USER_ROLE_OPTIONS[3].label]: {
    // Técnico de Informática
    servicios: HealthcaresRoutes.filter((route) => route.action === "read"),
    empleados: PractitionersRoutes,
    usuarios: UsersRoutes,
    organizaciones: OrganizationsRoutes,
    eventos: EventsRoutes,
  },
};
