import Keycloak from "keycloak-js";

const clientId = import.meta.env.VITE_KEYCLOAK_CLIENT_ID;

export function getRolesFromToken(keycloak: Keycloak): string[] {
  if (!keycloak?.authenticated || !keycloak?.tokenParsed) return [];

  const token = keycloak.tokenParsed as any;

  const realmRoles = token?.realm_access?.roles || [];
  const clientRoles = token?.resource_access?.[clientId]?.roles || [];

  return [...realmRoles, ...clientRoles];
}
