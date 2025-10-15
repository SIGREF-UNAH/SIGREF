import {
  Card,
  Col,
  Row,
  Spin,
  Statistic,
} from "antd";
import { useState } from "react";
import { Link } from "react-router";
import { EmployeesListForm } from "../components/ui";
import { useGetApiPractitioner } from "../../../api/practitioner/practitioner";

type Practitioner = {
  id: { value: string };
  name?: { given?: { value?: string }[]; family?: { value?: string } }[];
  gender?: { value?: string };
  birthDate?: { value?: string };
  active?: { value?: boolean };
  telecom?: { system?: { value: string }; value?: { value: string } }[];
  identifier?: { value?: { value: string }; type?: { text?: { value: string } } }[];
};


export const ManageEmployeesPage = () => {
  const [activeButton, setActiveButton] = useState<
    "listar" | "crear" | "editar" | null
  >("listar");

  const { data: employees, isLoading } = useGetApiPractitioner<Practitioner[]>();


  const totalEmployees = employees?.length || 0;
  const activeEmployees = employees?.filter(emp => emp.active?.value)?.length || 0;
  const inactiveEmployees = totalEmployees - activeEmployees;

  // Función para manejar el clic en un botón
  const handleButtonClick = (button: "listar" | "crear" | "editar") => {
    setActiveButton(button);
  };

if (isLoading) {
    return (
      <div className="flex justify-center items-center h-screen">
        <Spin size="large" tip="Cargando empleados..." />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-white">
      <main className="p-6">
        {/* Header */}
        <div className="relative mb-6">
          <div className="flex justify-between items-center relative z-10">
            <h1 className="text-3xl font-bold text-[#333333]">
              Gestión de Empleados
            </h1>

            {/* Opciones */}
            <div className="flex items-center gap-3 relative">
              <Link
                to="/employees"
                onClick={() => handleButtonClick("listar")}
                className={`px-4 py-2 transition-colors z-10 cursor-pointer ${
                  activeButton === "listar"
                    ? "border border-gray-300 bg-transparent text-[#7BA2D4] rounded-t-md"
                    : "text-[#163C65]"
                }`}
              >
                Listar Empleados
              </Link>

              <Link
                to="/employees/create"
                onClick={() => handleButtonClick("crear")}
                className={`px-4 py-2 relative z-10 cursor-pointer ${
                  activeButton === "crear"
                    ? "border border-gray-300 bg-transparent text-[#7BA2D4] rounded-t-md"
                    : "text-[#163C65]"
                }`}
              >
                Crear Empleado
              </Link>

              <Link
                to="/employees/edit"
                onClick={() => handleButtonClick("editar")}
                className={`px-4 py-2 transition-colors z-10 cursor-pointer ${
                  activeButton === "editar"
                    ? "border border-gray-300 bg-transparent text-[#7BA2D4] rounded-t-md"
                    : "text-[#163C65]"
                }`}
              >
                Editar Empleado
              </Link>

              <div className="absolute bottom-0 h-[1px] bg-gray-300 z-0 left-0 right-0"></div>
            </div>
          </div>
        </div>

        {/* Tarjetas resumen */}
        <Row gutter={16} className="mb-6">
          <Col span={8}>
            <Card bordered={false}>
              <Statistic
                title="Total Empleados"
                value={totalEmployees}
                valueStyle={{ color: "#1677ff" }}
                suffix="empleados"
              />
            </Card>
          </Col>
          <Col span={8}>
            <Card bordered={false}>
              <Statistic
                title="Empleados Activos"
                value={activeEmployees}
                valueStyle={{ color: "#52c41a" }}
                suffix="activos"
              />
            </Card>
          </Col>
          <Col span={8}>
            <Card bordered={false}>
              <Statistic
                title="Empleados Inactivos"
                value={inactiveEmployees}
                valueStyle={{ color: "#faad14" }}
                suffix="inactivos"
              />
            </Card>
          </Col>
        </Row>

        { /* employee Employee Form */ }
        <EmployeesListForm />

        
      </main>
    </div>
  );
};
