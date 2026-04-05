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
  message,
  Tooltip,
} from "antd";
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

  console.log(data);

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
      message.success(`Estado de ${user.firstName} actualizado correctamente`);

      queryClient.invalidateQueries({
        queryKey: getGetApiUsersListQueryKey({
          PageNumber: pageNumber,
          PageSize: pageSize,
        }),
      });
    } catch (error) {
      message.error("No se pudo cambiar el estado");
    } finally {
      setTogglingId(null);
    }
  };

  const columns = [
    { title: "Nombre", dataIndex: "firstName", key: "firstName" },
    { title: "Apellido", dataIndex: "lastName", key: "lastName" },
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
            placeholder="Buscar por nombre o email"
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
              {selectedUser.firstName} {selectedUser.lastName}
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
          </Descriptions>
        )}
      </Drawer>
    </div>
  );
};
