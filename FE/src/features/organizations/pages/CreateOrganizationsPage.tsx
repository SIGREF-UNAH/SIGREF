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
      <div className="px-8">
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
          defaultActive="crear"
        />
      </div>

      {/* Main Content */}
      <main>
        <OrganizationsForm
          onFinish={handleFinish}
          onCancel={handleCancel}
          isPending={isPending}
          submitButtonText="Crear Organización"
        />
      </main>
    </div>
  );
};

export default CreateOrganizationsPage;
