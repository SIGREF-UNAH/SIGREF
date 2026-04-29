import { message } from 'antd';
import { useGetApiUsersByIdId } from '../../../api/users/users';
import { useGetApiPractitionerId } from '../../../api/practitioner/practitioner';

export const useUserDetail = (userId: string | null) => {
  // Fetch de datos del usuario
  const { data: userDetail } = useGetApiUsersByIdId(userId!, {
    query: { enabled: !!userId },
  });

  const detailedUserData = userDetail?.data;

  // Fetch de datos de empleado (si existe practitionerId en el usuario)
  const practitionerId = detailedUserData?.practitionerId ?? null;

  const { data: practitionerData, isLoading: practitionerLoading } =
    useGetApiPractitionerId(practitionerId!, {
      query: { enabled: !!practitionerId },
    });

  // Helpers
  const getGenderLabel = (gender?: string | null): string => {
    const map: Record<string, string> = {
      male: 'Masculino',
      female: 'Femenino',
      other: 'Otro',
      unknown: 'Desconocido',
    };
    return gender ? (map[gender.toLowerCase()] ?? gender) : '-';
  };

  const formatDate = (date?: string | null): string => {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('es-HN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  };

  const practitionerName = (() => {
    const name = practitionerData?.name?.[0];
    if (!name) return '-';
    if (name.text) return name.text;
    const parts = [...(name.given ?? []), name.family].filter(Boolean);
    return parts.length > 0 ? parts.join(' ') : '-';
  })();

  // Lógica de copiado de datos al portapapeles
  const handleCopyData = () => {
    if (!userId || !detailedUserData) {
      message.error('No hay usuario seleccionado para copiar');
      return;
    }

    const identifier = practitionerData?.identifier?.[0];
    const phone =
      practitionerData?.telecom?.find(
        (t) => String(t.system)?.toLowerCase() === 'phone',
      )?.value ?? '-';
    const activeRole = practitionerData?.roles?.[0];

    const userData = `=== Datos de Usuario ===
ID: ${detailedUserData.id}
Usuario: ${detailedUserData.username}
Nombre: ${detailedUserData.firstName} ${detailedUserData.lastName}
Correo: ${detailedUserData.email}
Estado: ${detailedUserData.enabled ? 'Activo' : 'Inactivo'}
Role(s): ${detailedUserData.roles?.length ? detailedUserData.roles.join(', ') : 'N/A'}
Fecha de Creación: ${detailedUserData.createdAt ? new Date(detailedUserData.createdAt).toLocaleString() : 'N/A'}
Fecha de Actualización: ${detailedUserData.lastModifiedAt ? new Date(detailedUserData.lastModifiedAt).toLocaleString() : 'N/A'}`;

    const employeeData = practitionerData
      ? `

=== Datos de Empleado ===
Nombre Completo: ${practitionerName}
Identificación: ${identifier?.value ?? '-'}
Tipo de Identificación: ${identifier?.type?.text ?? '-'}
Fecha de Nacimiento: ${formatDate(practitionerData.birthDate)}
Teléfono: ${phone}
Género: ${getGenderLabel(practitionerData.gender)}`
      : '';

    const roleData = activeRole
      ? `

=== Cargo Desempeñado ===
Título: ${activeRole.code?.[0]?.text ?? '-'}
Tipo de Cargo: ${activeRole.code?.[0]?.coding?.[0]?.display ?? '-'}
Organización: ${activeRole.organization?.display ?? '-'}
Ubicación: ${activeRole.location?.[0]?.display ?? '-'}
Fecha de Inicio: ${formatDate(activeRole.period?.start)}
Fecha de Fin: ${formatDate(activeRole.period?.end)}
Estado: ${activeRole.active ? 'Activo' : 'Inactivo'}`
      : '';

    const fullData = `${userData}${employeeData}${roleData}`;

    if (navigator.clipboard) {
      navigator.clipboard
        .writeText(fullData)
        .then(() => {
          message.success(`¡Datos de ${detailedUserData.firstName} copiados!`);
        })
        .catch((err) => {
          console.error('Error al copiar:', err);
          message.error('No se pudo copiar al portapapeles');
        });
    }
  };

  return {
    detailedUserData,
    practitionerData,
    practitionerName,
    practitionerLoading,
    getGenderLabel,
    formatDate,
    handleCopyData,
  };
};