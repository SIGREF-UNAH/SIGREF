import { Navigate, Outlet, useLocation, useNavigate } from "react-router";
import { useKeycloak } from "@react-keycloak/web";
import { Spin, Result, Button } from "antd";
import { LockOutlined } from "@ant-design/icons";
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
  const navigate = useNavigate();
  const { keycloak, initialized } = useKeycloak();
  const location = useLocation();
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

  // Validar que el usuario es un cashier
  if (requiresCashierRole && !isCashier) {
    return (
      <div>
        <Result
          status="403"
          title="Acceso Denegado"
          subTitle="No tienes permisos para acceder a esta sección."
          extra={
            <Button type="primary" onClick={() => navigate(-1)}>
              Volver
            </Button>
          }
        />
      </div>
    );
  }

  // Validar que la sesión de caja esté abierta
  if (requiresActiveSession && !hasActiveSession) {
    // Si está intentando acceder a /incomes sin sesión, mostrar mensaje específico
    if (location.pathname.startsWith("/incomes")) {
      return (
        <div>
          <Result
            icon={<LockOutlined style={{ color: "#faad14" }} />}
            title="Iniciar Turno Requerido"
            subTitle="Debes iniciar un turno de trabajo para poder acceder a esta sección"
            extra={
              <Button type="primary" onClick={() => navigate("/cashier/open-session")}>
                Abrir Turno
              </Button>
            }
          />
        </div>
      );
    }
    return <Navigate to="/cashier/open-session" replace />;
  }

  return <Outlet />;
}
