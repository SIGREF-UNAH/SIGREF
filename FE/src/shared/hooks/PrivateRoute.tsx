import { useKeycloak } from "@react-keycloak/web";
import type { ReactNode } from "react";
import { Navigate } from "react-router";

interface Props {
  children: ReactNode;
  roles?: string[];
}

export const PrivateRoute = ({ children, roles }: Props) => {
  const { keycloak } = useKeycloak();

  if (!keycloak?.authenticated) {
    return <Navigate to="/login" />; // TODO: verificaer si es /login
  }

  const userRoles: string[] = keycloak.tokenParsed?.realm_access?.roles || [];

  if (roles && !roles.some((r) => userRoles.includes(r))) {
    return <Navigate to="/" />; // TODO: agregar pagina de no autorizado
  }

  return <>{children}</>;
};


