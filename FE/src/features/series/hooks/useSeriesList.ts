import { useState, useMemo } from "react";
import { useNavigate } from "react-router";
import { useQueryClient } from "@tanstack/react-query";
import { useAbility } from "../../../config";
import { useMessage, useUrlFilters } from "../../../shared/hooks";
import {
  getGetApiSeriesQueryKey,
  useDeleteApiSeriesId,
  useGetApiSeries,
} from "../../../api/series/series";
import type { TablePaginationConfig } from "antd";

export function useSeriesList() {
  const navigate = useNavigate();
  const ability = useAbility();
  const queryClient = useQueryClient();
  const msg = useMessage();
  const [searchInput, setSearchInput] = useState("");

  const { filters, setFilter, setFilters } = useUrlFilters({
    defaultValues: {
      search: "",
      pageNumber: 1,
      pageSize: 10,
    },
  });

  const queryParams = useMemo(() => {
    const params: any = {
      pageNumber: filters.pageNumber,
      pageSize: filters.pageSize,
    };
    if (filters.search) {
      params.name = filters.search;
    }
    return params;
  }, [filters]);

  const {
    data: response,
    isLoading,
    isFetching,
    isError,
    refetch,
  } = useGetApiSeries(queryParams, {
    query: {
      placeholderData: (prev) => prev,
    },
  });

  // Extraer series y paginación de la respuesta
  const series = response?.data?.items || response?.items || [];
  const pagination = response?.data?.pagination || response?.pagination;

  // Eliminar serie
  const { mutate: deleteSerie, isPending: isDeleting } = useDeleteApiSeriesId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: ["/api/Series"],
          exact: false,
        });
        queryClient.refetchQueries({
          queryKey: getGetApiSeriesQueryKey(queryParams),
        });
        msg.success("Serie desactivada correctamente");
      },
      onError: (error: any) => {
        const errorMsg =
          error?.response?.data?.title ||
          error?.response?.data?.detail ||
          "No se puede eliminar: esta serie ya tiene recibos/facturas asociadas. Desactívela en su lugar.";
        msg.error(errorMsg);
      },
    },
  });

  const handleCreate = () => navigate("/series/create");

  const handleDelete = (id: string) => {
    deleteSerie({ id });
  };

  const handleSearchInputChange = (value: string) => setSearchInput(value);

  const handleSearch = () => setFilter("search", searchInput);

  const handleClearSearch = () => {
    setSearchInput("");
    setFilter("search", "");
  };

  const paginationConfig: TablePaginationConfig = {
    current: pagination?.currentPage || filters.pageNumber || 1,
    pageSize: pagination?.pageSize || filters.pageSize || 10,
    total: pagination?.totalItems || pagination?.total || 0,
    showSizeChanger: true,
    pageSizeOptions: ["10", "20", "50", "100"],
    onChange: (page, pageSize) => setFilters({ pageNumber: page, pageSize }),
    showTotal: (total, range) => `${range[0]}-${range[1]} de ${total}`,
  };

  return {
    filters,
    series,
    paginationConfig,
    isLoading,
    isFetching,
    isError,
    isDeleting,
    searchInput,
    ability,
    handleCreate,
    handleDelete,
    setFilter,
    handleSearchInputChange,
    handleSearch,
    handleClearSearch,
    refetch,
  };
}
