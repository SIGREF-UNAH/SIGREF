import { AbilityBuilder, Ability } from "@casl/ability";

export type Actions = "manage" | "read" | "create" | "update" | "delete";
export type Subjects =
  | "incomes"         // Fondos
  | "healthcares"     // Servicios
  | "patients"        // Pacientes
  | "practitioners"   // Empleados
  | "events"          // Eventos/Logs
  | "organizations"   // Organizaciones
  | "locations"       // Ubicaciones
  | "reports"         // Reportes
  | "all";

export const defineAbilitiesFor = (roles: string[]) => {
  const { can, cannot, build } = new AbilityBuilder(Ability);

  // Administrador
  if (roles.includes("admin")) {
    can("manage", "all");
  }

  // Auxiliar de caja
  if (roles.includes("cashier")) {
    can(["create", "read", "update"], "incomes");
    can(["create", "read", "update"], "healthcares");
    can(["create", "read", "update"], "patients");
  }

  // Auditor
  if (roles.includes("auditor")) {
    can("read", "healthcares");
    can("read", "practitioners");
    can("read", "events");
    can("read", "incomes");
  }

  // Tecnico de Informática
  if (roles.includes("ti")) {
    can(["create", "read", "update"], "practitioners");
    can(["create", "read", "update"], "events");
    can(["create", "read", "update"], "organizations");
    can(["create", "read", "update"], "locations");
  }

  // Restricciones generales
  cannot("delete", "practitioners");
  cannot("delete", "User");

  return build();
};
