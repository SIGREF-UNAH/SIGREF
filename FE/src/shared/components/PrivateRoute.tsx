import type { PropsWithChildren, ReactNode } from "react";
import { useKeycloak } from "@react-keycloak/web";

export type PrivateRouteProps = PropsWithChildren<{
  roles?: string[]; // Roles requeridos (realm o de cliente)
  requireAll?: boolean; // Si true, requiere todos los roles; si false, al menos uno
  clientId?: string; // Client ID para roles de recurso (Keycloak client roles)
  fallback?: ReactNode; // UI alternativa cuando no hay autorización
}>;

export function PrivateRoute({
  children,
  roles,
  requireAll = false,
  clientId = import.meta.env.VITE_KEYCLOAK_CLIENT_ID,
  fallback = null,
}: PrivateRouteProps) {
  const { keycloak, initialized } = useKeycloak();

  if (!initialized) {
    return null; // Aquí puedes renderizar un loader si tienes uno
  }

  if (!keycloak.authenticated) {
    keycloak.login({ redirectUri: window.location.href });
    return null;
  }

  // Si no se especifican roles, solo requiere autenticación
  if (!roles || roles.length === 0) {
    return <>{children}</>;
  }

  const hasRole = (role: string) => {
    const hasRealm = typeof keycloak.hasRealmRole === "function" && keycloak.hasRealmRole(role);
    const hasClient =
      !!clientId && typeof keycloak.hasResourceRole === "function" && keycloak.hasResourceRole(role, clientId);
    return Boolean(hasRealm || hasClient);
  };

  const isAllowed = requireAll ? roles.every(hasRole) : roles.some(hasRole);

  if (!isAllowed) {
    // Si se provee un fallback, lo mostramos; de lo contrario, no renderizamos nada
    return <>{fallback}</>;
  }

  return <>{children}</>;
}
