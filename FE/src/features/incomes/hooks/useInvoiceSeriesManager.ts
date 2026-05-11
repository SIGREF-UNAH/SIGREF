import { useEffect, useState } from "react";
import { useGetSerieList } from "../../../api/series/series";
import type { SerieDto } from "../../../api/models";

export function useInvoiceSeriesManager() {
  const {
    data,
    isLoading,
    isError,
    refetch,
  } = useGetSerieList({}, {
    query: {
      staleTime: 1000 * 60 * 5, // 5 minutos
      refetchOnWindowFocus: false,
    },
  });

  // Extraer series del response
  const series = data?.items ?? [];

  // Estados controlados
  const [serieId, setSerieId] = useState<string>("");
  const [seriePrefix, setSeriePrefix] = useState<string>("");
  const [numeroRecibo, setNumeroRecibo] = useState<string>("");

  // Helpers
  const getSerieById = (id: string): SerieDto | undefined =>
    series.find((s) => s.id === id);

  const getSerieByPrefix = (prefix: string): SerieDto | undefined =>
    series.find((s) => s.prefix === prefix);

  const getNextNumber = (serie: SerieDto): number => {
    if (typeof serie.currentNumber === "number") {
      return serie.currentNumber + 1;
    }
    if (typeof serie.startNumber === "number") {
      return serie.startNumber;
    }
    return 1;
  };

  // Seleccionar serie
  const selectSerie = (serie: SerieDto) => {
    if (!serie.id || !serie.prefix) return;
    setSerieId(serie.id);
    setSeriePrefix(serie.prefix);
    setNumeroRecibo(getNextNumber(serie).toString());
  };

  // Reset de serie
  const resetSerie = () => {
    setSerieId("");
    setSeriePrefix("");
    setNumeroRecibo("");
  };

  // Default automático - seleccionar la primera serie disponible
  useEffect(() => {
    if (isLoading || series.length === 0) return;
    if (serieId) return; // no pisar selección manual

    const firstSerie = series[0];
    if (firstSerie?.id && firstSerie?.prefix) {
      selectSerie(firstSerie);
    }
  }, [isLoading, series, serieId]);

  // Handler público para cambio de serie
  const handleSerieChange = (prefix: string) => {
    const selected = getSerieByPrefix(prefix);
    if (selected) {
      selectSerie(selected);
    }
  };

  // Validaciones
  const isSerieValid = Boolean(serieId && seriePrefix && numeroRecibo);
  const currentSerie = serieId ? getSerieById(serieId) : undefined;

  // Verificar si el número está dentro del rango permitido
  const isNumberInRange = (): boolean => {
    if (!currentSerie || !numeroRecibo) return false;
    const num = parseInt(numeroRecibo, 10);
    if (isNaN(num)) return false;

    const hasStart = typeof currentSerie.startNumber === "number";
    const hasEnd = typeof currentSerie.endNumber === "number";

    if (hasStart && num < currentSerie.startNumber!) return false;
    if (hasEnd && num > currentSerie.endNumber!) return false;

    return true;
  };

  return {
    // Data
    series,
    serieId,
    seriePrefix,
    numeroRecibo,
    currentSerie,

    // Estados
    isLoading,
    isError,

    // Helpers
    getSerieById,
    getSerieByPrefix,
    getNextNumber,

    // Validaciones
    isSerieValid,
    isNumberInRange: isNumberInRange(),

    // Setters
    setSerieId,
    setSeriePrefix,
    setNumeroRecibo,

    // Handlers
    handleSerieChange,
    selectSerie,
    resetSerie,
    refetchSeries: refetch,
  };
}