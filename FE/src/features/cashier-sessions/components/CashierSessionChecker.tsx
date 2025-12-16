import { useEffect, useRef } from "react";
import { useNavigate, useLocation } from "react-router";
import { useKeycloak } from "@react-keycloak/web";
import { useCashierSessionStore } from "../store";
import { getRolesFromToken } from "../../../auth";

/*
 * Componente que verifica si un cajero tiene sesión activa
 * y lo redirige a abrir sesión SOLO AL INICIAR SESIÓN
*/

export function CashierSessionChecker() {
  const { keycloak, initialized } = useKeycloak();
  const navigate = useNavigate();
  const location = useLocation();
  const hasActiveSession = useCashierSessionStore((state) => state.hasActiveSession());
  const hasCheckedRef = useRef(false);

  useEffect(() => {
    if (!initialized || !keycloak.authenticated) return;

    if (hasCheckedRef.current) return;

    const roles = getRolesFromToken(keycloak);
    const isCashier = roles.includes("cashier");

    if (isCashier && !hasActiveSession) {
      if (location.pathname !== "/cashier/open-session") {
        navigate("/cashier/open-session", { replace: true });
      }
    }

    hasCheckedRef.current = true;
  }, [
    initialized,
    keycloak.authenticated,
    hasActiveSession,
    navigate,
    location.pathname,
  ]);

  return null;
}
