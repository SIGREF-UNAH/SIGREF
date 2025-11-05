import OrganizationForm from "../components/ui/OrganizationsForm";
import { PageHeaderTabs } from "../../../shared/components";

const CreateOrganizationsPage = () => {
  return (
    <div>
      <div className="px-8">
      {/* Header */}
      <PageHeaderTabs
        title="Gestión de Organizaciones"
        tabs={[
          {
            key: "listar",
            label: "Lista de Organizaciones",
            path: "/organizacion/list",
          },
          {
            key: "crear",
            label: "Crear Organización",
            path: "/organizacion/create",
          },
        ]}
        defaultActive="crear"
      />
      </div>
      {/* Main Content */}
      <main>
        <OrganizationForm />
      </main>
    </div>
  );
};

export default CreateOrganizationsPage;
