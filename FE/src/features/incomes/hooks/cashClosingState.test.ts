import { describe, expect, it, vi } from "vitest";
import { completeCashierSessionClose } from "./cashClosingState";

describe("completeCashierSessionClose", () => {
  it("clears the persisted session after explicit close succeeds", () => {
    const clearSession = vi.fn();
    const session = { id: "session-1", openAt: "2026-08-10T12:00:00.000Z" };

    const result = completeCashierSessionClose(
      session,
      { id: "session-1", systemAmount: 250 },
      clearSession,
    );

    expect(clearSession).toHaveBeenCalledOnce();
    expect(result).toEqual({
      closedSession: session,
      closedSessionId: "session-1",
      systemAmount: 250,
    });
  });
});
