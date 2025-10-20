import { HealthcareForm } from "../components";
import { useHealthcareForm, useUpdateHealthcare } from "../hooks";
import { FormTitle } from "../components/ui/FormTitle";
import { HealthcareFormSkeleton } from "../components/skeletons";
import { PageHeaderTabs } from "../../../shared/components/ui";

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
      <PageHeaderTabs
        title="Gestión de Servicios"
        tabs={[
          { key: "listar", label: "Lista de Servicios", path: "/healthcares/list" },
          { key: "crear", label: "Crear Servicio", path: "/healthcares/create" },
        ]}
        defaultActive="null"
      />

      {/* Contenido Principal */}
      <div className="p-6 border-2 bg-card border-gray-300 shadow-md rounded-lg">
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
