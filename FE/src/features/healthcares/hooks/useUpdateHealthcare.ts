import { useNavigate, useParams } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import type { UpdateHealthcareDto } from "../../../api/models";
import {
  getGetApiHealthcaresQueryKey,
  getGetApiHealthcaresIdQueryKey,
  useGetApiHealthcaresId,
  usePutApiHealthcaresId,
} from "../../../api/healthcares/healthcares";

export function useUpdateHealthcare() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  // Obtener datos del servicio a editar
  const { data: healthcareData, isLoading } = useGetApiHealthcaresId(id!, {
    query: {
      enabled: !!id, // Solo ejecuta si hay id
      // Forzar refetch para evitar datos cacheados incorrectos
      staleTime: 0,
      gcTime: 0,
    },
  });

  const healthcare = healthcareData?.data;

  // Mutación para actualizar servicio
  const { mutateAsync: updateHealthcare, isPending } = usePutApiHealthcaresId({
    mutation: {
      onSuccess: () => {
        // Invalidar la lista de healthcares
        queryClient.invalidateQueries({
          queryKey: getGetApiHealthcaresQueryKey(),
        });
        
        // Invalidar específicamente el healthcare que se acaba de actualizar
        if (id) {
          queryClient.invalidateQueries({
            queryKey: getGetApiHealthcaresIdQueryKey(id),
          });
        }
        
        // Remover todas las queries individuales de healthcares para evitar cache
        queryClient.removeQueries({
          queryKey: ['/api/Healthcares'],
          exact: false,
        });
        
        msg.success("Servicio médico actualizado correctamente");
        navigate("/healthcares");
      },
      onError: (error: any) => {
        console.error("Error al actualizar el servicio médico:", error);
        msg.error(
          error?.response?.data?.message ||
            "Error al actualizar el servicio médico"
        );
      },
    },
  });

  // Función para manejar el submit del formulario
  const handleFinish = async (values: UpdateHealthcareDto) => {
    if (!id) {
      msg.error("ID del servicio no encontrado");
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