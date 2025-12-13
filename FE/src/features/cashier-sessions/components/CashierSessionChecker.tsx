import { useEffect } from "react";
import { useNavigate } from "react-router";
import { useKeycloak } from "@react-keycloak/web";
import { useCashierSessionStore } from "../store";
import { getRolesFromToken } from "../../../auth";

/*
* Componente que verifica si un cajero tiene sesión activa
* y lo redirige a abrir sesión si no la tiene
*/

export function CashierSessionChecker() {
  const { keycloak, initialized } = useKeycloak();
  const navigate = useNavigate();
  const hasActiveSession = useCashierSessionStore((state) =>
    state.hasActiveSession()
  );

  useEffect(() => {
    if (!initialized || !keycloak.authenticated) return;

    const roles = getRolesFromToken(keycloak);
    const isCashier = roles.includes("cashier");

    // Si es cajero y no tiene sesión activa, redirigir
    if (isCashier && !hasActiveSession) {
      navigate("/cashier/open-session", { replace: true });
    }
  }, [initialized, keycloak, hasActiveSession, navigate]);

  return null;
}
