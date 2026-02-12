import { useState, useMemo } from "react";
import { useNavigate } from "react-router";
import { useAbility } from "../../../config";
import { useUrlFilters } from "../../../shared/hooks";
import { useGetApiSeries } from "../../../api/series/series";
import type { TablePaginationConfig } from "antd";

export type SeriesStatusFilter = "all" | "active" | "inactive";

export function useSeriesList() {
  const navigate = useNavigate();
  const ability = useAbility();

  const [searchInput, setSearchInput] = useState("");
  const [statusFilter, setStatusFilter] =
    useState<SeriesStatusFilter>("active"); 

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

 // Extraer datos
  const allSeries = response?.data?.items || [];

  // Filtrar series según el tab activo
  const series = useMemo(() => {
    if (statusFilter === "all") return allSeries;
    if (statusFilter === "active")
      return allSeries.filter((s) => s.isActive === true);
    if (statusFilter === "inactive")
      return allSeries.filter((s) => s.isActive === false);
    return allSeries;
  }, [allSeries, statusFilter]);

  const pagination = response?.data?.pagination || null;

  // Estadísticas
  const stats = useMemo(() => {
    const active = allSeries.filter((s) => s.isActive === true).length;
    const inactive = allSeries.filter((s) => s.isActive === false).length;
    return {
      active,
      inactive,
      total: allSeries.length,
    };
  }, [allSeries]);

  // Handlers
  const handleCreate = () => navigate("/series/create");

  const handleSearchInputChange = (value: string) => setSearchInput(value);

  const handleSearch = () => setFilter("search", searchInput);

  const handleClearSearch = () => {
    setSearchInput("");
    setFilter("search", "");
  };


  const handleStatusFilterChange = (status: SeriesStatusFilter) => {
    setStatusFilter(status);
    setFilters({ pageNumber: 1 }); // Reset a página 1 al cambiar filtro
  };

  const paginationConfig: TablePaginationConfig = {
    current: pagination?.currentPage || filters.pageNumber || 1,
    pageSize: pagination?.pageSize || filters.pageSize || 10,
    total: pagination?.totalItems || 0, 
    showSizeChanger: true,
    pageSizeOptions: ["10", "20", "50", "100"],
    onChange: (page, pageSize) => setFilters({ pageNumber: page, pageSize }),
    showTotal: (total, range) => `${range[0]}-${range[1]} de ${total}`,
  };

  return {
    // Estados
    filters,
    series, 
    allSeries, 
    stats, 
    statusFilter, 
    paginationConfig,
    isLoading,
    isFetching,
    isError,
    searchInput,
    ability,

    // Handlers
    handleCreate,
    handleStatusFilterChange, 
    setFilter,
    handleSearchInputChange,
    handleSearch,
    handleClearSearch,
    refetch,
  };
}
