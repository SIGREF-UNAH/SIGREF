import { useAbility } from "../../../config";
import { PageHeaderTabs } from "../../../shared/components";
import PractitionerForm from "../components/PractitionerForm";

export const CreatePractitionerPage = () => {
  const ability = useAbility();

  return (
    <div>
      {/* Header */}
      <PageHeaderTabs
        title="Gestión de Empleados"
        tabs={[
          ...(ability.can("read", "practitioners") ? [{
            key: "listar",
            label: "Lista de Empleados",
            path: "/practitioners/list",
          }] : []),
          ...(ability.can("create", "practitioners") ? [{
            key: "crear",
            label: "Crear Empleado",
            path: "/practitioners/create",
          }] : []),
        ]}
        defaultActive="listar"
      />

      {/* Create Practitioner Form */}
      <PractitionerForm />
    </div>
  );
};
