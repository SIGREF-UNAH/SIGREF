import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormTextArea,
} from "@ant-design/pro-components";
import { Button } from "antd";
import {
  BarChartOutlined,
  EnvironmentOutlined,
  CheckOutlined,
  CloseOutlined,
} from "@ant-design/icons";
import type { CreateOrganizationDto } from "../../../api/models";
import { useRef, useState } from "react";
import ccsj from "countrycitystatejson";
import type { ProFormInstance } from "@ant-design/pro-components";

type OrganizationFormValues = {
  name: string;
  identifier: string;
  types: string[];
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
  onFinish: (values: CreateOrganizationDto) => Promise<void>;
  onCancel: () => void;
  isPending?: boolean;
  submitButtonText?: string;
}

export default function OrganizationsForm({
  initialValues,
  onFinish,
  onCancel,
  isPending = false,
  submitButtonText,
}: OrganizationsFormProps) {
  const handleFinish = async (values: any) => {
    const payload: CreateOrganizationDto = {
      name: values.name,
      identifier: values.identifier,
      types: values.types,
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
  const [countryOptions] = useState(
    ccsj.getCountries().map((c) => ({ label: c.name, value: c.shortName }))
  );
  const [stateOptions, setStateOptions] = useState<
    { label: string; value: string }[]
  >([]);
  const [cityOptions, setCityOptions] = useState<
    { label: string; value: string }[]
  >([]);

  //  Corregido: ahora se guarda el país y se limpian los otros campos
  const handleCountryChange = (countryShort?: string) => {
    if (!countryShort) {
      setStateOptions([]);
      setCityOptions([]);
      formRef.current?.setFieldValue("state", null);
      formRef.current?.setFieldValue("city", null);
      return;
    }

    const states = ccsj.getStatesByShort(countryShort) ?? [];
    setStateOptions(states.map((s: string) => ({ label: s, value: s })));
    setCityOptions([]);
    formRef.current?.setFieldValue("state", null);
    formRef.current?.setFieldValue("city", null);
    formRef.current?.setFieldValue("country", countryShort); // clave
  };

  // Corregido: solo carga ciudades, no reescribe estados
  const handleStateChange = (stateName?: string) => {
    const countryShort = formRef.current?.getFieldValue("country");
    if (!countryShort || !stateName) {
      setCityOptions([]);
      formRef.current?.setFieldValue("city", null);
      return;
    }

    const cities = ccsj.getCities(countryShort, stateName) ?? [];
    setCityOptions(cities.map((c: string) => ({ label: c, value: c })));
    formRef.current?.setFieldValue("city", null);
  };

  return (
    <div className="min-h-screen w-full bg-gray-50">
      <main className="w-full px-8">
        <div className="w-full bg-white border border-gray-200 rounded-lg shadow-sm p-8">
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
                    className="!bg-red-500 !text-white hover:!bg-red-500 border-none px-8"
                  >
                    Cancelar
                  </Button>

                  <Button
                    type="primary"
                    htmlType="submit"
                    icon={<CheckOutlined />}
                    size="large"
                    loading={isPending}
                    className="!bg-green-500 !hover:bg-green-600 border-none px-8"
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
              name: initialValues?.name || "",
              identifier: initialValues?.identifier || "",
              types: initialValues?.types || "",
              active: initialValues?.active ?? true,
              email: initialValues?.email || "",
              phone: initialValues?.phone || "",
              address: initialValues?.address || "",
              city: initialValues?.city || "",
              state: initialValues?.state || "",
              country: initialValues?.country || "",
              description: initialValues?.description || "",
            }}
          >
            {/* Información de la Organización */}
            <div className="mb-8">
              <div className="flex items-center gap-2 mb-6 pb-3 border-b border-gray-200">
                <BarChartOutlined className="text-lg !text-blue-400" />
                <h2 className="text-lg font-semibold text-gray-900">
                  Información de la Organización
                </h2>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                <ProFormText
                  name="name"
                  label="Nombre de la Organización"
                  placeholder="Ej. Hospital Regional Norte"
                  rules={[
                    { required: true, message: "Este campo es requerido" },
                  ]}
                  fieldProps={{ className: "bg-gray-100" }}
                />
                <ProFormText
                  name="identifier"
                  label="Identificador"
                  placeholder="# Ej. HSJ-HRN"
                  rules={[
                    { required: true, message: "Este campo es requerido" },
                  ]}
                />
                <ProFormSelect
                  name="types"
                  label="Tipo de Organización"
                  placeholder="Seleccionar tipo"
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
                  options={[
                    { label: "Activo", value: true },
                    { label: "Inactivo", value: false },
                  ]}
                  rules={[
                    { required: true, message: "Este campo es requerido" },
                  ]}
                  fieldProps={{ className: "bg-gray-100" }}
                />
              </div>
            </div>

            {/* Información de Contacto */}
            <div className="mb-8">
              <h3 className="text-base font-semibold text-gray-900 mb-4 pb-3 border-b border-gray-200">
                Información de Contacto
              </h3>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <ProFormText
                  name="email"
                  label="Email de Contacto"
                  placeholder="contacto@gmail.com"
                  rules={[
                    { type: "email", message: "Ingrese un email válido" },
                  ]}
                  fieldProps={{ className: "bg-gray-100" }}
                />
                <ProFormText
                  name="phone"
                  label="Teléfono"
                  placeholder="+504 8819-4589"
                  fieldProps={{ className: "bg-gray-100" }}
                />
              </div>
            </div>

            {/* Ubicación */}
            <div className="mb-8">
              <div className="flex items-center gap-2 mb-6 pb-3 border-b border-gray-200">
                <EnvironmentOutlined className="text-lg !text-blue-400" />
                <h2 className="text-lg font-semibold text-gray-900">
                  Ubicación
                </h2>
              </div>

              <div className="mb-4">
                <ProFormText
                  name="address"
                  label="Dirección Completa"
                  placeholder="Ej. Av. Principal 123, Ciudad"
                  fieldProps={{
                    prefix: <EnvironmentOutlined className="text-gray-400" />,
                    className: "bg-gray-100",
                  }}
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <ProFormSelect
                  name="country"
                  label="País"
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
            </div>

            {/* Descripción */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <ProFormTextArea
                name="description"
                label="Descripción Opcional"
                placeholder="Breve descripción de la organización..."
                fieldProps={{
                  rows: 4,
                  className: "bg-gray-100",
                }}
              />
            </div>
          </ProForm>
        </div>
      </main>
    </div>
  );
}
