import { useState, useCallback } from "react";
import dayjs, { Dayjs } from "dayjs";

export interface ReportFilters {
  dateRange: [Dayjs, Dayjs];
  pageNumber: number;
  pageSize: number;
}

export const DEFAULT_PAGE_SIZE = 10;

export const useReportFilters = () => {
  const [filters, setFilters] = useState<ReportFilters>({
    dateRange: [dayjs().startOf("month"), dayjs()],
    pageNumber: 1,
    pageSize: DEFAULT_PAGE_SIZE,
  });


  const updateDateRange = useCallback((range: [Dayjs, Dayjs] | null) => {
    if (!range) return;
    setFilters((prev) => ({ ...prev, dateRange: range, pageNumber: 1 }));
  }, []);

  const updateLocations = useCallback((values: string[]) => {
    setFilters((prev) => ({
      ...prev,
      selectedLocations: values,
      pageNumber: 1,
    }));
  }, []);

  const updateUsers = useCallback((values: string[]) => {
    setFilters((prev) => ({ ...prev, selectedUsers: values, pageNumber: 1 }));
  }, []);

  const updateServices = useCallback((values: string[]) => {
    setFilters((prev) => ({
      ...prev,
      selectedServices: values,
      pageNumber: 1,
    }));
  }, []);

  const updatePage = useCallback((page: number, pageSize?: number) => {
    setFilters((prev) => ({
      ...prev,
      pageNumber: page,
      pageSize: pageSize ?? prev.pageSize,
    }));
  }, []);

  const clearFilters = useCallback(() => {
    setFilters({
      dateRange: [dayjs().startOf("month"), dayjs()],
      pageNumber: 1,
      pageSize: DEFAULT_PAGE_SIZE,
    });
  }, []);

  // Convertir los filtros a parámetros de consulta para la API
  const toQueryParams = useCallback(() => {
    const { dateRange, pageNumber, pageSize } =
      filters;

    return {
      StartDate: dateRange[0].toISOString(),
      EndDate: dateRange[1].endOf("day").toISOString(),
      PageNumber: pageNumber,
      PageSize: pageSize,
    };
  }, [filters]);

  return {
    filters,
    updateDateRange,
    updateLocations,
    updateUsers,
    updateServices,
    updatePage,
    clearFilters,
    toQueryParams,
  };
};
