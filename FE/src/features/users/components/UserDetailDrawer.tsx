import { CopyOutlined } from "@ant-design/icons";
import { Button, Descriptions, Drawer, Tag } from "antd";
import { useUserDetail } from "../hooks/useUserDetail";

interface Props {
  userId: string | null;
  onClose: () => void;
}

const UserDetailDrawer = ({ userId, onClose }: Props) => {
const { detailedUserData, handleCopyData } = useUserDetail(userId);

  return (
    <div>
      {/* Drawer */}

      <Drawer
        title="Detalle del Usuario"
        open={!!userId}
        onClose={onClose}
        width={520}
      >
        {detailedUserData && (
          <div className="flex flex-col gap-6">
            <Descriptions column={1} bordered>
              <Descriptions.Item label="ID">
                {detailedUserData.id}
              </Descriptions.Item>
              <Descriptions.Item label="Username">
                {detailedUserData.username}
              </Descriptions.Item>
              <Descriptions.Item label="Nombre">
                {detailedUserData.firstName || "-"}
              </Descriptions.Item>
              <Descriptions.Item label="Apellido">
                {detailedUserData.lastName || "-"}
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
                  ? new Date(detailedUserData.createdAt).toLocaleString(
                      "es-HN",
                      {
                        day: "2-digit",
                        month: "2-digit",
                        year: "numeric",
                        hour: "2-digit",
                        minute: "2-digit",
                        hour12: true,
                      },
                    )
                  : "No disponible"}
              </Descriptions.Item>

              <Descriptions.Item label="Fecha de Actualización">
                {detailedUserData.lastModifiedAt ? (
                  new Date(detailedUserData.lastModifiedAt).toLocaleString(
                    "es-HN",
                    {
                      day: "2-digit",
                      month: "2-digit",
                      year: "numeric",
                      hour: "2-digit",
                      minute: "2-digit",
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
                  <span className="text-gray-400">
                    El usuario no tiene roles
                  </span>
                )}
              </Descriptions.Item>
            </Descriptions>

            <div className="mt-4 pt-4 border-t flex justify-center">
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
    </div>
  );
};

export default UserDetailDrawer;
