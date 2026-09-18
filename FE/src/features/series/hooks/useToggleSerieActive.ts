import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import {
  getGetSerieListQueryKey,
  useUpdateSerieById,
  useDeleteSerieById,
} from "@endpoints/series/series";
import type { UpdateSeriesDto } from "@models";

interface ToggleSerieData {
  name: string;
  prefix: string;
  startNumber: number;
  endNumber: number;
  currentNumber?: number;
  isActive?: boolean;
}

/**
 * Hook personalizado para activar/desactivar una serie
 */
export const useToggleSerieActive = () => {
  const queryClient = useQueryClient();
  const msg = useMessage();

  // Hook para actualizar (activar/reactivar) serie
  const { mutateAsync: activateSerie, isPending: isActivating } = useUpdateSerieById({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetSerieListQueryKey() });
        msg.success("Serie activada correctamente");
      },
      onError: (error: any) => {
        console.error("Error al activar serie:", error?.response?.data || error);
        const errorMsg =
          error?.response?.data?.detail ||
          error?.response?.data?.title ||
          "No se pudo activar la serie.";
        msg.error(errorMsg);
      },
    },
  });

  // Hook para desactivar (soft delete) serie
  const { mutateAsync: deactivateSerie, isPending: isDeactivating } = useDeleteSerieById({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetSerieListQueryKey() });
        msg.success("Serie desactivada correctamente");
      },
      onError: (error: any) => {
        console.error("Error al desactivar serie:", error?.response?.data || error);
        const errorMsg =
          error?.response?.data?.detail ||
          error?.response?.data?.title ||
          "No se pudo desactivar la serie.";
        msg.error(errorMsg);
      },
    },
  });

  const isPending = isActivating || isDeactivating;

  /**
   * Alterna el estado activo/inactivo de una serie
   * 
   * Si la serie está activa, la desactiva (soft delete)
   * Si la serie está inactiva, la reactiva con sus datos
   */
  const handleToggle = async (id: string, currentData: ToggleSerieData) => {
    if (!id) {
      msg.error("ID de serie no encontrado");
      return;
    }

    try {
      const isCurrentlyActive = currentData.isActive ?? true;

      if (isCurrentlyActive) {
        // Está activa → desactivar (soft delete)
        await deactivateSerie({ id });
      } else {
        // Está inactiva → activar (actualizar)
        const updateData: UpdateSeriesDto = {
          name: currentData.name,
          prefix: currentData.prefix,
          startNumber: currentData.startNumber,
          endNumber: currentData.endNumber,
          isActive: true,
        };

        await activateSerie({
          id,
          data: updateData,
        });
      }

      // Refrescar la lista
      await queryClient.invalidateQueries({ 
        queryKey: getGetSerieListQueryKey() 
      });
    } catch (error: any) {
      console.error("Error en handleToggle:", error);
    }
  };

  return {
    handleToggle,
    isPending,
  };
};