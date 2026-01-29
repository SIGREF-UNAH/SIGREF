import { useMemo } from "react";
import { useGetApiSeries } from "../../../api/series/series";
import type { SerieDto } from "../../../api/models";

export function useInvocesSeries() {
  // Obtener series desde la API
  const {
    data: response,
    isLoading,
    isError,
    refetch,
  } = useGetApiSeries(undefined, {
    query: {
      refetchOnWindowFocus: false,
      staleTime: 5 * 60 * 1000,
    },
  });

  // Procesar las series desde la respuesta de la API
  const series = useMemo(() => {
    if (!response?.data) {
      return [];
    }

    let rawSeries: SerieDto[] = [];

    if ("items" in response.data && Array.isArray(response.data.items)) {
      rawSeries = response.data.items as SerieDto[];
    } else if (Array.isArray(response.data)) {
      rawSeries = response.data as SerieDto[];
    }

    // Filtrar duplicados por ID (mantener solo el primero de cada ID)
    const uniqueSeries = rawSeries.reduce(
      (acc: SerieDto[], current: SerieDto) => {
        const currentId = (current as any).id;

        // Validar que tenga ID
        if (!currentId) {
          console.warn("Serie sin ID encontrada:", current);
          return acc;
        }

        // Buscar si ya existe
        const exists = acc.find((item: any) => item.id === currentId);
        if (!exists) {
          acc.push(current);
        } else {
          console.warn("Serie duplicada ignorada:", currentId, current.name);
        }

        return acc;
      },
      [],
    );

    return uniqueSeries;
  }, [response]);

  // Funciones helper
  const getSerieById = (id: string) => {
    return series.find((s: any) => s.id === id);
  };

  const getSerieByPrefix = (prefix: string) => {
    return series.find((s) => s.prefix === prefix);
  };

  const getNextNumber = (serieId: string) => {
    const serie = getSerieById(serieId);
    // Si no hay currentNumber, usar startNumber
    if (serie) {
      return (serie as any).currentNumber ?? serie.startNumber ?? 1;
    }
    return 1;
  };

  return {
    series,
    isLoading,
    isError,
    refetch,
    getSerieById,
    getSerieByPrefix,
    getNextNumber,
  };
}
