import { useEffect, useRef } from "react";
import { useNavigate, useParams } from "react-router-dom";
import type { ProFormInstance } from "@ant-design/pro-components";
import { Form } from "antd";
import { useMessage } from "../../../shared/hooks";
import dayjs from "dayjs";
import type {
  PractitionerDto,
  CreatePractitionerDto,
  UpdatePractitionerDto,
} from "../../../api/models";
import {
  useGetApiPractitionerId,
  usePostApiPractitioner,
  usePutApiPractitionerId,
} from "../../../api/practitioner/practitioner";

export function usePractitionerForm() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [form] = Form.useForm();
  const isEditMode = !!id;
  const formRef = useRef<ProFormInstance>(null);
  const msg = useMessage();

  // Cargar datos
  const {
    data: practitionerData,
    isLoading: isFetching,
    isError,
  } = useGetApiPractitionerId<PractitionerDto>(id!, {
    query: {
      enabled: isEditMode,
      staleTime: 0,
    },
  });

  // Crear
  const { mutateAsync: createPractitioner, isPending: isCreating } =
    usePostApiPractitioner({
      mutation: {
        onSuccess: () => {
          msg.success("Empleado creado correctamente");
          navigate("/practitioners/list");
        },
        onError: (error: any) => {
          console.error("Error al crear empleado:", error);
          msg.error(
            error?.response?.data?.title || "No se pudo crear el empleado"
          );
        },
      },
    });

  // Actualizar
  const { mutateAsync: updatePractitioner, isPending: isUpdating } =
    usePutApiPractitionerId({
      mutation: {
        onSuccess: () => {
          msg.success("Empleado actualizado correctamente");
          navigate("/practitioners/list");
        },
        onError: (error: any) => {
          console.error("Error al actualizar empleado:", error);
          msg.error(
            error?.response?.data?.title || "No se pudo actualizar el empleado"
          );
        },
      },
    });

  // Cargar datos
  useEffect(() => {
    if (!isEditMode || !practitionerData) return;

    const firstName = practitionerData.name?.[0]?.given?.[0] || "";
    const middleName = practitionerData.name?.[0]?.given?.[1] || "";
    const lastName = practitionerData.name?.[0]?.family || "";
    const dni = practitionerData.identifier?.[0]?.value || "";
    const idType = practitionerData.identifier?.[0]?.type?.text || "DNI";
    const phone = practitionerData.telecom?.find((t) => String(t.system)?.toLowerCase() === "phone")?.value || "";
    const email = practitionerData.telecom?.find((t) => String(t.system)?.toLowerCase() === "email")?.value || "";
    const gender = practitionerData.gender ?? 0;
    const birthDate = practitionerData.birthDate
      ? dayjs(practitionerData.birthDate)
      : null;
    const active = practitionerData.active ?? true;

    form.setFieldsValue({
      firstName,
      middleName,
      lastName,
      dni,
      idType,
      phone,
      email,
      gender,
      birthDate,
      active,
    });
  }, [practitionerData, form, isEditMode]);

  // Mandar formulario
  const onFinish = async (values: any) => {
    try {
      const telecom: any[] = [];

      if (values.phone) {
        telecom.push({
          system: "Phone",
          value: values.phone,
          use: "Home",
          rank: 1,
        });
      }

      if (values.email) {
        telecom.push({
          system: "Email",
          value: values.email,
          use: "Home",
          rank: telecom.length + 1,
        });
      }

      const givenNames = [values.firstName, values.middleName].filter(Boolean);
      const fullName = `${values.firstName} ${values.middleName || ""} ${
        values.lastName
      }`.trim();

      let formattedBirthDate = null;
    
      if (values.birthDate) {
        try {
          let dateObj;
          
          // Si ya es un objeto dayjs (del formulario)
          if (values.birthDate && typeof values.birthDate.format === 'function') {
            dateObj = values.birthDate;
          } else if (typeof values.birthDate === 'string') {
            // Si es un string, parsearlo con el formato correcto
            dateObj = dayjs(values.birthDate, 'DD/MM/YYYY');
          } else {
            // Para otros casos
            dateObj = dayjs(values.birthDate);
          }
          
          if (dateObj && dateObj.isValid()) {
            formattedBirthDate = dateObj.toISOString();
          } else {
            console.warn('Fecha inválida:', values.birthDate);
          }
        } catch (error) {
          console.error('Error al procesar fecha:', error);
        }
      }

      const payload: CreatePractitionerDto | UpdatePractitionerDto = {
        identifier: [
          {
            use: undefined,
            type: {
              text: values.idType || "DNI",
            },
            system: null,
            value: values.dni,
          },
        ],
        active: values.active ?? true,
        name: [
          {
            use: undefined,
            text: fullName,
            family: values.lastName,
            given: givenNames,
            prefix: [],
            suffix: [],
          },
        ],
        telecom,
        gender: values.gender ?? 0,
        birthDate: formattedBirthDate,
      };
      
      if (isEditMode) {
        await updatePractitioner({ id: id!, data: payload });
      } else {
        await createPractitioner({ data: payload });
      }

    } catch (error) {
      console.error("Error en el formulario:", error);
    }
  };

  // Cancelar
  const handleCancel = () => {
    navigate(-1);
  };

  return {
    form,
    formRef,
    isEditMode,
    isFetching,
    isCreating,
    isUpdating,
    isError,
    onFinish,
    handleCancel,
  };
}
