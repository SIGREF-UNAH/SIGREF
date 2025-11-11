import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormDatePicker,
  ProFormSwitch,
} from "@ant-design/pro-components";
import { FaUserEdit, FaCheck } from "react-icons/fa";
import { BsPersonVcardFill, BsShieldLock } from "react-icons/bs";
import { useEffect, useRef, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useGetApiPractitionerId, usePutApiPractitionerId } from "../../../../api/practitioner/practitioner";
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
    system: { value: string };
    value: { value: string };
    use: { value: string };
    rank: { value: number };
  }[];
  gender: { value: "male" | "female" | "other" };
  birthDate: { value: string };
};

export default function EditPractitionerForm() {
  const navigate = useNavigate();
  const { id } = useParams();
  const formRef = useRef<any>(null);
  const [loading, setLoading] = useState(true);
  const [initialValues, setInitialValues] = useState<any>({});

  const { data } = useGetApiPractitionerId<EmployeeDetail>(id!);

  const { mutateAsync, isPending } = usePutApiPractitionerId();

 useEffect(() => {
  if (!data) return;

  // const emailEntry = data.telecom?.find(t => t.system?.value === "email");

  setInitialValues({
  firstName: data.name?.[0]?.given?.[0]?.value ?? "",
  middleName: data.name?.[0]?.given?.[1]?.value ?? "",
  lastName: data.name?.[0]?.family?.value ?? "",
  dni: data.identifier?.[0]?.value?.value ?? "",
  idType: data.identifier?.[0]?.type?.text?.value ?? "",
  phone: data.telecom?.[0]?.value?.value ?? "",
  email: data.telecom?.find(t => t.system?.value === "email")?.value?.value ?? "",
  gender: data.gender?.value === "male" ? 1 : data.gender?.value === "female" ? 2 : 3,
  birthDate: data.birthDate?.value ? new Date(data.birthDate.value) : null,
  active: data.active?.value ?? true,
});

  setLoading(false);
}, [data]);

  useEffect(() => {
  if (formRef.current && Object.keys(initialValues).length) {
    formRef.current.setFieldsValue(initialValues);
  }
  }, [initialValues]);

  const onFinish = async (values: any) => {
  const payload = {
    identifier: [
      {
        use: 0,
        type: {
          coding: [
            {
              system: "https://localhost:7107",
              version: "1.0",
              code: values.idType ?? "DNI",
              display: values.idType ?? "DNI",
              userSelected: true,
            },
          ],
          text: values.idType,
        },
        system: "https://localhost:7107",
        value: values.dni,
      },
    ],
    active: values.active ?? true,
    name: [
      {
        use: 0,
        text: `${values.firstName} ${values.middleName ?? ""} ${values.lastName}`,
        family: values.lastName,
        given: [values.firstName, values.middleName].filter(Boolean),
        prefix: [],
        suffix: [],
      },
    ],
    telecom: values.phone
      ? [
          {
            system: 1,
            value: values.phone,
            use: 0,
            rank: 1,
          },
        ]
      : [],
    gender: values.gender ?? 0,
    birthDate: values.birthDate
      ? new Date(values.birthDate).toISOString()
      : null,
  };

  try {
    console.log("Enviando payload:", payload);
    await mutateAsync({ id, data: payload }); // 👈 Llamada real al backend
    message.success("Empleado actualizado correctamente");
    navigate("/practitioners/list");
  } catch (error) {
    console.error("Error al actualizar el empleado:", error);
    message.error("No se pudo actualizar el empleado");
  }
};


  if (loading) return <div className="text-gray-500">Cargando datos...</div>;

  return (
    <div className="bg-[#FAFAFA] rounded-lg border-2 border-[#D9D9D9] p-6">
      {/* Encabezado */}
      <div className="flex items-center gap-3 mb-8">
        <FaUserEdit className="w-10 h-10 text-blue-500" />
        <span className="text-xl font-semibold text-[#333333]">
          Editar Usuario
        </span>
      </div>

      <ProForm
        formRef={formRef}
        initialValues={initialValues}
        onFinish={onFinish}
        submitter={{
          searchConfig: { submitText: "Guardar Cambios" },
          resetButtonProps: false,
          submitButtonProps: {
          loading: isPending,
          icon: <FaCheck className="w-4 h-4" />,
          className:
            "px-6 py-2 !bg-blue-500 hover:!bg-blue-600 text-white font-medium rounded-md transition-colors duration-200 flex items-center gap-2",
        },
          render: (_, dom) => (
            <div className="flex justify-end pt-4 pb-2">{dom[0]}</div>
          ),
        }}
      >
        {/* Datos Personales */}
        <section className="mb-8">
          <div className="flex items-center gap-3 mb-6">
            <BsPersonVcardFill className="w-8 h-8 text-blue-500" />
            <span className="text-lg font-semibold text-[#333333]">
              Datos Personales
            </span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
            <ProFormText
              name="firstName"
              label="Primer Nombre"
              rules={[{ required: true, message: "Campo obligatorio" }]}
            />
            <ProFormText name="middleName" label="Segundo Nombre" />
            <ProFormText
              name="lastName"
              label="Apellidos"
              rules={[{ required: true, message: "Campo obligatorio" }]}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <ProFormText
              name="dni"
              label="Identificador"
              rules={[{ required: true, message: "Campo obligatorio" }]}
            />
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
            <ProFormSwitch name="active" label="Activo" />
          </div>
        </section>

        {/* Datos de Usuario */}
        <section>
          <div className="flex items-center gap-3 mb-6">
            <BsShieldLock className="w-8 h-8 text-blue-500" />
            <span className="text-lg font-semibold text-[#333333]">
              Datos de Usuario
            </span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
            <ProFormText
              name="username"
              label="Nombre de Usuario"
              rules={[{ required: true, message: "Campo obligatorio" }]}
            />
            <ProFormText
              name="email"
              label="Correo Electrónico"
              rules={[
                { required: true, message: "Campo obligatorio" },
                { type: "email", message: "Correo inválido" },
              ]}
            />
            <ProFormSelect
              name="role"
              label="Rol del Usuario"
              options={[
                { label: "Administrador", value: "admin" },
                { label: "Técnico", value: "technician" },
                { label: "Empleado", value: "employee" },
              ]}
            />
          </div>
        </section>
      </ProForm>
    </div>
  );
}
