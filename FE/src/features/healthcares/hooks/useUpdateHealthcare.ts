import { useNavigate, useParams } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import type { UpdateHealthcareDto } from "@models/healthcare-services/updateHealthcareDto";
import {
  getGetHealtcareListQueryKey,
  getGetHealtcareByIdQueryKey,
  useGetHealtcareById,
  useUpdateHealtcareById,
} from "@endpoints/healthcare-services/healthcare-services";

export function useUpdateHealthcare() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  // Obtener datos del servicio a editar
  const { data: healthcare, isLoading } = useGetHealtcareById(id!, {
    query: {
      enabled: !!id,
      staleTime: 0,
      gcTime: 0,
    },
  });

  // Mutación para actualizar servicio
  const { mutateAsync: updateHealthcare, isPending } = useUpdateHealtcareById({
    mutation: {
      onSuccess: () => {
        // Invalidar queries relacionadas
        queryClient.invalidateQueries({
          queryKey: getGetHealtcareListQueryKey(),
        });
        
        if (id) {
          queryClient.invalidateQueries({
            queryKey: getGetHealtcareByIdQueryKey(id),
          });
        }
        
        msg.success("Servicio médico actualizado correctamente");
        navigate("/healthcares");
      },
      onError: (error: any) => {
        // El middleware ya maneja la mayoría de errores, pero mostramos mensaje para errores de red
        const errorMessage = error?.response?.data?.detail 
          || error?.response?.data?.title 
          || "Error al actualizar el servicio médico";
        
        msg.error(errorMessage);
      },
    },
  });

  // Función para manejar el submit del formulario
  const handleFinish = async (values: UpdateHealthcareDto) => {
    if (!id) {
      msg.error("ID del servicio no encontrado");
      return;
    }

    await updateHealthcare({
      id,
      data: values,
    });
  };

  return {
    healthcare,
    isPending,
    isLoading,
    handleFinish,
  };
}
