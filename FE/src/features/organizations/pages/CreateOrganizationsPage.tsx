import { useNavigate } from "react-router-dom";
import { PageHeaderTabs } from "../../../shared/components";
import OrganizationsForm from "../components/OrganizationsForm";
import { useCreateOrganization } from "../hooks";

const CreateOrganizationsPage = () => {
  const navigate = useNavigate();
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
          {
            key: "listar",
            label: "Lista de Organizaciones",
            path: "/organizations/list",
          },
          {
            key: "crear",
            label: "Crear Organización",
            path: "/organizations/create",
          },
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
