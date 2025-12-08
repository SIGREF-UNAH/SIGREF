// layout-options.ts
import { validRoles, type Subjects } from "../auth";

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
    action: "read"
  },
  { 
    path: "/incomes/create", 
    name: "Generar Ingreso",
    subject: "incomes" as Subjects,
    action: "create"
  },
  { 
    path: "/incomes/close", 
    name: "Cerrar Caja",
    subject: "incomes" as Subjects,
    action: "update"
  },
  { 
    path: "/incomes/history", 
    name: "Historial de Cierres de Caja",
    subject: "incomes" as Subjects,
    action: "read"
  },
];

// Gestión de Servicios
export const HealthcaresRoutes: RouteItem[] = [
  { 
    path: "/healthcares/list", 
    name: "Lista de Servicios",
    subject: "healthcares" as Subjects,
    action: "read"
  },
  { 
    path: "/healthcares/create", 
    name: "Crear Servicio",
    subject: "healthcares" as Subjects,
    action: "create"
  },
];

// Gestión de Paquetes
export const ServiceGroupsRoutes: RouteItem[] = [
  { 
    path: "/service-groups/list", 
    name: "Lista de Paquetes",
    subject: "service-groups" as Subjects,
    action: "read"
  },
  { 
    path: "/service-groups/create", 
    name: "Crear Paquete",
    subject: "service-groups" as Subjects,
    action: "create"
  },
];

// Gestión de Ubicaciones
export const LocationsRoutes: RouteItem[] = [
  { 
    path: "/locations/list", 
    name: "Lista de Ubicaciones",
    subject: "locations" as Subjects,
    action: "read"
  },
  { 
    path: "/locations/create", 
    name: "Crear Ubicación",
    subject: "locations" as Subjects,
    action: "create"
  },
];

// Gestión de Turnos
export const ShiftsRoutes: RouteItem[] = [
  { 
    path: "/shifts/list", 
    name: "Lista de Turnos",
    subject: "shifts" as Subjects,
    action: "read"
  },
];

// Gestión de Pacientes
export const PatientsRoutes: RouteItem[] = [
  { 
    path: "/patients/list", 
    name: "Lista de Pacientes",
    subject: "patients" as Subjects,
    action: "read"
  },
  { 
    path: "/patients/create", 
    name: "Crear Paciente",
    subject: "patients" as Subjects,
    action: "create"
  },
];

// Gestión de Empleados
export const PractitionersRoutes: RouteItem[] = [
  { 
    path: "/practitioners/list", 
    name: "Lista de Empleados",
    subject: "practitioners" as Subjects,
    action: "read"
  },
  { 
    path: "/practitioners/create", 
    name: "Crear Empleado",
    subject: "practitioners" as Subjects,
    action: "create"
  },
];

// Gestión de Organizaciones
export const OrganizationsRoutes: RouteItem[] = [
  { 
    path: "/organizations/list", 
    name: "Lista de Organizaciones",
    subject: "organizations" as Subjects,
    action: "read"
  },
  { 
    path: "/organizations/create", 
    name: "Crear Organización",
    subject: "organizations" as Subjects,
    action: "create"
  },
];

// Gestión de Reportes
export const ReportsRoutes: RouteItem[] = [
  { 
    path: "/reports/list", 
    name: "Control de Reportes",
    subject: "reports" as Subjects,
    action: "read"
  },
  { 
    path: "/reports/create", 
    name: "Generar Reporte",
    subject: "reports" as Subjects,
    action: "create"
  },
  { 
    path: "/reports/history", 
    name: "Historial de Reportes",
    subject: "reports" as Subjects,
    action: "read"
  },
];

// Gestión de Eventos/Logs
export const EventsRoutes: RouteItem[] = [
  { 
    path: "/events/list", 
    name: "Listar Eventos",
    subject: "events" as Subjects,
    action: "read"
  },
];

// Rutas por rol
export const RoutesByRole: Record<
  string,
  {
    fondos?: RouteItem[];
    servicios?: RouteItem[];
    pacientes?: RouteItem[];
    empleados?: RouteItem[];
    turnos?: RouteItem[];
    ubicaciones?: RouteItem[];
    organizaciones?: RouteItem[];
    reportes?: RouteItem[];
    eventos?: RouteItem[];
  }
> = {
  [validRoles.admin]: {
    fondos: IncomesRoutes,
    servicios: [...HealthcaresRoutes, ...ServiceGroupsRoutes],
    pacientes: PatientsRoutes,
    empleados: PractitionersRoutes,
    turnos: ShiftsRoutes,
    ubicaciones: LocationsRoutes,
    organizaciones: OrganizationsRoutes,
    reportes: ReportsRoutes,
    eventos: EventsRoutes,
  },
  [validRoles.auditor]: {
    fondos: IncomesRoutes.filter(route => route.action === "read"), // Solo lectura
    servicios: [...HealthcaresRoutes, ...ServiceGroupsRoutes].filter(route => route.action === "read"),
    pacientes: PatientsRoutes.filter(route => route.action === "read"),
    empleados: PractitionersRoutes.filter(route => route.action === "read"),
    turnos: ShiftsRoutes.filter(route => route.action === "read"),
    ubicaciones: LocationsRoutes.filter(route => route.action === "read"),
    organizaciones: OrganizationsRoutes.filter(route => route.action === "read"),
    reportes: ReportsRoutes.filter(route => route.action === "read"),
    eventos: EventsRoutes,
  },
  [validRoles.cashier]: {
    fondos: IncomesRoutes,
    pacientes: PatientsRoutes,
    turnos: ShiftsRoutes,
    ubicaciones: LocationsRoutes.filter(route => route.action === "read"),
    reportes: ReportsRoutes,
  },
  [validRoles.ti]: {
    servicios: HealthcaresRoutes.filter(route => route.action === "read"),
    empleados: PractitionersRoutes,
    organizaciones: OrganizationsRoutes,
    reportes: ReportsRoutes,
    eventos: EventsRoutes,
  },
};

/*
* ==========================================================================
* HELPERS METHODS
* ==========================================================================
*/

// Función para filtrar rutas según abilities
export const filterRoutesByAbility = (routes: RouteItem[], ability: any): RouteItem[] => {
  return routes.filter(route => ability.can(route.action, route.subject));
};

// Función para obtener rutas filtradas por rol y ability
export const getFilteredRoutesByRole = (role: string, ability: any) => {
  const baseRoutes = RoutesByRole[role] || {};
  const filteredRoutes: any = {};

  // Filtrar cada categoría de rutas según las abilities
  Object.entries(baseRoutes).forEach(([key, routes]) => {
    if (Array.isArray(routes)) {
      filteredRoutes[key] = filterRoutesByAbility(routes as RouteItem[], ability);
    }
  });

  return filteredRoutes;
};