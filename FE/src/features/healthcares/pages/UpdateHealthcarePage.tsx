import { HealthcareForm } from "../components";
import { useHealthcareForm, useUpdateHealthcare } from "../hooks";
import { FormTitle } from "../components/ui/FormTitle";
import { PageHeaderTabs } from "../../../shared/components/ui";
import { Spin } from "antd";

export const UpdateHealthcarePage = () => {
  const {
    healthcare,
    isPending,
    isLoading: isLoadingHealthcare,
    handleFinish,
  } = useUpdateHealthcare();

  const {
    organizations,
    locations,
    isLoading: isLoadingFormData,
    handleCancel,
  } = useHealthcareForm();

  const isLoading = isLoadingHealthcare || isLoadingFormData;

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
        defaultActive="null"
      />

      {/* Contenido Principal */}
      <div className="primary-card">
        <FormTitle title="Editar Servicio" icon="edit" />
        <HealthcareForm
          initialValues={healthcare}
          organizations={organizations}
          locations={locations}
          onFinish={handleFinish}
          onCancel={handleCancel}
          submitButtonText="Actualizar servicio"
          isPending={isPending}
        />
      </div>
    </div>
  );
};
