import { useCashierSessionStore } from "../features/cashier-sessions/store";

export function cleanupClientSession() {
  useCashierSessionStore.getState().clearSession();
  localStorage.removeItem("kc_token");
}
