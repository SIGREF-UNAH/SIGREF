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
    incomes?: any[];
    healthcares?: any[];
    patients?: any[];
    practitioners?: any[];
    events?: any[];
    organizations?: any[];
    locations?: any[];
    reports?: any[];
  }
> = {
  cashier: {
    incomes: IncomesRoutes,
    healthcares: HealthcaresRoutes,
    patients: PatientsRoutes,
  },
  auditor: {
    healthcares: HealthcaresRoutes,
    practitioners: PractitionersRoutes,
    events: EventsRoutes,
  },
  admin: {
    incomes: IncomesRoutes,
    healthcares: HealthcaresRoutes,
    patients: PatientsRoutes,
    practitioners: PractitionersRoutes,
    organizations: OrganizationsRoutes,
  },
  ti: {
    practitioners: PractitionersRoutes,
    events: EventsRoutes,
    organizations: OrganizationsRoutes,
  },
};
