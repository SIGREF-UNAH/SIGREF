import { useNavigate } from "react-router-dom";
import { PageHeaderTabs } from "../../../shared/components";
import OrganizationsForm from "../components/OrganizationsForm";
import { useCreateOrganization } from "../hooks";
import { useAbility } from "../../../config";

const CreateOrganizationsPage = () => {
  const navigate = useNavigate();
  const ability = useAbility();
  const { handleFinish, isPending } = useCreateOrganization();

  const handleCancel = () => {
    navigate("/organizations/list");
  };

  return (
    <div>
      {/* Header */}
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
        defaultActive="listar"
      />

      {/* Formulario */}
      <OrganizationsForm
        onFinish={handleFinish as any}
        onCancel={handleCancel}
        isPending={isPending}
        submitButtonText="Crear Organización"
      />
    </div>
  );
};

export default CreateOrganizationsPage;
