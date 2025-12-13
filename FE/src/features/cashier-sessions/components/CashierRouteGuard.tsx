import { Navigate, Outlet } from "react-router";
import { useKeycloak } from "@react-keycloak/web";
import { Spin } from "antd";
import { useCashierSessionStore } from "../store";
import { getRolesFromToken } from "../../../auth";

interface CashierRouteGuardProps {
  requiresActiveSession?: boolean;
  requiresCashierRole?: boolean;
}

export function CashierRouteGuard({
  requiresActiveSession = false,
  requiresCashierRole = true,
}: CashierRouteGuardProps) {
  
  const { keycloak, initialized } = useKeycloak();
  const hasActiveSession = useCashierSessionStore((state) =>state.hasActiveSession());

  if (!initialized) {
    return (
      <div className="flex items-center justify-center h-screen">
        <Spin size="large" />
      </div>
    );
  }

  if (!keycloak.authenticated) {
    return <Navigate to="/" replace />;
  }

  const roles = getRolesFromToken(keycloak);
  const isCashier = roles.includes("cashier");

  // Revisar si el usuario tiene el rol de cashier
  if (requiresCashierRole && !isCashier) {
    return <Navigate to="/" replace />;
  }

  // Revisar si el usuario tiene una sesión de caja activa
  if (requiresActiveSession && !hasActiveSession) {
    return <Navigate to="/cashier/open-session" replace />;
  }

  // Revisar si el usuario tiene una sesión de caja activa y quiere iniciar otra
  if (!requiresActiveSession && hasActiveSession) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}
