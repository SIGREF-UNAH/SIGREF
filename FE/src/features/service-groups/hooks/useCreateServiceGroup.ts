import { useNavigate } from "react-router";
import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import type { CreateServiceGroupDto } from "@models";
import {
  getGetServiceGroupListQueryKey,
  useCreateServiceGroup as useCreateServiceGroupMutation,
} from "@endpoints/service-groups/service-groups";

/**
 * Hook personalizado para crear un paquete de servicios
 */
export function useCreateServiceGroup() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  const { mutateAsync, isPending } = useCreateServiceGroupMutation({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetServiceGroupListQueryKey(),
        });
        msg.success("Paquete creado correctamente");
        navigate("/service-groups/list");
      },
      onError: (error: any) => {
        const errorMessage =
          error?.response?.data?.detail ||
          error?.message ||
          "Error al crear el paquete";
        msg.error(errorMessage);
      },
    },
  });

  /**
   * Maneja la finalización del formulario de creación
   * 
   * @param values - Datos del paquete de servicios a crear
   */
  const handleFinish = async (values: CreateServiceGroupDto) => {
    await mutateAsync({ data: values });
  };

  return {
    isPending,
    handleFinish,
  };
}