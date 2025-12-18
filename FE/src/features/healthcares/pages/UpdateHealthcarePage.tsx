import { useParams } from "react-router-dom";
import { HealthcareForm } from "../components";
import { useHealthcareForm, useUpdateHealthcare } from "../hooks";
import { FormTitle } from "../components/FormTitle";
import { PageHeaderTabs } from "../../../shared/components/ui";
import { Spin } from "antd";
import { useAbility } from "../../../config";
import type { HealthcareDto } from "../../../api/models";

export const UpdateHealthcarePage = () => {
  const { id } = useParams<{ id: string }>();
  
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

  const ability = useAbility();
  const isLoading = isLoadingHealthcare || isLoadingFormData;

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-100">
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div key={id}> {/* Key para reiniciar el componente cuando cambie el ID */}
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
        defaultActive="null"
      />

      {/* Contenido Principal */}
      <div className="primary-card">
        <FormTitle title="Editar Servicio" icon="edit" />
        <HealthcareForm
          key={healthcare?.id || id} 
          initialValues={healthcare as HealthcareDto}
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