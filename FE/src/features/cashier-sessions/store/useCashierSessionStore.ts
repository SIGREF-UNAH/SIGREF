import { create } from "zustand";
import { persist } from "zustand/middleware";
import {
  parseStoredSession,
  type CashierSession,
} from "./cashierSessionStorage";

export type { CashierSession } from "./cashierSessionStorage";

export type CashierSessionValidationState =
  | "idle"
  | "validating"
  | "validated"
  | "error";

interface CashierSessionState {
  session: CashierSession | null;
  userId: string | null;
  validationState: CashierSessionValidationState;
  setSessionForUser: (userId: string, session: CashierSession) => void;
  clearSession: () => void;
  clearIfUserChanged: (userId: string) => void;
  getSessionForUser: (userId: string) => CashierSession | null;
  hasActiveSession: () => boolean;
  setValidationState: (validationState: CashierSessionValidationState) => void;
}

export const useCashierSessionStore = create<CashierSessionState>()(
  persist(
    (set, get) => ({
      session: null,
      userId: null,
      validationState: "idle",
      setSessionForUser: (userId, session) =>
        set({ session, userId, validationState: "validated" }),
      clearSession: () => set({ session: null, userId: null }),
      clearIfUserChanged: (userId) => {
        if (get().userId !== userId) {
          set({ session: null, userId: null, validationState: "idle" });
        }
      },
      getSessionForUser: (userId) => {
        if (get().userId !== userId) return null;
        return parseStoredSession(
          { userId: get().userId, session: get().session },
          userId,
        );
      },
      hasActiveSession: () => get().session !== null,
      setValidationState: (validationState) => set({ validationState }),
    }),
    {
      name: "cashier-session-storage",
      version: 2,
      partialize: (state) => ({
        session: state.session,
        userId: state.userId,
      }),
      migrate: (persistedState) => {
        const session = parseStoredSession(persistedState);
        return {
          session,
          userId: null,
          validationState: "idle" as const,
        };
      },
    }
  )
);
