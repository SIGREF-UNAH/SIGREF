import { useNavigate, useParams } from "react-router-dom";
import { message } from "antd";
import {
  useGetApiPatientsId,
  usePutApiPatientsId,
} from "../../../api/patients/patients";
import type { PatientDto } from "../../../api/models";

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
          use: values.tipoNombre === "legal" ? 0 : 1,
          text: `${values.primerNombre} ${values.apellidos}`,
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
          url: "http://hl7.org/fhir/StructureDefinition/patient-nationality",
          valueCodeableConcept: {
            coding: [
              {
                system: "urn:iso:std:iso:3166",
                code: values.nacionalidadCodigo || "HN",
                display: values.nacionalidad || "Honduras",
              },
            ],
            text: values.nacionalidad || "Honduras",
          },
        },
      ],

      telecom:
        values.telecom?.map((item: any, index: number) => ({
          system: item.system,
          use: item.use,
          value: item.value || "",
          rank: index + 1,
          codigoPais: item.codigoPais || "+504",
          preferido: item.preferido ?? false,
        })) || [],

      address:
        values.address?.map((addr: any, index: number) => ({
          use: addr.tipoDireccion || "casa",
          type: 0,
          text: addr.line || "",
          line: addr.line ? [addr.line] : [],
          city: addr.city || "",
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
                version: "2.9",
                code: "ID",
                display: values.tipoIdentificacion,
                userSelected: true,
              },
            ],
            text: values.tipoIdentificacion,
          },
          system: values.emisor || "",
          value: values.identifier?.[0]?.value || "",
        },
      ],
    };

    console.log("DTO a enviar al backend:", updatePatientDto);

    updatePatient({
      id: id || "",
      data: updatePatientDto,
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
        primerNombre: patient.name?.[0]?.given?.[0] || "",
        segundoNombre: patient.name?.[0]?.given?.[1] || "",
        apellidos: patient.name?.[0]?.family || "",
        tipoNombre: patient.name?.[0]?.use === 0 ? "alias" : "legal",
        gender: patient.gender || 0,
        estadoVital: patient.active ? 1 : 0,
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
        fechanacimiento: patient.birthDate ? new Date(patient.birthDate) : null,
        tipoIdentificacion: patient.identifier?.[0]?.type?.text || "",
        identifier: [{ value: patient.identifier?.[0]?.value || "" }],
        emisor: patient.identifier?.[0]?.system || "No registrado",
        telecom:
          patient.telecom?.map((t) => ({
            system: t.system || "phone",
            use: t.use === 0 ? 0 : t.use === 1 ? 1 : 2,
            value: t.value || "",
          })) || [],
        address:
          patient.address?.map((a) => ({
            tipoDireccion: a.use || "home",
            country: a.country || "",
            state: a.state || "",
            city: a.city || "",
            line: a.line?.[0] || "",
            postalCode: a.postalCode || "",
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
