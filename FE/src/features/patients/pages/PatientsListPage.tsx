import { useAbility } from "../../../config";
import { PageHeaderTabs } from "../../../shared/components";
import PatientsInformation from "../components/PatientsInformation";

export const PatientsListPage = () => {
  const ability = useAbility();

  return (
    <div>
      {/* Header */}
      <PageHeaderTabs
        title="Gestión de Pacientes"
        tabs={[
          ...(ability.can("read", "patients") ? [{
            key: "listar",
            label: "Lista de Pacientes",
            path: "/patients/list",
          }] : []),
          ...(ability.can("create", "patients") ? [{
            key: "crear",
            label: "Crear Paciente",
            path: "/patients/create",
          }] : []),
        ]}
        defaultActive="crear"
      />

      {/* Formulario */}
      <div className="primary-card">
        <PatientsInformation />
      </div>
    </div>
  );
};
