import { useAbility } from "../../../config";
import { PageHeaderTabs } from "../../../shared/components/ui";
import OrganizationsList from "../components/OrganizationsList";

export default function OrganizationsListPage() {
  const ability = useAbility();

  return (
    <div>
      <main>
        {/* Navegación */}
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
          defaultActive="list"
        />

        {/* Lista de Organizaciones */}
        <OrganizationsList />
      </main>
    </div>
  );
}