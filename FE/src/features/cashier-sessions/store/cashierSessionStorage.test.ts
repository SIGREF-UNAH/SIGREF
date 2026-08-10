import { describe, expect, it } from "vitest";
import {
  getSessionForUser,
  parseStoredSession,
  type CashierSession,
} from "./cashierSessionStorage";

const activeSession: CashierSession = {
  id: "session-1",
  openAt: "2026-08-10T12:00:00.000Z",
};

describe("cashier session storage", () => {
  it("does not expose cashier A session to cashier B", () => {
    const stored = { userId: "cashier-a", session: activeSession };

    expect(getSessionForUser(stored, "cashier-b")).toBeNull();
  });

  it("rejects legacy storage without an owner", () => {
    expect(parseStoredSession({ session: activeSession }, "cashier-a")).toBeNull();
  });

  it("returns the cached session only for its owner", () => {
    expect(
      getSessionForUser(
        { userId: "cashier-a", session: activeSession },
        "cashier-a",
      ),
    ).toEqual(activeSession);
  });
});
