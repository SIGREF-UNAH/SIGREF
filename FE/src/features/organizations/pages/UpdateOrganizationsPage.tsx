import { Spin } from "antd";
import type { OrganizationDto } from "../../../api/models";
import { PageHeaderTabs } from "../../../shared/components";
import OrganizationsForm from "../components/OrganizationsForm";
import { useUpdateOrganization } from "../hooks/useUpdateOrganizations";

export const UpdateOrganizationPage = () => {
  const {
    initialValues, 
    isPending, 
    isLoading, 
    handleFinish, 
  } = useUpdateOrganization();

  return (
    <div>
      {/* Header Tabs */}
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
        defaultActive="null"
      />

      {/* Contenido principal */}
      <div>
        {isLoading ? (
          <div className="flex items-center justify-center h-100">
            <Spin size="large" />
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
