import { HealthcareForm } from "../components/HealthcareForm";
import { HealthcareHeader } from "../components/ui";
import { FormTitle } from "../components/ui/FormTitle";
import { useCreateHealthcare, useHealthcareForm } from "../hooks";

export const CreateHealthcarePage = () => {
  const { formik, isPending } = useCreateHealthcare();
  const { organizations, locations, handleCancel } = useHealthcareForm();

  return (
    <div>
      {/* Encabezado */}
      <div className="mb-6">
        <HealthcareHeader />
      </div>

      {/* Contenido Principal */}
      <div className="p-6 border-2 bg-card border-primary shadow-md rounded-lg">
        <FormTitle title="Crear Servicio" icon="create" />
        <HealthcareForm
          formik={formik}
          organizations={organizations as any}
          locations={locations as any}
          onCancel={handleCancel}
          submitButtonText="Crear servicio"
          isPending={isPending}
        />
      </div>
    </div>
  );
};