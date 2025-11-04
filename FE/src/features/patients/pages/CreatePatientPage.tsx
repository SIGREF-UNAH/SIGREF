import { PageHeaderTabs } from "../../../shared/components";
import PatientForm from "../components/PatientForm";
import useCreatePatientForm from "../hooks/useCreatePatient";

export const CreatePatientPage = () => {
  const { handleSubmit, isSubmitting, error } = useCreatePatientForm();

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
        <PatientForm
          mode="create"
          onSubmit={handleSubmit}
          isSubmitting={isSubmitting}
          error={error}
        />
      </div>
    </div>
  );
};