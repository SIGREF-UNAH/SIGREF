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
          prefix: [],
          suffix: [],
        },
      ],
      gender: values.gender,
      birthDate: values.fechanacimiento
        ? new Date(values.fechanacimiento).toISOString()
        : undefined,
      active: values.estadoVital === 1,
      telecom: values.telecom?.map((item: any, index: number) => ({
        system: item.system,
        use:
          item.use === "Casa"
            ? 0
            : item.use === "Trabajo"
              ? 1
              : item.use === "Móvil"
                ? 2
                : 3,
        value: item.value || "",
        rank: index + 1,
      })),
      address:
        values.address?.map((addr: any, index: number) => ({
          use: 0,
          type: 0,
          text: addr.line?.[0] || "",
          line: addr.line || [],
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
                version: "2.9",
                code: "ID",
                display: values.tipoIdentificacion,
                userSelected: true,
              },
            ],
            text: values.tipoIdentificacion,
          },
          system: "https://example.com/identifiers",
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
        gender: patient.gender || 0,
        estadoVital: patient.active ? 1 : 0,
        fechanacimiento: patient.birthDate ? new Date(patient.birthDate) : null,
        tipoIdentificacion: patient.identifier?.[0]?.type?.text || "",
        identifier: [{ value: patient.identifier?.[0]?.value || "" }],
        telecom:
          patient.telecom?.map((t) => ({
            system: t.system,
            use:
              t.use === 0
                ? "Casa"
                : t.use === 1
                  ? "Trabajo"
                  : t.use === 2
                    ? "Mobile"
                    : "Otro",
            value: t.value,
          })) || [],
        address:
          patient.address?.map((a) => ({
            country: a.country || "",
            state: a.state || "",
            city: a.city || "",
            line: a.line || [""],
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
