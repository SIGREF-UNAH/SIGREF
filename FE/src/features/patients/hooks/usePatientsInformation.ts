import { useNavigate, useParams } from "react-router";
import { useMemo } from "react";
import { message } from "antd";
import {
  useDeleteApiPatientsId,
  useGetApiPatients,
  useGetApiPatientsId,
} from "../../../api/patients/patients";
import type { PatientDto } from "../../../api/models";
import { useUrlFilters } from "../../../shared/hooks";
import type { TablePaginationConfig } from "antd";

export type PaginationDto = {
  currentPage: number;
  pageSize: number;
  totalItems: number;
};

type PatientsResponse = {
  items: PatientDto[];
  pagination: PaginationDto;
};

export function usePatientsInformation() {
  const { id } = useParams();
  const [messageApi, contextHolder] = message.useMessage();
  const navigate = useNavigate();

  // Detalle del paciente
  const {
    data,
    isLoading: loadingPatientDetail,
    error,
  } = useGetApiPatientsId(id ?? "") as {
    data?: PatientDto;
    isLoading: boolean;
    error?: any;
  };

  // Filtros y paginación
  const { filters, setFilters, setFilter } = useUrlFilters({
    defaultValues: {
      search: "",
      pageNumber: 1,
      pageSize: 10,
      nombreCompleto: "",
      genero: "todos",
      estadoVital: "todos",
      tipoIdentificador: "",
      identificador: "",
      fechaNacimiento: "",
      nacionalidad: "",
    },
  });

  // Construcción de queryParams con filtros que el backend soporta
  const queryParams = useMemo(() => {
    const params: any = {
      pageNumber: filters.pageNumber,
      pageSize: filters.pageSize,
    };

    if (filters.nombreCompleto) params.name = filters.nombreCompleto;

    if (filters.genero && filters.genero !== "todos") {
      params.gender =
        filters.genero === "H" ? 1 : filters.genero === "M" ? 2 : undefined;
    }

    if (filters.estadoVital && filters.estadoVital !== "todos") {
      params.active =
        filters.estadoVital === "Vivo"
          ? true
          : filters.estadoVital === "Sin vida"
            ? false
            : undefined;
    }

    if (filters.tipoIdentificador && filters.tipoIdentificador !== "todos")
      params.IdentifierType = filters.tipoIdentificador;

    if (filters.identificador) params.IdentifierValue = filters.identificador;

    if (filters.fechaNacimiento) {
      let f = filters.fechaNacimiento;

      // Si es string en formato DD/MM/YYYY
      if (typeof f === "string" && /^\d{2}\/\d{2}\/\d{4}$/.test(f)) {
        const [day, month, year] = f.split("/");
        f = `${year}-${month.padStart(2, "0")}-${day.padStart(2, "0")}`;
      }

      // Asignar al queryParams
      params.BirthDate = f;
    }

    if (filters.search) params.search = filters.search;

    return params;
  }, [filters]);

  // Lista de pacientes con paginación
  const { data: response, isLoading: loadingPatients } =
    useGetApiPatients<PatientsResponse>(queryParams, {
      query: {
        placeholderData: (prev) => prev,
      },
    });

  const patient: PatientDto | undefined = Array.isArray(data) ? data[0] : data;

  // Mapeo de paciente actual
  const phone =
    data?.telecom?.find((t) => String(t.system).toLowerCase() === "phone")
      ?.value ?? "No registrado";

  const email =
    data?.telecom?.find((t) => String(t.system).toLowerCase() === "email")
      ?.value ?? "No registrado";

  const fax =
    data?.telecom?.find((t) => String(t.system).toLowerCase() === "fax")
      ?.value ?? "No registrado";

  const pager =
    data?.telecom?.find((t) => String(t.system).toLowerCase() === "pager")
      ?.value ?? "No registrado";

  const url =
    data?.telecom?.find((t) => String(t.system).toLowerCase() === "url")
      ?.value ?? "No registrado";

  const sms =
    data?.telecom?.find((t) => String(t.system).toLowerCase() === "sms")
      ?.value ?? "No registrado";

  const other = data?.telecom?.find(
    (t) => String(t.system).toLowerCase() === "other"
  );

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
          : patient?.gender === 1
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
      identificadores:
        patient?.identifier?.map((id) => ({
          tipo: id.type?.coding?.[0]?.display || id.type?.text || "Desconocido",
          valor: id.value || "No disponible",
          emisor: id.system || "Desconocido",
        })) || [],
      movil: phone,
      email,
      fax,
      pager,
      url,
      sms,
      other,
      casaDireccion: patient?.address?.[0]?.text ?? "No disponible",
      casaDetalles: `${patient?.address?.[0]?.city ?? ""}, ${
        patient?.address?.[0]?.country ?? ""
      }`,
      trabajoDireccion: patient?.address?.[1]?.text ?? "No registrada",
      trabajoDetalles: `${patient?.address?.[1]?.city ?? ""}, ${
        patient?.address?.[1]?.country ?? ""
      }`,
    };
  }, [data, phone, email]);

  // Mapeo de pacientes para tabla
  const patients = useMemo(() => {
    const items = Array.isArray(response) ? response : response?.items || [];
    return items.map((p: PatientDto, index: number) => ({
      id: p.id || String(index),
      key: p.id || String(index),
      nombre:
        p.name?.[0]?.text ??
        p.name?.[0]?.given?.join(" ") ??
        "Nombre no disponible",
      identificadorTipo: (() => {
        const code =
          p.identifier?.[0]?.type?.coding?.[0]?.code?.toUpperCase() ?? "DNI";
        if (code === "PPN") return "PST";
        if (code === "NI") return "ID";
        return "DNI";
      })(),
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
  }, [response]);

  // Configuración de paginación
  const paginationConfig: TablePaginationConfig = {
    current: response?.pagination?.currentPage || filters.pageNumber || 1,
    pageSize: response?.pagination?.pageSize || filters.pageSize || 10,
    total: response?.pagination?.totalItems || 0,
    showSizeChanger: true,
    pageSizeOptions: ["10", "20", "50"],
    onChange: (page, pageSize) => {
      setFilters({ pageNumber: page, pageSize });
    },
    showTotal: (total, range) => `${range[0]}-${range[1]} de ${total}`,
  };

  // Eliminar paciente
  const { mutate: deletePatient } = useDeleteApiPatientsId({
    mutation: {
      onSuccess: () => {
        messageApi.success("Paciente eliminado correctamente");
        setTimeout(() => {
          navigate("/patients/list");
        }, 500);
      },
      onError: (error) => {
        messageApi.error(
          "Error al eliminar paciente: " + (error?.message || "Desconocido")
        );
      },
    },
  });

  // Copiar datos del paciente
  const handleCopyData = () => {
    if (!selectedPatient) {
      messageApi.warning("No hay datos del paciente para copiar.");
      return;
    }

    const info = `
Nombre: ${selectedPatient.nombre} ${selectedPatient.apellidos}
Fecha de Nacimiento: ${selectedPatient.fechaNacimiento}
Edad: ${selectedPatient.edad}
Género: ${selectedPatient.genero}
Nacionalidad: ${selectedPatient.nacionalidad}
Estado Vital: ${selectedPatient.estadoVital}

${selectedPatient.identificadores
  .map((id) => `${id.tipo}: ${id.valor} (${id.emisor})`)
  .join("\n")}

Móvil: ${selectedPatient.movil}
Email: ${selectedPatient.email}

Dirección Casa: ${selectedPatient.casaDireccion}
Dirección Trabajo: ${selectedPatient.trabajoDireccion}
`.trim();

    navigator.clipboard.writeText(info);
    messageApi.success("Datos del paciente copiados al portapapeles.");
  };

  // Color para tipo de identificador
  const getIdentificadorColor = (tipo: string) => {
    switch (tipo) {
      case "DNI":
        return "blue";
      case "PPT":
        return "purple";
      case "NI":
        return "red";
      default:
        return "default";
    }
  };

  return {
    id,
    data,
    error,
    selectedPatient,
    patients,
    paginationConfig,
    filters,
    loadingPatients,
    messageApi,
    contextHolder,
    isLoading: loadingPatientDetail || loadingPatients,
    setFilter,
    setFilters,
    handleCopyData,
    deletePatient,
    getIdentificadorColor,
  };
}
