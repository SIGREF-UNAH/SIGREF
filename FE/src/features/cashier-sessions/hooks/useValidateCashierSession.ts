import { useEffect, useState } from "react";
import { getActiveSession } from "../../../api/cashier-sessions/cashier-sessions";
import type { CashierSessionDto } from "../../../api/models/cashierSessionDto";
import { useCashierSessionStore } from "../store";

export type ActiveSessionFetcher = () => Promise<CashierSessionDto>;
type SessionSetter = (
  userId: string,
  session: { id: string; openAt: string; shiftId?: string },
) => void;

function getErrorStatus(error: unknown): number | undefined {
  if (!error || typeof error !== "object") return undefined;
  const response = (error as { response?: { status?: number } }).response;
  return response?.status;
}

function mapSession(session: CashierSessionDto) {
  if (!session.id || !session.openAt) {
    throw new Error("La sesión activa devuelta por Backend es inválida");
  }

  return {
    id: session.id,
    openAt: session.openAt,
    shiftId: session.shiftId,
  };
}

export async function validateCashierSession(
  userId: string,
  fetchSession: ActiveSessionFetcher,
  setSessionForUser: SessionSetter,
  clearSession: () => void,
): Promise<"active" | "none"> {
  try {
    const session = await fetchSession();
    setSessionForUser(userId, mapSession(session));
    return "active";
  } catch (error) {
    if (getErrorStatus(error) === 404) {
      clearSession();
      return "none";
    }

    throw error;
  }
}

export function useValidateCashierSession(userId: string | undefined) {
  const session = useCashierSessionStore((state) => state.session);
  const storedUserId = useCashierSessionStore((state) => state.userId);
  const validationState = useCashierSessionStore(
    (state) => state.validationState,
  );
  const setSessionForUser = useCashierSessionStore(
    (state) => state.setSessionForUser,
  );
  const clearSession = useCashierSessionStore((state) => state.clearSession);
  const clearIfUserChanged = useCashierSessionStore(
    (state) => state.clearIfUserChanged,
  );
  const setValidationState = useCashierSessionStore(
    (state) => state.setValidationState,
  );
  const [error, setError] = useState<unknown>(null);

  useEffect(() => {
    if (!userId) {
      clearSession();
      setValidationState("idle");
      return;
    }

    let cancelled = false;
    clearIfUserChanged(userId);
    setValidationState("validating");
    setError(null);

    validateCashierSession(
      userId,
      () => getActiveSession(),
      setSessionForUser,
      clearSession,
    )
      .then(() => {
        if (!cancelled) setValidationState("validated");
      })
      .catch((validationError) => {
        if (!cancelled) {
          setError(validationError);
          setValidationState("error");
        }
      });

    return () => {
      cancelled = true;
    };
  }, [
    clearIfUserChanged,
    clearSession,
    setSessionForUser,
    setValidationState,
    userId,
  ]);

  return {
    session: storedUserId === userId ? session : null,
    isValidating: validationState === "validating",
    isValidated: validationState === "validated",
    hasActiveSession:
      validationState === "validated" && storedUserId === userId && !!session,
    error,
  };
}
