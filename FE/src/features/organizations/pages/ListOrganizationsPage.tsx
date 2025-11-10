import { PageHeaderTabs } from "../../../shared/components/ui";
import OrganizationsList from "../components/ui/OrganizationsList";

export default function OrganizationsListPage() {
  return (
    <div>
      <main>
        {/* Header Tabs */}
        <PageHeaderTabs
          title="SIGREF - Gestión de Organizaciones"
          tabs={[
            { key: "list", label: "Lista de Organizaciones", path: "/organizations/list" },
            { key: "create", label: "Crear Organización", path: "/organizations/create" },
          ]}
          defaultActive="list"
        />
        {/* Se usa el componente OrganizationsList */}
        <OrganizationsList />
      </main>
    </div>
  );
}