import { Card, Col, Row, Spin, Statistic, Table, Tag, Input, Button, Drawer, Descriptions } from "antd";
import { UserOutlined } from "@ant-design/icons";
import { useState } from "react";
import { useGetApiKeycloakSeederList } from "../../../api/keycloak-seeder/keycloak-seeder";
import { PageHeaderTabs } from "../../../shared/components";
import { useAbility } from "../../../config";


// --------------------
// Página principal
// --------------------
export const UsersListPage = () => {
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [selectedUser, setSelectedUser] = useState<any | null>(null);

  const ability = useAbility();

  const pageSize = 5;

  const { data, isLoading } = useGetApiKeycloakSeederList({
    pageNumber: page,
    pageSize,
    search: search || undefined,
  });

  console.log(data);
  
  const users = data?.data?.items ?? [];
  const pagination = data?.data?.pagination;

  const hasNext = pagination?.hasNext ?? false;
  const hasPrevious = pagination?.hasPrevious ?? false;

  const totalUsers = users.length; // porque totalItems viene null
  const activeUsers = users.filter((u: any) => u.enabled).length;
  const inactiveUsers = totalUsers - activeUsers;

  const columns = [
    { title: "Nombre", dataIndex: "firstName", key: "firstName" },
    { title: "Apellido", dataIndex: "lastName", key: "lastName" },
    { title: "Correo", dataIndex: "email", key: "email" },
    {
      title: "Estado",
      dataIndex: "enabled",
      key: "enabled",
      render: (enabled: boolean) => (
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
          ...(ability.can("read", "practitioners") ? [{
            key: "read-practitioners",
            label: "Lista de Empleados",
            path: "/practitioners/list",
          }] : []),
          ...(ability.can("create", "practitioners") ? [{
            key: "create-practitioners",
            label: "Crear Empleado",
            path: "/practitioners/create",
          }] : []),
          ...(ability.can("read", "users") ? [{
            key: "read-users",
            label: "Lista de Usuarios",
            path: "/users/list",
          }] : []),
            ...(ability.can("create", "users") ? [{
              key: "create-users",
              label: "Crear Usuario",
              path: "/users/create",
          }] : []),
        ]}
        defaultActive="read-users"
      />

      {/* Resumen */}
      <Row gutter={16} className="mb-4">
        <Col span={8}>
          <Card bordered={false} className="primary-card">
            <Statistic title="Total Usuarios" value={totalUsers} />
          </Card>
        </Col>
        <Col span={8}>
          <Card bordered={false} className="primary-card">
            <Statistic
              title="Usuarios Activos"
              value={activeUsers}
              valueStyle={{ color: "#52c41a" }}
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card bordered={false} className="primary-card">
            <Statistic
              title="Usuarios Inactivos"
              value={inactiveUsers}
              valueStyle={{ color: "#faad14" }}
            />
          </Card>
        </Col>
      </Row>

      {/* Búsqueda */}
      <Card className="primary-card mb-4">
        <Input
          placeholder="Buscar por nombre o email"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(1);
          }}
        />
      </Card>

      {/* Tabla */}
      <Card className="primary-card">
        <div className="flex items-center gap-2 mb-3">
          <UserOutlined />
          <span className="text-lg">Lista de Usuarios</span>
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
          <Button
            disabled={!hasPrevious}
            onClick={() => setPage((p) => p - 1)}
          >
            Página anterior
          </Button>
          <Button
            disabled={!hasNext}
            onClick={() => setPage((p) => p + 1)}
          >
            Página siguiente
          </Button>
        </div>
      </Card>

      {/* Drawer */}
      <Drawer
        title="Detalle del Usuario"
        open={!!selectedUser}
        onClose={() => setSelectedUser(null)}
        width={520}
      >
        {selectedUser && (
          <Descriptions column={1} bordered>
            <Descriptions.Item label="ID">
              {selectedUser.id}
            </Descriptions.Item>
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
              {selectedUser.active ? "Activo" : "Inactivo"}
            </Descriptions.Item>
          </Descriptions>
        )}
      </Drawer>
    </div>
  );
};

