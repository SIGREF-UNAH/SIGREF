import { useKeycloak } from "@react-keycloak/web";

export function useAuth() {
  const { keycloak } = useKeycloak();

  const isAuthenticated = keycloak?.authenticated ?? false;
  const roles: string[] = keycloak?.tokenParsed?.realm_access?.roles || [];

  const hasRole = (role: string) => roles.includes(role);
  const hasAnyRole = (checkRoles: string[]) =>
    checkRoles.some((r) => roles.includes(r));

  return { isAuthenticated, roles, hasRole, hasAnyRole };
}
