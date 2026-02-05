import { ServiceGroupForm } from "../components/ServiceGroupForm";
import { useUpdateServiceGroup, useServiceGroupForm } from "../hooks";
import { PageHeaderTabs } from "../../../shared/components/ui";
import { Spin } from "antd";

export const UpdateServiceGroupPage = () => {
  const {
    serviceGroup,
    isLoading: isLoadingServiceGroup,
    isPending,
    handleFinish,
  } = useUpdateServiceGroup();

  const {
    healthcares,
    locations,
    healthcarePagination,
    locationPagination,
    isLoadingHealthcares,
    isLoadingLocations,
    isFetchingHealthcares,
    isFetchingLocations,
    healthcareScope,
    setHealthcarePageNumber,
    setHealthcarePageSize,
    setHealthcareSearch,
    setHealthcareScope,
    setLocationPageNumber,
    setLocationPageSize,
    setLocationSearch,
    isLoading: isLoadingFormData,
    handleCancel,
  } = useServiceGroupForm();

  const isLoading = isLoadingServiceGroup || isLoadingFormData;

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-screen">
        <Spin size="large" tip="Cargando datos..." />
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

      <div className="p-6 border-2 bg-card border-gray-300 shadow-md rounded-lg">
        <div className="mb-6">
          <h2 className="text-2xl font-bold text-gray-800">Editar Paquete</h2>
        </div>
        <ServiceGroupForm
          initialValues={serviceGroup}
          healthcares={healthcares}
          locations={locations}
          healthcarePagination={healthcarePagination}
          locationPagination={locationPagination}
          isLoadingHealthcares={isLoadingHealthcares}
          isLoadingLocations={isLoadingLocations}
          isFetchingHealthcares={isFetchingHealthcares}
          isFetchingLocations={isFetchingLocations}
          healthcareScope={healthcareScope}
          onHealthcarePageChange={(page, pageSize) => {
            setHealthcarePageNumber(page);
            setHealthcarePageSize(pageSize);
          }}
          onLocationPageChange={(page, pageSize) => {
            setLocationPageNumber(page);
            setLocationPageSize(pageSize);
          }}
          onHealthcareSearch={setHealthcareSearch}
          onLocationSearch={setLocationSearch}
          onHealthcareScopeChange={setHealthcareScope}
          onFinish={handleFinish}
          onCancel={handleCancel}
          submitButtonText="Actualizar paquete"
          isPending={isPending}
        />
      </div>
    </div>
  );
};