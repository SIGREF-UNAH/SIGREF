import { PageHeaderTabs } from "../../../shared/components";
import EditPractitionerForm from "../components/ui/EditPractitionerForm";

export const EditPractitionerPage = () => {
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
        defaultActive="null"
      />

      {/* Edit Employee Form */}
      <EditPractitionerForm />
    </div>
  );
};
