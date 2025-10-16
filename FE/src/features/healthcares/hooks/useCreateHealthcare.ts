import { useNavigate } from "react-router";
import type { CreateHealthcareDto } from "../../../api/models";
import { useQueryClient } from "@tanstack/react-query";
import { getGetApiHealthcaresQueryKey, usePostApiHealthcares } from "../../../api/healthcares/healthcares";
import { useMessage } from "../../../shared/components";

export function useCreateHealthcare() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  // Mutación para crear servicio
  const { mutateAsync: createHealthcare, isPending } = usePostApiHealthcares({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiHealthcaresQueryKey() });
        msg.success("Servicio médico creado correctamente");
        navigate("/healthcares");
      },
      onError: (error: any) => {
        console.error("Error al crear el servicio médico:", error);
        msg.error(
          error?.response?.data?.message || "Error al crear el servicio médico"
        );
      },
    },
  });

  // Función para manejar el submit del formulario
  const handleFinish = async (values: CreateHealthcareDto) => {
    try {
      await createHealthcare({ data: values });
    } catch (error) {
      console.error("Error en handleFinish:", error);
    }
  };

  return {
    handleFinish,
    isPending,
  };
}