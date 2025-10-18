import { useHotkeys } from 'react-hotkeys-hook';
import { useNavigate } from 'react-router-dom';
import { appShortcuts } from '../../config';
import { useKeycloak } from '@react-keycloak/web';
import { getRolesFromToken } from '../../auth';

export function useShortcuts() {
  const navigate = useNavigate();
  const { keycloak } = useKeycloak();

  const roles = getRolesFromToken(keycloak);

  // Registrar todos los shortcuts
  appShortcuts.forEach(shortcut => {
    // Verificar si el usuario tiene permisos para este shortcut
    const hasPermission = roles?.some(role => 
      shortcut.roles.includes(role)
    );

    if (hasPermission) {
      useHotkeys(
        shortcut.keys, 
        (e) => {
          e.preventDefault();
          shortcut.action(navigate);
        },
        { enableOnFormTags: ['INPUT', 'TEXTAREA', 'SELECT'] }
      );
    }
  });
}