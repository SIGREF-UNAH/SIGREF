import type { NavigateFunction } from "react-router";
import type { Subjects } from "../auth";

export interface ShortcutConfig {
  keys: string;
  title: string;
  description: string;
  action: (
    navigate: NavigateFunction,
    setShowHelp?: (show: boolean) => void
  ) => void;
  category: Subjects; // Cambia a Subjects para que coincida con abilities
  requiredAction: "read" | "create" | "update" | "delete"; // Acción requerida
  requiredSubject: Subjects; // Sujeto/recurso requerido
}

export interface ShortcutSection {
  title: string;
  category: Subjects;
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
    requiredAction: "read",
    requiredSubject: "incomes",
  },
  {
    keys: "ctrl+shift+f",
    description: "Generar ingreso",
    action: (navigate) => navigate("/incomes/create"),
    title: "Gestión de Fondos",
    category: "incomes",
    requiredAction: "create",
    requiredSubject: "incomes",
  },
  {
    keys: "ctrl+f+c",
    description: "Cierre de caja",
    action: (navigate) => navigate("/incomes/close"),
    title: "Gestión de Fondos",
    category: "incomes",
    requiredAction: "update",
    requiredSubject: "incomes",
  },
  {
    keys: "ctrl+f+h",
    description: "Historial de cierres",
    action: (navigate) => navigate("/incomes/history"),
    title: "Gestión de Fondos",
    category: "incomes",
    requiredAction: "read",
    requiredSubject: "incomes",
  },

  // Gestión de Servicios
  {
    keys: "ctrl+s",
    description: "Listar servicios",
    action: (navigate) => navigate("/healthcares/list"),
    title: "Gestión de Servicios",
    category: "healthcares",
    requiredAction: "read",
    requiredSubject: "healthcares",
  },
  {
    keys: "ctrl+shift+s",
    description: "Crear servicio",
    action: (navigate) => navigate("/healthcares/create"),
    title: "Gestión de Servicios",
    category: "healthcares",
    requiredAction: "create",
    requiredSubject: "healthcares",
  },
  {
    keys: "alt+s",
    description: "Listar paquetes",
    action: (navigate) => navigate("/service-groups/list"),
    title: "Gestión de Servicios",
    category: "healthcares",
    requiredAction: "read",
    requiredSubject: "service-groups",
  },
  {
    keys: "alt+shift+s",
    description: "Crear paquete",
    action: (navigate) => navigate("/service-groups/create"),
    title: "Gestión de Servicios",
    category: "healthcares",
    requiredAction: "create",
    requiredSubject: "service-groups",
  },

  // Gestión de Pacientes
  {
    keys: "ctrl+p",
    description: "Listar pacientes",
    action: (navigate) => navigate("/patients/list"),
    title: "Gestión de Pacientes",
    category: "patients",
    requiredAction: "read",
    requiredSubject: "patients",
  },
  {
    keys: "ctrl+shift+p",
    description: "Crear paciente",
    action: (navigate) => navigate("/patients/create"),
    title: "Gestión de Pacientes",
    category: "patients",
    requiredAction: "create",
    requiredSubject: "patients",
  },

  // Gestión de Empleados
  {
    keys: "ctrl+e",
    description: "Listar empleados",
    action: (navigate) => navigate("/practitioners/list"),
    title: "Gestión de Empleados",
    category: "practitioners",
    requiredAction: "read",
    requiredSubject: "practitioners",
  },
  {
    keys: "ctrl+shift+e",
    description: "Crear empleado",
    action: (navigate) => navigate("/practitioners/create"),
    title: "Gestión de Empleados",
    category: "practitioners",
    requiredAction: "create",
    requiredSubject: "practitioners",
  },

  // Gestión de Turnos
  {
    keys: "ctrl+t",
    description: "Listar Turnos",
    action: (navigate) => navigate("/shifts/list"),
    title: "Gestión de Turnos",
    category: "shifts",
    requiredAction: "read",
    requiredSubject: "shifts",
  },
  {
    keys: "alt+t",
    description: "Iniciar Turno",
    action: (navigate) => navigate("/cashier/open-session"),
    title: "Gestión de Turnos",
    category: "shifts",
    requiredAction: "create",
    requiredSubject: "cashier-sessions",
  },

  // Gestión de Ubicaciones
  {
    keys: "ctrl+u",
    description: "Listar ubicaciones",
    action: (navigate) => navigate("/locations/list"),
    title: "Gestión de Ubicaciones",
    category: "locations",
    requiredAction: "read",
    requiredSubject: "locations",
  },
  {
    keys: "ctrl+shift+u",
    description: "Crear ubicación",
    action: (navigate) => navigate("/locations/create"),
    title: "Gestión de Ubicaciones",
    category: "locations",
    requiredAction: "create",
    requiredSubject: "locations",
  },

  // Gestión de Organizaciones
  {
    keys: "ctrl+o",
    description: "Listar organizaciones",
    action: (navigate) => navigate("/organizations/list"),
    title: "Gestión de Organizaciones",
    category: "organizations",
    requiredAction: "read",
    requiredSubject: "organizations",
  },
  {
    keys: "ctrl+shift+o",
    description: "Crear organización",
    action: (navigate) => navigate("/organizations/create"),
    title: "Gestión de Organizaciones",
    category: "organizations",
    requiredAction: "create",
    requiredSubject: "organizations",
  },

  // Gestión de Reportes
  {
    keys: "ctrl+r",
    description: "Control de reportes",
    action: (navigate) => navigate("/reports/list"),
    title: "Gestión de Reportes",
    category: "reports",
    requiredAction: "read",
    requiredSubject: "reports",
  },
  {
    keys: "ctrl+shift+r",
    description: "Generar reporte",
    action: (navigate) => navigate("/reports/create"),
    title: "Gestión de Reportes",
    category: "reports",
    requiredAction: "create",
    requiredSubject: "reports",
  },
  {
    keys: "ctrl+r+h",
    description: "Historial de reportes",
    action: (navigate) => navigate("/reports/history"),
    title: "Gestión de Reportes",
    category: "reports",
    requiredAction: "read",
    requiredSubject: "reports",
  },

  // Gestión de Series
  {
    keys: "ctrl+shift+b",
    description: "Generar series",
    action: (navigate) => navigate("/series/create"),
    title: "Gestión de Series",
    category: "series",
    requiredAction: "create",
    requiredSubject: "series",
  },
  {
    keys: "ctrl+b",
    description: "Listar series",
    action: (navigate) => navigate("/series/list"),
    title: "Gestión de Series",
    category: "series",
    requiredAction: "read",
    requiredSubject: "series",
  },

  // Gestión de Eventos/Logs
  {
    keys: "ctrl+l",
    description: "Control de eventos/logs",
    action: (navigate) => navigate("/events/list"),
    title: "Gestión de Eventos/Logs",
    category: "events",
    requiredAction: "read",
    requiredSubject: "events",
  },
];

// Función para filtrar shortcuts según abilities
export const filterShortcutsByAbility = (shortcuts: ShortcutConfig[], ability: any): ShortcutConfig[] => {
  return shortcuts.filter(shortcut => 
    ability.can(shortcut.requiredAction, shortcut.requiredSubject)
  );
};

// Función para crear secciones filtradas
export const getFilteredShortcutSections = (ability: any): ShortcutSection[] => {
  const filteredShortcuts = filterShortcutsByAbility(appShortcuts, ability);
  
  const sectionsMap = new Map<string, ShortcutSection>();
  
  filteredShortcuts.forEach(shortcut => {
    const key = `${shortcut.title}|${shortcut.category}`;
    
    if (!sectionsMap.has(key)) {
      sectionsMap.set(key, {
        title: shortcut.title,
        category: shortcut.category,
        shortcuts: []
      });
    }
    
    const section = sectionsMap.get(key)!;
    section.shortcuts.push({
      keys: shortcut.keys,
      description: shortcut.description
    });
  });
  
  return Array.from(sectionsMap.values());
};