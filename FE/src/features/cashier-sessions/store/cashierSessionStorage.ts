export interface CashierSession {
  id: string;
  openAt: string;
  shiftName?: string;
  locationName?: string;
  shiftId?: string;
  locationId?: string;
}

export interface PersistedCashierSession {
  userId: string;
  session: CashierSession;
}

function isCashierSession(value: unknown): value is CashierSession {
  if (!value || typeof value !== "object") return false;

  const session = value as Partial<CashierSession>;
  return typeof session.id === "string" && typeof session.openAt === "string";
}

export function parseStoredSession(
  value: unknown,
  currentUserId?: string,
): CashierSession | null {
  if (!value || typeof value !== "object") return null;

  const stored = value as Partial<PersistedCashierSession>;
  if (typeof stored.userId !== "string" || !isCashierSession(stored.session)) {
    return null;
  }

  if (currentUserId !== undefined && stored.userId !== currentUserId) {
    return null;
  }

  return stored.session;
}

export function getSessionForUser(
  value: unknown,
  currentUserId: string,
): CashierSession | null {
  return parseStoredSession(value, currentUserId);
}
