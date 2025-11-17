import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormDatePicker,
  ProFormSwitch,
} from "@ant-design/pro-components";
import { FaUserPlus, FaCheck } from "react-icons/fa";
import { BsPersonVcardFill, BsShieldLock } from "react-icons/bs";
import { useRef } from "react";
import { useNavigate } from "react-router-dom";
import { usePostApiPractitioner } from "../../../../api/practitioner/practitioner";

export default function CreateEmployeeForm() {
  
  const navigate = useNavigate();
  const formRef = useRef<any>(null);

  const { mutateAsync, isPending } = usePostApiPractitioner({
  mutation: {
    onSuccess: () => {
      formRef.current?.resetFields();
      navigate("/employees/list");
    },
    onError: (error) => {
      console.error("Error al crear empleado:", error);
    },
  },
});

  const onFinish = async (values: any) => {
  const payload = {
    identifier: [
      {
        use: 0,
        type: {
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
          {
            system: 2,
            value: values.email,
            use: 1,
            rank: 2
          }
        ]
      : [],
    gender: values.gender ?? 0,
    birthDate: values.birthDate ?? null,
  };

  try {
    await mutateAsync({ data: payload });
  } catch (err) {
    console.error(err);
  }
};


  return (
    <div className="bg-[#FAFAFA] rounded-lg border-2 border-[#D9D9D9] p-6">
      {/* Encabezado */}
      <div className="flex items-center gap-3 mb-8">
        <FaUserPlus className="w-10 h-10 text-blue-500" />
        <span className="text-xl font-semibold text-[#333333]">
          Crear Usuario
        </span>
      </div>

      <ProForm
        formRef={formRef}
        onFinish={onFinish}
        submitter={{
          searchConfig: { submitText: "Crear Usuario" },
          resetButtonProps: false,
          submitButtonProps: {
            icon: <FaCheck className="w-4 h-4" />,
            loading: isPending,
            className:
              "px-6 py-2 !bg-green-500 hover:!bg-green-600 text-white font-medium rounded-md transition-colors duration-200 flex items-center gap-2",
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
              placeholder="Ej. Juan"
              rules={[{ required: true, message: "Campo obligatorio" }]}
            />
            <ProFormText
              name="middleName"
              label="Segundo Nombre"
              placeholder="Ej. Leonor"
            />
            <ProFormText
              name="lastName"
              label="Apellidos"
              placeholder="Ej. Pérez Martínez"
              rules={[{ required: true, message: "Campo obligatorio" }]}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <ProFormText
              name="dni"
              label="Identificador"
              placeholder="Ej. 0801-1987-00234"
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
              placeholder="Selecciona tipo"
            />
            <ProFormText
              name="phone"
              label="Número de Teléfono"
              placeholder="Ej. +504 3245-0000"
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
            <ProFormSelect
              name="gender"
              label="Género"
              options={[
                { label: "Masculino", value: 0 },
                { label: "Femenino", value: 1 },
                { label: "Otro", value: 2 },
              ]}
              placeholder="Selecciona género"
            />
            <ProFormDatePicker
              name="birthDate"
              label="Fecha de Nacimiento"
              placeholder="Selecciona fecha"
            />
            <ProFormSwitch
              name="active"
              label="Activo"
              fieldProps={{ defaultChecked: true }}
            />
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
              placeholder="Ej. jperez"
              rules={[{ required: true, message: "Campo obligatorio" }]}
            />
            <ProFormText
              name="email"
              label="Correo Electrónico"
              placeholder="Ej. jperez@me.com"
              rules={[
                { required: true, message: "Campo obligatorio" },
                { type: "email", message: "Correo inválido" },
              ]}
            />
            <ProFormSelect
              name="role"
              label="Rol del Usuario"
              placeholder="Selecciona un rol"
              options={[
                { label: "Administrador", value: "admin" },
                { label: "Administrador de TI", value: "ti" },
                { label: "Auxiliar de Caja", value: "cashier" },
                { label: "Auditor", value: "auditor" },
              ]}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <ProFormText.Password
              name="password"
              label="Contraseña"
              placeholder="********************"
              rules={[{ required: true, message: "Campo obligatorio" }]}
            />
            <ProFormText.Password
              name="confirmPassword"
              label="Confirmar Contraseña"
              placeholder="********************"
              dependencies={["password"]}
              rules={[
                { required: true, message: "Campo obligatorio" },
                ({ getFieldValue }) => ({
                  validator(_, value) {
                    if (!value || getFieldValue("password") === value) {
                      return Promise.resolve();
                    }
                    return Promise.reject(
                      new Error("Las contraseñas no coinciden")
                    );
                  },
                }),
              ]}
            />
          </div>
        </section>
      </ProForm>
    </div>
  );
}
