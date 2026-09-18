import { HospitalForm } from "../components/HospitalForm";
import useCreateHospital from "../hooks/useCreateHospital";
import type { CreateHospitalPropertiesDto } from "@models";

export const CreateHospitalPage = () => {
  const { isPending, handleCreate, handleCancel } = useCreateHospital();

  return (
    <div className="primary-card">
      <HospitalForm<CreateHospitalPropertiesDto>
        onSubmit={handleCreate}
        onCancel={handleCancel}
        loading={isPending}
      />
    </div>
  );
};