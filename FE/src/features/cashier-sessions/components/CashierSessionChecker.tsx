import { useEffect } from "react";
import { useNavigate, useLocation } from "react-router";
import { useKeycloak } from "@react-keycloak/web";
import { getRolesFromToken } from "../../../auth";
import { useValidateCashierSession } from "../hooks";

/** Valida la sesiÃ³n activa contra Backend antes de redirigir o habilitar caja. */
export function CashierSessionChecker() {
  const { keycloak, initialized } = useKeycloak();
  const navigate = useNavigate();
  const location = useLocation();
  const userId = keycloak.tokenParsed?.sub;
  const isCashier = getRolesFromToken(keycloak).includes("cashier");
  const { isValidated, hasActiveSession } = useValidateCashierSession(
    isCashier ? userId : undefined,
  );

  useEffect(() => {
    if (!initialized || !keycloak.authenticated || !isCashier || !userId) {
      return;
    }

    if (!isValidated) return;

    if (!hasActiveSession && location.pathname !== "/cashier/open-session") {
      navigate("/cashier/open-session", { replace: true });
    }
  }, [
    initialized,
    keycloak.authenticated,
    isCashier,
    userId,
    isValidated,
    hasActiveSession,
    navigate,
    location.pathname,
  ]);

  return null;
}
