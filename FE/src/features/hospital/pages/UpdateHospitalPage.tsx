import React from "react";
import { HospitalForm } from "../components/HospitalForm";
import { Button, Result, Spin } from "antd";
import useUpdateHospital from "../hooks/useUpdateHospital";

export const UpdateHospitalPage: React.FC = () => {
  const {
    hospital,
    updateMutation,
    isLoading,
    isError,
    navigate,
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
            <Button
              type="primary"
              onClick={() => navigate("/hospital/details")}
            >
              Volver a Detalles
            </Button>
          }
        />
      </div>
    );
  }

  return (
    <div className="primary-card">
      <HospitalForm
        initialValues={hospital}
        onSubmit={handleUpdate}
        onCancel={handleCancel}
        isEdit
        loading={updateMutation.isPending}
      />
    </div>
  );
};
