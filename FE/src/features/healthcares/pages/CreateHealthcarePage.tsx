import { HealthcareForm } from "../components/HealthcareForm";
import { FormTitle } from "../components/FormTitle";
import { useCreateHealthcare, useHealthcareForm } from "../hooks";
import { PageHeaderTabs } from "../../../shared/components/ui";
import { Spin } from "antd";
import { useAbility } from "../../../config";

export const CreateHealthcarePage = () => {
  const ability = useAbility();
  const { isPending, handleFinish } = useCreateHealthcare();
  const { organizations, locations, isLoading, handleCancel } = useHealthcareForm();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-100">
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div>
      {/* Encabezado */}
      <PageHeaderTabs
        title="Gestión de Servicios"
        tabs={[
          ...(ability.can("read", "healthcares") ? [{
            key: "listar1",
            label: "Lista de Servicios",
            path: "/healthcares/list",
          }] : []),
          ...(ability.can("create", "healthcares") ? [{
            key: "crear1",
            label: "Crear Servicio",
            path: "/healthcares/create",
          }] : []),
          ...(ability.can("read", "service-groups") ? [{
            key: "listar2",
            label: "Lista de Paquetes",
            path: "/service-groups/list",
          }] : []),
          ...(ability.can("create", "service-groups") ? [{
            key: "crear2",
            label: "Crear Paquete",
            path: "/service-groups/create",
          }] : []),
        ]}
        defaultActive="crear1"
      />

      {/* Contenido Principal */}
      <div className="p-6 border-2 bg-card border-gray-300 shadow-md rounded-lg">
        <FormTitle title="Crear Servicio" icon="create" />
        <HealthcareForm
          organizations={organizations}
          locations={locations}
          onFinish={handleFinish}
          onCancel={handleCancel}
          submitButtonText="Crear servicio"
          isPending={isPending}
        />
      </div>
    </div>
  );
};
