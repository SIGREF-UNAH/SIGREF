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
import { useNavigate, useParams } from "react-router-dom";
import { useRef, useState, useMemo, useEffect } from "react";
import { BsBuilding, BsGeoAltFill, BsPersonFill } from "react-icons/bs";
import { Button, message, Spin } from "antd";
import { DeleteOutlined, PlusOutlined } from "@ant-design/icons";
import type { ProFormInstance } from "@ant-design/pro-components";
import {
  useGetApiLocations,
  useGetApiLocationsId,
  usePutApiLocationsId,
} from "../../../../api/locations/locations";
import { useGetApiOrganizations } from "../../../../api/organizations/organizations";
import ccsj from "countrycitystatejson";

interface CountryOption {
  label: string;
  value: string;
}

interface StateOption {
  label: string;
  value: string;
}

interface CityOption {
  label: string;
  value: string;
}

export default function EditLocation() {
  const { id } = useParams();
  const numericId = Number(id);
  const navigate = useNavigate();
  const formRef = useRef<ProFormInstance>(null);

  // Estado de contactos dinámicos
  const [contacts, setContacts] = useState<
    { id: string; system?: number; value?: string }[]
  >([{ id: "1", system: undefined, value: "" }]);

  // Cargar la ubicación actual
  const {
    data: location,
    isLoading: locationLoading,
    isError: locationError,
  } = useGetApiLocationsId(numericId);

  // Mutación para actualizar
  const { mutate: updateLocation, isPending: isUpdating } =
    usePutApiLocationsId({
      mutation: {
        onSuccess: () => {
          message.success("Ubicación actualizada con éxito");
          navigate("/locations/list");
        },
        onError: () => {
          message.error("Error al actualizar la ubicación");
        },
      },
    });

  // Cargar las organizaciones
  const {
    data: orgsData,
    isLoading: orgsLoading,
    isError: orgsError,
  } = useGetApiOrganizations(undefined, {
    query: {
      select: (data) => data.items || [],
    },
  });

  // Opciones para país, estado y ciudad
  const [countryOptions, setCountryOptions] = useState<CountryOption[]>([]);
  const [stateOptions, setStateOptions] = useState<StateOption[]>([]);
  const [cityOptions, setCityOptions] = useState<CityOption[]>([]);

  useEffect(() => {
    const countries = ccsj.getCountries().map((c: any) => ({
      label: `${c.name}`,
      value: c.shortName,
    }));
    setCountryOptions(countries);
  }, []);

  // Cargar todas las ubicaciones
  const {
    data: locationsData,
    isLoading: locationsLoading,
    isError: locationsError,
  } = useGetApiLocations(undefined, {
    query: {
      select: (data) => data.items || [],
    },
  });

  // Opciones para selects
  const organizationOptions = useMemo(
    () =>
      (orgsData || []).map((org) => ({
        label: org.name,
        value: org.id,
      })),
    [orgsData]
  );

  // Filtrar las ubicaciones excluyendo la actual
  const locationOptions = useMemo(
    () =>
      (locationsData || [])
        .filter((loc) => String(loc.id) !== String(numericId))
        .map((loc) => ({
          label: loc.name,
          value: loc.id,
        })),
    [locationsData, numericId]
  );

  // Cargar datos de la ubicación en el formulario
  useEffect(() => {
    if (location && formRef.current) {
      if (location.telecom && location.telecom.length > 0) {
        setContacts(
          location.telecom.map((t: any, index: number) => ({
            id: Date.now().toString() + index,
            system: t.system,
            value: t.value,
          }))
        );
      }

      // Cargar estados y ciudades si hay país seleccionado
      if (location.address?.country) {
        const states = ccsj.getStatesByShort(location.address.country) ?? [];
        setStateOptions(states.map((s: string) => ({ label: s, value: s })));

        if (location.address?.state) {
          const cities =
            ccsj.getCities(location.address.country, location.address.state) ??
            [];
          setCityOptions(cities.map((c: string) => ({ label: c, value: c })));
        }
      }

      // Establecer valores del formulario
      formRef.current.setFieldsValue({
        name: location.name || "",
        alias: location.alias || [],
        description: location.description || null,
        status: location.status ?? LocationStatus.NUMBER_0,
        mode: location.mode ?? LocationMode.NUMBER_0,
        address: {
          line: location.address?.line || [""],
          city: location.address?.city || null,
          state: location.address?.state || null,
          postalCode: location.address?.postalCode || null,
          country: location.address?.country || null,
        },
        type: location.type || null,
        partOfId: location.partOfId || null,
        partOfName: location.partOfName || null,
        managingOrganizationId: location.managingOrganizationId || null,
        managingOrganizationName: location.managingOrganizationName || null,
        // Cargar telecom en el formulario
        telecom:
          location.telecom?.map((t: any) => ({
            system: t.system,
            value: t.value,
          })) || [],
      });
    }
  }, [location]);

  // Envío del formulario
  const onFinish = async (values: any) => {
    const payload = {
      ...values,
      telecom: contacts.map((c) => ({
        system: c.system,
        value: c.value,
      })),
    };

    updateLocation({ id: numericId, data: payload });
  };

  const addContact = () => {
    setContacts((prev) => [
      ...prev,
      { id: Date.now().toString(), system: undefined, value: "" },
    ]);
  };

  const removeContact = (id: string) => {
    if (contacts.length > 1) {
      setContacts((prev) => prev.filter((c) => c.id !== id));
    }
  };

  if (locationLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <Spin size="large" />
      </div>
    );
  }

  if (locationError || !location) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="text-center">
          <p className="text-red-500 text-lg mb-4">
            Error al cargar la ubicación
          </p>
          <Button type="primary" onClick={() => navigate("/locations/list")}>
            Volver al listado
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-[#FAFAFA] rounded-lg border-2 border-[#D9D9D9] p-6">
      {/* Header */}
      <div className="flex items-center gap-3 mb-8">
        <MdOutlineAddLocationAlt className="w-10 h-10 text-blue-500" />
        <span className="text-xl font-semibold text-[#333333]">
          Editar Ubicación
        </span>
      </div>

      <ProForm
        formRef={formRef}
        onFinish={onFinish}
        submitter={{
          searchConfig: { submitText: "Actualizar Ubicación" },
          resetButtonProps: false,
          submitButtonProps: {
            size: "large",
            loading: isUpdating,
            icon: <FaCheck className="w-4 h-4" />,
            className:
              "px-6 !bg-green-500 hover:!bg-green-600 text-white font-medium rounded-md transition-colors duration-200 flex items-center gap-2",
          },
          render: (_, dom) => (
            <div className="flex justify-end gap-4 pt-2 pb-2">
              <Button
                type="default"
                onClick={() => navigate("/locations/list")}
                className="border-gray-300 hover:border-blue-500"
                size="large"
              >
                Cancelar
              </Button>
              {dom[0]}
            </div>
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

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-2">
            <ProFormText
              name="name"
              label="Nombre de la ubicación"
              placeholder="Ej. Sala de emergencias"
              rules={[
                { required: true, message: "El nombre es obligatorio" },
                { min: 3, message: "Debe tener al menos 3 caracteres" },
              ]}
            />

            <ProFormSelect
              name="alias"
              label="Alias"
              mode="tags"
              placeholder="Ej. Emergencias, ER (Presione Enter para agregar)"
              fieldProps={{
                tokenSeparators: [","],
                onChange: (value) => {
                  formRef.current?.setFieldValue("alias", value);
                },
              }}
              rules={[
                {
                  validator: (_, value) =>
                    value && value.length > 5
                      ? Promise.reject("Máximo 5 alias permitidos")
                      : Promise.resolve(),
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
              rules={[{ required: true, message: "El estado es obligatorio" }]}
              fieldProps={{
                suffixIcon: (
                  <BiChevronDown className="w-4 h-4 text-[#616161]" />
                ),
              }}
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-2">
            <ProFormSelect
              name="mode"
              label="Modo"
              options={[
                { label: "Instance", value: LocationMode.NUMBER_0 },
                { label: "Kind", value: LocationMode.NUMBER_1 },
              ]}
              rules={[{ required: true, message: "El modo es obligatorio" }]}
              fieldProps={{
                suffixIcon: (
                  <BiChevronDown className="w-4 h-4 text-[#616161]" />
                ),
              }}
            />
            <ProFormText
              name="type"
              label="Tipo de función"
              placeholder="Ej. Cuarto de emergencias"
            />
          </div>

          <ProFormTextArea
            name="description"
            label="Descripción"
            placeholder="Descripción adicional"
            fieldProps={{ rows: 2 }}
          />
        </section>

        {/* Dirección física */}
        <section className="mt-6">
          <div className="flex items-center gap-3 mb-6">
            <BsGeoAltFill className="w-8 h-8 text-blue-500" />
            <span className="text-lg font-semibold text-[#333333]">
              Dirección física
            </span>
          </div>

          <ProFormText
            name={["address", "line", 0]}
            label="Dirección"
            placeholder="Ej. Avenida principal 123"
            rules={[{ required: true, message: "La dirección es obligatoria" }]}
          />

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <ProFormSelect
              name={["address", "country"]}
              label="País"
              placeholder="Seleccione país"
              options={countryOptions}
              showSearch
              allowClear
              fieldProps={{
                onChange: (countryShort?: string) => {
                  if (!countryShort || typeof countryShort !== "string") {
                    setStateOptions([]);
                    setCityOptions([]);
                    formRef.current?.setFieldValue(["address", "state"], null);
                    formRef.current?.setFieldValue(["address", "city"], null);
                    return;
                  }

                  const states = ccsj.getStatesByShort(countryShort) ?? [];
                  setStateOptions(
                    states.map((s: string) => ({ label: s, value: s }))
                  );
                  setCityOptions([]);
                  formRef.current?.setFieldValue(["address", "state"], null);
                  formRef.current?.setFieldValue(["address", "city"], null);
                },
                suffixIcon: (
                  <BiChevronDown className="w-4 h-4 text-[#616161]" />
                ),
              }}
            />

            <ProFormSelect
              name={["address", "state"]}
              label="Estado / Provincia"
              placeholder="Seleccione estado"
              options={stateOptions}
              showSearch
              allowClear
              fieldProps={{
                onChange: (stateName?: string) => {
                  const countryShort = formRef.current?.getFieldValue([
                    "address",
                    "country",
                  ]);
                  if (
                    !countryShort ||
                    typeof countryShort !== "string" ||
                    !stateName
                  ) {
                    setCityOptions([]);
                    formRef.current?.setFieldValue(["address", "city"], null);
                    return;
                  }

                  const cities = ccsj.getCities(countryShort, stateName) ?? [];
                  setCityOptions(
                    cities.map((c: string) => ({ label: c, value: c }))
                  );
                  formRef.current?.setFieldValue(["address", "city"], null);
                },
                suffixIcon: (
                  <BiChevronDown className="w-4 h-4 text-[#616161]" />
                ),
              }}
            />

            <ProFormSelect
              name={["address", "city"]}
              label="Ciudad"
              placeholder="Seleccione ciudad"
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
        </section>

        {/* Información de contacto */}
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
              Agregar contacto
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
                  label={index === 0 ? "Tipo de contacto" : undefined}
                  placeholder="Seleccione tipo"
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
                    onChange: (value) => {
                      setContacts((prev) =>
                        prev.map((c) =>
                          c.id === contact.id
                            ? { ...c, system: value, value: c.value }
                            : c
                        )
                      );
                      // Actualizar el valor en el formulario
                      formRef.current?.setFieldValue(
                        ["telecom", index, "system"],
                        value
                      );
                    },
                    suffixIcon: (
                      <BiChevronDown className="w-4 h-4 text-[#616161]" />
                    ),
                  }}
                  rules={[
                    {
                      required: true,
                      message: "Seleccione el tipo de contacto",
                    },
                  ]}
                />

                <ProFormText
                  name={["telecom", index, "value"]}
                  label={index === 0 ? "Valor" : undefined}
                  placeholder={
                    contact.system === 0
                      ? "Ej. +504 2550-1234"
                      : contact.system === 2
                        ? "Ej. correo@dominio.com"
                        : contact.system === 4
                          ? "Ej. https://miweb.com"
                          : "Ingrese valor"
                  }
                  fieldProps={{
                    value: contact.value,
                    type:
                      contact.system === 2
                        ? "email"
                        : contact.system === 4
                          ? "url"
                          : "text",
                    onChange: (e) => {
                      setContacts((prev) =>
                        prev.map((c) =>
                          c.id === contact.id
                            ? { ...c, value: e.target.value }
                            : c
                        )
                      );
                      // Actualizar el valor en el formulario
                      formRef.current?.setFieldValue(
                        ["telecom", index, "value"],
                        e.target.value
                      );
                    },
                  }}
                  rules={[
                    { required: true, message: "Ingrese el valor" },
                    {
                      validator: (_, value) => {
                        if (!value) return Promise.resolve();
                        if (
                          contact.system === 2 &&
                          !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)
                        ) {
                          return Promise.reject(
                            "Ingrese un correo electrónico válido"
                          );
                        }
                        if (
                          contact.system === 4 &&
                          !/^https?:\/\/.+/.test(value)
                        ) {
                          return Promise.reject("Ingrese una URL válida");
                        }
                        if (
                          contact.system === 0 &&
                          !/^[+0-9\s-]{6,}$/.test(value)
                        ) {
                          return Promise.reject(
                            "Ingrese un número de teléfono válido"
                          );
                        }
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
                  className="mb-1"
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
              name="managingOrganizationId"
              label="Organización responsable"
              placeholder="Seleccione una organización"
              options={organizationOptions}
              showSearch
              allowClear
              fieldProps={{
                loading: orgsLoading,
                suffixIcon: (
                  <BiChevronDown className="w-4 h-4 text-[#616161]" />
                ),
                optionFilterProp: "label",
                filterOption: (input, option) =>
                  (option?.label ?? "")
                    .toLowerCase()
                    .includes(input.toLowerCase()),
                onChange: (value, option) => {
                  const opt = option as {
                    label: string;
                    value: string | null | undefined;
                  };
                  formRef.current?.setFieldValue(
                    "managingOrganizationName",
                    opt?.label ?? null
                  );
                },
              }}
              tooltip={orgsError ? "Error al cargar organizaciones" : undefined}
            />

            {/* Campo oculto para enviar el nombre */}
            <ProFormText name="managingOrganizationName" hidden />

            <ProFormSelect
              name="partOfId"
              label="Parte de (ubicación padre)"
              placeholder="Seleccione ubicación padre"
              options={locationOptions}
              showSearch
              allowClear
              fieldProps={{
                loading: locationsLoading,
                suffixIcon: (
                  <BiChevronDown className="w-4 h-4 text-[#616161]" />
                ),
                optionFilterProp: "label",
                filterOption: (input, option) =>
                  (option?.label ?? "")
                    .toLowerCase()
                    .includes(input.toLowerCase()),
                onChange: (value, option) => {
                  const opt = option as {
                    label: string;
                    value: string | null | undefined;
                  };
                  formRef.current?.setFieldValue(
                    "partOfName",
                    opt?.label ?? null
                  );
                },
              }}
              tooltip={
                locationsError ? "Error al cargar ubicaciones" : undefined
              }
            />

            {/* Campo oculto para enviar el nombre del padre */}
            <ProFormText name="partOfName" hidden />
          </div>
        </section>
      </ProForm>
    </div>
  );
}
