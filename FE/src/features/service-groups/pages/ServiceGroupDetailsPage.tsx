import { useAbility } from "../../../config";
import { PageHeaderTabs } from "../../../shared/components";

export const ServiceGroupDetailsPage = () => {
  const ability = useAbility();

  return (
    <div>
      <PageHeaderTabs
        title="Gestión de Paquetes"
        tabs={[
          ...(ability.can("read", "healthcares")
            ? [
                {
                  key: "listar1",
                  label: "Lista de Servicios",
                  path: "/healthcares/list",
                },
              ]
            : []),
          ...(ability.can("create", "healthcares")
            ? [
                {
                  key: "crear1",
                  label: "Crear Servicio",
                  path: "/healthcares/create",
                },
              ]
            : []),
          ...(ability.can("read", "service-groups")
            ? [
                {
                  key: "listar2",
                  label: "Lista de Paquetes",
                  path: "/service-groups/list",
                },
              ]
            : []),
          ...(ability.can("create", "service-groups")
            ? [
                {
                  key: "crear2",
                  label: "Crear Paquete",
                  path: "/service-groups/create",
                },
              ]
            : []),
        ]}
        defaultActive="null"
      />

      <div className="primary-card">
        
      </div>
    </div>
  );
};
