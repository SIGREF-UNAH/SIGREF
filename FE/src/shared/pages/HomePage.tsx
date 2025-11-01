<<<<<<< HEAD
import React from "react";
import { Card, Button, Tooltip } from "antd";
import { useNavigate } from "react-router-dom";
import { useKeycloak } from "@react-keycloak/web";
import { ProtectedComponent } from "../components";
import { validRoles } from "../../auth";
=======
import React, { useState } from "react";
import { Card, Button, Tooltip } from "antd";
import { useNavigate } from "react-router-dom";
import { useKeycloak } from "@react-keycloak/web";
import { validRoles } from "../../auth";
import { ShortcutsGuideModal } from "../components/modals";
import { useAbility } from "../../config";
import { Can } from "@casl/react"
>>>>>>> origin/main
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

<<<<<<< HEAD
// TODO: Mejorar el diseño y dar funcionalidad a los botones
// TODO: Investigar si pueden funcionar los shortcuts en Web
// TODO: Agregar menu de comandos
// TODO: Coincidir diseño de los demas modulos

=======
>>>>>>> origin/main
interface ModuleCardProps {
  title: string;
  description: string;
  icon: React.ReactNode;
  shortcut: string;
  path: string;
}

<<<<<<< HEAD
=======
// Card de módulo
>>>>>>> origin/main
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
<<<<<<< HEAD
  const { keycloak } = useKeycloak();
=======
  const [isModalOpen, setIsModalOpen] = useState(false);
  const { keycloak } = useKeycloak();
  const ability = useAbility();
>>>>>>> origin/main

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
<<<<<<< HEAD
          <Button icon={<QuestionCircleOutlined />} type="default">
=======
          <Button 
            icon={<QuestionCircleOutlined />} 
            type="default" 
            onClick={() => setIsModalOpen(true)}
          >
>>>>>>> origin/main
            Atajos
          </Button>
        </div>
      </div>

      {/* Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {/* Gestión de Fondos */}
<<<<<<< HEAD
        <ProtectedComponent allowedRoles={["admin", "cashier", "auditor"]}>
=======
        <Can I="read" a="incomes" ability={ability}>
>>>>>>> origin/main
          <ModuleCard
            title="Gestión de Fondos"
            description={"Recepción y gestión de fondos e ingresos monetarios"}
            icon={<DollarOutlined />}
            shortcut="Ctrl + F"
            path="/incomes/list"
          />
<<<<<<< HEAD
        </ProtectedComponent>

        {/* Gestión de Servicios Médicos */}
        <ProtectedComponent allowedRoles={["admin", "cashier", "auditor"]}>
=======
        </Can>

        {/* Gestión de Servicios Médicos */}
        <Can I="read" a="healthcares" ability={ability}>
>>>>>>> origin/main
          <ModuleCard
            title="Gestión de Servicios"
            description={"Administre los servicios médicos que se ofrecen a los pacientes"}
            icon={<MedicineBoxOutlined />}
            shortcut="Ctrl + S"
            path="/healthcares/list"
          />
<<<<<<< HEAD
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
            shortcut="Ctrl + A"
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
            shortcut="Ctrl + M"
            path="/practitioners/list"
          />
        </ProtectedComponent>
        
        {/* Gestión de Pacientes */}
        <ProtectedComponent allowedRoles={["admin", "cashier"]}>
=======
        </Can>
        
        {/* Gestión de Pacientes */}
        <Can I="read" a="patients" ability={ability}>
>>>>>>> origin/main
          <ModuleCard
            title="Gestión de Pacientes"
            description={"Administre los pacientes que se encuentran en el hospital"}
            icon={<UserOutlined />}
            shortcut="Ctrl + P"
            path="/patients/list"
          />
<<<<<<< HEAD
        </ProtectedComponent>
        
        {/* Gestión de Eventos/Logs */}
        <ProtectedComponent allowedRoles={["admin", "ti", "auditor"]}>
=======
        </Can>

        {/* Gestión de Empleados */}
        <Can I="read" a="practitioners" ability={ability}>
          <ModuleCard
            title="Gestión de Empleados"
            description={"Lleve a cabo las tareas de gestión de los empleados del hospital"}
            icon={<TeamOutlined />}
            shortcut="Ctrl + E"
            path="/practitioners/list"
          />
        </Can>
        
        {/* Gestión de Ubicaciones */}
        <Can I="read" a="locations" ability={ability}>
          <ModuleCard
            title="Gestión de Ubicaciones"
            description={"Administre las áreas donde se ofrecen los servicios médicos"}
            icon={<EnvironmentOutlined />}
            shortcut="Ctrl + U"
            path="/locations/list"
          />
        </Can>
        
        {/* Gestión de Organizaciones */}
        <Can I="read" a="organizations" ability={ability}>
          <ModuleCard
            title="Gestión de Organizaciones"
            description={"Gestione las organizaciones que contribuyen al hospital"}
            icon={<ApartmentOutlined />}
            shortcut="Ctrl + O"
            path="/organizations/list"
          />
        </Can>
        
        {/* Gestión de Reportes */}
        <Can I="read" a="reports" ability={ability}>
          <ModuleCard
            title="Gestión de Reportes"
            description={"Genere informes financieros, estadísticos y análisis comparativos"}
            icon={<BarChartOutlined />}
            shortcut="Ctrl + R"
            path="/reports/list"
          />
        </Can>

        {/* Gestión de Eventos/Logs */}
        <Can I="read" a="events" ability={ability}>
>>>>>>> origin/main
          <ModuleCard
            title="Gestión de Eventos/Logs"
            description={"Visualiza los eventos y los registros del sistema"}
            icon={<FileTextOutlined />}
            shortcut="Ctrl + L"
            path="/events/list"
          />
<<<<<<< HEAD
        </ProtectedComponent>
      </div>
=======
        </Can>
      </div>

      {/* Modal de Atajos */}
      <ShortcutsGuideModal
        open={isModalOpen}
        onClose={() => setIsModalOpen(false)}
      />
>>>>>>> origin/main
    </div>
  );
};
