import {
  Col,
  Row,
  Spin,
  Statistic,
  Table,
  Tag,
  Input,
  Button,
  Drawer,
  Descriptions,
  Switch,
  Tooltip,
  message,
} from "antd";

import { CopyOutlined } from "@ant-design/icons";
import { useMessage } from "../../../shared/hooks/useMessage";
import { UserOutlined } from "@ant-design/icons";
import { useState } from "react";
import { PageHeaderTabs } from "../../../shared/components";
import { useAbility } from "../../../config";
import { useQueryClient } from "@tanstack/react-query";
import {
  useGetApiUsersByIdId,
  usePatchApiUsersIdToggleStatus,
} from "../../../api/users/users";
import { useGetApiUsersList } from "../../../api/users/users";
import { getGetApiUsersListQueryKey } from "../../../api/users/users";

export const UsersListPage = () => {
  const [search, setSearch] = useState("");
  const [pageNumber, setPage] = useState(1);
  const [togglingId, setTogglingId] = useState<string | null>(null);
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);
  const queryClient = useQueryClient();
  const ability = useAbility();

  const pageSize = 5;

  const { data, isLoading } = useGetApiUsersList({
    PageNumber: pageNumber,
    PageSize: pageSize,
  });

  const { mutateAsync: toggleUserStatus } = usePatchApiUsersIdToggleStatus();
  const { success, error } = useMessage();

  const users = data?.data?.items ?? [];
  const pagination = data?.data?.pagination;

  const hasNext = pagination?.hasNext ?? false;
  const hasPrevious = pagination?.hasPrevious ?? false;

  const totalUsers = users.length; // porque totalItems viene null
  const activeUsers = users.filter((u: any) => u.enabled).length;
  const inactiveUsers = totalUsers - activeUsers;

  const { data: userDetail } = useGetApiUsersByIdId(selectedUserId!, {
    query: { enabled: !!selectedUserId },
  });
  const detailedUserData = userDetail?.data;

  const handleToggleUserStatus = async (user: any) => {
    setTogglingId(user.id);
    try {
      await toggleUserStatus({ id: user.id });
      success(
        `Estado de ${user.firstName ?? user.username} actualizado correctamente`,
      );

      queryClient.invalidateQueries({
        queryKey: getGetApiUsersListQueryKey({
          PageNumber: pageNumber,
          PageSize: pageSize,
        }),
      });
    } catch (err) {
      error("No se pudo cambiar el estado");
    } finally {
      setTogglingId(null);
    }
  };

  const columns = [
    {
      title: "Nombre",
      dataIndex: "firstName",
      key: "firstName",
      render: (val: string) => val || "-",
    },
    {
      title: "Apellido",
      dataIndex: "lastName",
      key: "lastName",
      render: (val: string) => val || "-",
    },
    { title: "Correo", dataIndex: "email", key: "email" },
    {
      title: "Estado",
      dataIndex: "enabled",
      key: "enabled",
      align: "center",
      render: (enabled: boolean, record: any) =>
        ability.can("update", "users") ? (
          <Tooltip title={enabled ? "Desactivar usuario" : "Activar usuario"}>
            <Switch
              checked={enabled}
              loading={togglingId === record.id} // Estado de carga individual
              checkedChildren="Activo"
              unCheckedChildren="Inactivo"
              onChange={() => handleToggleUserStatus(record)}
            />
          </Tooltip>
        ) : (
          <Tag color={enabled ? "green" : "red"}>
            {enabled ? "✓ Activo" : "✗ Inactivo"}
          </Tag>
        ),
    },
    {
      title: "Acciones",
      key: "actions",
      render: (_: any, record: any) => (
        <Button type="link" onClick={() => setSelectedUserId(record.id)}>
          {" "}
          Ver detalles
        </Button>
      ),
    },
  ];

  const handleCopyData = () => {
    if (!selectedUserId || !detailedUserData) {
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

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-screen">
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div>
      {/* Header */}
      <PageHeaderTabs
        title="Gestión de Empleados"
        tabs={[
          ...(ability.can("read", "practitioners")
            ? [
                {
                  key: "read-practitioners",
                  label: "Lista de Empleados",
                  path: "/practitioners/list",
                },
              ]
            : []),
          ...(ability.can("create", "practitioners")
            ? [
                {
                  key: "create-practitioners",
                  label: "Crear Empleado",
                  path: "/practitioners/create",
                },
              ]
            : []),
          ...(ability.can("read", "users")
            ? [
                {
                  key: "read-users",
                  label: "Lista de Usuarios",
                  path: "/users/list",
                },
              ]
            : []),
          ...(ability.can("create", "users")
            ? [
                {
                  key: "create-users",
                  label: "Crear Usuario",
                  path: "/users/create",
                },
              ]
            : []),
        ]}
        defaultActive="read-users"
      />

      {/* Resumen */}
      <Row gutter={16} className="mb-4">
        <Col span={8}>
          <div className="primary-card">
            <Statistic title="Total Usuarios" value={totalUsers} />
          </div>
        </Col>
        <Col span={8}>
          <div className="primary-card">
            <Statistic
              title="Usuarios Activos"
              value={activeUsers}
              valueStyle={{ color: "#52c41a" }}
            />
          </div>
        </Col>
        <Col span={8}>
          <div className="primary-card">
            <Statistic
              title="Usuarios Inactivos"
              value={inactiveUsers}
              valueStyle={{ color: "#faad14" }}
            />
          </div>
        </Col>
      </Row>

      {/* Tabla */}
      <div className="primary-card">
        <div className="flex items-center gap-2 mb-3">
          <UserOutlined />
          <span className="text-lg">Lista de Usuarios</span>
        </div>

        {/* Búsqueda */}
        <div className="mb-4">
          <Input
            placeholder="Buscar por nombre"
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
          />
        </div>

        <Table
          columns={columns}
          dataSource={users}
          rowKey="id"
          // pagination={{
          //   current: page,
          //   pageSize: pageSize,
          //   total: totalUsers,
          //   onChange: (newPage) => setPage(newPage),
          // }}
          pagination={false}
          bordered
        />

        <div className="flex justify-end gap-2 mt-4">
          <Button disabled={!hasPrevious} onClick={() => setPage((p) => p - 1)}>
            Página anterior
          </Button>
          <Button disabled={!hasNext} onClick={() => setPage((p) => p + 1)}>
            Página siguiente
          </Button>
        </div>
      </div>

      {/* Drawer */}

      <Drawer
        title="Detalle del Usuario"
        open={!!selectedUserId}
        onClose={() => setSelectedUserId(null)}
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
