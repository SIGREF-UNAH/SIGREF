import { useState, useMemo } from "react";
import type { TablePaginationConfig } from "antd";
import type { PatientDto } from "../../../api/models";
import { useGetApiPatients } from "../../../api/patients/patients";

export function useListPatients() {
  // Estado para la paginación
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  // Hook generado por Orval para obtener los pacientes
  const { data: apiPatients, isLoading, isError } = useGetApiPatients({});

  // Mapeo de datos para la tabla
  const patients = useMemo(
    () =>
      (apiPatients || []).map((p: PatientDto, index: number) => ({
        id: p.id || String(index),
        key: p.id || String(index),
        nombre:
          p.name?.[0]?.text ??
          p.name?.[0]?.given?.join(" ") ??
          "Nombre no disponible",
        identificadorTipo: p.identifier?.[0]?.type?.text ?? "DNI",
        identificador: p.identifier?.[0]?.value || "-",
        contacto: p.telecom?.[0]?.value || "-",
        nacimiento: p.birthDate
          ? new Date(p.birthDate).toLocaleDateString()
          : "-",
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
      })),
    [apiPatients]
  );

  // Configuración de paginación para usar en la tabla
  const paginationConfig: TablePaginationConfig = {
    current: currentPage,
    pageSize,
    total: patients.length,
    showSizeChanger: true,
    pageSizeOptions: ["10", "20", "50", "100"],
    onChange: (page, size) => {
      setCurrentPage(page);
      setPageSize(size);
    },
    showTotal: (total, range) => `${range[0]}-${range[1]} de ${total}`,
  };

  // Color para el tipo de identificador
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
    patients,
    isLoading,
    isError,
    currentPage,
    setCurrentPage,
    pageSize,
    setPageSize,
    paginationConfig,
    getIdentificadorColor,
  };
}
