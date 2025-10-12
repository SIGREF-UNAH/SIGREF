import { useNavigate, useParams } from "react-router-dom";
import { message } from "antd";
import { useQueryClient } from "@tanstack/react-query";
import type { UpdateHealthcareDto } from "../../../api/models";
import {
  getGetApiHealthcaresQueryKey,
  useGetApiHealthcaresId,
  usePutApiHealthcaresId,
} from "../../../api/healthcares/healthcares";

export function useUpdateHealthcare() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  // Obtener datos del servicio a editar
  const { data: healthcare, isLoading } = useGetApiHealthcaresId(id!, {
    query: {
      enabled: !! id, // Solo ejecuta si hay id
    },
  });

  // Mutación para actualizar servicio
  const { mutateAsync: updateHealthcare, isPending } = usePutApiHealthcaresId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiHealthcaresQueryKey(),
        });
        message.success("Servicio médico actualizado exitosamente");
        navigate("/healthcares");
      },
      onError: (error: any) => {
        console.error("Error al actualizar el servicio médico:", error);
        message.error(
          error?.response?.data?.message ||
            "Error al actualizar el servicio médico"
        );
      },
    },
  });

  // Función para manejar el submit del formulario
  const handleFinish = async (values: UpdateHealthcareDto) => {
    if (!id) {
      message.error("ID del servicio no encontrado");
      return;
    }

    try {
      await updateHealthcare({
        id,
        data: values,
      });
    } catch (error) {
      console.error("Error en handleFinish:", error);
    }
  };

  return {
    healthcare,
    isPending,
    isLoading,
    handleFinish,
  };
}
