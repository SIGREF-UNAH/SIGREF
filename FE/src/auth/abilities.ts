import { AbilityBuilder, Ability } from "@casl/ability";

export type Actions = "manage" | "read" | "create" | "update" | "delete";
export type Subjects =
  | "incomes"             // Fondos
  | "shifts"              // Turnos
  | "healthcares"         // Servicios
  | "service-groups"      // Paquetes de Servicios
  | "patients"            // Pacientes
  | "practitioners"       // Empleados
  | "practitioner-roles"  // Cargos
  | "events"              // Eventos/Logs
  | "organizations"       // Organizaciones
  | "locations"           // Ubicaciones
  | "reports"             // Reportes
  | "users"               // Usuarios
  | "hospital"            // Hospital
  | "support"             // Soporte
  | "all";
  
export const defineAbilitiesFor = (roles: string[]) => {
  const { can, build } = new AbilityBuilder(Ability);

  // Administrador
  if (roles.includes("admin")) {
    can(["read", "create", "update", "delete"], "organizations");
    can(["read", "create", "update", "delete"], "locations");
    can(["read", "create", "update", "delete"], "practitioners");
    can(["read", "create", "update", "delete"], "practitioner-roles");
    can(["read", "create", "update", "delete"], "patients");
    can(["read", "create", "update", "delete"], "shifts");
    can(["read", "create", "update", "delete"], "healthcares");
    can(["read", "create", "update", "delete"], "service-groups");

    // TODO: Falta definir reports / incomes
    can(["read", "create", "update", "delete"], "incomes");
    can(["read", "create", "update", "delete"], "reports");
    
    // Menu desplegable
    can(["read", "create", "update", "delete"], "users");
    can(["read"], "hospital");
  }

  // Tecnico de Informática
  if (roles.includes("ti")) {
    can(["read", "create", "update", "delete"], "organizations");
    can(["read", "create", "update", "delete"], "practitioners");
    can(["read", "create", "update", "delete"], "practitioner-roles");
    can(["read"], "healthcares");
    can(["read"], "events");

    // TODO: Falta definir reports / incomes
    can(["read", "create", "update", "delete"], "reports");

    // Menu desplegable
    can(["read", "create", "update", "delete"], "users");
    can(["read", "create", "update"], "hospital");
    can(["read"], "support");
  }

  // Auditor
  if (roles.includes("auditor")) {
    can(["read"], "organizations");
    can(["read"], "locations");
    can(["read"], "practitioners");
    can(["read"], "patients");
    can(["read"], "shifts");
    can(["read"], "healthcares");
    can(["read"], "service-groups");
    can(["read"], "reports");
    can(["read"], "incomes");
    can(["read"], "events");

    // Menu desplegable
    can(["read"], "users");
    can(["read"], "hospital");
  }

  // Auxiliar de caja
  if (roles.includes("cashier")) {
    can(["read"], "locations");
    can(["read", "create", "update"], "patients");
    can(["read"], "shifts");

    // TODO: Falta definir reports / incomes
    can(["read", "create", "update", "delete"], "incomes");
    can(["read", "create", "update", "delete"], "reports");
    
    // Menu desplegable
    can(["read"], "hospital");
  }

  return build();
};
