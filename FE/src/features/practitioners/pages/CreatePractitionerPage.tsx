import { PageHeaderTabs } from "../../../shared/components";
import PractitionerForm from "../components/ui/PractitionerForm";

export const CreatePractitionerPage = () => {
  return (
    <div>
      {/* Header */}
      <PageHeaderTabs
        title="Gestión de Empleados"
        tabs={[
          {
            key: "listar",
            label: "Lista de Empleados",
            path: "/practitioners/list",
          },
          {
            key: "crear",
            label: "Crear Empleado",
            path: "/practitioners/create",
          },
        ]}
        defaultActive="listar"
      />

      {/* Create Practitioner Form */}
      <PractitionerForm />
    </div>
  );
};
