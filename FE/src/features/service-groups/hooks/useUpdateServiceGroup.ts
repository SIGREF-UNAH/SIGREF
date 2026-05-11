import { useNavigate, useParams } from "react-router";
import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import type { UpdateServiceGroupDto } from "../../../api/models";
import {
  getGetServiceGroupListQueryKey,
  useUpdateServiceGroupById,
  useGetServiceGroupById,
} from "../../../api/service-group/service-group";

export function useUpdateServiceGroup() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();
  const { id } = useParams<{ id: string }>();

  const { data: serviceGroup, isLoading } = useGetServiceGroupById(id || "", {
    query: {
      enabled: !!id,
    },
  });

  const { mutateAsync, isPending } = useUpdateServiceGroupById({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetServiceGroupListQueryKey(),
        });
        msg.success("Paquete actualizado correctamente");
        navigate("/service-groups/list");
      },
      onError: (error: any) => {
        const errorMessage =
          error?.response?.data?.detail ||
          error?.message ||
          "Error al actualizar el paquete";
        msg.error(errorMessage);
      },
    },
  });

  const handleFinish = async (values: UpdateServiceGroupDto) => {
    if (!id) return;
    await mutateAsync({ id, data: values });
  };

  return {
    serviceGroup,
    isLoading,
    isPending,
    handleFinish,
  };
}
