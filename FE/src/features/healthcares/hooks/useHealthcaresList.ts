import { useState, useCallback, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/hooks";
import { createTablePagination } from "../../../shared/components/ui";
import { useAbility } from "../../../config";
import type {
  HealthcareDto,
  GetHealtcareListParams,
} from "@models";
import type { HealthcareScope } from "../../../api/generated/schemas/types/healthcare-services/healthcareScope";
import {
  getGetHealtcareListQueryKey,
  useGetHealtcareList,
  useDeleteHealtcareById,
} from "@endpoints/healthcare-services/healthcare-services";
import { useGetLocationList } from "@endpoints/locations/locations";

interface FiltersState {
  location?: string;
  scope?: HealthcareScope;
  active?: boolean;
  includeCost: boolean;
  name?: string;
}

export function useHealthcaresList() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();
  const ability = useAbility();

  // Estados locales
  const [filters, setFilters] = useState<FiltersState>({
    includeCost: false,
  });
  const [searchInput, setSearchInput] = useState("");
  const [selectedHealthcare, setSelectedHealthcare] =
    useState<HealthcareDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  // Paginación
  const [pagination, setPagination] = useState({
    current: 1,
    pageSize: 20,
  });

  // Obtener ubicaciones para el filtro
  const { data: locationsData } = useGetLocationList();
  const locations = locationsData?.items || [];

  // Construir parámetros para la API con los nombres correctos
  const buildParams = useCallback((): GetHealtcareListParams => {
    const params: GetHealtcareListParams = {
      PageNumber: pagination.current,
      PageSize: pagination.pageSize,
    };

    // Solo agregar filtros que tengan valor
    if (filters.name?.trim()) {
      params.Name = filters.name.trim();
    }
    if (filters.location) {
      params.Location = filters.location;
    }
    if (filters.scope) {
      params.Scope = filters.scope;
    }
    if (filters.active !== undefined && filters.active !== null) {
      params.Active = filters.active;
    }
    if (filters.includeCost) {
      params.IncludeCost = filters.includeCost;
    }

    return params;
  }, [filters, pagination]);

  // Consulta principal
  const {
    data: healthcaresData,
    isLoading,
    isFetching,
    isError,
  } = useGetHealtcareList(buildParams(), {
    query: {
      placeholderData: (prev) => prev,
    },
  });

  const healthcares = healthcaresData?.items || [];
  const paginationData = healthcaresData?.pagination;

  // Configuración de paginación para Ant Design Table
  const paginationConfig = useMemo(
    () => createTablePagination({
      current: paginationData?.currentPage || pagination.current,
      pageSize: paginationData?.pageSize || pagination.pageSize,
      total: paginationData?.totalItems || 0,
      showTotal: (total: number) => `Total ${total} servicios`,
      pageSizeOptions: ["10", "20", "50", "100"],
      onChange: (page: number, pageSize: number) => {
        setPagination({ current: page, pageSize });
      },
      showQuickJumper: true,
    }),
    [paginationData, pagination]
  );

  // Mutación para eliminar
  const { mutateAsync: deleteHealthcare } = useDeleteHealtcareById({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetHealtcareListQueryKey(),
        });
        msg.success("Servicio médico eliminado correctamente");
      },
      onError: (error: any) => {
        const errorMessage =
          error?.response?.data?.detail ||
          error?.response?.data?.title ||
          "Error al eliminar el servicio médico";
        msg.error(errorMessage);
      },
    },
  });

  // Handlers
  const handleEdit = (id: string) => {
    navigate(`/healthcares/edit/${id}`);
  };

  const handleDelete = async (id: string) => {
    await deleteHealthcare({ id });
  };

  const handleViewDetails = (record: HealthcareDto) => {
    setSelectedHealthcare(record);
    setIsModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedHealthcare(null);
  };

  const setFilter = (key: keyof FiltersState, value: any) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
    setPagination((prev) => ({ ...prev, current: 1 })); // Reset a primera página
  };

  const handleSearchInputChange = (value: string) => {
    setSearchInput(value);
  };

  const handleSearch = (value: string) => {
    setFilters((prev) => ({ ...prev, name: value }));
    setPagination((prev) => ({ ...prev, current: 1 }));
  };

  const handleClearSearch = () => {
    setSearchInput("");
    setFilters((prev) => ({ ...prev, name: undefined }));
    setPagination((prev) => ({ ...prev, current: 1 }));
  };

  return {
    filters,
    locations,
    healthcares,
    paginationConfig,
    isLoading,
    isFetching,
    isError,
    selectedHealthcare,
    isModalOpen,
    searchInput,
    ability,
    handleEdit,
    handleDelete,
    handleViewDetails,
    setFilter,
    handleCloseModal,
    handleSearchInputChange,
    handleSearch,
    handleClearSearch,
  };
}
