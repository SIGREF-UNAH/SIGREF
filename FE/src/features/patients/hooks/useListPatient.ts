import { useState, useMemo } from "react";
import { useGetApiPatients } from "../../../api/patients/patients";

export function useListPatients() {
  const { data: apiPatients } = useGetApiPatients<PatientApi[]>();
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const patients = useMemo(
    () =>
      apiPatients?.map((p: PatientApi, index: number) => ({
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
        nacionalidad: p.nationality || "-",
        genero:
          p.gender === 1
            ? "Masculino"
            : p.gender === 2
            ? "Femenino"
            : p.gender === 3
            ? "Otro"
            : "No especificado",
        estadoVital: p.active ? "vivo" : "sin vida",
      })) || [],
    [apiPatients]
  );

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
    currentPage,
    setCurrentPage,
    pageSize,
    setPageSize,
    getIdentificadorColor,
  };
}
