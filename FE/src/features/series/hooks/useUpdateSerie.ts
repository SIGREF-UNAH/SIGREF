import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import {
  getGetSerieListQueryKey,
  useUpdateSerieById,
} from "../../../api/series/series";
import type { UpdateSeriesDto } from "../../../api/models";

/**
 * Hook personalizado para actualizar una serie
 */
export const useUpdateSerie = () => {
  const queryClient = useQueryClient();
  const msg = useMessage();

  // Actualizar serie
  const { mutateAsync: updateSerie, isPending } = useUpdateSerieById({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetSerieListQueryKey() });
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

  /**
   * Maneja la finalización del formulario de edición
   * 
   * @param id - ID de la serie a actualizar
   * @param values - Datos actualizados de la serie
   * @returns true si la actualización fue exitosa, false en caso contrario
   */
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