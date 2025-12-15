
import { message } from "antd";
import { useGetApiSeries } from "../../../api/series/series";

export interface InvoiceSerie {
  id: string;
  name: string;
  prefix: string;
  startNumber: number;
  endNumber: number;
  currentNumber?: number;
}

export const useInvoiceSeries = () => {
  const query = useGetApiSeries({
    query: {
      select: (data: any): InvoiceSerie[] => {
        // Si el backend devuelve un array directo
        if (Array.isArray(data)) return data;

        // Si viene envuelto en una propiedad (ej: { items: [...] })
        return data?.items || data?.data || [];
      },
      onError: (error: any) => {
        console.error("Error cargando series de facturación:", error);
        message.error("No se pudieron cargar las series de facturación");
      },
      staleTime: 1000 * 60 * 10, // 10 minutos
      cacheTime: 1000 * 60 * 30, // 30 minutos
    },
  });

  return {
    series: query.data ?? [],
    isLoading: query.isLoading,
    isError: query.isError,
    error: query.error,
    refetch: query.refetch,
  };
};