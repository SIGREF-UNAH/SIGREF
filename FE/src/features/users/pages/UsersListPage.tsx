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
import { usePatchApiUsersIdToggleStatus } from "../../../api/users/users";
import { useGetApiUsersList } from "../../../api/users/users";
import { getGetApiUsersListQueryKey } from "../../../api/users/users";

export const UsersListPage = () => {
  const [search, setSearch] = useState("");
  const [pageNumber, setPage] = useState(1);
  const [selectedUser, setSelectedUser] = useState<any | null>(null);
  const [togglingId, setTogglingId] = useState<string | null>(null);
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

  const handleToggleUserStatus = async (user: any) => {
    setTogglingId(user.id);
    try {
      await toggleUserStatus({ id: user.id });
      success(`Estado de ${user.firstName ?? user.username} actualizado correctamente`);

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
        <Button type="link" onClick={() => setSelectedUser(record)}>
          Ver detalles
        </Button>
      ),
    },
  ];

  const handleCopyData = () => {
    if (!selectedUser)
      {
      message.error("No hay usuario seleccionado para copiar");
      return;
    };
    
    const userData = `ID: ${selectedUser.id}
    Username: ${selectedUser.username}
    Nombre: ${selectedUser.firstName} ${selectedUser.lastName}  
    Correo: ${selectedUser.email}
    Estado: ${selectedUser.enabled ? "Activo" : "Inactivo"}`;
  
    // 3. Intento de copia
  if (navigator.clipboard) {
    navigator.clipboard.writeText(userData)
      .then(() => {
        message.success(`¡Datos de ${selectedUser.firstName} copiados!`);
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
        open={!!selectedUser}
        onClose={() => setSelectedUser(null)}
        width={520}
      >
        {selectedUser && (
          <Descriptions column={1} bordered>
            <Descriptions.Item label="ID">{selectedUser.id}</Descriptions.Item>
            <Descriptions.Item label="Username">
              {selectedUser.username}
            </Descriptions.Item>
            <Descriptions.Item label="Nombre">
              {selectedUser.firstName || "-"}
            </Descriptions.Item>
            <Descriptions.Item label="Apellido">
              {selectedUser.lastName || "-"}
            </Descriptions.Item>
            <Descriptions.Item label="Correo">
              {selectedUser.email}
            </Descriptions.Item>
            <Descriptions.Item label="Estado">
              {selectedUser.enabled ? (
                <Tag color="green">Activo</Tag>
              ) : (
                <Tag color="red">Inactivo</Tag>
              )}
            </Descriptions.Item>
            <Descriptions.Item label="Acción">
            <Button
              type="dashed"
              icon={<CopyOutlined />}
              className="bg-gray-200 hover:bg-gray-300"
              onClick={handleCopyData}
            >
              Copiar Datos
            </Button>
            </Descriptions.Item>
          </Descriptions>
        )}
      </Drawer>
    </div>
  );
};
