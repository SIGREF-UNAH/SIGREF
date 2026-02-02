import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import {
  getGetApiSeriesQueryKey,
  usePutApiSeriesId,
} from "../../../api/series/series";
import type { UpdateSeriesDto } from "../../../api/models";

export const useUpdateSerie = () => {
  const queryClient = useQueryClient();
  const msg = useMessage();

  // Actualizar serie
  const { mutateAsync: updateSerie, isPending } = usePutApiSeriesId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiSeriesQueryKey() });
        msg.success("Serie actualizada correctamente");
      },
      onError: (error: any) => {
        console.error(
          "Error al actualizar serie:",
          error?.response?.data || error,
        );

        const errorMsg =
          error?.response?.data?.detail ||
          error?.response?.data?.title ||
          error?.response?.data?.message ||
          error?.message ||
          "Error al actualizar la serie. Revisa si faltan campos obligatorios.";

        msg.error(errorMsg);
      },
    },
  });

  // Manejar la finalización del formulario de edición
  const handleEdit = async (id: string, values: UpdateSeriesDto) => {
    if (!id) {
      msg.error("ID de serie no encontrado");
      return false;
    }

    try {
      await updateSerie({ id, data: values });
      return true;
    } catch (error) {
      return false;
    }
  };

  return {
    handleEdit,
    isPending,
  };
};
