import CreatePractitionerForm from "../components/ui/CreatePractitionerForm";
import { PageHeaderTabs } from "../../../shared/components";

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

      {/* Form */}
      <CreatePractitionerForm />
    </div>
  );
};
