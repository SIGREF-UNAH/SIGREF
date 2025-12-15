import Keycloak from "keycloak-js";

export const keycloak = new Keycloak({
  url: import.meta.env.VITE_KEYCLOAK_URL,
  realm: import.meta.env.VITE_KEYCLOAK_REALM,
  clientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID,
});

const clientId = import.meta.env.VITE_KEYCLOAK_CLIENT_ID;

// Función para obtener los roles del token de Keycloak
export function getRolesFromToken(keycloak: Keycloak): string[] {
  if (!keycloak?.authenticated || !keycloak?.tokenParsed) return [];

  const token = keycloak.tokenParsed as any;

  const realmRoles = token?.realm_access?.roles || [];
  const clientRoles = token?.resource_access?.[clientId]?.roles || [];

  return [...realmRoles, ...clientRoles];
}
