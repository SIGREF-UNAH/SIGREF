import type { OrganizationDto } from "../../../api/models";
import { PageHeaderTabs } from "../../../shared/components";
import OrganizationsForm from "../components/OrganizationsForm";
import { useUpdateOrganization } from "../hooks/useUpdateOrganizations";

export const UpdateOrganizationPage = () => {
  const {
    initialValues, // valores iniciales listos para el formulario
    isPending, // estado de la mutación (update)
    isLoading, // estado de la carga inicial (get)
    handleFinish, // función para guardar
  } = useUpdateOrganization();

  return (
    <div>
      {/* Header Tabs */}
      <div className="px-8">
      <PageHeaderTabs
        title="Gestión de Organizaciones"
        tabs={[
          { key: "listar", 
            label: "Lista de Organizaciones", 
            path: "/organizations/list" },
          { key: "crear", 
            label: "Crear Organización", 
            path: "/organizations/create" },
        ]}
        defaultActive="undefined"
      />
      </div>

      {/* Contenido principal */}
      <div className=" ">
        {isLoading ? (
          <div className="flex justify-center py-10">
            <p>Cargando organización...</p>
          </div>
        ) : (
          <OrganizationsForm
            initialValues={initialValues as Partial<OrganizationDto>} // se define asi para quitar error de tipado
            onFinish={handleFinish}
            onCancel={() => window.history.back()}
            submitButtonText="Actualizar Organización"
            isPending={isPending}
          />
        )}
      </div>
    </div>
  );
};
