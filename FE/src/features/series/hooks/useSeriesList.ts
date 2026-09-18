import { useState, useMemo } from "react";
import { useNavigate } from "react-router";
import { useAbility } from "../../../config";
import { useUrlFilters } from "../../../shared/hooks";
import { useGetSerieList } from "@endpoints/series/series";
import type { SerieDto, GetSerieListParams } from "@models";
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

  const queryParams = useMemo((): GetSerieListParams => {
    const params: GetSerieListParams = {
      PageNumber: filters.pageNumber,
      PageSize: filters.pageSize,
    };
    if (filters.search) {
      params.Name = filters.search;
    }
    if (statusFilter !== "all") {
      params.IsActive = statusFilter === "active";
    }
    return params;
  }, [filters, statusFilter]);

  const {
    data: response,
    isLoading,
    isFetching,
    isError,
    refetch,
  } = useGetSerieList(queryParams, {
    query: {
      placeholderData: (prev) => prev,
    },
  });

  // Extraer datos
  const allSeries: SerieDto[] = response?.items || [];

  // Filtrar series según el tab activo (ahora el filtro se hace en la API)
  const series = useMemo(() => {
    return allSeries;
  }, [allSeries]);

  const pagination = response?.pagination || null;

  // Estadísticas (nota: ahora solo muestra las de la página actual si el filtro es por API)
  const stats = useMemo(() => {
    const active = allSeries.filter((s: SerieDto) => s.isActive === true).length;
    const inactive = allSeries.filter((s: SerieDto) => s.isActive === false).length;
    return {
      active,
      inactive,
      total: allSeries.length,
    };
  }, [allSeries]);

  // Handlers
  const handleCreate = () => navigate("/series/create");

  const handleSearchInputChange = (value: string) => setSearchInput(value);

  const handleSearch = () => {
    setFilter("search", searchInput);
    setFilters({ pageNumber: 1 });
  };

  const handleClearSearch = () => {
    setSearchInput("");
    setFilter("search", "");
    setFilters({ pageNumber: 1 });
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