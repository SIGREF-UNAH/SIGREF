import { Card, Button, Tooltip } from "antd";
import { useNavigate } from "react-router-dom";
import { useKeycloak } from "@react-keycloak/web";
import { validRoles } from "../../auth";
import { useAbility } from "../../config";
import { Can } from "@casl/react"
import {
  DollarOutlined,
  MedicineBoxOutlined,
  ApartmentOutlined,
  EnvironmentOutlined,
  FileTextOutlined,
  TeamOutlined,
  UserOutlined,
  BarChartOutlined,
  UserSwitchOutlined,
} from "@ant-design/icons";

interface ModuleCardProps {
  title: string;
  description: string;
  icon: React.ReactNode;
  shortcut: string;
  path: string;
}

// Card de módulo
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
  const { keycloak } = useKeycloak();
  const ability = useAbility();

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
      </div>

      {/* Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-3 gap-6">
        {/* Gestión de Fondos */}
        <Can I="read" a="incomes" ability={ability}>
          <ModuleCard
            title="Gestión de Fondos"
            description={"Recepción y gestión de fondos e ingresos monetarios"}
            icon={<DollarOutlined />}
            shortcut="Ctrl + F"
            path="/incomes/list"
          />
        </Can>
        
        {/* Gestión de Turnos */}
        <Can I="read" a="shifts" ability={ability}>
          <ModuleCard
            title="Gestión de Turnos"
            description={"Administre los turnos de trabajo del hospital por su ubicación"}
            icon={<UserSwitchOutlined />}
            shortcut="Ctrl + T"
            path="/shifts/list"
          />
        </Can>

        {/* Gestión de Servicios Médicos */}
        <Can I="read" a="healthcares" ability={ability}>
          <ModuleCard
            title="Gestión de Servicios"
            description={"Administre los servicios médicos que se ofrecen a los pacientes"}
            icon={<MedicineBoxOutlined />}
            shortcut="Ctrl + S"
            path="/healthcares/list"
          />
        </Can>
        
        {/* Gestión de Pacientes */}
        <Can I="read" a="patients" ability={ability}>
          <ModuleCard
            title="Gestión de Pacientes"
            description={"Administre los pacientes que se encuentran en el hospital"}
            icon={<UserOutlined />}
            shortcut="Ctrl + P"
            path="/patients/list"
          />
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
          <ModuleCard
            title="Gestión de Eventos/Logs"
            description={"Visualiza los eventos y los registros del sistema"}
            icon={<FileTextOutlined />}
            shortcut="Ctrl + L"
            path="/events/list"
          />
        </Can>
      </div>
    </div>
  );
};
