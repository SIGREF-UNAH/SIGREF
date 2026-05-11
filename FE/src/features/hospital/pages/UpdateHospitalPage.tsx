import { HospitalForm } from "../components/HospitalForm";
import { Button, Result, Spin } from "antd";
import useUpdateHospital from "../hooks/useUpdateHospital";
import type { UpdateHospitalPropertiesDto } from "../../../api/models";

export const UpdateHospitalPage = () => {
  const {
    hospital,
    isPending,
    isLoading,
    isError,
    handleUpdate,
    handleCancel,
  } = useUpdateHospital();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-100">
        <Spin size="large" />
      </div>
    );
  }

  if (isError || !hospital) {
    return (
      <div className="max-w-4xl mx-auto p-6">
        <Result
          status="error"
          title="Error al cargar la información"
          subTitle="No se pudo cargar la información del hospital para editar."
          extra={
            <Button type="primary" onClick={handleCancel}>
              Volver a Detalles
            </Button>
          }
        />
      </div>
    );
  }

  return (
    <div className="primary-card">
      <HospitalForm<UpdateHospitalPropertiesDto>
        initialValues={hospital}
        onSubmit={handleUpdate}
        onCancel={handleCancel}
        isEdit
        loading={isPending}
      />
    </div>
  );
};