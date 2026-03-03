import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import {
  getGetApiSeriesQueryKey,
  usePutApiSeriesId,        
  usePutApiSeriesDeleteId,  
} from "../../../api/series/series";

interface ToggleSerieData {
  name: string;
  prefix: string;
  startNumber: number;
  endNumber: number;
  currentNumber?: number;
  isActive?: boolean;
}

export const useToggleSerieActive = () => {
  const queryClient = useQueryClient();
  const msg = useMessage();

  // Hook para activar serie
  const { mutateAsync: activateSerie, isPending: isActivating } = usePutApiSeriesId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiSeriesQueryKey() });
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

  // Hook para desactivar serie
  const { mutateAsync: deactivateSerie, isPending: isDeactivating } = usePutApiSeriesDeleteId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiSeriesQueryKey() });
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

  const handleToggle = async (id: string, currentData: ToggleSerieData) => {
    if (!id) {
      msg.error("ID de serie no encontrado");
      return;
    }

    try {
      // 
      const isCurrentlyActive = currentData.isActive ?? true;

      if (isCurrentlyActive) {
        // Está activa → desactivar
        console.log("Desactivando serie:", id);
        await deactivateSerie({ id });
      } else {
        // Está inactiva → activar
        console.log("Activando serie:", id);
        await activateSerie({
          id,
          data: {
            name: currentData.name,
            prefix: currentData.prefix,
            startNumber: currentData.startNumber,
            endNumber: currentData.endNumber,
            isActive: true, 
          },
        });
      }

      // Refrescar la lista
      await queryClient.invalidateQueries({ 
        queryKey: getGetApiSeriesQueryKey() 
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