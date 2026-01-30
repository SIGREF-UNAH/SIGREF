import { Card, Col, Row, Spin, Statistic, Table, Tag, Input, Button, Drawer, Descriptions } from "antd";
import { UserOutlined } from "@ant-design/icons";
import { useMemo, useState } from "react";

// --------------------
// Tipos
// --------------------
type FhirIdentifier = {
  system: string;
  value: string;
};

type User = {
  id: string;
  name: string;
  lastName: string;
  email: string;
  active: boolean;
  practitionerId: string;
  fhir: {
    id: string;
    names: string[];
    identifiers: FhirIdentifier[];
  };
};

// --------------------
// Datos simulados
// --------------------
const MOCK_USERS: User[] = Array.from({ length: 23 }).map((_, i) => ({
  id: `${i + 1}`,
  name: `Usuario${i + 1}`,
  lastName: `Apellido${i + 1}`,
  email: `usuario${i + 1}@correo.com`,
  active: i % 3 !== 0,
  practitionerId: `PRAC-${1000 + i}`,
  fhir: {
    id: `FHIR-${i + 1}`,
    names: [`Usuario${i + 1} Apellido${i + 1}`],
    identifiers: [
      { system: "DNI", value: `0101-199${i}-000${i}` },
      { system: "Internal", value: `INT-${i + 1}` },
    ],
  },
}));

// --------------------
// Página principal
// --------------------
export const UsersListPage = () => {
  const [loading] = useState(false);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);

  const pageSize = 5;

  const filteredUsers = useMemo(() => {
    return MOCK_USERS.filter(
      (u) =>
        u.name.toLowerCase().includes(search.toLowerCase()) ||
        u.email.toLowerCase().includes(search.toLowerCase())
    );
  }, [search]);

  const paginatedUsers = useMemo(() => {
    const start = (page - 1) * pageSize;
    return filteredUsers.slice(start, start + pageSize);
  }, [filteredUsers, page]);

  const hasNext = page * pageSize < filteredUsers.length;
  const hasPrevious = page > 1;

  const totalUsers = MOCK_USERS.length;
  const activeUsers = MOCK_USERS.filter((u) => u.active).length;
  const inactiveUsers = totalUsers - activeUsers;

  const columns = [
    { title: "Nombre", dataIndex: "name", key: "name" },
    { title: "Apellido", dataIndex: "lastName", key: "lastName" },
    { title: "Correo", dataIndex: "email", key: "email" },
    {
      title: "Estado",
      dataIndex: "active",
      key: "active",
      render: (active: boolean) => (
        <Tag color={active ? "green" : "red"}>
          {active ? "✓ Activo" : "✗ Inactivo"}
        </Tag>
      ),
    },
    {
      title: "Acciones",
      key: "actions",
      render: (_: any, record: User) => (
        <Button type="link" onClick={() => setSelectedUser(record)}>
          Ver detalles
        </Button>
      ),
    },
  ];

  if (loading) {
    return (
      <div className="flex justify-center items-center h-screen">
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div>
      {/* Resumen */}
      <Row gutter={16} className="mb-4">
        <Col span={8}>
          <Card bordered={false} className="primary-card">
            <Statistic title="Total Usuarios" value={totalUsers} />
          </Card>
        </Col>
        <Col span={8}>
          <Card bordered={false} className="primary-card">
            <Statistic title="Usuarios Activos" value={activeUsers} valueStyle={{ color: "#52c41a" }} />
          </Card>
        </Col>
        <Col span={8}>
          <Card bordered={false} className="primary-card">
            <Statistic title="Usuarios Inactivos" value={inactiveUsers} valueStyle={{ color: "#faad14" }} />
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
          dataSource={paginatedUsers}
          rowKey="id"
          pagination={false}
          bordered
        />

        {/* Paginación simple */}
        <div className="flex justify-end gap-2 mt-4">
          <Button disabled={!hasPrevious} onClick={() => setPage(page - 1)}>
            Página anterior
          </Button>
          <Button disabled={!hasNext} onClick={() => setPage(page + 1)}>
            Página siguiente
          </Button>
        </div>
      </Card>

      {/* Detalle usuario */}
      <Drawer
        title="Detalle del Usuario"
        open={!!selectedUser}
        onClose={() => setSelectedUser(null)}
        width={520}
      >
        {selectedUser && (
          <Descriptions column={1} bordered>
            <Descriptions.Item label="Nombre">
              {selectedUser.name} {selectedUser.lastName}
            </Descriptions.Item>
            <Descriptions.Item label="Correo">
              {selectedUser.email}
            </Descriptions.Item>
            <Descriptions.Item label="Estado">
              {selectedUser.active ? "Activo" : "Inactivo"}
            </Descriptions.Item>
            <Descriptions.Item label="Practitioner ID">
              {selectedUser.practitionerId}
            </Descriptions.Item>
            <Descriptions.Item label="FHIR ID">
              {selectedUser.fhir.id}
            </Descriptions.Item>
            <Descriptions.Item label="FHIR Names">
              {selectedUser.fhir.names.join(", ")}
            </Descriptions.Item>
            <Descriptions.Item label="FHIR Identifiers">
              {selectedUser.fhir.identifiers.map((id) => (
                <div key={id.value}>
                  <strong>{id.system}:</strong> {id.value}
                </div>
              ))}
            </Descriptions.Item>
          </Descriptions>
        )}
      </Drawer>
    </div>
  );
};
