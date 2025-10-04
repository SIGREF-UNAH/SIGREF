import type { TablePaginationConfig } from "antd";
import { useUrlFilters } from "../../../shared/hooks";
import { mockData } from "../store";
import { useNavigate } from "react-router";

export function useHealthcares() {
  const navigate = useNavigate();
    
  // Manejar todos los filtros en la URL
  const { filters, setFilter, setFilters } = useUrlFilters({
    defaultValues: {
      search: "",
      department: undefined as string | undefined,
      page: 1,
      pageSize: 10,
    },
  });

  // Filtrar datos basados en búsqueda y ubicación
  const filteredData = mockData.filter((item) => {
    const matchesSearch =
      item.name.toLowerCase().includes(filters.search.toLowerCase()) ||
      item.abbreviation.toLowerCase().includes(filters.search.toLowerCase());

    const matchesDepartment =
      !filters.department ||
      item.location?.some((loc) => loc.display === filters.department);

    return matchesSearch && matchesDepartment;
  });

  // Crear nuevo servicio
  const handleCreate = () => {
    navigate("/healthcares/create");
  };

  // Editar
  const handleEdit = (id: string) => {
    navigate(`/healthcares/update/${id}`);
  };

  // Eliminar
  const handleDelete = (id: string) => {
    console.log("Delete healthcare:", id);
    // TODO: Abrir modal de confirmación
  };

  // Configuración de paginación
  const paginationConfig: TablePaginationConfig = {
    current: filters.page,
    pageSize: filters.pageSize,
    showSizeChanger: true,
    pageSizeOptions: ["10", "20", "50", "100"],
    onChange: (page, pageSize) => {
      setFilters({ page, pageSize });
    },
    showTotal: (total, range) => `${range[0]}-${range[1]} of ${total}`,
  };

  // Obtener las ubicaciones únicas para filtro
  const departments = Array.from(
    new Set(
      mockData
        .flatMap((item) => item.location?.map((loc) => loc.display) || [])
        .filter(Boolean)
    )
  );

  return {
    filters,
    departments,
    filteredData,
    paginationConfig,
    handleCreate,
    handleEdit,
    handleDelete,
    setFilter,
  };
}
