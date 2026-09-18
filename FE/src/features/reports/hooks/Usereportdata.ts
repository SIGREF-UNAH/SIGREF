import { useState, useCallback } from "react";
import type {
  GetReportDetailParams,
  GetReportSummaryParams,
} from "@models";
import {
  useGetReportDetail,
  useGetReportSummary,
} from "@endpoints/reports/reports";

export const useReportData = () => {
  const [committed, setCommitted] = useState<{
    params: GetReportSummaryParams & GetReportDetailParams;
    enabled: boolean;
  }>({ params: {}, enabled: false });

  // Summary
  const summaryQuery = useGetReportSummary(committed.params, {
    query: { enabled: committed.enabled },
  });

  // Detail
  const detailQuery = useGetReportDetail(committed.params, {
    query: { enabled: committed.enabled },
  });

  // Acciones para generar, paginar y resetear el reporte
  const generateReport = useCallback(
    (
      params: GetReportSummaryParams & GetReportDetailParams,
    ) => {
      setCommitted({ params, enabled: true });
    },
    [],
  );

  const changePage = useCallback((pageNumber: number, pageSize: number) => {
    setCommitted((prev) => ({
      ...prev,
      params: { ...prev.params, PageNumber: pageNumber, PageSize: pageSize },
    }));
  }, []);

  const resetReport = useCallback(() => {
    setCommitted({ params: {}, enabled: false });
  }, []);

  // Estados combinados y datos derivados
  const isLoading = summaryQuery.isLoading || detailQuery.isLoading;
  const isError = summaryQuery.isError || detailQuery.isError;
  const hasData = committed.enabled && !isLoading;

  const summaryData = summaryQuery.data;
  const detailData = detailQuery.data;

  const summaryStats = summaryData?.summary;
  const hospitalInfo = summaryData?.hospital;
  const metadata = summaryData?.metadata ?? detailData?.metadata;

  const items = detailData?.items ?? [];
  const pagination = detailData?.pagination;

  return {
    // Estados
    isLoading,
    isError,
    hasData,
    isGenerated: committed.enabled,

    // Queries
    summaryQuery,
    detailQuery,

    // Datos
    summaryStats,
    hospitalInfo,
    metadata,
    items,
    pagination,

    // Acciones
    generateReport,
    changePage,
    resetReport,
  };
};
