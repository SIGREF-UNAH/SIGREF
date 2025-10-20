import { useNavigate, useParams } from "react-router-dom";
import { message } from "antd";
import { useGetApiPatientsId, usePutApiPatientsId } from "../../../api/patients/patients"

export const useEditPatient = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [messageApi, contextHolder] = message.useMessage();

  // Obtener paciente por ID
  const { data } = useGetApiPatientsId(id);

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
          given: [values.primerNombre, values.segundoNombre || ""].filter(Boolean),
          prefix: [],
          suffix: [],
        },
      ],
      gender: Number(values.gender),
      birthDate: values.fechanacimiento,
      active: values.estadoVital === 1,
      telecom: values.telecom?.map((item: any, index: number) => ({
        system: item.system,
        use: item.use,
        value: item.value || "",
        rank: index + 1,
      })),
      address: [
        {
          use: 0,
          type: 0,
          text: values.address?.[0]?.line?.[0] || "",
          line: [values.address?.[0]?.line?.[0] || ""],
          city: values.address?.[0]?.city || "",
          district: "",
          state: values.address?.[0]?.state || "",
          postalCode: "",
          country: values.address?.[0]?.country || "",
        },
      ],
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
  const initialValues = data
    ? {
        primerNombre: data.name?.[0]?.given?.[0] || "",
        segundoNombre: data.name?.[0]?.given?.[1] || "",
        apellidos: data.name?.[0]?.family || "",
        gender: data.gender || 0,
        estadoVital: data.active ? 1 : 0,
        fechanacimiento: data.birthDate ? new Date(data.birthDate) : null,
        tipoIdentificacion: data.identifier?.[0]?.type?.text || "",
        identifier: [{ value: data.identifier?.[0]?.value || "" }],
        telecom:
          data.telecom?.map((t) => ({
            system: t.system === "Phone" ? 0 : 1,
            use: t.use?.toLowerCase() || "home",
            value: t.value,
          })) || [],
        address:
          data.address?.map((a) => ({
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
