import { useParams } from "react-router";
import { useState, useMemo } from "react";
import { message } from "antd";
import { useGetApiPatients, useGetApiPatientsId } from "../../../api/patients/patients";

export function useDetailsPatient() {
  const { id } = useParams();
  const [messageApi, contextHolder] = message.useMessage();

  const { data, isLoading, error } = useGetApiPatientsId(id);
  const { data: apiPatients } = useGetApiPatients<PatientApi[]>();

  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const handleCopyData = () => {
    if (!data) {
      messageApi.warning("No hay datos del paciente para copiar.");
      return;
    }

    // Texto que se copia del paciente
    const info = `
   Nombre: ${selectedPatient.nombre} ${selectedPatient.apellidos}
   Fecha de Nacimiento: ${selectedPatient.fechaNacimiento}
   Edad: ${selectedPatient.edad}
   Género: ${selectedPatient.genero}
   Nacionalidad: ${selectedPatient.nacionalidad}
   Estado Vital: ${selectedPatient.estadoVital}

   DNI: ${selectedPatient.dni} (${selectedPatient.dniEmisor})
   Pasaporte: ${selectedPatient.pasaporte} (${selectedPatient.pasaporteEmisor})

   Móvil: ${selectedPatient.movil}
   Email: ${selectedPatient.email}

   Dirección Casa: ${selectedPatient.casaDireccion}
   Dirección Trabajo: ${selectedPatient.trabajoDireccion}
  `.trim();

    navigator.clipboard.writeText(info);
    messageApi.success("Datos del paciente copiados al portapapeles.");
  };


  // Extrae teléfono y correo
  const phone =
    data?.telecom?.find((t) => t.system?.toLowerCase() === "phone")?.value ??
    "No registrado";
  const email =
    data?.telecom?.find((t) => t.system?.toLowerCase() === "email")?.value ??
    "No registrado";

  //  Datos del paciente seleccionado
  const selectedPatient = useMemo(
    () => ({
      nombre: data?.name?.[0]?.given?.join(" ") ?? "Desconocido",
      apellidos: data?.name?.[0]?.family ?? "Desconocido",
      fechaNacimiento: data?.birthDate
        ? new Date(data.birthDate).toLocaleDateString("es-HN", {
            day: "2-digit",
            month: "long",
            year: "numeric",
          })
        : "No especificada",
      edad: data?.birthDate
        ? `${Math.floor(
            (Date.now() - new Date(data.birthDate).getTime()) /
              (365.25 * 24 * 60 * 60 * 1000)
          )} años`
        : "No especificada",
      genero:
        data?.gender === 1
          ? "Masculino"
          : data?.gender === 2
          ? "Femenino"
          : "No especificado",
      nacionalidad: data?.address?.[0]?.country ?? "No registrada",
      estadoVital: data?.active ? "Con Vida" : "Sin Vida",
      dni: data?.identifier?.[0]?.value ?? "No disponible",
      dniEmisor: data?.identifier?.[0]?.system ?? "Desconocido",
      pasaporte: data?.identifier?.[1]?.value ?? "No disponible",
      pasaporteEmisor: data?.identifier?.[1]?.system ?? "Desconocido",
      movil: phone,
      preferido: "Preferido",
      email: email,
      casaDireccion: data?.address?.[0]?.text ?? "No disponible",
      casaDetalles: `${data?.address?.[0]?.city ?? ""}, ${
        data?.address?.[0]?.country ?? ""
      }`,
      trabajoDireccion: data?.address?.[1]?.text ?? "No registrada",
      trabajoDetalles: `${data?.address?.[1]?.city ?? ""}, ${
        data?.address?.[1]?.country ?? ""
      }`,
    }),
    [data, phone, email]
  );

  // mapeo de pacientes para listado
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

  return {
    id,
    data,
    isLoading,
    error,
    selectedPatient,
    patients,
    currentPage,
    setCurrentPage,
    pageSize,
    setPageSize,
    messageApi,
    contextHolder,
    handleCopyData,
  };
}
