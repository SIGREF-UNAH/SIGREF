import { useHotkeys } from "react-hotkeys-hook";
import { useNavigate } from "react-router-dom";
import { appShortcuts } from "../../config";

export function useShortcuts() {
  const navigate = useNavigate();

  // Registrar todos los shortcuts
  appShortcuts.forEach((shortcut) => {
    useHotkeys(
      shortcut.keys,
      (e) => {
        e.preventDefault();
        shortcut.action(navigate);
      },
      { enableOnFormTags: ["INPUT", "TEXTAREA", "SELECT"] }
    );
  });
}
