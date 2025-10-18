import { useState } from "react";
import { useNavigate } from "react-router";
import type { TablePaginationConfig } from "antd";
import { useUrlFilters } from "../../../shared/hooks";
import { useQueryClient } from "@tanstack/react-query";
import { useMessage } from "../../../shared/components";
import type { HealthcareDto } from "../../../api/models";
import {
  getGetApiHealthcaresQueryKey,
  useDeleteApiHealthcaresId,
  useGetApiHealthcares,
} from "../../../api/healthcares/healthcares";

export function useHealthcaresList() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  // Estado para el modal de detalles
  const [selectedHealthcare, setSelectedHealthcare] = useState<HealthcareDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  // Obtener datos de la API
  const { data: healthcares, isLoading, isError } = useGetApiHealthcares({});

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

  // Manejar todos los filtros en la URL
  const { filters, setFilter, setFilters } = useUrlFilters({
    defaultValues: {
      search: "",
      department: undefined as string | undefined,
      status: undefined as string | undefined,
      page: 1,
      pageSize: 10,
    },
  });

  // Filtrar datos basados en búsqueda, ubicación y estado
  const filteredData = (healthcares || []).filter((item) => {
    const matchesSearch =
      item.name?.toLowerCase().includes(filters.search.toLowerCase()) ||
      item.abbreviation?.toLowerCase().includes(filters.search.toLowerCase());

    const matchesDepartment =
      !filters.department ||
      item.location?.some((loc) => loc.display === filters.department);

    const matchesStatus =
      !filters.status ||
      (filters.status === "active" && item.active) ||
      (filters.status === "inactive" && !item.active);

    return matchesSearch && matchesDepartment && matchesStatus;
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

  // Configuración de paginación
  const paginationConfig: TablePaginationConfig = {
    current: filters.page,
    pageSize: filters.pageSize,
    showSizeChanger: true,
    pageSizeOptions: ["10", "20", "50", "100"],
    total: filteredData.length,
    onChange: (page, pageSize) => {
      setFilters({ page, pageSize });
    },
    showTotal: (total, range) => `${range[0]}-${range[1]} of ${total}`,
  };

  // Obtener las ubicaciones únicas para filtro
  const departments = Array.from(
    new Set(
      (healthcares || [])
        .flatMap((item) => item.location?.map((loc) => loc.display) || [])
        .filter(Boolean)
    )
  );

  return {
    filters,
    departments,
    filteredData,
    paginationConfig,
    isLoading,
    isError,
    selectedHealthcare,
    isModalOpen,
    handleCreate,
    handleEdit,
    handleDelete,
    setFilter,
    handleViewDetails,
    handleCloseModal,
  };
}
