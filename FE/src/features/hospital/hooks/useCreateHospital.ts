import { useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { useMessage } from "../../../shared/hooks";
import type { CreateHospitalPropertiesDto } from "../../../api/models";
import {
  getGetHospitalPropertiesDetailsQueryKey,
  useCreateHospitalProperties,
} from "../../../api/hospital-properties/hospital-properties";

export default function useCreateHospital() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  const { mutateAsync: createHospital, isPending } = useCreateHospitalProperties({
    mutation: {
      onSuccess: () => {
        // Invalidar la query de detalles para que se recargue al navegar
        queryClient.invalidateQueries({
          queryKey: getGetHospitalPropertiesDetailsQueryKey(),
        });
        
        msg.success("Información del hospital registrada exitosamente");
        navigate("/hospital/details");
      },
      onError: (error: any) => {
        // Extraer mensaje según ProblemDetails (RFC 7807)
        const errorMessage =
          error?.response?.data?.detail ||
          error?.response?.data?.title ||
          "Error al registrar la información del hospital";

        msg.error(errorMessage);
      },
    },
  });

  const handleCreate = async (values: CreateHospitalPropertiesDto) => {
    await createHospital({ data: values });
  };

  const handleCancel = () => {
    navigate("/hospital/details");
  };

  return {
    isPending,
    handleCreate,
    handleCancel,
  };
}