import { useAbility } from "../../../config";
import { PageHeaderTabs } from "../../../shared/components";
import PractitionerForm from "../components/PractitionerForm";

export const CreatePractitionerPage = () => {
  const ability = useAbility();

  return (
    <div>
      {/* Encabezado */}
      <PageHeaderTabs
        title="Gestión de Empleados"
        tabs={[
          ...(ability.can("read", "practitioners") ? [{
            key: "read-practitioners",
            label: "Lista de Empleados",
            path: "/practitioners/list",
          }] : []),
          ...(ability.can("create", "practitioners") ? [{
            key: "create-practitioners",
            label: "Crear Empleado",
            path: "/practitioners/create",
          }] : []),
        ]}
        defaultActive="create-practitioners"
      />

      {/* Formulario */}
      <PractitionerForm />
    </div>
  );
};
