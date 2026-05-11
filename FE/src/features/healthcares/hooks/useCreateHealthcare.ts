import { useNavigate } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import type { CreateHealthcareDto } from "../../../api/models";
import {
  getGetHealtcareListQueryKey,
  useCreateHealtcare,
} from "../../../api/healthcares/healthcares";

export function useCreateHealthcare() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  // Mutación para crear servicio
  const { mutateAsync: createHealthcare, isPending } = useCreateHealtcare({
    mutation: {
      onSuccess: () => {
        // Invalidar la lista de healthcares para que se refresque
        queryClient.invalidateQueries({
          queryKey: getGetHealtcareListQueryKey(),
        });
        
        msg.success("Servicio médico creado correctamente");
        navigate("/healthcares/list");
      },
      onError: (error: any) => {
        // Extraer mensaje según ProblemDetails (RFC 7807)
        const errorMessage =
          error?.response?.data?.detail ||
          error?.response?.data?.title ||
          "Error al crear el servicio médico";

        msg.error(errorMessage);
      },
    },
  });

  // Función para manejar el submit del formulario
  const handleFinish = async (values: CreateHealthcareDto) => {
    await createHealthcare({ data: values });
  };

  return {
    handleFinish,
    isPending,
  };
}