import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormTextArea,
} from "@ant-design/pro-components";
import { BiChevronDown } from "react-icons/bi";
import { MdOutlineAddLocationAlt } from "react-icons/md";
import { FaCheck } from "react-icons/fa";
import { LocationStatus, LocationMode } from "../../../../api/models";
import { BsBuilding, BsGeoAltFill, BsPersonFill } from "react-icons/bs";
import { Button, Spin } from "antd";
import { DeleteOutlined, PlusOutlined } from "@ant-design/icons";
import useLocationForm from "../../hooks/useLocationForm";

interface LocationFormProps {
  mode: "create" | "edit";
}

export default function LocationForm({ mode }: LocationFormProps) {
  const {
    formRef,
    onFinish,
    isSubmitting,
    contacts,
    countryOptions,
    stateOptions,
    cityOptions,
    organizationOptions,
    locationOptions,
    locationLoading,
    locationError,
    orgsLoading,
    orgsError,
    handleCountryChange,
    handleStateChange,
    addContact,
    removeContact,
    updateContact,
    isEdit,
    title,
  } = useLocationForm({ mode });

  if (isEdit && locationLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <Spin size="large" />
      </div>
    );
  }

  if (isEdit && locationError) {
    return (
      <div className="text-center py-10">
        <p className="text-red-500 mb-4">Error al cargar la ubicación</p>
        <Button type="primary" onClick={() => window.history.back()}>
          Volver
        </Button>
      </div>
    );
  }

  return (
    <div className="primary-card">
      <div className="flex items-center gap-3 mb-8">
        <MdOutlineAddLocationAlt className="w-10 h-10 text-blue-500" />
        <span className="text-xl font-semibold text-[#333333]">{title}</span>
      </div>

      <ProForm
        formRef={formRef}
        onFinish={onFinish}
        initialValues={{
          name: "",
          alias: [],
          description: null,
          status: LocationStatus.NUMBER_0,
          mode: LocationMode.NUMBER_0,
          address: {
            line: [""],
            city: null,
            state: null,
            postalCode: null,
            country: null,
          },
          type: null,
          partOf: null,
          managingOrganization: null,
          telecom: [],
        }}
        submitter={{
          searchConfig: { submitText: isEdit ? "Actualizar" : "Crear" },
          resetButtonProps: false,
          submitButtonProps: {
            loading: isSubmitting,
            icon: <FaCheck className="w-4 h-4" />,
            className:
              "px-6 py-2 !bg-green-500 hover:!bg-green-600 text-white font-medium rounded-md transition-colors duration-200 flex items-center gap-2",
          },
          render: (_, dom) =>
            isEdit ? (
              <div className="flex justify-end gap-4 pt-2 pb-2">
                <Button type="default" onClick={() => window.history.back()}>
                  Cancelar
                </Button>
                {dom[0]}
              </div>
            ) : (
              <div className="flex justify-end pt-2 pb-2">{dom[0]}</div>
            ),
        }}
      >
        {/* Información Básica */}
        <section>
          <div className="flex items-center gap-3 mb-6">
            <BsBuilding className="w-8 h-8 text-blue-500" />
            <span className="text-lg font-semibold text-[#333333]">
              Información Básica
            </span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
            <ProFormText
              name="name"
              label="Nombre"
              placeholder="Ej. Sala de emergencias"
              rules={[{ required: true }, { min: 3 }]}
            />
            <ProFormSelect
              name="alias"
              label="Alias"
              mode="tags"
              placeholder="Presione Enter"
              rules={[
                {
                  validator: (_, v) =>
                    v?.length > 5 ? Promise.reject("Máx 5") : Promise.resolve(),
                },
              ]}
            />
            <ProFormSelect
              name="status"
              label="Estado"
              options={[
                { label: "Activo", value: LocationStatus.NUMBER_0 },
                { label: "Suspendido", value: LocationStatus.NUMBER_1 },
                { label: "Inactivo", value: LocationStatus.NUMBER_2 },
              ]}
              rules={[{ required: true }]}
              fieldProps={{
                suffixIcon: (
                  <BiChevronDown className="w-4 h-4 text-[#616161]" />
                ),
              }}
            />
            <ProFormSelect
              name="mode"
              label="Modo"
              options={[
                { label: "Instancia", value: LocationMode.NUMBER_0 },
                { label: "Tipo", value: LocationMode.NUMBER_1 },
              ]}
              rules={[{ required: true }]}
              fieldProps={{
                suffixIcon: (
                  <BiChevronDown className="w-4 h-4 text-[#616161]" />
                ),
              }}
            />
          </div>

          {/* <ProFormText name="type" label="Tipo de función" /> */}

          <ProFormTextArea
            name="description"
            label="Descripción"
            fieldProps={{ rows: 3 }}
          />
        </section>

        {/* Dirección */}
        <section className="mt-6 border-t pt-6">
          <div className="flex items-center gap-3 mb-6">
            <BsGeoAltFill className="w-8 h-8 text-blue-500" />
            <span className="text-lg font-semibold text-[#333333]">
              Dirección física
            </span>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <ProFormSelect
              name={["address", "country"]}
              label="País"
              options={countryOptions}
              showSearch
              allowClear
              rules={[{ required: true }]}
              fieldProps={{
                onChange: handleCountryChange,
                suffixIcon: (
                  <BiChevronDown className="w-4 h-4 text-[#616161]" />
                ),
              }}
            />
            <ProFormSelect
              name={["address", "state"]}
              label="Estado"
              options={stateOptions}
              showSearch
              allowClear
              rules={[{ required: true }]}
              fieldProps={{
                onChange: handleStateChange,
                suffixIcon: (
                  <BiChevronDown className="w-4 h-4 text-[#616161]" />
                ),
              }}
            />
            <ProFormSelect
              name={["address", "city"]}
              label="Ciudad"
              options={cityOptions}
              showSearch
              allowClear
              fieldProps={{
                suffixIcon: (
                  <BiChevronDown className="w-4 h-4 text-[#616161]" />
                ),
              }}
            />
          </div>
          <ProFormText
            name={["address", "line", 0]}
            label="Dirección"
            rules={[{ required: true }]}
          />
        </section>

        {/* Contactos */}
        <section className="mb-8 border-t pt-6">
          <div className="mb-4 flex items-center justify-between">
            <div className="flex items-center gap-2">
              <BsPersonFill className="w-8 h-8 text-blue-500" />
              <span className="text-lg font-semibold text-[#333333]">
                Información de contacto
              </span>
            </div>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={addContact}
              className="bg-green-500 hover:bg-green-600"
            >
              Agregar
            </Button>
          </div>

          <div className="space-y-6">
            {contacts.map((contact, index) => (
              <div
                key={contact.id}
                className="grid grid-cols-[1fr_2fr_auto] items-end gap-4 border-b pb-4"
              >
                <ProFormSelect
                  name={["telecom", index, "system"]}
                  label={index === 0 ? "Tipo" : undefined}
                  options={[
                    { label: "Teléfono", value: 0 },
                    { label: "Fax", value: 1 },
                    { label: "Correo electrónico", value: 2 },
                    { label: "Pager", value: 3 },
                    { label: "Dirección web", value: 4 },
                    { label: "Mensaje de texto", value: 5 },
                    { label: "Otro", value: 6 },
                  ]}
                  fieldProps={{
                    value: contact.system,
                    onChange: (v) => updateContact(contact.id, "system", v),
                    suffixIcon: (
                      <BiChevronDown className="w-4 h-4 text-[#616161]" />
                    ),
                  }}
                  rules={[{ required: true }]}
                />
                <ProFormText
                  name={["telecom", index, "value"]}
                  label={index === 0 ? "Valor" : undefined}
                  fieldProps={{
                    value: contact.value,
                    type:
                      contact.system === 2
                        ? "email"
                        : contact.system === 4
                          ? "url"
                          : "text",
                    onChange: (e) =>
                      updateContact(contact.id, "value", e.target.value),
                  }}
                  rules={[
                    { required: true },
                    {
                      validator: (_, v) => {
                        if (!v) return Promise.resolve();
                        if (
                          contact.system === 2 &&
                          !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v)
                        )
                          return Promise.reject("Correo inválido");
                        if (contact.system === 4 && !/^https?:\/\/.+/.test(v))
                          return Promise.reject("URL inválida");
                        return Promise.resolve();
                      },
                    },
                  ]}
                />
                <Button
                  danger
                  icon={<DeleteOutlined />}
                  onClick={() => removeContact(contact.id)}
                  disabled={contacts.length === 1}
                />
              </div>
            ))}
          </div>
        </section>

        {/* Jerarquía */}
        <section className="mt-6">
          <div className="flex items-center gap-3 mb-6">
            <BsBuilding className="w-8 h-8 text-blue-500" />
            <span className="text-lg font-semibold text-[#333333]">
              Organización y jerarquía
            </span>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <ProFormSelect
              name="managingOrganization"
              label="Organización"
              options={organizationOptions}
              showSearch
              allowClear
              fieldProps={{
                loading: orgsLoading,
              }}
              tooltip={orgsError ? "Error al cargar" : undefined}
            />

            <ProFormSelect
              name="partOf"
              label="Ubicación padre"
              options={locationOptions}
              showSearch
              allowClear
            />
          </div>
        </section>
      </ProForm>
    </div>
  );
}
