import { PageHeaderTabs } from "../../../shared/components";
import { useEditPatient } from "../hooks";
import PatientForm from "../components/PatientForm";

export const UpdatePatientPage = () => {
  const { handleFinish, initialValues, isPending } = useEditPatient();

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