import { useNavigate, useParams } from "react-router-dom";
import { message } from "antd";
import {
  useGetApiPatientsId,
  usePutApiPatientsId,
} from "../../../api/patients/patients";
import type { PatientDto } from "../../../api/models";
import { PatientExtensionsUrls } from "../../../shared/constants";

export const useEditPatient = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [messageApi, contextHolder] = message.useMessage();

  // Obtener paciente por ID
  const { data } = useGetApiPatientsId(id ?? "") as {
    data?: PatientDto;
    isLoading: boolean;
    error?: any;
  };
  const patient: PatientDto | undefined = Array.isArray(data) ? data[0] : data;
  // Mutación para actualizar
  const { mutate: updatePatient, isPending } = usePutApiPatientsId({
    mutation: {
      onSuccess: () => {
        messageApi.success("Paciente actualizado correctamente");
        setTimeout(() => {
          navigate("/patients/list");
        }, 600);
      },
      onError: (error) => {
        console.error(error);
        messageApi.error("Error al actualizar el paciente");
      },
    },
  });

  // Enviar datos al BE
  const handleFinish = async (values: any) => {
    const updatePatientDto = {
      name: [
        {
          use: values.tipoNombre,
          family: values.apellidos,
          given: [values.primerNombre, values.segundoNombre || ""].filter(
            Boolean
          ),
        },
      ],
      gender: values.gender,
      birthDate: values.fechanacimiento,
      active: values.estadoVital === 1,
      maritalStatus: {
        coding: [
          {
            system: "http://terminology.hl7.org/CodeSystem/v3-MaritalStatus",
            code: values.estadoCivilCodigo || "UNK",
            display: values.estadoCivil || "Desconocido",
          },
        ],
        text: values.estadoCivil || "Desconocido",
      },
      extension: [
        {
          url: PatientExtensionsUrls.nationality,
          valueString: values.nacionalidad || null,
        },
      ],

      telecom:
        values.telecom?.map((item: any, index: number) => ({
          system: item.system,
          use: item.use,
          value: item.value || "",
          rank: index + 1,
        })) || [],

      address:
        values.address?.map((addr: any, index: number) => ({
          use: addr.tipoDireccion || "casa",
          type: addr.type || 0,
          text: Array.isArray(addr.line) ? addr.line[0] : addr.line || "",
          line: Array.isArray(addr.line) ? addr.line : [addr.line || ""], 
          city: addr.city || "",
          district: addr.district || "",
          state: addr.state || "",
          postalCode: addr.postalCode || "",
          country: addr.country || "",
          rank: index + 1,
        })) || [],

      identifier: [
        {
          use: 0,
          type: {
            coding: [
              {
                system: "http://terminology.hl7.org/CodeSystem/v2-0203",
                code: values.tipoIdentificacion,
                display:
                  values.tipoIdentificacion === "PPN"
                    ? "Número de Pasaporte"
                    : values.tipoIdentificacion === "NI"
                      ? "Documento de Identificación"
                      : values.tipoIdentificacion === "DNI"
                        ? "Documento Nacional de Identidad"
                        : null,
                userSelected: true,
              },
            ],
            text: values.tipoIdentificacion,
          },
          system: values.emisor || null,
          value: values.identifier?.[0]?.value || null,
        },
      ],
    };

    updatePatient({
      id: id || "",
      data: updatePatientDto as any,
    });
  };

  // Cancelar
  const handleCancel = () => {
    messageApi.info("Operación cancelada");
    navigate("/patients/list");
  };

  // Valores iniciales
  const initialValues = patient
    ? {
        primerNombre: patient.name?.[0]?.given?.[0],
        segundoNombre: patient.name?.[0]?.given?.[1],
        apellidos: patient.name?.[0]?.family,
        tipoNombre: patient.name?.[0]?.use,
        gender: patient.gender,
        estadoVital: patient.active ? 1 : 0,
        estadoCivil:
          patient?.maritalStatus?.text ||
          patient?.maritalStatus?.coding?.[0]?.display,
        nacionalidad:
          patient?.extension?.find(
            (ext) =>
              ext.url === PatientExtensionsUrls.nationality
          )?.valueString,
        fechanacimiento: patient.birthDate ? new Date(patient.birthDate) : null,
        tipoIdentificacion: (() => {
          const code =
            patient.identifier?.[0]?.type?.coding?.[0]?.code || null;
          if (code === "DNI") return "DNI";
          if (code === "PPN") return "PPN";
          if (code === "NI") return "NI";
          return null;
        })(),
        identifier: [{ value: patient.identifier?.[0]?.value }],
        emisor: patient.identifier?.[0]?.system,
        telecom:
          patient.telecom?.map((t) => ({
            system: t.system,
            use: t.use,
            value: t.value,
          })) || [],
        address:
          patient.address?.map((a) => ({
            tipoDireccion: a.use,
            country: a.country,
            state: a.state,
            city: a.city,
            line: a.line?.[0],
            postalCode: a.postalCode,
          })) || [],
      }
    : {};

  return {
    id,
    data,
    contextHolder,
    isPending,
    handleFinish,
    handleCancel,
    initialValues,
    messageApi,
  };
};
