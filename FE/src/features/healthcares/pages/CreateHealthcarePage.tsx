import { HealthcareForm } from "../components/HealthcareForm";
import { HealthcareHeader } from "../components/ui";
import { FormTitle } from "../components/ui/FormTitle";
import { useCreateHealthcare, useHealthcareForm } from "../hooks";
import { HealthcareFormSkeleton } from "../components/skeletons";

export const CreateHealthcarePage = () => {
  const { 
    isPending, 
    handleFinish 
  } = useCreateHealthcare();
  
  const { 
    organizations, 
    locations, 
    isLoading, 
    handleCancel 
  } = useHealthcareForm();

  return (
    <div>
      {/* Encabezado */}
      <div className="mb-6">
        <HealthcareHeader />
      </div>

      {/* Contenido Principal */}
      <div className="p-6 border-2 bg-card border-primary shadow-md rounded-lg">
        <FormTitle title="Crear Servicio" icon="create" />
        
        {isLoading ? (
          <HealthcareFormSkeleton />
        ) : (
          <HealthcareForm
            organizations={organizations}
            locations={locations}
            onFinish={handleFinish}
            onCancel={handleCancel}
            submitButtonText="Crear servicio"
            isPending={isPending}
          />
        )}
      </div>
    </div>
  );
};