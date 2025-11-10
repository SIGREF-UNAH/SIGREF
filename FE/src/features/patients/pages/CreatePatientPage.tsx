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
      <div className="primary-card">
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