import { PageHeaderTabs } from "../../../shared/components/ui";
import LocationList from "../components/ui/LocationsList";

export default function LocationListPage() {
  return (
    <div>
      <main>
        {/* Header */}
        <PageHeaderTabs
          title="Gestión de Ubicaciones"
          tabs={[
            { key: "listar", label: "Lista de Ubicaciones", path: "/locations/list" },
            { key: "crear", label: "Crear Ubicación", path: "/locations/create" },
          ]}
          defaultActive="listar"
        />

        {/* Location Form */}
        <LocationList />
      </main>
    </div>
  );
}
