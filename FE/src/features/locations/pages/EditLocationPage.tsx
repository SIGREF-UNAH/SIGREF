import EditLocation from "../components/ui/EditFormulario";
import { PageHeaderTabs } from "../../../shared/components/ui";

export default function EditLocationPage() {
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
          defaultActive="null"
        />

        {/* Formulario de gestión de ubicaciones */}
        <EditLocation />
      </main>
    </div>
  );
}
