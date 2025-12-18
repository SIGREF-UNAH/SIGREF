import { useState, useMemo } from "react";
import { useNavigate } from "react-router";
import { useQueryClient } from "@tanstack/react-query";
import { useUrlFilters } from "../../../shared/hooks";
import { useMessage } from "../../../shared/hooks";
import { useAbility } from "../../../config";
import { useGetApiLocations } from "../../../api/locations/locations";
import type { TablePaginationConfig } from "antd";
import type { HealthcareDto } from "../../../api/models";
import {
  getGetApiHealthcaresQueryKey,
  useDeleteApiHealthcaresId,
  useGetApiHealthcares,
} from "../../../api/healthcares/healthcares";

export function useHealthcaresList() {
  const navigate = useNavigate();
  const ability = useAbility();
  const queryClient = useQueryClient();
  const msg = useMessage();

  // Estado para el modal de detalles
  const [selectedHealthcare, setSelectedHealthcare] = useState<HealthcareDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  // Estado local para el input de búsqueda (antes de aplicar el filtro)
  const [searchInput, setSearchInput] = useState("");

  // Manejar todos los filtros en la URL
  const { filters, setFilter, setFilters } = useUrlFilters({
    defaultValues: {
      search: "",
      location: undefined as string | undefined,
      status: undefined as string | undefined,
      scope: undefined as string | undefined,
      includeCost: false,
      pageNumber: 1,
      pageSize: 10,
    },
  });

  // Construir parámetros para la petición al backend
  const queryParams = useMemo(() => {
    const params: any = {
      pageNumber: filters.pageNumber,
      pageSize: filters.pageSize,
    };

    // Filtro de búsqueda por nombre
    if (filters.search) {
      params.name = filters.search;
    }

    // Filtro por ubicación
    if (filters.location) {
      params.location = filters.location;
    }

    // Filtro por estado
    if (filters.status === "active") {
      params.active = true;
    } else if (filters.status === "inactive") {
      params.active = false;
    }

    // Filtro por tipo
    if (filters.scope) {
      params.scope = filters.scope;
    }

    // Filtro por costo
    if (filters.includeCost) {
      params.includeCost = filters.includeCost;
    }

    return params;
  }, [filters]);

  // Obtener datos de la API con filtros
  const { data: response, isLoading, isFetching, isError } = useGetApiHealthcares(queryParams, {
    query: {
      placeholderData: (previousData) => previousData, // Mantener datos previos mientras se cargan los nuevos
    }
  });

  // Obtener todas las ubicaciones para el filtro
  const { data: locationsData, isLoading: isLoadingLocations } = useGetApiLocations();

  // Extraer datos de la respuesta
  const healthcares = (response as any)?.data?.items || [];
  const pagination = (response as any)?.data?.pagination;

  // Procesar los datos - ahora abbreviation, scope y cost vienen directamente
  const processedHealthcares = useMemo(() => {
    return healthcares.map((healthcare : HealthcareDto) => {
      return {
        ...healthcare,
      };
    });
  }, [healthcares]);

  // Mutación para eliminar
  const { mutate: deleteHealthcare } = useDeleteApiHealthcaresId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiHealthcaresQueryKey(),
        });
        msg.success("Servicio médico eliminado correctamente");
      },
      onError: () => msg.error("Error al eliminar el servicio médico"),
    },
  });

  // Crear
  const handleCreate = () => {
    navigate("/healthcares/create");
  };

  // Editar
  const handleEdit = (id: string) => {
    navigate(`/healthcares/update/${id}`);
  };

  // Eliminar
  const handleDelete = (id: string) => {
    deleteHealthcare({ id });
  };

  // Ver detalles
  const handleViewDetails = (healthcare: HealthcareDto) => {
    setSelectedHealthcare(healthcare);
    setIsModalOpen(true);
  };

  // Cerrar modal
  const handleCloseModal = () => {
    setIsModalOpen(false);
    setSelectedHealthcare(null);
  };

  // Manejar cambio en el input de búsqueda
  const handleSearchInputChange = (value: string) => {
    setSearchInput(value);
  };

  // Aplicar búsqueda al presionar el botón
  const handleSearch = () => {
    setFilter("search", searchInput);
  };

  // Limpiar búsqueda al presionar el icono X
  const handleClearSearch = () => {
    setSearchInput("");
    setFilter("search", "");
  };

  // Configuración de paginación con datos del backend
  const paginationConfig: TablePaginationConfig = {
    current: pagination?.currentPage || 1,
    pageSize: pagination?.pageSize || 10,
    showSizeChanger: true,
    pageSizeOptions: ["10", "20", "50", "100"],
    total: pagination?.totalItems || 0,
    onChange: (page, pageSize) => {
      setFilters({ pageNumber: page, pageSize });
    },
    showTotal: (total, range) => `${range[0]}-${range[1]} de ${total}`,
  };

  // Obtener las ubicaciones para filtro desde el endpoint de locations
  const locations = locationsData?.items || [];

  return {
    filters,
    locations,
    isLoadingLocations,
    healthcares: processedHealthcares,
    paginationConfig,
    isLoading,
    isFetching,
    isError,
    selectedHealthcare,
    isModalOpen,
    searchInput,
    ability,
    handleCreate,
    handleEdit,
    handleDelete,
    setFilter,
    handleViewDetails,
    handleCloseModal,
    handleSearchInputChange,
    handleSearch,
    handleClearSearch,
  };
}