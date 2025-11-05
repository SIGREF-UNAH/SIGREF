import { PageHeaderTabs } from "../../../shared/components";
import PatientsInformation from "../components/PatientsInformation";

export const PatientsListPage = () => {
  return (
    <div>
      {/* Header */}
      <PageHeaderTabs
        title="Gestión de Pacientes"
        tabs={[
          {
            key: "listar",
            label: "Lista de Pacientes",
            path: "/patients/list",
          },
          {
            key: "crear",
            label: "Crear Paciente",
            path: "/patients/create",
          },
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
