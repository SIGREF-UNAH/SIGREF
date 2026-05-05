import { useState, useMemo } from "react";
import { useUrlFilters } from "../../../shared/hooks";
//import { useGetApiAudit } from "../../../api/audit/audit";
//import type { AuditLogDto } from "../../../api/models/auditLogDto";
import dayjs, { Dayjs } from "dayjs";
import { Form } from "antd";


type AuditLogDto = {
  id: number;
  action: string;}


export default function useEventHistory() {
  const [form] = Form.useForm();
  const [selectedRecord, setSelectedRecord] = useState<AuditLogDto | null>(null);
  const [modalOpen, setModalOpen] = useState(false);

  // Filtros en la URL
  const { filters, setFilters, resetFilters } = useUrlFilters({
    defaultValues: {
      action: undefined as string | undefined,
      userName: undefined as string | undefined,
      from: undefined as string | undefined,
      to: undefined as string | undefined,
      page: 1,
      pageSize: 10,
    },
  });

  const [formValues, setFormValues] = useState({
    action: filters.action,
    userName: filters.userName || "",
    dateRange:
      filters.from && filters.to
        ? ([dayjs(filters.from), dayjs(filters.to)] as [Dayjs, Dayjs])
        : null,
  });

  // Query params para backend
  const queryParams = useMemo(
    () => ({
      page: filters.page,
      pageSize: filters.pageSize,
      action: filters.action,
      userName: filters.userName,
      from: filters.from,
      to: filters.to,
    }),
    [filters]
  );

  // Fetch data
  //const { data: response, isLoading } = useGetApiAudit(queryParams);
 const response = null;
  const responseData = response as any;
  const data = responseData?.data || [];
  const pagination = responseData;

  // Busqueda
  const handleSearch = () => {
    const newFilters: any = {
      action: formValues.action,
      userName: formValues.userName || undefined,
      page: 1, // Resetear a página 1 al buscar
    };

    if (formValues.dateRange) {
      newFilters.from = formValues.dateRange[0].toISOString();
      newFilters.to = formValues.dateRange[1].toISOString();
    } else {
      newFilters.from = undefined;
      newFilters.to = undefined;
    }

    setFilters(newFilters);
  };

  // Limpiar filtros
  const handleClearFilters = () => {
    // Limpiar formulario
    setFormValues({
      action: undefined,
      userName: "",
      dateRange: null,
    });
    form.resetFields();
    // Limpiar filtros de URL
    resetFilters();
  };

  // Ver de detalles
  const handleViewDetails = (record: AuditLogDto) => {
    setSelectedRecord(record);
    setModalOpen(true);
  };

  // Colores
  const getActionColor = (action: string | null | undefined) => {
    if (!action) return "default";
    const actionLower = action.toLowerCase();
    if (actionLower.includes("create")) return "cyan";
    if (actionLower.includes("read")) return "green";
    if (actionLower.includes("update")) return "orange";
    if (actionLower.includes("delete")) return "red";
    return "blue";
  };

  const getStatusColor = (statusCode: number | undefined) => {
    if (!statusCode) return "default";
    if (statusCode >= 200 && statusCode < 300) return "success";
    if (statusCode >= 400 && statusCode < 500) return "warning";
    if (statusCode >= 500) return "error";
    return "default";
  };

  return {
    data,
    form,
    filters,
    formValues,
    selectedRecord,
    modalOpen,
    pagination,
    //isLoading,
    setFilters,
    handleSearch,
    handleClearFilters,
    handleViewDetails,
    getActionColor,
    getStatusColor,
    setFormValues,
    setSelectedRecord,
    setModalOpen,
  };
}
