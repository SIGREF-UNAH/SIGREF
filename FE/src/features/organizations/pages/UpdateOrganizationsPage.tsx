import { Spin } from "antd";
import type { OrganizationDto } from "../../../api/models";
import { PageHeaderTabs } from "../../../shared/components";
import OrganizationsForm from "../components/OrganizationsForm";
import { useUpdateOrganization } from "../hooks/useUpdateOrganizations";
import { useAbility } from "../../../config";

export const UpdateOrganizationPage = () => {
  const {
    initialValues, 
    isPending, 
    isLoading, 
    handleFinish, 
  } = useUpdateOrganization();
  const ability = useAbility();

  return (
    <div>
      {/* Header Tabs */}
      <PageHeaderTabs
        title="Gestión de Organizaciones"
        tabs={[
          ...(ability.can("read", "organizations") ? [{
            key: "list", 
            label: "Lista de Organizaciones", 
            path: "/organizations/list",
          }] : []),
          ...(ability.can("create", "organizations") ? [{
            key: "create", 
            label: "Crear Organización", 
            path: "/organizations/create",
          }] : []),
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
