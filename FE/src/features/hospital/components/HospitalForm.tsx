import { Button, Form, Space } from "antd";
import { BankOutlined, ArrowLeftOutlined } from "@ant-design/icons";
import PhoneInput from "react-phone-number-input";
import "react-phone-number-input/style.css";
import {
  ProForm,
  ProFormText,
  ProFormSelect,
} from "@ant-design/pro-components";
import useHospitalForm from "../hooks/useHospitalForm";

interface HospitalFormProps<T extends Record<string, any>> {
  onSubmit: (values: T) => void | Promise<void>;
  onCancel: () => void;
  initialValues?: T;
  isEdit?: boolean;
  loading?: boolean;
}

export const HospitalForm = <T extends Record<string, any>>({
  onSubmit,
  onCancel,
  initialValues,
  isEdit = false,
  loading = false,
}: HospitalFormProps<T>) => {
  // Funciones y variables en hook personalizado
  const {
    formRef,
    countryOptions,
    stateOptions,
    cityOptions,
    selectedCountry,
    selectedState,
    selectedCity,
    buildUbication,
    buildCompleteLocation,
    handleCountryChange,
    handleStateChange,
    handleCityChange,
  } = useHospitalForm(initialValues);

  // Manejar el envío del formulario
  const handleSubmit = async (values: any) => {
    const location = buildUbication(
      selectedCountry,
      selectedState,
      selectedCity
    );
    const details = values.details || "";
    const completeLocation = buildCompleteLocation(location, details);

    // Asegurarnos de incluir los valores de ubicación en el envío
    const submitValues = {
      ...values,
      country: selectedCountry,
      state: selectedState,
      city: selectedCity,
      location: completeLocation,
      details: details,
    };

    // Remover campos temporales del DTO final
    delete submitValues.country;
    delete submitValues.state;
    delete submitValues.city;

    await onSubmit(submitValues as T);
  };

  return (
    <div>
      {/* Encabezado */}
      <div className="flex justify-between mb-6">
        <Space>
          <BankOutlined className="text-2xl text-blue-600" />
          <span className="text-xl font-semibold">
            {isEdit
              ? "Actualizar Información del Hospital"
              : "Registrar Información del Hospital"}
          </span>
        </Space>
        <Button icon={<ArrowLeftOutlined />} onClick={onCancel}>
          Cancelar
        </Button>
      </div>

      {/* Formulario */}
      <ProForm
        initialValues={initialValues}
        onFinish={handleSubmit}
        formRef={formRef}
        submitter={{
          searchConfig: {
            submitText: isEdit ? "Actualizar" : "Registrar",
          },
          resetButtonProps: {
            style: {
              display: "none",
            },
          },
        }}
        loading={loading}
        layout="vertical"
      >
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <ProFormText
            name="name"
            label="Nombre del Hospital"
            placeholder="Ej. Hospital de Occidente"
            rules={[
              { required: true, message: "El nombre es obligatorio" },
              { min: 3, message: "Mínimo 3 caracteres" },
            ]}
          />

          <ProFormText
            name="hospitalCode"
            label="Código del Hospital"
            placeholder="Ej. HSP0401"
            rules={[
              { required: false, message: "El código es obligatorio" },
              {
                pattern: /^[A-Z0-9]+$/,
                message: "Solo letras mayúsculas y números",
              },
            ]}
          />

          <ProFormText
            name="director"
            label="Director"
            placeholder="Ej. Dra Rosa Maria del Carmen"
            rules={[{ required: true, message: "El director es obligatorio" }]}
          />

          <ProFormText
            name="subdirector"
            label="Subdirector"
            placeholder="Ej. Dr Mario Hugo Lopez"
            rules={[
              { required: false, message: "El subdirector es obligatorio" },
            ]}
          />

          {/* Selectores de ubicación */}
          <div className="md:col-span-2">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
              <ProFormSelect
                name="country"
                label="País"
                placeholder="Seleccionar"
                options={countryOptions}
                fieldProps={{
                  showSearch: true,
                  onChange: (value) => handleCountryChange(value as string),
                  value: selectedCountry || undefined,
                  filterOption: (input, option) =>
                    (option?.label ?? "")
                      .toString()
                      .toLowerCase()
                      .includes(input.toLowerCase()),
                }}
                rules={[{ required: false, message: "Seleccione un país" }]}
              />

              <ProFormSelect
                name="state"
                label="Estado"
                placeholder="Seleccionar"
                options={stateOptions}
                fieldProps={{
                  showSearch: true,
                  onChange: (value) => handleStateChange(value as string),
                  value: selectedState || undefined,
                  disabled: !selectedCountry,
                  filterOption: (input, option) =>
                    (option?.label ?? "")
                      .toString()
                      .toLowerCase()
                      .includes(input.toLowerCase()),
                }}
                rules={[
                  { required: false, message: "Seleccione un departamento" },
                ]}
              />

              <ProFormSelect
                name="city"
                label="Ciudad"
                placeholder="Seleccionar"
                options={cityOptions}
                fieldProps={{
                  showSearch: true,
                  onChange: (value) => handleCityChange(value as string),
                  value: selectedCity || undefined,
                  disabled: !selectedState,
                  filterOption: (input, option) =>
                    (option?.label ?? "")
                      .toString()
                      .toLowerCase()
                      .includes(input.toLowerCase()),
                }}
                rules={[{ required: false, message: "Seleccione una ciudad" }]}
              />
            </div>

            {/* Campo de detalles */}
            <ProFormText
              name="details"
              label="Detalles"
              placeholder="Ej. Avenida Principal 123"
              rules={[
                { required: false, message: "Los detalles son opcionales" },
              ]}
            />
          </div>

          <Form.Item
            name="phoneNumber"
            label="Número de Teléfono"
            rules={[
              {
                required: true,
                message: "El teléfono es obligatorio",
                validator: (_, value) => {
                  if (!value) return Promise.resolve();
                  if (value && value.length < 5) {
                    return Promise.reject(
                      new Error("Número de teléfono muy corto")
                    );
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
                width: "100%",
                padding: "4px 11px",
              }}
            />
          </Form.Item>

          <ProFormText
            name="email"
            label="Correo Electrónico"
            placeholder="Ej. hospital@example.com"
            rules={[
              { required: false, message: "El correo es obligatorio" },
              { type: "email", message: "Correo inválido" },
            ]}
          />

          <ProFormText
            name="website"
            label="Sitio Web"
            placeholder="Ej. www.hospital-occidente.com"
            rules={[
              { required: false, message: "El sitio web es obligatorio" },
            ]}
          />

          <ProFormText
            name="rtn"
            label="RTN"
            placeholder="Ej. 04019006506550"
            rules={[
              { required: false, message: "El RTN es obligatorio" },
              { pattern: /^[0-9]{14}$/, message: "Debe tener 14 dígitos" },
            ]}
          />

          <ProFormText
            name="currency"
            label="Moneda"
            placeholder="Ej. LPS"
            rules={[
              { required: false, message: "La moneda es obligatoria" },
              {
                pattern: /^[A-Z]{3}$/,
                message: "Código de 3 letras (Ej: LPS, USD)",
              },
            ]}
            initialValue="LPS"
          />
        </div>
      </ProForm>
    </div>
  );
};