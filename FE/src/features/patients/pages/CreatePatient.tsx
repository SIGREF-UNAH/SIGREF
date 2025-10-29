import CreateFormPatient from "../components/ui/FormPatients";
import { PageHeaderTabs } from "../../../shared/components";

export const CreatePatient = () => {
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
        defaultActive="listar"
      />

      {/* Formulario */}
      <div className="p-6 border-2 bg-card border-gray-300 shadow-md rounded-lg">
        <CreateFormPatient />
      </div>
    </div>
  );
};
