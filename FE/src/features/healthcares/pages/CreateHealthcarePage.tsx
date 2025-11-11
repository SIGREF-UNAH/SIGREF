import { HealthcareForm } from "../components/HealthcareForm";
import { FormTitle } from "../components/ui/FormTitle";
import { useCreateHealthcare, useHealthcareForm } from "../hooks";
import { PageHeaderTabs } from "../../../shared/components/ui";
import { Spin } from "antd";

export const CreateHealthcarePage = () => {
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
          {
            key: "listar",
            label: "Lista de Servicios",
            path: "/healthcares/list",
          },
          {
            key: "crear",
            label: "Crear Servicio",
            path: "/healthcares/create",
          },
        ]}
        defaultActive="listar"
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
