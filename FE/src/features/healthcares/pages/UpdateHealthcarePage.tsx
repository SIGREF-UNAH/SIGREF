import { HealthcareForm } from "../components";
import { HealthcareHeader } from "../components/ui";
import { useHealthcareForm, useUpdateHealthcare } from "../hooks";
import { FormTitle } from "../components/ui/FormTitle";
import { HealthcareFormSkeleton } from "../components/skeletons";

export const UpdateHealthcarePage = () => {
  const { formik, isPending, isLoading } = useUpdateHealthcare();
  const { organizations, locations, handleCancel } = useHealthcareForm();

  return (
    <div>
      {/* Encabezado */}
      <div className="mb-6">
        <HealthcareHeader />
      </div>

      {/* Contenido Principal */}
      <div className="p-6 border-2 bg-card border-primary shadow-md rounded-lg">
        <FormTitle title="Editar Servicio" icon="edit" />
        {isLoading ? (
          <HealthcareFormSkeleton />
        ) : (
          <HealthcareForm
            formik={formik}
            organizations={organizations as any}
            locations={locations as any}
            onCancel={handleCancel}
            submitButtonText="Actualizar servicio"
            isPending={isPending}
          />
        )}
      </div>
    </div>
  );
};