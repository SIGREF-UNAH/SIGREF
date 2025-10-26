import {  useNavigate, useParams } from "react-router";
import { useState, useMemo } from "react";
import { message } from "antd";
import {
  useDeleteApiPatientsId,
  useGetApiPatients,
  useGetApiPatientsId,
} from "../../../api/patients/patients";
import type { PatientDto } from "../../../api/models";

export function useDetailsPatient() {
  const { id } = useParams();
  const [messageApi, contextHolder] = message.useMessage();
   const navigate = useNavigate();

  const { data, isLoading, error } = useGetApiPatientsId(id ?? "") as {
    data?: PatientDto;
    isLoading: boolean;
    error?: any;
  };
  const { data: apiPatients } = useGetApiPatients();
  const patient: PatientDto | undefined = Array.isArray(data) ? data[0] : data;

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

  // eliminar pacientes
  const { mutate: deletePatient } = useDeleteApiPatientsId({
    mutation: {
      onSuccess: () => {
        messageApi.success("Paciente eliminado correctamente");
         navigate("/patients/list");
      },
      onError: (error) => {
        messageApi.error(
          "Error al eliminar paciente: " + (error?.message || "Desconocido")
        );
      },
    },
  });

  // Extrae teléfono y correo
  const phone =
    data?.telecom?.find((t) => String(t.system).toLowerCase() === "phone")
      ?.value ?? "No registrado";

  const email =
    data?.telecom?.find((t) => String(t.system).toLowerCase() === "email")
      ?.value ?? "No registrado";

  //  Datos del paciente seleccionado
  const selectedPatient = useMemo(() => {
    return {
      id: patient?.id ?? "",
      nombre: patient?.name?.[0]?.given?.join(" ") ?? "Desconocido",
      apellidos: patient?.name?.[0]?.family ?? "Desconocido",
      tipo: patient?.name?.[0]?.use,
      fechaNacimiento: patient?.birthDate
        ? new Date(patient.birthDate).toLocaleDateString("es-HN", {
            day: "2-digit",
            month: "long",
            year: "numeric",
          })
        : "No especificada",
      edad: patient?.birthDate
        ? `${Math.floor(
            (Date.now() - new Date(patient.birthDate).getTime()) /
              (365.25 * 24 * 60 * 60 * 1000)
          )} años`
        : "No especificada",

      genero:
        typeof patient?.gender === "string"
          ? patient.gender === "male"
            ? "Masculino"
            : patient.gender === "female"
              ? "Femenino"
              : patient.gender === "other"
                ? "Otro"
                : "Desconocido"
          : // si tu backend devuelve números aún:
            patient?.gender === 1
            ? "Masculino"
            : patient?.gender === 2
              ? "Femenino"
              : patient?.gender === 3
                ? "Otro"
                : "No especificado",
      estadoCivil:
        patient?.maritalStatus?.text ||
        patient?.maritalStatus?.coding?.[0]?.display ||
        "No registrado",
      nacionalidad:
        patient?.extension?.find(
          (ext) =>
            ext.url ===
            "http://hl7.org/fhir/StructureDefinition/patient-nationality"
        )?.valueCodeableConcept?.text || "No registrada",
      estadoVital: patient?.active ? "Con Vida" : "Sin Vida",
      dni: patient?.identifier?.[0]?.value ?? "No disponible",
      dniEmisor: patient?.identifier?.[0]?.system ?? "Desconocido",
      pasaporte: patient?.identifier?.[1]?.value ?? "No disponible",
      pasaporteEmisor: patient?.identifier?.[1]?.system ?? "Desconocido",
      movil: phone,
      email,
      casaDireccion: patient?.address?.[0]?.text ?? "No disponible",
      casaDetalles: `${patient?.address?.[0]?.city ?? ""}, ${patient?.address?.[0]?.country ?? ""}`,
      trabajoDireccion: patient?.address?.[1]?.text ?? "No registrada",
      trabajoDetalles: `${patient?.address?.[1]?.city ?? ""}, ${patient?.address?.[1]?.country ?? ""}`,
    };
  }, [data, phone, email]);

  // mapeo de pacientes para listado
  const patients = useMemo(() => {
    const items = Array.isArray(apiPatients)
      ? apiPatients
      : apiPatients?.items || [];
    return items.map((p: PatientDto, index: number) => ({
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
    }));
  }, [apiPatients]);

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
    deletePatient,
  };
}
