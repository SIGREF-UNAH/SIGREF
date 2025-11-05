import { HealthcareForm } from "../components/HealthcareForm";
import { FormTitle } from "../components/ui/FormTitle";
import { useCreateHealthcare, useHealthcareForm } from "../hooks";
import { HealthcareFormSkeleton } from "../components/skeletons";
import { PageHeaderTabs } from "../../../shared/components/ui";

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
      <PageHeaderTabs
        title="Gestión de Servicios"
        tabs={[
          { key: "listar", label: "Lista de Servicios", path: "/healthcares/list" },
          { key: "crear", label: "Crear Servicio", path: "/healthcares/create" },
        ]}
        defaultActive="listar"
      />

      {/* Contenido Principal */}
      <div className="primary-card">
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