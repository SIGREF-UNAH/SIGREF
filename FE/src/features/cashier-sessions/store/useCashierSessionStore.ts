import { create } from "zustand";
import { persist } from "zustand/middleware";

interface CashierSession {
  id: string;
  openAt: string;
  shiftName?: string;
  locationName?: string;
  shiftId?: string;
  locationId?: string;
}

interface CashierSessionState {
  session: CashierSession | null;
  setSession: (session: CashierSession) => void;
  clearSession: () => void;
  hasActiveSession: () => boolean;
}

export const useCashierSessionStore = create<CashierSessionState>()(
  persist(
    (set, get) => ({
      session: null,
      setSession: (session) => set({ session }),
      clearSession: () => set({ session: null }),
      hasActiveSession: () => get().session !== null,
    }),
    {
      name: "cashier-session-storage",
      version: 1,
    }
  )
);
