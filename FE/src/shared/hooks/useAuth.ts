import { useKeycloak } from "@react-keycloak/web";

export const useAuth = () => {
  const { keycloak, initialized } = useKeycloak();

  const token = keycloak?.token ?? null;
  const isAuthenticated = Boolean(keycloak?.authenticated);

  const hasRealmRole = (role: string) => keycloak?.hasRealmRole?.(role) ?? false;
  const hasResourceRole = (role: string, resource?: string) =>
    keycloak?.hasResourceRole?.(role, resource) ?? false;

  return { keycloak, initialized, token, isAuthenticated, hasRealmRole, hasResourceRole };
};
