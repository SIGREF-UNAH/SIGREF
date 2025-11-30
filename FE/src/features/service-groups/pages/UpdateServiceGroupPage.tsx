import { ServiceGroupForm } from "../components/ServiceGroupForm";
import { useUpdateServiceGroup, useServiceGroupForm } from "../hooks";
import { PageHeaderTabs } from "../../../shared/components/ui";
import { Spin } from "antd";

export const UpdateServiceGroupPage = () => {
  const { serviceGroup, isLoading, isPending, handleFinish } = useUpdateServiceGroup();
  const { handleCancel } = useServiceGroupForm();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-100">
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div>
      <PageHeaderTabs
        title="Gestión de Paquetes"
        tabs={[
          {
            key: "listar",
            label: "Lista de Paquetes",
            path: "/service-groups/list",
          },
          {
            key: "crear",
            label: "Crear Paquete",
            path: "/service-groups/create",
          },
        ]}
        defaultActive="listar"
      />

      <div className="p-6 border-2 bg-card border-gray-300 shadow-md rounded-lg">
        <div className="mb-6">
          <h2 className="text-2xl font-bold text-gray-800">Editar Paquete</h2>
        </div>
        <ServiceGroupForm
          initialValues={serviceGroup}
          onFinish={handleFinish}
          onCancel={handleCancel}
          submitButtonText="Actualizar paquete"
          isPending={isPending}
        />
      </div>
    </div>
  );
};
