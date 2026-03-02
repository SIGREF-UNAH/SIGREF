import { useAbility } from "../../../config";
import { PageHeaderTabs } from "../../../shared/components";
import PractitionerForm from "../components/PractitionerForm";

export const UpdatePractitionerPage = () => {
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
          ...(ability.can("read", "users") ? [{
            key: "read-users",
            label: "Lista de Usuarios",
            path: "/users/list",
          }] : []),
            ...(ability.can("create", "users") ? [{
              key: "create-users",
              label: "Crear Usuario",
              path: "/users/create",
          }] : []),
        ]}
        defaultActive="null"
      />

      {/* Formulario */}
      <PractitionerForm />
    </div>
  );
};