import React from 'react';
import { useKeycloak } from '@react-keycloak/web';
import { getRolesFromToken, validRoles } from '../../auth';

// Roles validos (admin, cashier, ti, auditor)
type UserRole = keyof typeof validRoles;

interface ProtectedComponentProps {
  allowedRoles: UserRole[]; // Roles permitidos
  children: React.ReactNode; // Contenido a proteger/mostrar
  fallback?: React.ReactNode; // Componente alternativo a mostrar cuando no tiene permisos (opcional)
  hideIfUnauthorized?: boolean; // true = oculta el componente | false = muestra el fallback
}

// Hook personalizado para obtener los roles del usuario desde Keycloak JWT
const useUserRoles = (): UserRole[] => {
  const { keycloak } = useKeycloak();
  
  if (!keycloak.authenticated) {
    return [];
  }

  // Obtener roles del token
  const allRoles = getRolesFromToken(keycloak);
  
  // Filtrar solo los roles válidos del sistema
  const validRoleKeys = Object.keys(validRoles);
  
  return allRoles.filter((role): role is UserRole => 
    validRoleKeys.includes(role)
  );
};

/**
 * Componente para proteger contenido basado en roles de usuario
 * 
 * @example
 * ```tsx
 * <ProtectedComponent allowedRoles={['admin', 'ti']}>
 *   <AdminPanel />
 * </ProtectedComponent>
 * ```
 */
export const ProtectedComponent: React.FC<ProtectedComponentProps> = ({
  allowedRoles,
  children,
  fallback = null,
  hideIfUnauthorized = false,
}) => {
  const userRoles = useUserRoles();
  
  // Verificar si el usuario tiene al menos uno de los roles permitidos
  const hasPermission = allowedRoles.some(role => userRoles.includes(role));
  
  if (!hasPermission) {
    // Si debe ocultarse completamente, no renderizar nada
    if (hideIfUnauthorized) {
      return null;
    }
    // Si no, mostrar el componente fallback
    return <>{fallback}</>;
  }
  
  return <>{children}</>;
};
