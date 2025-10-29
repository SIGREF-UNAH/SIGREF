import { PageHeaderTabs } from "../../../shared/components";
import EditFormPatient from "../components/ui/EditFormPatients";

export const UpdatePatient = () => {
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
        defaultActive="null"
      />

      {/* Formulario */}
      <div className="p-6 border-2 bg-card border-gray-300 shadow-md rounded-lg">
        <EditFormPatient />
      </div>
    </div>
  );
};
