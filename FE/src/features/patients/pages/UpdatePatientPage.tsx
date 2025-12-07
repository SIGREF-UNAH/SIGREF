import { PageHeaderTabs } from "../../../shared/components";
import { useEditPatient } from "../hooks";
import PatientForm from "../components/PatientForm";
import { useAbility } from "../../../config";

export const UpdatePatientPage = () => {
  const { handleFinish, initialValues, isPending } = useEditPatient();
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
        defaultActive="null"
      />

      {/* Formulario */}
      <div className="primary-card">
        <PatientForm
          mode="edit"
          onSubmit={handleFinish as any}
          isSubmitting={isPending}
          initialValues={initialValues}
        />
      </div>
    </div>
  );
};