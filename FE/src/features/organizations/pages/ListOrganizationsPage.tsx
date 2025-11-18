import { PageHeaderTabs } from "../../../shared/components/ui";
import OrganizationsList from "../components/ui/OrganizationsList";

export default function OrganizationsListPage() {
  return (
    <div>
      <main>
        {/* Navegación */}
        <PageHeaderTabs
          title="Gestión de Organizaciones"
          tabs={[
            { key: "list", 
              label: "Lista de Organizaciones", 
              path: "/organizations/list" },
            { key: "create", 
              label: "Crear Organización", 
              path: "/organizations/create" },
          ]}
          defaultActive="list"
        />

        {/* Lista de Organizaciones */}
        <OrganizationsList />
      </main>
    </div>
  );
}