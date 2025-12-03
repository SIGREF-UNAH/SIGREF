import { AbilityBuilder, Ability } from "@casl/ability";

export type Actions = "manage" | "read" | "create" | "update" | "delete";
export type Subjects =
  | "incomes"         // Fondos
  | "shifts"          // Turnos
  | "healthcares"     // Servicios
  | "patients"        // Pacientes
  | "practitioners"   // Empleados
  | "events"          // Eventos/Logs
  | "organizations"   // Organizaciones
  | "locations"       // Ubicaciones
  | "reports"         // Reportes
  | "users"            // Usuarios
  | "hospital"        // Hospital
  | "support"         // Soporte
  | "all";
  
export const defineAbilitiesFor = (roles: string[]) => {
  const { can, cannot, build } = new AbilityBuilder(Ability);

  // Administrador
  if (roles.includes("admin")) {
    can("manage", "all");
    cannot("read", "hospital");
    cannot("read", "support");

    //* Administrador no puede crear todos los usuarios
    can("create", "users");
  }

  // Auxiliar de caja
  if (roles.includes("cashier")) {
    can(["create", "read", "update"], "incomes");
    can(["create", "read", "update"], "healthcares");
    can(["create", "read", "update"], "patients");
    can("read", "shifts");
    cannot("read", "hospital");
    cannot("read", "support");

    //* No puede crear usuarios
    cannot("create", "users");
  }

  // Auditor
  if (roles.includes("auditor")) {
    can("read", "healthcares");
    can("read", "practitioners");
    can("read", "events");
    can("read", "incomes");
    can("read", "shifts");
    cannot("read", "hospital");
    cannot("read", "support");

    //* No puede crear usuarios
    cannot("create", "users");
  }

  // Tecnico de Informática
  if (roles.includes("ti")) {
    can(["create", "read", "update"], "practitioners");
    can(["create", "read", "update"], "events");
    can(["create", "read", "update"], "organizations");
    can(["create", "read", "update"], "locations");
    can(["create", "read", "update"], "users");
    can("read", "hospital");
    can("read", "support");

    //* Puede crear cualquier tipo de usuario
    can("create", "users");
  }

  // Restricciones generales
  cannot("delete", "practitioners");
  cannot("delete", "User");

  return build();
};
