import { ServiceGroupForm } from "../components/ServiceGroupForm";
import { useCreateServiceGroup, useServiceGroupForm } from "../hooks";
import { PageHeaderTabs } from "../../../shared/components/ui";

export const CreateServiceGroupPage = () => {
  const { isPending, handleFinish } = useCreateServiceGroup();
  const { handleCancel } = useServiceGroupForm();

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
          <h2 className="text-2xl font-bold text-gray-800">Crear Paquete</h2>
        </div>
        <ServiceGroupForm
          onFinish={handleFinish}
          onCancel={handleCancel}
          submitButtonText="Crear paquete"
          isPending={isPending}
        />
      </div>
    </div>
  );
};
