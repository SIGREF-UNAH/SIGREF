import { useAbility } from "../../../config";
import { PageHeaderTabs } from "../../../shared/components/ui";
import LocationList from "../components/LocationsList";

export default function LocationListPage() {
  const ability = useAbility();

  return (
    <div>
      <main>
        {/* Header */}
        <PageHeaderTabs
          title="Gestión de Ubicaciones"
          tabs={[
            ...(ability.can("read", "locations") ? [{
              key: "listar", label: "Lista de Ubicaciones", path: "/locations/list",
            }] : []),
            ...(ability.can("create", "locations") ? [{
              key: "crear", label: "Crear Ubicación", path: "/locations/create",
            }] : []),
          ]}
          defaultActive="listar"
        />

        {/* Location Form */}
        <LocationList />
      </main>
    </div>
  );
}
