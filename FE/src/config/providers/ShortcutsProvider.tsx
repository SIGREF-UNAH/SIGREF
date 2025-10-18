import { useShortcuts } from "../../shared/hooks";

export function ShortcutsProvider({ children }: { children: React.ReactNode }) {
  useShortcuts();
  return <>{children}</>;
}
