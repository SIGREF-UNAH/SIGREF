import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import {
  getGetApiSeriesQueryKey,
  usePutApiSeriesId,
} from "../../../api/series/series";

export const useToggleSerieActive = () => {
  const queryClient = useQueryClient();
  const msg = useMessage();

  // Toggle serie active state
  const { mutate: toggleActive, isPending } = usePutApiSeriesId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiSeriesQueryKey() });
        msg.success("Estado actualizado correctamente");
      },
      onError: (error: any) => {
        console.error(
          "Error al togglear estado:",
          error?.response?.data || error,
        );
        const errorMsg =
          error?.response?.data?.detail ||
          error?.response?.data?.title ||
          error?.response?.data?.message ||
          "No se pudo cambiar el estado de la serie.";
        msg.error(errorMsg);
      },
    },
  });

  const handleToggle = (id: string, isActive: boolean, currentData: any) => {
    toggleActive({
      id,
      data: {
        ...currentData,
        isActive,
      },
    });
  };

  return {
    handleToggle,
    isPending,
  };
};
