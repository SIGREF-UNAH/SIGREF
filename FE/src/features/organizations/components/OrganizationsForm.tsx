import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormTextArea,
} from "@ant-design/pro-components";
import {
  BarChartOutlined,
  EnvironmentOutlined,
  CheckOutlined,
  CloseOutlined,
  PhoneOutlined,
} from "@ant-design/icons";
import type { CreateOrganizationDto } from "../../../api/models";
import type { ProFormInstance } from "@ant-design/pro-components";
import { Button, Form } from "antd";
import { useRef, useState } from "react";
import PhoneInput from "react-phone-number-input";
import 'react-phone-number-input/style.css';
import {
  getCityOptionsByCountryAndState,
  getCountryOptions,
  getStateOptionsByCountry,
} from "../../../shared/utils";

type OrganizationFormValues = {
  name: string;
  identifier: string;
  type: string[];
  active: boolean;
  description?: string;
  phone?: string;
  email?: string;
  apiLink?: string;
  address: string;
  city: string;
  state: string;
  country?: string;
};

interface OrganizationsFormProps {
  initialValues?: OrganizationFormValues;
  isPending?: boolean;
  submitButtonText?: string;
  onFinish: (values: CreateOrganizationDto) => Promise<void>;
  onCancel: () => void;
}

export default function OrganizationsForm({
  initialValues,
  isPending = false,
  submitButtonText,
  onFinish,
  onCancel,
}: OrganizationsFormProps) {
  
  const handleFinish = async (values: any) => {
    const payload: CreateOrganizationDto = {
      name: values.name,
      identifier: values.identifier,
      type: values.type,
      active: values.active,
      email: values.email,
      phone: values.phone,
      address: values.address,
      country: values.country,
      city: values.city,
      state: values.state,
      description: values.description,
    };
    await onFinish(payload);
  };

  const formRef = useRef<ProFormInstance>(null);

  // Cargar países correctamente
  const [countryOptions] = useState(() => getCountryOptions());
  const [stateOptions, setStateOptions] = useState<{ label: string; value: string }[]>([]);
  const [cityOptions, setCityOptions] = useState<{ label: string; value: string }[]>([]);

  const handleCountryChange = (countryShort?: string) => {
    if (!countryShort) {
      setStateOptions([]);
      setCityOptions([]);
      formRef.current?.setFieldValue("state", null);
      formRef.current?.setFieldValue("city", null);
      return;
    }

    setStateOptions(getStateOptionsByCountry(countryShort));
    setCityOptions([]);
    formRef.current?.setFieldValue("state", null);
    formRef.current?.setFieldValue("city", null);
    formRef.current?.setFieldValue("country", countryShort);
  };

  const handleStateChange = (stateName?: string) => {
    const countryShort = formRef.current?.getFieldValue("country");
    if (!countryShort || !stateName) {
      setCityOptions([]);
      formRef.current?.setFieldValue("city", null);
      return;
    }

    setCityOptions(getCityOptionsByCountryAndState(countryShort, stateName));
    formRef.current?.setFieldValue("city", null);
  };

  return (
    <div className="primary-card">
      <ProForm
        formRef={formRef}
        onFinish={handleFinish}
        submitter={{
          render: () => (
            <div className="flex justify-end mt-6 gap-3">
              <Button
                type="default"
                icon={<CloseOutlined />}
                size="large"
                onClick={onCancel}
                disabled={isPending}
                className="bg-red-500! text-white! hover:bg-red-500! border-none px-8"
              >
                Cancelar
              </Button>

              <Button
                type="primary"
                htmlType="submit"
                icon={<CheckOutlined />}
                size="large"
                loading={isPending}
                className="bg-green-500! hover:bg-green-600! border-none px-8"
              >
                {submitButtonText ||
                  (initialValues
                    ? "Actualizar Organización"
                    : "Crear Organización")}
              </Button>
            </div>
          ),
        }}
        initialValues={{
          name: initialValues?.name || null,
          identifier: initialValues?.identifier || null,
          type: initialValues?.type || null,
          active: initialValues?.active,
          email: initialValues?.email || null,
          phone: initialValues?.phone || null,
          address: initialValues?.address || null,
          city: initialValues?.city || null,
          state: initialValues?.state || null,
          country: initialValues?.country || null,
          description: initialValues?.description || null,
        }}
      >
        {/* Información de la Organización */}
        <div>
          <div className="flex items-center gap-2 mb-6 pb-3 border-b border-gray-200">
            <BarChartOutlined className="text-lg text-blue-400!" />
            <h2 className="text-lg font-semibold text-gray-900">Información</h2>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <ProFormText
              name="name"
              label="Nombre"
              placeholder="Ej. Hospital de Salud Pública"
              rules={[{ required: true, message: "Este campo es requerido" }]}
              fieldProps={{ className: "bg-gray-100" }}
            />
            <ProFormText
              name="identifier"
              label="Identificador"
              placeholder="Ej. HSP0401"
              rules={[{ required: true, message: "Este campo es requerido" }]}
            />
            <ProFormSelect
              name="type"
              label="Tipo"
              placeholder="Seleccionar"
              options={[
                { label: "Proveedor de salud", value: "prov" },
                { label: "Departamento", value: "dept" },
                { label: "Equipo", value: "team" },
                { label: "Gobierno", value: "govt" },
                { label: "Aseguradora", value: "ins" },
                { label: "Pagador", value: "pay" },
                { label: "Educativo", value: "edu" },
                { label: "Religioso", value: "reli" },
                { label: "Investigación clínica", value: "crs" },
                { label: "Comunidad", value: "cg" },
                { label: "Negocio no médico", value: "bus" },
                { label: "Otro", value: "other" },
              ]}
              fieldProps={{ className: "bg-gray-100" }}
            />
            <ProFormSelect
              name="active"
              label="Estado"
              placeholder="Seleccionar"
              options={[
                { label: "Activo", value: true },
                { label: "Inactivo", value: false },
              ]}
              rules={[{ required: true, message: "Este campo es requerido" }]}
              fieldProps={{ className: "bg-gray-100" }}
            />
          </div>
          <ProFormTextArea
            name="description"
            label="Descripción"
            placeholder="Ej. Detalles adicionales sobre la organización..."
            style={{ width: "100%" }}
          />
        </div>

        {/* Información de Contacto */}
        <div>
          <div className="flex items-center gap-2 mb-6 pb-3 border-b border-gray-200">
            <PhoneOutlined className="text-lg text-blue-400!" />
            <h2 className="text-lg font-semibold text-gray-900">Contacto</h2>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <ProFormText
              name="email"
              label="Correo electrónico"
              placeholder="Ej. organizacion@example.com"
              rules={[{ type: "email", message: "Ingrese un email válido" }]}
              fieldProps={{ className: "bg-gray-100" }}
            />
            
            {/* Campo de teléfono con selector de país */}
            <Form.Item
              name="phone"
              label="Teléfono"
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
        </div>

        {/* Ubicación */}
        <div>
          <div className="flex items-center gap-2 mb-6 pb-3 border-b border-gray-200">
            <EnvironmentOutlined className="text-lg text-blue-400!" />
            <h2 className="text-lg font-semibold text-gray-900">Ubicación</h2>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <ProFormSelect
              name="country"
              label="País"
              placeholder="Seleccionar"
              options={countryOptions}
              fieldProps={{
                showSearch: true,
                onChange: handleCountryChange,
                filterOption: (input, option) =>
                  (option?.label ?? "")
                    .toString()
                    .toLowerCase()
                    .includes(input.toLowerCase()),
              }}
            />

            <ProFormSelect
              name="state"
              label="Departamento"
              placeholder="Seleccionar"
              options={stateOptions}
              fieldProps={{
                showSearch: true,
                onChange: handleStateChange,
                filterOption: (input, option) =>
                  (option?.label ?? "")
                    .toString()
                    .toLowerCase()
                    .includes(input.toLowerCase()),
              }}
            />

            <ProFormSelect
              name="city"
              label="Ciudad"
              placeholder="Seleccionar"
              options={cityOptions}
              fieldProps={{
                showSearch: true,
                filterOption: (input, option) =>
                  (option?.label ?? "")
                    .toString()
                    .toLowerCase()
                    .includes(input.toLowerCase()),
              }}
            />
          </div>

          <div className="mb-4">
            <ProFormText
              name="address"
              label="Detalles"
              placeholder="Ej. Av. Principal 123, Ciudad"
            />
          </div>
        </div>
      </ProForm>
    </div>
  );
}