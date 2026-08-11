import { describe, expect, it, vi } from "vitest";
import {
  validateCashierSession,
  type ActiveSessionFetcher,
} from "./useValidateCashierSession";

const backendSession = {
  id: "session-from-backend",
  userId: "cashier-a",
  openAt: "2026-08-10T13:00:00.000Z",
  shiftId: "shift-1",
};

describe("validateCashierSession", () => {
  it("replaces stale local data with the Backend session", async () => {
    const setSessionForUser = vi.fn();
    const clearSession = vi.fn();
    const fetchSession: ActiveSessionFetcher = vi
      .fn()
      .mockResolvedValue(backendSession);

    await validateCashierSession(
      "cashier-a",
      fetchSession,
      setSessionForUser,
      clearSession,
    );

    expect(setSessionForUser).toHaveBeenCalledWith("cashier-a", {
      id: backendSession.id,
      openAt: backendSession.openAt,
      shiftId: backendSession.shiftId,
    });
    expect(clearSession).not.toHaveBeenCalled();
  });

  it("clears local data when Backend returns no active session", async () => {
    const error = Object.assign(new Error("not found"), {
      response: { status: 404 },
    });
    const fetchSession: ActiveSessionFetcher = vi.fn().mockRejectedValue(error);
    const setSessionForUser = vi.fn();
    const clearSession = vi.fn();

    await expect(
      validateCashierSession(
        "cashier-a",
        fetchSession,
        setSessionForUser,
        clearSession,
      ),
    ).resolves.toBe("none");
    expect(clearSession).toHaveBeenCalledOnce();
    expect(setSessionForUser).not.toHaveBeenCalled();
  });

  it("does not authorize stale cache when validation fails", async () => {
    const error = Object.assign(new Error("server error"), {
      response: { status: 500 },
    });
    const fetchSession: ActiveSessionFetcher = vi.fn().mockRejectedValue(error);
    const setSessionForUser = vi.fn();
    const clearSession = vi.fn();

    await expect(
      validateCashierSession(
        "cashier-a",
        fetchSession,
        setSessionForUser,
        clearSession,
      ),
    ).rejects.toBe(error);
    expect(setSessionForUser).not.toHaveBeenCalled();
    expect(clearSession).not.toHaveBeenCalled();
  });
});
