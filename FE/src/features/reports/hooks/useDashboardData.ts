import { useMemo } from "react";
import dayjs from "dayjs";
import {
  useGetApiDashboardLocationIncome,
  useGetApiDashboardPackageUsage,
  useGetApiDashboardServiceUsage,
  useGetApiDashboardShiftIncome,
  useGetApiDashboardSummary,
  useGetApiDashboardWeeklyIncome,
} from "../../../api/dashboard/dashboard";

type PeriodoType = "semana" | "mes" | "personalizado";

export function useDashboardData(
  periodo: PeriodoType,
  fechaInicio: dayjs.Dayjs | null,
  fechaFin: dayjs.Dayjs | null,
) {
  const params = useMemo(() => {
    let start: string | undefined;
    let end: string | undefined;
    const format = "YYYY-MM-DDTHH:mm:ss";
    if (periodo === "personalizado" && fechaInicio && fechaFin) {
      // Inicio del día → 00:00:00
      start = fechaInicio.startOf("day").format(format);
      // Fin del día → 23:59:59 (para incluir todo el EndDate)
      end = fechaFin.endOf("day").format(format);
    } else {
      const today = dayjs();
      if (periodo === "semana") {
        start = today.startOf("isoWeek").startOf("day").format(format);
        end = today.endOf("isoWeek").endOf("day").format(format);
      } else if (periodo === "mes") {
        start = today.startOf("month").startOf("day").format(format);
        end = today.endOf("month").endOf("day").format(format);
      }
    }

    const finalParams = { StartDate: start, EndDate: end };

    return finalParams;
  }, [periodo, fechaInicio, fechaFin]);

  const qSummary = useGetApiDashboardSummary(params);
  const qServices = useGetApiDashboardServiceUsage(params);
  const qPackages = useGetApiDashboardPackageUsage(params);
  const qWeekly = useGetApiDashboardWeeklyIncome(params);
  const qShifts = useGetApiDashboardShiftIncome(params);
  const qLocations = useGetApiDashboardLocationIncome(params);

  const isLoading =
    qSummary.isLoading ||
    qServices.isLoading ||
    qPackages.isLoading ||
    qWeekly.isLoading ||
    qShifts.isLoading ||
    qLocations.isLoading;
  // TODO: quitar qServices.error cuando el endpoint esté listo
  const error =
    qSummary.error ||
    // qServices.error ||
    qPackages.error ||
    qWeekly.error ||
    qShifts.error ||
    qLocations.error;

  // Datos preparados para los gráficos / tarjetas
  const prepared = useMemo(() => {
    if (isLoading || error) return {};

    return {
      totalIncome: qSummary.data?.data?.totalIncome ?? 0,
      totalServices: qSummary.data?.data?.totalServices ?? 0,
      totalPatients: qSummary.data?.data?.totalPatients ?? 0,
      totalErrors: qSummary.data?.data?.totalCashierClosuresWithErrors ?? 0,

      serviciosMasSolicitados: (qServices.data?.data?.topUsed ?? []).map(
        (item) => ({
          nombre: item.serviceName || "Sin nombre",
          cantidad: item.count ?? 0,
        }),
      ),

      serviciosMenosSolicitados: (qServices.data?.data?.bottomUsed ?? []).map(
        (item) => ({
          nombre: item.serviceName || "Sin nombre",
          cantidad: item.count ?? 0,
        }),
      ),

      paquetesMasUtilizados: [
        ...(qPackages.data?.data?.top5 ?? []).map((p) => ({
          nombre: p.packageName || "Paquete",
          cantidad: p.count ?? 0,
        })),
      ].slice(0, 5), // limitamos a 5 para el pie chart

      ingresosDiarios: (qWeekly.data?.data ?? []).map((w) => ({
        dia: dayjs(w.weekStart).format("DD MMM"), // o usa weekStart directamente
        ingreso: w.totalIncome ?? 0,
      })),

      totalIngresosSemanalMensual:
        qWeekly.data?.data?.reduce((sum, w) => sum + (w.totalIncome ?? 0), 0) ??
        0,

      ingresosPorModulo: (qLocations.data?.data ?? []).map((loc) => ({
        modulo: loc.locationName || "Ubicación",
        ingreso: loc.totalIncome ?? 0,
        cantidad: loc.totalInvoices ?? 0, // o servicios si lo tuvieras
      })),

      ingresosPorTurno: (qShifts.data?.data ?? []).map((s) => ({
        turno: s.shiftName || "Turno",
        consultaExterna: 0,
        emergencia: 0,
        total: s.totalIncome ?? 0,
      })),
    };
  }, [
    qSummary.data,
    qServices.data,
    qPackages.data,
    qWeekly.data,
    qShifts.data,
    qLocations.data,
    isLoading,
    error,
  ]);

  return {
    isLoading,
    error: error ? (error as any).message || "Error desconocido" : null,
    ...prepared,
  };
}
