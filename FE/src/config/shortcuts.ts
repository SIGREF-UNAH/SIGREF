import type { NavigateFunction } from "react-router";

export interface ShortcutConfig {
  keys: string;
  description: string;
  action: (
    navigate: NavigateFunction,
    setShowHelp?: (show: boolean) => void
  ) => void;
  roles: string[];
  category: string;
}

export interface ShortcutSection {
  title: string;
  roles: string[];
  shortcuts: {
    keys: string;
    description: string;
  }[];
}

export const appShortcuts: ShortcutConfig[] = [
  // Gestión de Fondos
  {
    keys: "ctrl+f",
    description: "Lista de Ingresos",
    action: (navigate) => navigate("/incomes/list"),
    roles: ["admin", "cashier", "auditor"],
    category: "Gestión de Fondos",
  },
  {
    keys: "ctrl+shift+f",
    description: "Generar ingreso",
    action: (navigate) => navigate("/incomes/create"),
    roles: ["admin", "cashier", "auditor"],
    category: "Gestión de Fondos",
  },
  {
    keys: "ctrl+f+c",
    description: "Cierre de caja",
    action: (navigate) => navigate("/incomes/close"),
    roles: ["admin", "cashier", "auditor"],
    category: "Gestión de Fondos",
  },
  {
    keys: "ctrl+f+h",
    description: "Historial de cierres",
    action: (navigate) => navigate("/incomes/history"),
    roles: ["admin", "cashier", "auditor"],
    category: "Gestión de Fondos",
  },

  // Gestión de Servicios
  {
    keys: "ctrl+s",
    description: "Listar servicios",
    action: (navigate) => navigate("/healthcares/list"),
    roles: ["admin", "cashier", "auditor"],
    category: "Gestión de Servicios",
  },
  {
    keys: "ctrl+shift+s",
    description: "Crear servicio",
    action: (navigate) => navigate("/healthcares/create"),
    roles: ["admin", "cashier", "auditor"],
    category: "Gestión de Servicios",
  },

  // Gestión de Organizaciones
  {
    keys: "ctrl+o",
    description: "Listar organizaciones",
    action: (navigate) => navigate("/organizations/list"),
    roles: ["admin", "ti"],
    category: "Gestión de Organizaciones",
  },
  {
    keys: "ctrl+shift+o",
    description: "Crear organización",
    action: (navigate) => navigate("/organizations/create"),
    roles: ["admin", "ti"],
    category: "Gestión de Organizaciones",
  },

  // Gestión de Ubicaciones
  {
    keys: "ctrl+u",
    description: "Listar ubicaciones",
    action: (navigate) => navigate("/locations/list"),
    roles: ["admin", "ti"],
    category: "Gestión de Ubicaciones",
  },
  {
    keys: "ctrl+shift+u",
    description: "Crear ubicación",
    action: (navigate) => navigate("/locations/create"),
    roles: ["admin", "ti"],
    category: "Gestión de Ubicaciones",
  },

  // Gestión de Reportes
  {
    keys: "ctrl+r",
    description: "Control de reportes",
    action: (navigate) => navigate("/reports/list"),
    roles: ["admin"],
    category: "Gestión de Reportes",
  },
  {
    keys: "ctrl+shift+r",
    description: "Generar reporte",
    action: (navigate) => navigate("/reports/create"),
    roles: ["admin"],
    category: "Gestión de Reportes",
  },
  {
    keys: "ctrl+r+h",
    description: "Historial de reportes",
    action: (navigate) => navigate("/reports/history"),
    roles: ["admin"],
    category: "Gestión de Reportes",
  },

  // Gestión de Empleados
  {
    keys: "ctrl+e",
    description: "Listar empleados",
    action: (navigate) => navigate("/practitioners/list"),
    roles: ["admin", "ti", "auditor"],
    category: "Gestión de Empleados",
  },
  {
    keys: "ctrl+shift+e",
    description: "Crear empleado",
    action: (navigate) => navigate("/practitioners/create"),
    roles: ["admin", "ti", "auditor"],
    category: "Gestión de Empleados",
  },

  // Gestión de Pacientes
  {
    keys: "ctrl+p",
    description: "Listar pacientes",
    action: (navigate) => navigate("/patients/list"),
    roles: ["admin", "cashier"],
    category: "Gestión de Pacientes",
  },
  {
    keys: "ctrl+shift+p",
    description: "Crear paciente",
    action: (navigate) => navigate("/patients/create"),
    roles: ["admin", "cashier"],
    category: "Gestión de Pacientes",
  },

  // Gestión de Eventos/Logs
  {
    keys: "ctrl+l",
    description: "Control de eventos/logs",
    action: (navigate) => navigate("/events/list"),
    roles: ["admin", "ti", "auditor"],
    category: "Gestión de Eventos/Logs",
  },
];
