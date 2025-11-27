import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormDatePicker,
  ProFormSwitch,
} from "@ant-design/pro-components";
import { FaUserPlus, FaUserEdit, FaCheck } from "react-icons/fa";
import { BsPersonVcardFill } from "react-icons/bs";
import { useEffect, useRef, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  useGetApiPractitionerId,
  usePostApiPractitioner,
  usePutApiPractitionerId,
} from "../../../../api/practitioner/practitioner";
import { message } from "antd";

type EmployeeDetail = {
  id: { value: string };
  meta: { lastUpdated: { value: string }, versionId: { value: string } };
  identifier: {
    use: { value: string };
    type: { text: { value: string } };
    system: { value: string };
    value: { value: string };
  }[];
  active: { value: boolean };
  name: {
    use: { value: string };
    text: { value: string };
    family: { value: string };
    given: { value: string }[];
    prefix?: { value: string }[];
    suffix?: { value: string }[];
  }[];
  telecom: {
    system: string;
    value: { value: string };
    use: { value: string };
    rank: { value: number };
  }[];
  gender: number;
  birthDate: string;
};

export default function PractitionerForm() {
  const { id } = useParams(); // Si existe, es edición
  const navigate = useNavigate();
  const formRef = useRef<any>(null);
  const [loading, setLoading] = useState(!!id);
  const [setInitialValues] = useState<any>({});

  // Hooks API
  const { data } = useGetApiPractitionerId<EmployeeDetail>(id!);
  const { mutateAsync: createPractitioner, isPending: creating } = usePostApiPractitioner({
    mutation: {
      onSuccess: () => {
        formRef.current?.resetFields();
        message.success("Empleado creado correctamente");
        navigate("/practitioners/list");
      },
      onError: (error) => {
        console.error("Error al crear empleado:", error);
        message.error("No se pudo crear el empleado");
      },
    },
  });
  const { mutateAsync: updatePractitioner, isPending: updating } = usePutApiPractitionerId({
    mutation: {
      onSuccess: () => {
        message.success("Empleado actualizado correctamente");
        navigate("/practitioners/list");
      },
      onError: (error) => {
        console.error("Error al actualizar empleado:", error);
        message.error("No se pudo actualizar el empleado");
      },
    },
  });

  // Cargar datos si es edición
  useEffect(() => {
    if (!data) return;

    const values = {
      firstName: data.name?.[0]?.given?.[0] ?? "",
      middleName: data.name?.[0]?.given?.[1] ?? "",
      lastName: data.name?.[0]?.family ?? "",
      dni: data.identifier?.[0]?.value ?? "",
      idType: data.identifier?.[0]?.type?.text ?? "",
      phone: data.telecom?.find((t) => t.system?.toLowerCase() === "phone")?.value ?? "",
      email: data.telecom?.find((t) => t.system?.toLowerCase() === "email")?.value ?? "",
      gender: data.gender ?? 0,
      birthDate: data.birthDate ? new Date(data.birthDate) : null,
      active: data.active?.value ?? true,
    };

    setInitialValues(values);
    

    setTimeout(() => formRef.current?.setFieldsValue(values), 50);
    setLoading(false);
  }, [data]);

  const onFinish = async (values: any) => {
    const telecom: any[] = [];
    if (values.phone) telecom.push({ system: "Phone", value: values.phone, use: "Home", rank: 1 });
    if (values.email) telecom.push({ system: "Email", value: values.email, use: "Home", rank: telecom.length + 1 });

    const payload = {
      identifier: [
        { use: "Usual", type: { text: values.idType }, system: "https://localhost:7107", value: values.dni },
      ],
      active: values.active,
      name: [
        {
          use: "Usual",
          text: `${values.firstName} ${values.middleName ?? ""} ${values.lastName}`,
          family: values.lastName,
          given: [values.firstName, values.middleName].filter(Boolean),
        },
      ],
      telecom,
      gender: values.gender,
      birthDate: values.birthDate ? new Date(values.birthDate).toISOString() : null,
    };

    try {
      if (id) {
        await updatePractitioner({ id, data: payload });
      } else {
        await createPractitioner({ data: payload });
      }
    } catch (error) {
      console.error(error);
    }
  };

  if (loading) return <div className="text-gray-500">Cargando datos...</div>;

  return (
    <div className="primary-card">
      <div className="flex items-center gap-3 mb-8">
        {id ? <FaUserEdit className="w-10 h-10 text-blue-500" /> : <FaUserPlus className="w-10 h-10 text-blue-500" />}
        <span className="text-xl font-semibold text-general">
          {id ? "Editar Empleado" : "Crear Empleado"}
        </span>
      </div>

      <ProForm
        formRef={formRef}
        onFinish={onFinish}
        submitter={{
          searchConfig: { submitText: id ? "Guardar Cambios" : "Crear Empleado" },
          resetButtonProps: false,
          submitButtonProps: {
            loading: id ? updating : creating,
            icon: <FaCheck className="w-4 h-4" />,
            className:
              `px-6 py-2 ${id ? "!bg-blue-500 hover:!bg-blue-600" : "!bg-green-500 hover:!bg-green-600"} 
              text-white font-medium rounded-md transition-colors duration-200 flex items-center gap-2`,
          },
          render: (_, dom) => (
            <div className="flex justify-end pt-4 pb-2 gap-2">
              {/* Botón Cancelar */}
              <button
                type="button"
                onClick={() => navigate("/practitioners/list")}
                className="px-6 py-2 bg-gray-300 hover:bg-gray-400 text-gray-800 font-medium rounded-md transition-colors duration-200"
              >
                Cancelar
              </button>
              {/* Botón Crear / Guardar */}
              {dom[0]}
            </div>
          ),
        }}
      >
        {/* Datos Personales */}
        <section className="mb-8">
          <div className="flex items-center gap-3 mb-6">
            <BsPersonVcardFill className="w-8 h-8 text-blue-500" />
            <span className="text-lg font-semibold text-general">Datos Personales</span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
            <ProFormText name="firstName" label="Primer Nombre" rules={[{ required: true }]} />
            <ProFormText name="middleName" label="Segundo Nombre" />
            <ProFormText name="lastName" label="Apellidos" rules={[{ required: true }]} />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <ProFormText name="dni" label="Identificador" rules={[{ required: true }]} />
            <ProFormSelect
              name="idType"
              label="Tipo de Identificador"
              options={[
                { label: "DNI", value: "DNI" },
                { label: "Pasaporte", value: "Pasaporte" },
                { label: "RTN", value: "RTN" },
              ]}
            />
            <ProFormText name="phone" label="Número de Teléfono" />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
            <ProFormText name="email" label="Correo Electrónico" rules={[{ type: "email" }]} />
            <ProFormSelect
              name="gender"
              label="Género"
              options={[
                { label: "Desconocido", value: 0 },
                { label: "Masculino", value: 1 },
                { label: "Femenino", value: 2 },
                { label: "Otro", value: 3 },
              ]}
            />
            <ProFormDatePicker name="birthDate" label="Fecha de Nacimiento" />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
            <ProFormSwitch name="active" label="Activo" />
          </div>
        </section>
      </ProForm>
    </div>
  );
}
