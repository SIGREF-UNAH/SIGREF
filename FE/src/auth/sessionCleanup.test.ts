import { beforeEach, describe, expect, it, vi } from "vitest";
import { cleanupClientSession } from "./sessionCleanup";

const clearSession = vi.fn();
const storage = new Map<string, string>();

Object.defineProperty(globalThis, "localStorage", {
  configurable: true,
  value: {
    getItem: (key: string) => storage.get(key) ?? null,
    removeItem: (key: string) => storage.delete(key),
    setItem: (key: string, value: string) => storage.set(key, value),
    clear: () => storage.clear(),
  },
});

vi.mock("../features/cashier-sessions/store", () => ({
  useCashierSessionStore: {
    getState: () => ({ clearSession }),
  },
}));

describe("cleanupClientSession", () => {
  beforeEach(() => {
    clearSession.mockClear();
    localStorage.clear();
  });

  it("clears cashier session and kc_token on logout cleanup", () => {
    localStorage.setItem("kc_token", "token-from-cashier-a");

    cleanupClientSession();

    expect(localStorage.getItem("kc_token")).toBeNull();
    expect(clearSession).toHaveBeenCalledOnce();
  });

  it("does not call the cashier-session close endpoint", () => {
    const fetchSpy = vi.spyOn(globalThis, "fetch");

    cleanupClientSession();

    expect(fetchSpy).not.toHaveBeenCalled();
    fetchSpy.mockRestore();
  });
});
