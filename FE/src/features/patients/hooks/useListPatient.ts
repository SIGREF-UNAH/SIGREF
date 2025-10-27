import { useState, useMemo } from "react";
import { useUrlFilters } from "../../../shared/hooks";
import { useGetApiPatients } from "../../../api/patients/patients";
import type { TablePaginationConfig } from "antd";
import type { PatientDto } from "../../../api/models";

export type PatientFilters = {
  search: string;
  pageNumber: number;
  pageSize: number;
  nombreCompleto?: string;
  tipoIdentificador?: string;
  identificador?: string;
  genero?: string;
  nacionalidad?: string;
  estadoVital?: string;
  fechaNacimiento?: any;
  tipoContacto?: string;
  contacto?: string;
};

export type PaginationDto = {
  currentPage: number;
  pageSize: number;
  totalItems: number;
};

export type PatientsResponse = {
  items: PatientDto[];
  pagination: PaginationDto;
};

export function useListPatients() {
  // Estado local para búsqueda antes de aplicar filtro
  const [searchInput, setSearchInput] = useState("");

  // Filtros manejados desde URL
  const { filters, setFilter, setFilters } = useUrlFilters<PatientFilters>({
    defaultValues: {
      search: "",
      pageNumber: 1,
      pageSize: 10,
      nombreCompleto: "",
      tipoIdentificador: "todos",
      identificador: "",
      genero: "todos",
      nacionalidad: "",
      estadoVital: "todos",
      fechaNacimiento: undefined,
      tipoContacto: "todos",
      contacto: "",
    },
  });

  // Construir parámetros de la API
  const queryParams = useMemo(() => {
    const params: any = {
      pageNumber: filters.pageNumber,
      pageSize: filters.pageSize,
    };

  if (filters.nombreCompleto) params.name = filters.nombreCompleto;
 

  if (filters.genero && filters.genero !== "todos") {
    params.gender =
      filters.genero === "D" ? 0 : filters.genero === "H" ? 1 : filters.genero === "M" ? 2 : undefined;
  }

  if (filters.estadoVital && filters.estadoVital !== "todos") {
    params.active =
      filters.estadoVital === "Vivo"
        ? true
        : filters.estadoVital === "Sin vida"
        ? false
        : undefined;
  }
   
  return params;
}, [filters]);

  // Llamada a la API
  const { data: response, isLoading, isFetching, isError } = useGetApiPatients<PatientsResponse>(
    queryParams,
    { query: { placeholderData: (prev) => prev } }
  );

  // Datos de la tabla
  const patients = useMemo(() => {
    const items = response?.items || [];
    return items.map((p: PatientDto, index: number) => ({
      id: p.id || String(index),
      key: p.id || String(index),
      nombre: p.name?.[0]?.text ?? p.name?.[0]?.given?.join(" ") ?? "Nombre no disponible",
      identificadorTipo: p.identifier?.[0]?.type?.text ?? "DNI",
      identificador: p.identifier?.[0]?.value || "-",
      contacto: p.telecom?.[0]?.value || "-",
      nacimiento: p.birthDate ? new Date(p.birthDate).toLocaleDateString() : "-",
      nacionalidad: p.address?.[0]?.country || "-",
      genero:
        p.gender === 1
          ? "Masculino"
          : p.gender === 2
          ? "Femenino"
          : p.gender === 3
          ? "Otro"
          : "No especificado",
      estadoVital: p.active ? "Vivo" : "Sin vida",
    }));
  }, [response]);

  // Paginación
  const paginationConfig: TablePaginationConfig = {
    current: response?.pagination?.currentPage || filters.pageNumber || 1,
    pageSize: response?.pagination?.pageSize || filters.pageSize || 10,
    total: response?.pagination?.totalItems || 0,
    showSizeChanger: true,
    pageSizeOptions: ["10", "20", "50"],
    onChange: (page, pageSize) => setFilters({ pageNumber: page, pageSize }),
    showTotal: (total, range) => `${range[0]}-${range[1]} de ${total}`,
  };

  // Funciones de búsqueda
  const handleSearchInputChange = (value: string) => setSearchInput(value);
  const handleSearch = () => setFilter("search", searchInput);
  const handleClearSearch = () => {
    setSearchInput("");
    setFilter("search", "");
  };


  const getIdentificadorColor = (tipo: string) => {
    switch (tipo) {
      case "DNI":
        return "blue";
      case "PST":
        return "purple";
      case "ID":
        return "red";
      default:
        return "default";
    }
  };

  return {
    filters,
    patients,
    paginationConfig,
    isLoading,
    isFetching,
    isError,
    searchInput,
    setFilter,
    setFilters,
    handleSearchInputChange,
    handleSearch,
    handleClearSearch,
    getIdentificadorColor,
  };
}
