// useEventHistory.ts
import { useState, useCallback } from "react";
import { Form } from "antd";
import type { Dayjs } from "dayjs";
import type { AuditLog, PagedResultDtoOfAuditLog as AuditLogPagedResultDto } from "@models/audit";
import { useGetAuditLogs } from "@endpoints/audit/audit";
import type { GetAuditLogsParams } from "@models/audit";

interface FormValues {
  userName?: string;
  userId?: string;
  action?: string;
  httpMethod?: string;
  resourceType?: string;
  traceId?: string;
  ipAddress?: string;
  success?: boolean;
  dateRange: [Dayjs, Dayjs] | null;
}

const initialFormValues: FormValues = {
  userName: "",
  userId: "",
  action: undefined,
  httpMethod: undefined,
  resourceType: "",
  traceId: "",
  ipAddress: "",
  success: undefined,
  dateRange: null,
};

export const useEventHistory = (initialFilters?: Partial<GetAuditLogsParams>) => {
  const [form] = Form.useForm();
  const [formValues, setFormValues] = useState<FormValues>(initialFormValues);
  const [selectedRecord, setSelectedRecord] = useState<AuditLog | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [filters, setFilters] = useState<GetAuditLogsParams>({
    CurrentPage: 1,
    PageSize: 20,
    ...initialFilters,
  });

  const buildParams = useCallback((): GetAuditLogsParams => {
    const params: GetAuditLogsParams = {
      CurrentPage: filters.CurrentPage || 1,
      PageSize: filters.PageSize || 20,
    };

    if (formValues.userName?.trim()) params.UserName = formValues.userName.trim();
    if (formValues.userId?.trim()) params.UserId = formValues.userId.trim();
    if (formValues.action) params.Action = formValues.action;
    if (formValues.httpMethod) params.HttpMethod = formValues.httpMethod;
    if (formValues.resourceType?.trim()) params.ResourceType = formValues.resourceType.trim();
    if (formValues.traceId?.trim()) params.TraceId = formValues.traceId.trim();
    if (formValues.ipAddress?.trim()) params.IpAddress = formValues.ipAddress.trim();
    if (formValues.success !== undefined) params.Success = formValues.success;
    // Las propiedades correctas son FromDate y ToDate, no From/To
    if (formValues.dateRange?.[0]) params.FromDate = formValues.dateRange[0].toISOString();
    if (formValues.dateRange?.[1]) params.ToDate = formValues.dateRange[1].toISOString();

    return params;
  }, [filters.CurrentPage, filters.PageSize, formValues]);

  const { data, isLoading } = useGetAuditLogs(
    buildParams(),
    {
      query: {
        placeholderData: (prev) => prev,
        select: (response): AuditLogPagedResultDto => ({
          items: response.items ?? [],
          pagination: response.pagination ?? {
            currentPage: 1,
            pageSize: 20,
            totalItems: 0,
            totalPages: 0,
            hasPrevious: false,
            hasNext: false,
          },
        }),
      },
    }
  );

  const handleSearch = () => {
    setFilters((prev) => ({
      ...prev,
      CurrentPage: 1,
    }));
  };

  const handleClearFilters = () => {
    setFormValues(initialFormValues);
    form.resetFields();
    setFilters({
      CurrentPage: 1,
      PageSize: 20,
    });
  };

  const handleViewDetails = (record: AuditLog) => {
    setSelectedRecord(record);
    setModalOpen(true);
  };

  const getActionColor = (action: string | null | undefined): string => {
    if (!action) return "default";
    const colors: Record<string, string> = {
      read: "blue",
      create: "green",
      update: "orange",
      delete: "red",
    };
    return colors[action.toLowerCase()] || "default";
};

  const getHttpMethodColor = (method: string): string => {
    const colors: Record<string, string> = {
      get: "blue",
      post: "green",
      put: "orange",
      patch: "purple",
      delete: "red",
    };
    return colors[method?.toLowerCase()] || "default";
  };

  const getStatusColor = (statusCode?: number): string => {
    if (!statusCode) return "default";
    if (statusCode >= 200 && statusCode < 300) return "success";
    if (statusCode >= 300 && statusCode < 400) return "warning";
    if (statusCode >= 400 && statusCode < 500) return "error";
    if (statusCode >= 500) return "magenta";
    return "default";
  };

  return {
    data: data ?? { items: [], pagination: { currentPage: 1, pageSize: 20, totalItems: 0, totalPages: 0, hasPrevious: false, hasNext: false } },
    form,
    formValues,
    filters,
    selectedRecord,
    modalOpen,
    isLoading,
    setFilters,
    handleSearch,
    handleClearFilters,
    handleViewDetails,
    getActionColor,
    getHttpMethodColor,
    getStatusColor,
    setFormValues,
    setModalOpen,
  };
};
