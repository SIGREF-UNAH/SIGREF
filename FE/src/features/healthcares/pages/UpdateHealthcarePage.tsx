import { HealthcareForm } from "../components";
import { HealthcareHeader } from "../components/ui";
import { useHealthcareForm, useUpdateHealthcare } from "../hooks";
import { FormTitle } from "../components/ui/FormTitle";
import { HealthcareFormSkeleton } from "../components/skeletons";

export const UpdateHealthcarePage = () => {
  const {
    healthcare,
    isPending,
    isLoading: isLoadingHealthcare,
    handleFinish,
  } = useUpdateHealthcare();
  
  const {
    organizations,
    locations,
    isLoading: isLoadingFormData,
    handleCancel,
  } = useHealthcareForm();

  const isLoading = isLoadingHealthcare || isLoadingFormData;

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
            initialValues={healthcare}
            organizations={organizations}
            locations={locations}
            onFinish={handleFinish}
            onCancel={handleCancel}
            submitButtonText="Actualizar servicio"
            isPending={isPending}
          />
        )}
      </div>
    </div>
  );
};
