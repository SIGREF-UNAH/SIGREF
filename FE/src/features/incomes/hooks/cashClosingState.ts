import type { CashierSession } from "../../cashier-sessions/store";

export interface ClosedSessionResponse {
  id?: string;
  systemAmount?: number | null;
}

export function completeCashierSessionClose(
  session: CashierSession,
  response: ClosedSessionResponse,
  clearSession: () => void,
) {
  clearSession();

  return {
    closedSession: session,
    closedSessionId: response.id || session.id,
    systemAmount: response.systemAmount || 0,
  };
}
