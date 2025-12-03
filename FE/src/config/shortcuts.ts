import type { NavigateFunction } from "react-router";

export interface ShortcutConfig {
  keys: string;
  title: string;
  description: string;
  roles: string[];
  action: (
    navigate: NavigateFunction,
    setShowHelp?: (show: boolean) => void
  ) => void;
  category: string;
}

export interface ShortcutSection {
  title: string;
  category: string;
  shortcuts: {
    keys: string;
    description: string;
  }[];
}

export const appShortcuts: ShortcutConfig[] = [
  // Gestión de Fondos
  {
    keys: "ctrl+f",
    description: "Listar Ingresos",
    action: (navigate) => navigate("/incomes/list"),
    title: "Gestión de Fondos",
    category: "incomes",
    roles: ["admin", "cashier", "auditor"],
  },
  {
    keys: "ctrl+shift+f",
    description: "Generar ingreso",
    action: (navigate) => navigate("/incomes/create"),
    title: "Gestión de Fondos",
    category: "incomes",
    roles: ["admin", "cashier", "auditor"],
  },
  {
    keys: "ctrl+f+c",
    description: "Cierre de caja",
    action: (navigate) => navigate("/incomes/close"),
    title: "Gestión de Fondos",
    category: "incomes",
    roles: ["admin", "cashier", "auditor"],
  },
  {
    keys: "ctrl+f+h",
    description: "Historial de cierres",
    action: (navigate) => navigate("/incomes/history"),
    title: "Gestión de Fondos",
    category: "incomes",
    roles: ["admin", "cashier", "auditor"],
  },

  // Gestión de Turnos
  {
    keys: "ctrl+t",
    description: "Listar Turnos",
    action: (navigate) => navigate("/shifts/list"),
    title: "Gestión de Turnos",
    category: "shifts",
    roles: ["admin", "cashier", "auditor"],
  },

  // Gestión de Servicios
  {
    keys: "ctrl+s",
    description: "Listar servicios",
    action: (navigate) => navigate("/healthcares/list"),
    title: "Gestión de Servicios",
    category: "healthcares",
    roles: ["admin", "cashier", "auditor"],
  },
  {
    keys: "ctrl+shift+s",
    description: "Crear servicio",
    action: (navigate) => navigate("/healthcares/create"),
    title: "Gestión de Servicios",
    category: "healthcares",
    roles: ["admin", "cashier", "auditor"],
  },
  {
    keys: "alt+s",
    description: "Listar paquetes",
    action: (navigate) => navigate("/service-groups/list"),
    title: "Gestión de Servicios",
    category: "healthcares",
    roles: ["admin", "cashier", "auditor"],
  },
  {
    keys: "alt+shift+s",
    description: "Crear paquete",
    action: (navigate) => navigate("/service-groups/create"),
    title: "Gestión de Servicios",
    category: "healthcares",
    roles: ["admin", "cashier", "auditor"],
  },

  // Gestión de Organizaciones
  {
    keys: "ctrl+o",
    description: "Listar organizaciones",
    action: (navigate) => navigate("/organizations/list"),
    title: "Gestión de Organizaciones",
    category: "organizations",
    roles: ["admin", "ti"],
  },
  {
    keys: "ctrl+shift+o",
    description: "Crear organización",
    action: (navigate) => navigate("/organizations/create"),
    title: "Gestión de Organizaciones",
    category: "organizations",
    roles: ["admin", "ti"],
  },

  // Gestión de Ubicaciones
  {
    keys: "ctrl+u",
    description: "Listar ubicaciones",
    action: (navigate) => navigate("/locations/list"),
    title: "Gestión de Ubicaciones",
    category: "locations",
    roles: ["admin", "ti"],
  },
  {
    keys: "ctrl+shift+u",
    description: "Crear ubicación",
    action: (navigate) => navigate("/locations/create"),
    title: "Gestión de Ubicaciones",
    category: "locations",
    roles: ["admin", "ti"],
  },

  // Gestión de Reportes
  {
    keys: "ctrl+r",
    description: "Control de reportes",
    action: (navigate) => navigate("/reports/list"),
    title: "Gestión de Reportes",
    category: "reports",
    roles: ["admin"],
  },
  {
    keys: "ctrl+shift+r",
    description: "Generar reporte",
    action: (navigate) => navigate("/reports/create"),
    title: "Gestión de Reportes",
    category: "reports",
    roles: ["admin"],
  },
  {
    keys: "ctrl+r+h",
    description: "Historial de reportes",
    action: (navigate) => navigate("/reports/history"),
    title: "Gestión de Reportes",
    category: "reports",
    roles: ["admin"],
  },

  // Gestión de Empleados
  {
    keys: "ctrl+e",
    description: "Listar empleados",
    action: (navigate) => navigate("/practitioners/list"),
    title: "Gestión de Empleados",
    category: "practitioners",
    roles: ["admin", "ti", "auditor"],
  },
  {
    keys: "ctrl+shift+e",
    description: "Crear empleado",
    action: (navigate) => navigate("/practitioners/create"),
    title: "Gestión de Empleados",
    category: "practitioners",
    roles: ["admin", "ti", "auditor"],
  },

  // Gestión de Pacientes
  {
    keys: "ctrl+p",
    description: "Listar pacientes",
    action: (navigate) => navigate("/patients/list"),
    title: "Gestión de Pacientes",
    category: "patients",
    roles: ["admin", "cashier"],
  },
  {
    keys: "ctrl+shift+p",
    description: "Crear paciente",
    action: (navigate) => navigate("/patients/create"),
    title: "Gestión de Pacientes",
    category: "patients",
    roles: ["admin", "cashier"],
  },

  // Gestión de Eventos/Logs
  {
    keys: "ctrl+l",
    description: "Control de eventos/logs",
    action: (navigate) => navigate("/events/list"),
    title: "Gestión de Eventos/Logs",
    category: "events",
     roles: ["admin", "ti", "auditor"],
  },
];
