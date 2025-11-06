import { useState, useEffect } from "react";
import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormDatePicker,
  ProFormGroup,
  ProFormList,
  type ProFormInstance,
} from "@ant-design/pro-components";
import {
  GlobalOutlined,
  IdcardOutlined,
  UserOutlined,
  PhoneOutlined,
  HomeOutlined,
  PlusOutlined,
} from "@ant-design/icons";
import { Button, Collapse, message, Space } from "antd";
import type { CollapseProps } from "antd";
import { Link } from "react-router-dom";
import ccsj from "countrycitystatejson";
import { useRef } from "react";

// TODO: Validar campos de contecto y añadir libreria de codigos de telefono

interface PatientFormProps {
  mode: "create" | "edit";
  initialValues?: any;
  onSubmit: (values: any) => Promise<boolean>;
  isSubmitting: boolean;
  error?: string | null;
}

export default function PatientForm({
  mode,
  initialValues,
  isSubmitting,
  error,
  onSubmit,
}: PatientFormProps) {
  const [messageApi, contextHolder] = message.useMessage();
  const formRef = useRef<ProFormInstance>(null);

  // Estados para los selectores de ubicación
  const [countryOptions] = useState(() =>
    ccsj.getCountries().map((c: any) => ({
      label: c.name,
      value: c.shortName,
    }))
  );

  // Estado para guardar las opciones de estados y ciudades por cada dirección
  const [addressLocations, setAddressLocations] = useState<{
    [key: number]: {
      stateOptions: { label: string; value: string }[];
      cityOptions: { label: string; value: string }[];
    };
  }>({});

  // Función para manejar el cambio de país
  const handleCountryChange = (index: number, countryShort?: string) => {
    if (!countryShort) {
      setAddressLocations((prev) => ({
        ...prev,
        [index]: { stateOptions: [], cityOptions: [] },
      }));
      // Limpiar campos de estado y ciudad en el formulario
      if (formRef.current) {
        formRef.current.setFieldValue(["address", index, "state"], null);
        formRef.current.setFieldValue(["address", index, "city"], null);
      }
      return;
    }

    const states = ccsj.getStatesByShort(countryShort) ?? [];
    setAddressLocations((prev) => ({
      ...prev,
      [index]: {
        stateOptions: states.map((s) => ({ label: s, value: s })),
        cityOptions: [],
      },
    }));
    
    // Limpiar campos de estado y ciudad en el formulario
    if (formRef.current) {
      formRef.current.setFieldValue(["address", index, "state"], null);
      formRef.current.setFieldValue(["address", index, "city"], null);
    }
  };

  // Función para manejar el cambio de estado
  const handleStateChange = (
    index: number,
    countryShort: string,
    stateName?: string
  ) => {
    if (!countryShort || !stateName) {
      setAddressLocations((prev) => ({
        ...prev,
        [index]: {
          ...prev[index],
          cityOptions: [],
        },
      }));
      // Limpiar campo de ciudad en el formulario
      if (formRef.current) {
        formRef.current.setFieldValue(["address", index, "city"], null);
      }
      return;
    }

    const cities = ccsj.getCities(countryShort, stateName) ?? [];
    setAddressLocations((prev) => ({
      ...prev,
      [index]: {
        ...prev[index],
        cityOptions: cities.map((c) => ({ label: c, value: c })),
      },
    }));
    
    // Limpiar campo de ciudad en el formulario
    if (formRef.current) {
      formRef.current.setFieldValue(["address", index, "city"], null);
    }
  };

  // Inicializar opciones de estado y ciudad si hay valores iniciales
  useEffect(() => {
    if (mode === "edit" && initialValues?.address && formRef.current) {
      initialValues.address.forEach((addr: any, index: number) => {
        if (addr.country) {
          const states = ccsj.getStatesByShort(addr.country) ?? [];
          const stateOptions = states.map((s) => ({ label: s, value: s }));
          
          let cityOptions: { label: string; value: string }[] = [];
          if (addr.state) {
            const cities = ccsj.getCities(addr.country, addr.state) ?? [];
            cityOptions = cities.map((c) => ({ label: c, value: c }));
          }
          
          setAddressLocations((prev) => ({
            ...prev,
            [index]: { stateOptions, cityOptions },
          }));
        }
      });
    }
  }, [mode, initialValues]);

  // Función para enviar el formulario
  const onFinish = async (values: any) => {
    const success = await onSubmit(values);
    if (success) {
      messageApi.success(
        mode === "create"
          ? "Paciente creado exitosamente"
          : "Paciente actualizado exitosamente"
      );
    } else {
      messageApi.error(
        error ??
          (mode === "create"
            ? "Error al crear el paciente"
            : "Error al actualizar el paciente")
      );
    }
  };

  // Función para cancelar
  const onCancel = () => {
    messageApi.info("Operación cancelada");
  };

  // Sección de nacionalidad
  const nacionalidadContent = (
    <ProFormGroup>
      <ProFormSelect
        name="nacionalidad"
        label="País de Nacionalidad"
        placeholder="Seleccionar país"
        width="md"
        showSearch
        allowClear
        options={countryOptions}
      />
      <ProFormSelect
        name="gender"
        label="Género"
        placeholder="Seleccionar"
        width="sm"
        rules={[{ required: true, message: "Campo requerido" }]}
        options={[
          { label: "Masculino", value: 1 },
          { label: "Femenino", value: 2 },
          { label: "Otro", value: 3 },
          { label: "Desconocido", value: 0 },
        ]}
      />
      <ProFormSelect
        name="estadoCivil"
        label="Estado Civil"
        placeholder="Seleccionar"
        width="sm"
        rules={[{ required: true, message: "Campo requerido" }]}
        options={[
          { label: "Soltero/a", value: "U" },
          { label: "Casado/a", value: "M" },
          { label: "Divorciado/a", value: "D" },
          { label: "Viudo/a", value: "W" },
          { label: "Unión de hechos", value: "T" },
          { label: "Desconocido", value: "UNK" },
        ]}
      />
      <ProFormSelect
        name="estadoVital"
        label="Estado Vital"
        placeholder="Seleccionar"
        width="sm"
        rules={[{ required: true, message: "Campo requerido" }]}
        options={[
          { label: "Vivo/a", value: 1 },
          { label: "Fallecido/a", value: 0 },
        ]}
      />
      <ProFormDatePicker
        name="fechanacimiento"
        label="Fecha de Nacimiento"
        placeholder="Ej. 31/12/1999"
        width="md"
        rules={[{ required: true, message: "Campo requerido" }]}
      />
    </ProFormGroup>
  );

  // Sección de identificacion
  const identificacionesContent = (
    <ProFormGroup>
      <ProFormSelect
        name="tipoIdentificacion"
        label="Tipo de Identificación"
        placeholder="Seleccionar"
        width="sm"
        rules={[{ required: true, message: "Campo requerido" }]}
        options={[
          { label: "DNI", value: "DNI" },
          { label: "Pasaporte", value: "PPN" },
          { label: "Otro", value: "NI" },
        ]}
      />
      <ProFormText
        name={["identifier", 0, "value"]}
        label="Número / Código"
        placeholder="Ej. 0401202501031"
        width="md"
        rules={[{ required: true, message: "Campo requerido" }]}
      />
      <ProFormText
        name="emisor"
        label="Emisor"
        placeholder="Ej. RNP"
        width="sm"
      />
    </ProFormGroup>
  );

  // Sección de nombre
  const nombresContent = (
    <ProFormGroup>
      <ProFormSelect
        name="tipoNombre"
        label="Tipo de Nombre"
        placeholder="Seleccionar"
        width="sm"
        rules={[{ required: true, message: "Campo requerido" }]}
        options={[
          { label: "Legal", value: "Official" },
          { label: "Alias", value: "Usual" },
        ]}
      />
      <ProFormText
        name="primerNombre"
        label="Primer Nombre"
        placeholder="Ej. Juan"
        width="md"
        rules={[{ required: true, message: "Campo requerido" }]}
      />
      <ProFormText
        name="segundoNombre"
        label="Segundo Nombre"
        placeholder="Ej. Ernesto"
        width="md"
      />
      <ProFormText
        name="apellidos"
        label="Apellidos"
        placeholder="Ej. Perez Lopez"
        width="md"
      />
    </ProFormGroup>
  );

  // Sección de contactos
  const contactoContent = (
    <ProFormList
      name="telecom"
      creatorButtonProps={{
        creatorButtonText: "Agregar contacto",
        icon: <PlusOutlined />,
      }}
    >
      {(field) => (
        <ProFormGroup key={field.key}>
          <ProFormSelect
            {...(mode === "create" ? field : {})}
            name="system"
            label="Tipo de Contacto"
            placeholder="Seleccionar"
            width="sm"
            options={[
              { label: "Teléfono", value: "Phone" },
              { label: "Email", value: "Email" },
              { label: "URL", value: "Url" },
              { label: "Biper", value: "Pager" },
              { label: "Fax", value: "Fax" },
              { label: "SMS", value: "SMS" },
              { label: "Otro", value: "Other" },
            ]}
          />

          <ProFormSelect
            {...(mode === "create" ? field : {})}
            name="use"
            label="Uso"
            placeholder="Seleccionar"
            width="sm"
            options={[
              { label: "Personal", value: "Mobile" },
              { label: "Hogar", value: "Home" },
              { label: "Trabajo", value: "Work" },
              { label: "Temporal", value: "Temp" },
              { label: "Antiguo", value: "Old" },
            ]}
          />

          <ProFormText
            {...(mode === "create" ? field : {})}
            name="value"
            label="Valor"
            placeholder="Ej. 9999-9999 / ejemplo@correo.com"
            width="md"
          />
        </ProFormGroup>
      )}
    </ProFormList>
  );

  // Sección de direcciones con selectores en cascada
  const direccionesContent = (
    <ProFormList
      name="address"
      creatorButtonProps={{
        creatorButtonText: "Agregar dirección",
        icon: <PlusOutlined />,
      }}
      {...(mode === "edit" && initialValues?.address
        ? { initialValue: initialValues.address }
        : {})}
    >
      {(field, index) => (
        <ProFormGroup key={field.key}>
          <ProFormSelect
            {...(mode === "create" ? field : {})}
            name="tipoDireccion"
            label="Tipo de Dirección"
            placeholder="Seleccionar"
            width="sm"
            options={[
              { label: "Hogar", value: "Home" },
              { label: "Trabajo", value: "Work" },
              { label: "Temporal", value: "Temp" },
              { label: "Antiguo", value: "Old" },
            ]}
          />

          <ProFormSelect
            {...(mode === "create" ? field : {})}
            name="country"
            label="País"
            placeholder="Seleccionar país"
            width="md"
            showSearch
            allowClear
            options={countryOptions}
            fieldProps={{
              onChange: (value) => handleCountryChange(index, value as string | undefined),
            }}
          />

          <ProFormSelect
            {...(mode === "create" ? field : {})}
            name="state"
            label="Departamento"
            placeholder="Seleccionar departamento"
            width="md"
            showSearch
            allowClear
            options={addressLocations[index]?.stateOptions || []}
            fieldProps={{
              onChange: (value) => {
                const countryValue = formRef.current?.getFieldValue([
                  "address",
                  index,
                  "country",
                ]) as string | undefined;
                handleStateChange(index, countryValue as string, value as string | undefined);
              },
            }}
          />

          <ProFormSelect
            {...(mode === "create" ? field : {})}
            name="city"
            label="Ciudad"
            placeholder="Seleccionar ciudad"
            width="md"
            showSearch
            allowClear
            options={addressLocations[index]?.cityOptions || []}
          />

          <ProFormText
            {...(mode === "create" ? field : {})}
            name={mode === "create" ? ["line", 0] : "line"}
            label="Detalles"
            placeholder="Ej. Ave 13, Calle 7, Casa 2"
            width="xl"
            {...(mode === "edit"
              ? { fieldProps: { style: { width: "100%" } } }
              : {})}
          />
        </ProFormGroup>
      )}
    </ProFormList>
  );

  const collapseItems: CollapseProps["items"] = [
    {
      key: "1",
      label: (
        <Space>
          <GlobalOutlined style={{ color: "#6366f1" }} />
          <span>Nacionalidad</span>
        </Space>
      ),
      children: nacionalidadContent,
    },
    {
      key: "2",
      label: (
        <Space>
          <IdcardOutlined style={{ color: "#ef4444" }} />
          <span>
            {mode === "create" ? "Identificación" : "Identificaciones"}
          </span>
        </Space>
      ),
      children: identificacionesContent,
    },
    {
      key: "3",
      label: (
        <Space>
          <UserOutlined style={{ color: "#6366f1" }} />
          <span>{mode === "create" ? "Nombre" : "Nombres"}</span>
        </Space>
      ),
      children: nombresContent,
    },
    {
      key: "4",
      label: (
        <Space>
          <PhoneOutlined style={{ color: "#f97316" }} />
          <span>{mode === "create" ? "Contactos" : "Contacto"}</span>
        </Space>
      ),
      children: contactoContent,
    },
    {
      key: "5",
      label: (
        <Space>
          <HomeOutlined style={{ color: "#ef4444" }} />
          <span>Direcciones</span>
        </Space>
      ),
      children: direccionesContent,
    },
  ];

  return (
    <div className="flex flex-col min-h-screen bg-gray-50">
      {contextHolder}

      <main className="flex-1 w-full">
        <ProForm
          formRef={formRef}
          onFinish={onFinish}
          {...(mode === "edit" && initialValues ? { initialValues } : {})}
          submitter={{
            render: (_) => (
              <div className="flex justify-end gap-3 mt-6">
                <Link to={"/patients/list"}>
                  <Button size="large" onClick={onCancel}>
                    Cancelar
                  </Button>
                </Link>
                <Button
                  type="primary"
                  size="large"
                  htmlType="submit"
                  loading={isSubmitting}
                >
                  {mode === "create" ? "Guardar" : "Guardar Cambios"}
                </Button>
              </div>
            ),
          }}
        >
          <Collapse
            defaultActiveKey={["1", "2", "3", "4", "5"]}
            items={collapseItems}
            className="mb-6"
          />
        </ProForm>
      </main>
    </div>
  );
}