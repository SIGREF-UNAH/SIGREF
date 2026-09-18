import { useState, useMemo } from "react";
import { useNavigate } from "react-router";
import { useUrlFilters } from "../../../shared/hooks";
import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import { createTablePagination } from "../../../shared/components/ui";
import {
  getGetServiceGroupListQueryKey,
  useDeleteServiceGroupById,
  useGetServiceGroupList,
} from "@endpoints/service-groups/service-groups";

export function useServiceGroupsList() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  // Estado local para el input de búsqueda
  const [searchInput, setSearchInput] = useState("");

  // Manejar filtros en la URL
  const { filters, setFilter, setFilters } = useUrlFilters({
    defaultValues: {
      search: "",
      location: undefined as string | undefined,
      status: undefined as string | undefined,
      pageNumber: 1,
      pageSize: 10,
    },
  });

  // Construir parámetros para la petición
  const queryParams = useMemo(() => {
    const params: any = {
      pageNumber: filters.pageNumber,
      pageSize: filters.pageSize,
    };

    if (filters.search) {
      params.title = filters.search;
    }

    if (filters.location) {
      params.location = filters.location;
    }

    if (filters.status) {
      params.status = filters.status;
    }

    return params;
  }, [filters]);

  // Obtener datos con filtros
  const { data: response, isLoading, isFetching, isError } = useGetServiceGroupList(queryParams, {
    query: {
      placeholderData: (previousData) => previousData,
    }
  });

  const serviceGroups = response?.items || [];
  const pagination = response?.pagination;

  // Procesar datos
  const processedServiceGroups = useMemo(() => {
    return serviceGroups.map((serviceGroup) => {
      const abbreviation = serviceGroup.code?.coding?.[0]?.code || "-";

      return {
        ...serviceGroup,
        abbreviation,
      };
    });
  }, [serviceGroups]);

  // Mutación para eliminar
  const { mutate: deleteServiceGroup } = useDeleteServiceGroupById({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetServiceGroupListQueryKey(),
        });
        msg.success("Paquete eliminado correctamente");
      },
      onError: () => msg.error("Error al eliminar el paquete"),
    },
  });

  const handleEdit = (id: string) => {
    navigate(`/service-groups/update/${id}`);
  };

  const handleDelete = (id: string) => {
    deleteServiceGroup({ id });
  };

  const handleSearchInputChange = (value: string) => {
    setSearchInput(value);
  };

  const handleSearch = () => {
    setFilter("search", searchInput);
  };

  const handleClearSearch = () => {
    setSearchInput("");
    setFilter("search", "");
  };

  const paginationConfig = createTablePagination({
    current: pagination?.currentPage || 1,
    pageSize: pagination?.pageSize || 10,
    pageSizeOptions: ["10", "20", "50", "100"],
    total: pagination?.totalItems || 0,
    onChange: (page, pageSize) => {
      setFilters({ pageNumber: page, pageSize });
    },
  });

  return {
    filters,
    serviceGroups: processedServiceGroups,
    paginationConfig,
    isLoading,
    isFetching,
    isError,
    searchInput,
    navigate,
    handleEdit,
    handleDelete,
    setFilter,
    handleSearchInputChange,
    handleSearch,
    handleClearSearch,
  };
}
