import {
  Col,
  Row,
  Spin,
  Statistic,
  Table,
  Tag,
  Input,
  Button,
  Switch,
  Tooltip,
} from "antd";

import { useMessage } from "../../../shared/hooks/useMessage";
import { UserOutlined } from "@ant-design/icons";
import { useState } from "react";
import { PageHeaderTabs } from "../../../shared/components";
import { useAbility } from "../../../config";
import { useQueryClient } from "@tanstack/react-query";
import { usePatchApiUsersIdToggleStatus } from "../../../api/users/users";
import { useGetApiUsersList } from "../../../api/users/users";
import { getGetApiUsersListQueryKey } from "../../../api/users/users";
import UserDetailDrawer from "../components/UserDetailDrawer";

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
        title="Gestión de Usuarios"
        tabs={[
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

      <UserDetailDrawer
        userId={selectedUserId}
        onClose={() => setSelectedUserId(null)}
      />
    </div>
  );
};
