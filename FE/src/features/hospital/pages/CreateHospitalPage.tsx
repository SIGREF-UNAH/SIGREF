import React from "react";
import { HospitalForm } from "../components/HospitalForm";
import useCreateHospital from "../hooks/useCreateHospital";

export const CreateHospitalPage: React.FC = () => {
  const { createMutation, handleCreate, handleCancel } = useCreateHospital();

  return (
    <div className="primary-card">
      <HospitalForm
        onSubmit={handleCreate as any}
        onCancel={handleCancel}
        loading={createMutation.isPending}
      />
    </div>
  );
};
