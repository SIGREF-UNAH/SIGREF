import { message } from "antd";
import { useNavigate } from "react-router-dom";
import { usePostApiPatients } from "../../../api/patients/patients";
import type { CreatePatientDto } from "../../../api/models";

export function useCreatePatientForm() {
  const [messageApi, contextHolder] = message.useMessage();
  const navigate = useNavigate();

  const mutation = usePostApiPatients({
    mutation: {
      onSuccess: () => {
        messageApi.success("Paciente guardado exitosamente");
        navigate("/patients/list");
      },
      onError: (error: any) => {
        console.error("Error al crear paciente:", error);
        messageApi.error("Error al guardar el paciente. Intente de nuevo.");
      },
    },
  });

  const handleFinish = async (formValues: any) => {
    try {
      console.log("Valores del formulario:", formValues);

      const payload: CreatePatientDto = {
        active: true,
        gender: formValues.genero,
        birthDate: formValues.fechanacimiento,
        maritalStatus: formValues.estadoCivil,
        name: [
          {
            use: formValues.tipoNombre,
            given: [formValues.primerNombre, formValues.segundoNombre].filter(Boolean),
            family: formValues.apellidos,
            period: {
              start: formValues.fechaInicioNombre,
              end: formValues.fechaExpiracionNombre,
            },
          },
        ],
        telecom: [
          {
            system: formValues.tipoContacto,
            value: formValues.valor,
            use: formValues.contactoPreferido ? "preferred" : "home",
            period: {
              start: formValues.fechaInicioContacto,
              end: formValues.fechaExpiracionContacto,
            },
          },
        ],
        address: [
          {
            use: formValues.tipoDireccion,
            country: formValues.pais,
            state: formValues.departamento,
            city: formValues.ciudad,
            text: formValues.detalleDireccion,
            period: {
              start: formValues.fechaRegistro,
              end: formValues.fechaFinalizacion,
            },
          },
        ],
        identifier: [
          {
            type: formValues.tipoIdentificacion,
            value: formValues.numeroIdentificacion,
            assigner: formValues.emisor,
            period: {
              start: formValues.fechaExpedicion,
            },
          },
        ],
      };

      await mutation.mutateAsync({ data: payload });
    } catch (err) {
      console.error("Error en handleFinish:", err);
      messageApi.error("Error interno al procesar el formulario.");
    }
  };

  const handleCancel = () => {
    messageApi.info("Operación cancelada");
    navigate("/patients/list");
  };

  return {
    handleFinish,
    handleCancel,
    contextHolder,
    isLoading: mutation.isLoading,
  };
}
