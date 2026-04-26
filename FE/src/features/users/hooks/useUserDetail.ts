import { message } from 'antd';
import { useGetApiUsersByIdId } from '../../../api/users/users';

export const useUserDetail = (userId: string | null) => {
  // 1. El Fetch de datos
  const { data: userDetail } = useGetApiUsersByIdId(userId!, {
    query: { enabled: !!userId },
  });

  const detailedUserData = userDetail?.data;

  // 2. logica de copiado de datos al portapapeles
 const handleCopyData = () => {
    if (!userId || !detailedUserData) {
      message.error("No hay usuario seleccionado para copiar");
      return;
    }

    const userData = `ID: ${detailedUserData.id}
    Username: ${detailedUserData.username}
    Nombre: ${detailedUserData.firstName} ${detailedUserData.lastName}  
    Correo: ${detailedUserData.email}
    Estado: ${detailedUserData.enabled ? "Activo" : "Inactivo"}
    Role(s): ${detailedUserData.roles ? detailedUserData.roles.join(", ") : "N/A"}
    Fecha de Creación: ${detailedUserData.createdAt ? new Date(detailedUserData.createdAt).toLocaleString() : "N/A"}
    Fecha de Actualización: ${detailedUserData.lastModifiedAt ? new Date(detailedUserData.lastModifiedAt).toLocaleString() : "N/A"}`;

    if (navigator.clipboard) {
      navigator.clipboard
        .writeText(userData)
        .then(() => {
          message.success(`¡Datos de ${detailedUserData.firstName} copiados!`);
        })
        .catch((err) => {
          console.error("Error al copiar:", err);
          message.error("No se pudo copiar al portapapeles");
        });
    }
  };

  return {
    detailedUserData,
    handleCopyData,
  };
};