import { useState, useMemo } from "react";
import { useNavigate } from "react-router";
import { useUrlFilters, useMessage } from "../../../shared/hooks";
import { useQueryClient } from "@tanstack/react-query";
import type { TablePaginationConfig } from "antd";
import type { OrganizationDto } from "@models";
import {
  getGetOrganizationListQueryKey,
  useGetOrganizationList,
  useDeleteOrganizationById,
} from "@endpoints/organizations/organizations";

export function useOrganizationsList() {
  const [selectedOrganization, setSelectedOrganization] = useState<OrganizationDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const queryClient = useQueryClient();
  const msg = useMessage();
  const navigate = useNavigate();

  // Estado para búsqueda local
  const [searchInput, setSearchInput] = useState("");

  // Filtros desde URL
  const { filters, setFilter, setFilters } = useUrlFilters({
    defaultValues: {
      search: "",
      pageNumber: 1,
      pageSize: 10,
      status: undefined as string | undefined,
      type: undefined as string | undefined,
    },
  });

  // Parámetros para el backend
  const queryParams = useMemo(() => {
    const params: any = {
      pageNumber: filters.pageNumber,
      pageSize: filters.pageSize,
    };

    if (filters.search) params.name = filters.search;

    // Filtro por estado
    if (filters.status === "active") {
      params.active = true;
    } else if (filters.status === "inactive") {
      params.active = false;
    }

    // Filtro por tipo de organización
    if (filters.type) {
      params.type = filters.type; 
    }

    return params;
  }, [filters]);

  // Obtener organizaciones
  const {
    data: response,
    isLoading,
    isFetching,
    isError,
  } = useGetOrganizationList(queryParams, {
    query: { placeholderData: (prev) => prev },
  });

  // Datos
  const organizations = response?.items || [];
  const pagination = response?.pagination;

  // Eliminar organización
  const { mutate: deleteOrganization } = useDeleteOrganizationById({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetOrganizationListQueryKey(),
        });
        msg.success("Organización eliminada correctamente");
      },
      onError: () => msg.error("Error al eliminar la organización"),
    },
  });

  // Ver detalles
  const handleViewDetails = (organization: OrganizationDto) => {
    setSelectedOrganization(organization);
    setIsModalOpen(true);
  };

  // Editar
  const handleEdit = (id: string) => navigate(`/organizations/update/${id}`);

  // Eliminar
  const handleDelete = (id: string) => deleteOrganization({ id });

  // Búsqueda
  const handleSearchInputChange = (value: string) => setSearchInput(value);
  const handleSearch = () => setFilter("search", searchInput);
  const handleClearSearch = () => {
    setSearchInput("");
    setFilter("search", "");
  };
  const handleTypeChange = (value: string) => setFilter("type", value);

  // Manejo de filtro estado
  const handleStatusChange = (value: string) => setFilter("status", value);

  // Paginación
  const paginationConfig: TablePaginationConfig = {
    current: pagination?.currentPage || 1,
    pageSize: pagination?.pageSize || 10,
    showSizeChanger: true,
    total: pagination?.totalItems || 0,
    onChange: (page, pageSize) => setFilters({ pageNumber: page, pageSize }),
    showTotal: (total, range) => `${range[0]}-${range[1]} de ${total}`,
  };

  return {
    filters,
    organizations,
    paginationConfig,
    isLoading,
    isFetching,
    isError,
    searchInput,
    selectedOrganization,
    isModalOpen,
    setIsModalOpen,
    handleEdit,
    handleViewDetails,
    handleDelete,
    handleSearchInputChange,
    handleSearch,
    handleClearSearch,
    handleTypeChange,
    handleStatusChange,
  };
}