import { useAbility } from "../../../config";
import { PageHeaderTabs } from "../../../shared/components/ui";
import LocationForm from "../components/LocationForm";

export default function EditLocationPage() {
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
          defaultActive="null"
        />

        {/* Formulario de gestión de ubicaciones */}
        <LocationForm mode="edit" />
      </main>
    </div>
  );
}
