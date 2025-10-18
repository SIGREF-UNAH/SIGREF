export const IncomesRoutes = [
  { path: "/incomes/list", name: "Lista de Ingresos" },
  { path: "/incomes/create", name: "Generar Ingreso" },
  { path: "/incomes/close", name: "Cerrar Caja" },
  { path: "/incomes/history", name: "Historial de Cierres de Caja" },
];

export const HealthcaresRoutes = [
  { path: "/healthcares/list", name: "Listar Servicios Médicos" },
  { path: "/healthcares/create", name: "Crear Servicio Médico" },
];

export const PatientsRoutes = [
  { path: "/patients/list", name: "Listar Pacientes" },
  { path: "/patients/create", name: "Crear Paciente" },
];

export const PractitionersRoutes = [
  { path: "/practitioners/list", name: "Listar Empleados" },
  { path: "/practitioners/create", name: "Crear Empleado" },
];

export const EventsRoutes = [
  { path: "/events/list", name: "Listar Eventos" },
];

export const OrganizationsRoutes = [
  { path: "/organizations/list", name: "Listar Organizaciones" },
  { path: "/organizations/create", name: "Crear Organización" },
];

export const LocationsRoutes = [
  { path: "/locations/list", name: "Listar Ubicaciones" },
  { path: "/locations/create", name: "Crear Ubicación" },
];

export const ReportsRoutes = [
  { path: "/organizations/list", name: "Control de Reportes" },
  { path: "/organizations/create", name: "Generar Reporte" },
  { path: "/organizations/history", name: "Historial de Reportes" },
];

// Rutas validas para cada rol
export const RoutesByRole: Record<
  string,
  {
    fondos?: any[];
    servicios?: any[];
    pacientes?: any[];
    empleados?: any[];
    eventos?: any[];
    organizaciones?: any[];
    ubicaciones?: any[];
    reportes?: any[];
  }
> = {
  "Administrador": {
    fondos: IncomesRoutes,
    servicios: HealthcaresRoutes,
    organizaciones: OrganizationsRoutes,
    ubicaciones: LocationsRoutes,
    reportes: ReportsRoutes,
    empleados: PractitionersRoutes,
    pacientes: PatientsRoutes,
    eventos: EventsRoutes,
  },
  "Auditoria": {
    fondos: IncomesRoutes,
    servicios: HealthcaresRoutes,
    empleados: PractitionersRoutes,
    eventos: EventsRoutes,
  },
  "Auxiliar de Caja": {
    fondos: IncomesRoutes,
    servicios: HealthcaresRoutes,
    pacientes: PatientsRoutes,
  },
  "Técnico de Informática": {
    organizaciones: OrganizationsRoutes,
    ubicaciones: LocationsRoutes,
    empleados: PractitionersRoutes,
    eventos: EventsRoutes,
  },
};
