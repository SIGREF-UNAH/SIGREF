
import { Card, Col, Row, Spin, Statistic } from "antd";
import { PractitionersListForm } from "../components/ui";
import { useGetApiPractitioner } from "../../../api/practitioner/practitioner";
import { PageHeaderTabs } from "../../../shared/components";

type Practitioner = {
  id: { value: string };
  name?: { given?: { value?: string }[]; family?: { value?: string } }[];
  gender?: { value?: string };
  birthDate?: { value?: string };
  active?: { value?: boolean };
  telecom?: { system?: { value: string }; value?: { value: string } }[];
  identifier?: {
    value?: { value: string };
    type?: { text?: { value: string } };
  }[];
};

export const PractitionersListPage = () => {
  const { data, isLoading } = useGetApiPractitioner<{ items: Practitioner[]; }>();
  const practitioners = data?.items ?? [];
  const totalPractitioners = practitioners.length;
  const activePractitioners = practitioners.filter((emp) => emp.active)?.length || 0;
  const inactivePractitioners = totalPractitioners - activePractitioners;

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-screen">
        <Spin size="large" tip="Cargando empleados..." />
      </div>
    );
  }

  return (
    <div>
      {/* Header */}
      <PageHeaderTabs
        title="Gestión de Empleados"
        tabs={[
          {
            key: "listar",
            label: "Lista de Empleados",
            path: "/practitioners/list",
          },
          {
            key: "crear",
            label: "Crear Empleado",
            path: "/practitioners/create",
          },
        ]}
        defaultActive="crear"
      />

      {/* Tarjetas resumen */}
      <Row gutter={16} className="mb-4">
        <Col span={8}>
          <Card bordered={false} className="primary-card">
            <Statistic
              title="Total Empleados"
              value={totalPractitioners}
              valueStyle={{ color: "#1677ff" }}
              suffix="empleados"
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card bordered={false} className="primary-card">
            <Statistic
              title="Empleados Activos"
              value={activePractitioners}
              valueStyle={{ color: "#52c41a" }}
              suffix="activos"
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card bordered={false} className="primary-card">
            <Statistic
              title="Empleados Inactivos"
              value={inactivePractitioners}
              valueStyle={{ color: "#faad14" }}
              suffix="inactivos"
            />
          </Card>
        </Col>
      </Row>

      {/* employee Employee Form */}
      <PractitionersListForm />
    </div>
  );
};
