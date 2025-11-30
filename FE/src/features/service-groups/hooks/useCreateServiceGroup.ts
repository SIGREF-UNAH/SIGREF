import { useNavigate } from "react-router";
import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import type { CreateServiceGroupDto } from "../../../api/models";
import {
  getGetApiServiceGroupQueryKey,
  usePostApiServiceGroup,
} from "../../../api/service-group/service-group";

export function useCreateServiceGroup() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  const { mutateAsync, isPending } = usePostApiServiceGroup({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiServiceGroupQueryKey(),
        });
        msg.success("Paquete creado correctamente");
        navigate("/service-groups/list");
      },
      mutationKey:[],
      onError: (error: any) => {
        const errorMessage =
          error?.response?.data?.detail ||
          error?.message ||
          "Error al crear el paquete";
        msg.error(errorMessage);
      },
    },
  });

  const handleFinish = async (values: CreateServiceGroupDto) => {
    await mutateAsync({ data: values });
  };

  return {
    isPending,
    handleFinish,
  };
}
