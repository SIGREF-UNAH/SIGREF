import { CopyOutlined } from '@ant-design/icons';
import { Button, Descriptions, Drawer, Tag, Spin, Alert } from 'antd';
import { useUserDetail } from '../hooks/useUserDetail';

interface Props {
  userId: string | null;
  onClose: () => void;
}

const UserDetailDrawer = ({ userId, onClose }: Props) => {
  const {
    detailedUserData,
    practitionerData,
    practitionerName,
    practitionerLoading,
    getGenderLabel,
    formatDate,
    handleCopyData,
  } = useUserDetail(userId);

  return (
    <Drawer
      title="Detalle del Usuario"
      open={!!userId}
      onClose={onClose}
      width={640}
    >
      {detailedUserData && (
        <div className="flex flex-col gap-6">
          {/* Usuario */}
          <Descriptions title="Datos de Usuario" column={1} bordered>
            <Descriptions.Item label="ID">
              {detailedUserData.id}
            </Descriptions.Item>
            <Descriptions.Item label="Username">
              {detailedUserData.username}
            </Descriptions.Item>
            <Descriptions.Item label="Nombre">
              {detailedUserData.firstName || '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Apellido">
              {detailedUserData.lastName || '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Correo">
              {detailedUserData.email}
            </Descriptions.Item>
            <Descriptions.Item label="Estado">
              {detailedUserData.enabled ? (
                <Tag color="green">Activo</Tag>
              ) : (
                <Tag color="red">Inactivo</Tag>
              )}
            </Descriptions.Item>
            <Descriptions.Item label="Fecha de Creación">
              {detailedUserData.createdAt
                ? new Date(detailedUserData.createdAt).toLocaleString('es-HN', {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit',
                    hour12: true,
                  })
                : 'No disponible'}
            </Descriptions.Item>
            <Descriptions.Item label="Fecha de Actualización">
              {detailedUserData.lastModifiedAt ? (
                new Date(detailedUserData.lastModifiedAt).toLocaleString(
                  'es-HN',
                  {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit',
                    hour12: true,
                  },
                )
              ) : (
                <span className="text-gray-400">Sin modificaciones</span>
              )}
            </Descriptions.Item>
            <Descriptions.Item label="Roles">
              {detailedUserData.roles && detailedUserData.roles.length > 0 ? (
                <div className="flex flex-wrap gap-1">
                  {detailedUserData.roles.map((role: string) => (
                    <Tag color="blue" key={role}>
                      {role.toUpperCase()}
                    </Tag>
                  ))}
                </div>
              ) : (
                <span className="text-gray-400">El usuario no tiene roles</span>
              )}
            </Descriptions.Item>
          </Descriptions>

          {/* Empleado */}
          {practitionerLoading ? (
            <div className="flex justify-center py-6">
              <Spin />
            </div>
          ) : !detailedUserData.practitionerId || !practitionerData ? (
            <Alert
              type="warning"
              showIcon
              message="No existe un registro de empleado para este usuario"
            />
          ) : (
            <Descriptions title="Datos de Empleado" column={1} bordered>
              <Descriptions.Item label="Nombre Completo">
                {practitionerName}
              </Descriptions.Item>
              <Descriptions.Item label="Identificación">
                {practitionerData?.identifier?.[0]?.value || '-'}
              </Descriptions.Item>
              <Descriptions.Item label="Tipo de Identificación">
                {practitionerData?.identifier?.[0]?.type?.text || '-'}
              </Descriptions.Item>
              <Descriptions.Item label="Fecha de Nacimiento">
                {formatDate(practitionerData.birthDate)}
              </Descriptions.Item>
              <Descriptions.Item label="Teléfono">
                {practitionerData?.telecom?.find((t) => String(t.system)?.toLowerCase() === 'phone')?.value || '-'}
              </Descriptions.Item>
              <Descriptions.Item label="Género">
                {getGenderLabel(practitionerData.gender)}
              </Descriptions.Item>
            </Descriptions>
          )}

          {/* Cargo */}
          {detailedUserData.practitionerId && practitionerData && (
            !practitionerData?.roles?.[0] ? (
              <Alert
                type="info"
                showIcon
                message="Este empleado no tiene un cargo asignado"
              />
            ) : (
              <Descriptions title="Cargo Desempeñado" column={1} bordered>
                <Descriptions.Item label="Título">
                  {practitionerData?.roles?.[0].code?.[0]?.text || '-'}
                </Descriptions.Item>
                <Descriptions.Item label="Tipo de Cargo">
                  {practitionerData?.roles?.[0].code?.[0]?.coding?.[0]?.display || '-'}
                </Descriptions.Item>
                <Descriptions.Item label="Organización">
                  {practitionerData?.roles?.[0].organization?.display || '-'}
                </Descriptions.Item>
                <Descriptions.Item label="Ubicación">
                  {practitionerData?.roles?.[0].location?.[0]?.display || '-'}
                </Descriptions.Item>
                <Descriptions.Item label="Fecha de Inicio">
                  {formatDate(practitionerData?.roles?.[0].period?.start)}
                </Descriptions.Item>
                <Descriptions.Item label="Fecha de Fin">
                  {formatDate(practitionerData?.roles?.[0].period?.end)}
                </Descriptions.Item>
                <Descriptions.Item label="Estado">
                  {practitionerData?.roles?.[0].active ? (
                    <Tag color="green">Activo</Tag>
                  ) : (
                    <Tag color="red">Inactivo</Tag>
                  )}
                </Descriptions.Item>
              </Descriptions>
            )
          )}

          {/* Botón de Copiar */}
          <div className="pt-4 border-t">
            <Button
              type="dashed"
              icon={<CopyOutlined />}
              onClick={handleCopyData}
              className="w-full h-10 border-blue-400 text-blue-500 hover:bg-blue-50"
            >
              Copiar Datos al Portapapeles
            </Button>
          </div>
        </div>
      )}
    </Drawer>
  );
};

export default UserDetailDrawer;