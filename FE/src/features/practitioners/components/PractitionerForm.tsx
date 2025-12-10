import { FaUserPlus, FaUserEdit, FaCheck } from "react-icons/fa";
import { BsPersonVcardFill } from "react-icons/bs";
import { MdCancel } from "react-icons/md";
import { Form } from "antd";
import PhoneInput from "react-phone-number-input";
import 'react-phone-number-input/style.css';
import { usePractitionerForm } from "../hooks";
import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormDatePicker,
  ProFormSwitch,
} from "@ant-design/pro-components";

export default function PractitionerForm() {
  const {
    form,
    formRef,
    isEditMode,
    isFetching,
    isCreating,
    isUpdating,
    isError,
    onFinish,
    handleCancel,
  } =usePractitionerForm();

  // Pantalla de carga
  if (isEditMode && isFetching) {
    return (
      <div className="primary-card">
        <div className="flex items-center justify-center py-12">
          <div className="text-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto mb-4"></div>
            <p className="text-gray-600">Cargando datos del empleado...</p>
          </div>
        </div>
      </div>
    );
  }

  // Pantalla de error
  if (isEditMode && isError) {
    return (
      <div className="primary-card">
        <div className="text-center py-12">
          <p className="text-red-600 mb-4">
            Error al cargar los datos del empleado
          </p>
          <button
            onClick={handleCancel}
            className="px-6 py-2 bg-gray-300 hover:bg-gray-400 text-gray-800 font-medium rounded-md transition-colors"
          >
            Volver a la lista
          </button>
        </div>
      </div>
    );
  }

  const isPending = isCreating || isUpdating;

  return (
    <div className="primary-card">
      {/* Encabezado */}
      <div className="flex items-center gap-3 mb-8 pb-4 border-b border-gray-200">
        {isEditMode ? (
          <FaUserEdit className="w-10 h-10 text-blue-600" />
        ) : (
          <FaUserPlus className="w-10 h-10 text-green-600" />
        )}
        <div>
          <h1 className="text-2xl font-bold text-gray-800">
            {isEditMode ? "Editar Empleado" : "Crear Empleado"}
          </h1>
          <p className="text-sm text-gray-500 mt-1">
            {isEditMode
              ? "Modifica los datos del empleado"
              : "Completa el formulario para registrar un nuevo empleado"}
          </p>
        </div>
      </div>

      {/* Formulario */}
      <ProForm
        form={form}
        onFinish={onFinish}
        submitter={{
          searchConfig: {
            submitText: isEditMode ? "Guardar Cambios" : "Crear Empleado",
          },
          resetButtonProps: false,
          submitButtonProps: {
            loading: isPending,
            icon: <FaCheck className="w-4 h-4" />,
            className: `px-6 py-2.5 ${
              isEditMode
                ? "!bg-blue-600 hover:!bg-blue-700"
                : "!bg-green-600 hover:!bg-green-700"
            } text-white font-semibold rounded-lg shadow-md hover:shadow-lg transition-all duration-200 flex items-center gap-2`,
          },
          render: (_, dom) => (
            <div className="flex justify-end gap-3 pt-6 border-t border-gray-200">
              <button
                type="button"
                onClick={handleCancel}
                disabled={isPending}
                className="px-6 bg-gray-200 hover:bg-gray-300 text-gray-700 font-semibold rounded-lg transition-colors duration-200 flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <MdCancel className="w-4 h-4" />
                Cancelar
              </button>
              {dom[0]}
            </div>
          ),
        }}
      >
        <section className="mb-8">
          <div className="flex items-center gap-3 mb-6 pb-3 border-b border-gray-200">
            <BsPersonVcardFill className="w-7 h-7 text-blue-600" />
            <h2 className="text-lg font-semibold text-gray-800">
              Datos Personales
            </h2>
          </div>

          {/* Nombres */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
            <ProFormText
              name="firstName"
              label="Primer Nombre"
              placeholder="Ej. Maria"
              rules={[
                { required: true, message: "El primer nombre es obligatorio" },
                { min: 2, message: "Mínimo 2 caracteres" },
              ]}
            />
            <ProFormText
              name="middleName"
              label="Segundo Nombre"
              placeholder="Ej. Vanessa"
            />
            <ProFormText
              name="lastName"
              label="Apellidos"
              placeholder="Ej. Lopez Perez"
              rules={[
                { required: true, message: "Los apellidos son obligatorios" },
                { min: 2, message: "Mínimo 2 caracteres" },
              ]}
            />
          </div>

          {/* Identificacion */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
            <ProFormText
              name="dni"
              label="Número de Identificación"
              placeholder="Ej. 0401200098371"
              rules={[
                { required: true, message: "El identificador es obligatorio" },
              ]}
            />
            <ProFormSelect
              name="idType"
              label="Tipo de Identificación"
              placeholder="Seleccionar"
              options={[
                { label: "DNI", value: "DNI" },
                { label: "Pasaporte", value: "PPN" },
                { label: "Otro", value: "NI" },
              ]}
              rules={[{ required: true, message: "Seleccione un tipo" }]}
            />
            <Form.Item
              name="phone"
              label="Número de Teléfono"
              rules={[
                {
                  validator: (_, value) => {
                    if (!value) return Promise.resolve();
                    // Validación básica
                    if (value && value.length < 5) {
                      return Promise.reject(new Error('Número de teléfono muy corto'));
                    }
                    return Promise.resolve();
                  },
                },
              ]}
            >
              <PhoneInput
                international
                countryCallingCodeEditable={false}
                defaultCountry="HN"
                value={formRef.current?.getFieldValue("phone")}
                onChange={(value) => {
                  formRef.current?.setFieldValue("phone", value || "");
                }}
                className="ant-input bg-white rounded px-3 py-2 border border-gray-300 hover:border-blue-400 focus:border-blue-400 focus:shadow-outline"
                style={{
                  width: '100%',
                  padding: '4px 11px',
                }}
              />
            </Form.Item>
          </div>

          {/* Contacto */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
            <ProFormText
              name="email"
              label="Correo Electrónico"
              placeholder="Ej. maria@example.com"
              rules={[
                { required: true, message: "El correo es obligatorio" },
                { type: "email", message: "Correo inválido" },
              ]}
            />
            <ProFormSelect
              name="gender"
              label="Género"
              placeholder="Seleccionar"
              options={[
                { label: "Masculino", value: 1 },
                { label: "Femenino", value: 2 },
                { label: "Otro", value: 3 },
                { label: "Desconocido", value: 0 },
              ]}
            />
            <ProFormDatePicker
              name="birthDate"
              label="Fecha de Nacimiento"
              placeholder="Ej. 31/12/1999"
              fieldProps={{
                format: "DD/MM/YYYY",
                className: "w-full",
              }}
            />
          </div>

          {/* Estado */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <ProFormSwitch
              name="active"
              label="Estado"
              fieldProps={{
                checkedChildren: "Activo",
                unCheckedChildren: "Inactivo",
              }}
              initialValue={true}
            />
          </div>
        </section>
      </ProForm>
    </div>
  );
}
