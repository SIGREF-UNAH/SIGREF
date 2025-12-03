import { useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router";
import { useMessage } from "../../../shared/hooks";
import type { CreateHospitalPropertiesDto } from "../../../api/models";
import {
  getGetApiHospitalPropertiesDetailsQueryKey,
  usePostApiHospitalProperties,
} from "../../../api/hospital-properties/hospital-properties";

export default function useCreateHospital() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  const createMutation = usePostApiHospitalProperties({
    mutation: {
      onSuccess: () => {
        msg.success("Información del hospital registrada exitosamente");
        // Invalidar la query de detalles para que se recargue
        queryClient.invalidateQueries({
          queryKey: getGetApiHospitalPropertiesDetailsQueryKey(),
        });
        navigate("/hospital/details");
      },
      onError: (error) => {
        console.error("Error al crear hospital:", error);
        msg.error("Error al registrar la información del hospital");
      },
    },
  });

  const handleCreate = (values: CreateHospitalPropertiesDto) => {
    createMutation.mutate({ data: values });
  };

  const handleCancel = () => {
    navigate("/hospital/details");
  };

  return {
    createMutation,
    handleCreate,
    handleCancel,
  };
}
