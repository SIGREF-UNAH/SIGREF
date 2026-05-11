import { useMemo } from "react";
import dayjs from "dayjs";
import isoWeek from "dayjs/plugin/isoWeek";
import {
  useGetDashboardLocationIncome,
  useGetDashboardPackageUsage,
  useGetDashboardServiceUsage,
  useGetDashboardShiftIncome,
  useGetDashboardSummary,
  useGetDashboardWeeklyIncome,
} from "../../../api/dashboard/dashboard";

dayjs.extend(isoWeek);

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
      // Inicio del día para fechaInicio y fin del día para fechaFin  
      start = fechaInicio.startOf("day").format(format);
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

  const qSummary = useGetDashboardSummary(params);
  const qServices = useGetDashboardServiceUsage(params);
  const qPackages = useGetDashboardPackageUsage(params);
  const qWeekly = useGetDashboardWeeklyIncome(params);
  const qShifts = useGetDashboardShiftIncome(params);
  const qLocations = useGetDashboardLocationIncome(params);

  // TODO: Agregar qServices.isLoading y qServices.error cuando el endpoint esté arreglado
  const isLoading =
    qSummary.isLoading ||
    // qServices.isLoading ||
    qPackages.isLoading ||
    qWeekly.isLoading ||
    qShifts.isLoading ||
    qLocations.isLoading;

  const error =
    qSummary.error ||
    // qServices.error ||
    qPackages.error ||
    qWeekly.error ||
    qShifts.error ||
    qLocations.error;

  const prepared = useMemo(() => {
    if (isLoading || error) return {};

    return {
      totalIncome: qSummary.data?.data?.totalIncome ?? 0,
      totalServices: qSummary.data?.data?.totalServices ?? 0,
      totalPatients: qSummary.data?.data?.totalPatients ?? 0,
      totalCashierClosuresWithErrors: qSummary.data?.data?.totalCashierClosuresWithErrors ?? 0,

      serviciosMasSolicitados: (qServices.data?.data?.topUsed ?? []).map(
        (item) => ({
          serviceId: item.serviceId,
          fhirServiceId: item.fhirServiceId,
          serviceName: item.serviceName || "Sin nombre",
          count: item.count ?? 0,
          totalGenerated: item.totalGenerated ?? 0,
          percentage: item.percentage ?? 0,
        }),
      ),

      serviciosMenosSolicitados: (qServices.data?.data?.bottomUsed ?? []).map(
        (item) => ({
          serviceId: item.serviceId,
          fhirServiceId: item.fhirServiceId,
          serviceName: item.serviceName || "Sin nombre",
          count: item.count ?? 0,
          totalGenerated: item.totalGenerated ?? 0,
          percentage: item.percentage ?? 0,
        }),
      ),

      paquetesMasUtilizados: (qPackages.data?.data?.top5 ?? []).map((p) => ({
        fhirPackageId: p.fhirPackageId,
        packageName: p.packageName || "Sin nombre",
        count: p.count ?? 0,
        totalGenerated: p.totalGenerated ?? 0,
        percentage: p.percentage ?? 0,
      })),

      paquetesMenosUtilizados: (() => {
        const others = qPackages.data?.data?.others;
        if (!others) return [];

        return [
          {
            fhirPackageId: others.fhirPackageId,
            packageName: others.packageName || "Otros",
            count: others.count ?? 0,
            totalGenerated: others.totalGenerated ?? 0,
            percentage: others.percentage ?? 0,
          },
        ];
      })(),

      ingresosDiarios: (qWeekly.data?.data ?? []).map((w) => ({
        weekStart: w.weekStart,
        weekEnd: w.weekEnd,
        totalIncome: w.totalIncome ?? 0,
        invoiceCount: w.invoiceCount ?? 0,
      })),

      // MODIFICADO: Ahora retorna array para el gráfico comparativo
      totalIngresosSemanalMensual: (() => {
        const data = qWeekly.data?.data ?? [];
        if (data.length === 0) return [];

        // Para período semanal: mostrar por día de la semana
        if (periodo === "semana") {
          return data.map((w) => ({
            dia: dayjs(w.weekStart).format("ddd"), // Lun, Mar, Mié, etc.
            totalIncome: w.totalIncome ?? 0,
          }));
        }

        // Para período mensual: mostrar por semanas
        if (periodo === "mes") {
          return data.map((w, index) => ({
            dia: `Sem ${index + 1}`,
            totalIncome: w.totalIncome ?? 0,
          }));
        }

        // Para período personalizado: depende del rango
        if (periodo === "personalizado" && fechaInicio && fechaFin) {
          const diff = fechaFin.diff(fechaInicio, "day");
          
          // Si es menos de 14 días, mostrar por día
          if (diff <= 14) {
            return data.map((w) => ({
              dia: dayjs(w.weekStart).format("DD/MM"),
              totalIncome: w.totalIncome ?? 0,
            }));
          }
          
          // Si es más de 14 días, mostrar por semana
          return data.map((w, index) => ({
            dia: `Sem ${index + 1}`,
            totalIncome: w.totalIncome ?? 0,
          }));
        }

        return [];
      })(),

      ingresosPorModulo: (qLocations.data?.data ?? []).map((loc) => ({
        locationId: loc.locationId,
        locationName: loc.locationName || "Ubicación",
        totalIncome: loc.totalIncome ?? 0,
        totalInvoices: loc.totalInvoices ?? 0,
      })),

      ingresosPorTurno: (qShifts.data?.data ?? []).map((s) => ({
        shiftId: s.shiftId,
        shiftName: s.shiftName || "Turno",
        locationId: s.locationId,
        locationName: s.locationName || "Ubicación",
        totalIncome: s.totalIncome ?? 0,
        totalInvoices: s.totalInvoices ?? 0,
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
    periodo,
    fechaInicio,
    fechaFin,
  ]);

  return {
    isLoading,
    error: error ? (error as any).message || "Error desconocido" : null,
    ...prepared,
  };
}