import React, { useState } from "react";
import { Card, Button, Tooltip } from "antd";
import { useNavigate } from "react-router-dom";
import { useKeycloak } from "@react-keycloak/web";
import { ProtectedComponent } from "../components";
import { validRoles } from "../../auth";
import { ShortcutsGuideModal } from "../components/modals";
import {
  DollarOutlined,
  MedicineBoxOutlined,
  ApartmentOutlined,
  EnvironmentOutlined,
  FileTextOutlined,
  TeamOutlined,
  UserOutlined,
  BarChartOutlined,
  QuestionCircleOutlined,
} from "@ant-design/icons";

interface ModuleCardProps {
  title: string;
  description: string;
  icon: React.ReactNode;
  shortcut: string;
  path: string;
}

const ModuleCard: React.FC<ModuleCardProps> = ({
  title,
  description,
  icon,
  shortcut,
  path,
}) => {
  const navigate = useNavigate();

  return (
    <Card className="shadow-md rounded-2xl border hover:shadow-lg transition-all duration-300 flex flex-col items-center justify-between">
      <Tooltip title={`Atajo: ${shortcut}`}>
        <div className="absolute top-2 right-3 text-xs text-general-secondary select-none">
          {shortcut}
        </div>
      </Tooltip>
      <div className="flex h-35 flex-col items-center justify-center">
        <div className="text-primary text-5xl mb-3">{icon}</div>
        <h3 className="text-lg font-semibold mb-2 text-center">{title}</h3>
        <p className="text-general-secondary text-center text-sm mb-4 px-2">
          {description}
        </p>
      </div>
      <div className="flex h-16 items-center justify-center">
        <Button
          type="primary"
          className="rounded-lg px-6"
          onClick={() => navigate(path)}
          style={{ backgroundColor: "var(--color-primary)" }}
        >
          Acceder
        </Button>
      </div>
    </Card>
  );
};

export const HomePage: React.FC = () => {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const { keycloak } = useKeycloak();

  // Obtener el nombre del usuario
  const userName = 
    keycloak?.tokenParsed?.given_name + " " + 
    keycloak?.tokenParsed?.family_name || "Usuario";

  // Obtener el rol del usuario
  const userRole =
    keycloak?.realmAccess?.roles?.find((r) =>
    validRoles[r as keyof typeof validRoles]) || "SIN ROL";

  return (
    <div>
      {/* Header */}
      <div className="flex justify-between items-start mb-6">
        {/* Mensaje de bienvenida */}
        <div>
          <h1 className="text-2xl font-bold">
            Bienvenido,{" "}
            <span className="text-primary capitalize">{userName}</span>
          </h1>
          <p className="text-general-secondary text-sm mt-1">
            Rol: <span className="uppercase font-semibold">{userRole}</span>
          </p>
        </div>

        {/* Botones */}
        <div className="flex gap-2">
          <Button 
            icon={<QuestionCircleOutlined />} 
            type="default" 
            onClick={() => setIsModalOpen(true)}
          >
            Atajos
          </Button>
        </div>
      </div>

      {/* Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {/* Gestión de Fondos */}
        <ProtectedComponent allowedRoles={["admin", "cashier", "auditor"]}>
          <ModuleCard
            title="Gestión de Fondos"
            description={"Recepción y gestión de fondos e ingresos monetarios"}
            icon={<DollarOutlined />}
            shortcut="Ctrl + F"
            path="/incomes/list"
          />
        </ProtectedComponent>

        {/* Gestión de Servicios Médicos */}
        <ProtectedComponent allowedRoles={["admin", "cashier", "auditor"]}>
          <ModuleCard
            title="Gestión de Servicios"
            description={"Administre los servicios médicos que se ofrecen a los pacientes"}
            icon={<MedicineBoxOutlined />}
            shortcut="Ctrl + S"
            path="/healthcares/list"
          />
        </ProtectedComponent>

        {/* Gestión de Organizaciones */}
        <ProtectedComponent allowedRoles={["admin", "ti"]}>
          <ModuleCard
            title="Gestión de Organizaciones"
            description={"Gestione las organizaciones que contribuyen al hospital"}
            icon={<ApartmentOutlined />}
            shortcut="Ctrl + O"
            path="/organizations/list"
          />
        </ProtectedComponent>

        {/* Gestión de Ubicaciones */}
        <ProtectedComponent allowedRoles={["admin", "ti"]}>
          <ModuleCard
            title="Gestión de Ubicaciones"
            description={"Administre las áreas donde se ofrecen los servicios médicos"}
            icon={<EnvironmentOutlined />}
            shortcut="Ctrl + U"
            path="/locations/list"
          />
        </ProtectedComponent>

        {/* Gestión de Reportes */}
        <ProtectedComponent allowedRoles={["admin"]}>
          <ModuleCard
            title="Gestión de Reportes"
            description={"Genere informes financieros, estadísticos y análisis comparativos"}
            icon={<BarChartOutlined />}
            shortcut="Ctrl + R"
            path="/reports/list"
          />
        </ProtectedComponent>

        {/* Gestión de Empleados */}
        <ProtectedComponent allowedRoles={["admin", "ti", "auditor"]}>
          <ModuleCard
            title="Gestión de Empleados"
            description={"Lleve a cabo las tareas de gestión de los empleados del hospital"}
            icon={<TeamOutlined />}
            shortcut="Ctrl + E"
            path="/practitioners/list"
          />
        </ProtectedComponent>
        
        {/* Gestión de Pacientes */}
        <ProtectedComponent allowedRoles={["admin", "cashier"]}>
          <ModuleCard
            title="Gestión de Pacientes"
            description={"Administre los pacientes que se encuentran en el hospital"}
            icon={<UserOutlined />}
            shortcut="Ctrl + P"
            path="/patients/list"
          />
        </ProtectedComponent>
        
        {/* Gestión de Eventos/Logs */}
        <ProtectedComponent allowedRoles={["admin", "ti", "auditor"]}>
          <ModuleCard
            title="Gestión de Eventos/Logs"
            description={"Visualiza los eventos y los registros del sistema"}
            icon={<FileTextOutlined />}
            shortcut="Ctrl + L"
            path="/events/list"
          />
        </ProtectedComponent>
      </div>

      {/* Modal de Atajos */}
      <ShortcutsGuideModal
        open={isModalOpen}
        onClose={() => setIsModalOpen(false)}
      />
    </div>
  );
};
