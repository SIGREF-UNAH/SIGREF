import { useAbility } from "../../../config";
import { PageHeaderTabs } from "../../../shared/components/ui";
import LocationForm from "../components/LocationForm";

export default function CreateLocationPage() {
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
          defaultActive="crear"
        />

        {/* Location Form */}
        <LocationForm mode="create"/>
      </main>
    </div>
  );
}
