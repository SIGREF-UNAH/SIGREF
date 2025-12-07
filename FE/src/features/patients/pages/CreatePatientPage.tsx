import { useAbility } from "../../../config";
import { PageHeaderTabs } from "../../../shared/components";
import PatientForm from "../components/PatientForm";
import useCreatePatientForm from "../hooks/useCreatePatient";

export const CreatePatientPage = () => {
  const { handleSubmit, isSubmitting, error } = useCreatePatientForm();
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