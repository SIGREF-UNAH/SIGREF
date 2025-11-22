import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormDatePicker,
  ProFormGroup,
  ProFormList,
} from "@ant-design/pro-components";
import {
  GlobalOutlined,
  IdcardOutlined,
  UserOutlined,
  PhoneOutlined,
  HomeOutlined,
  PlusOutlined,
} from "@ant-design/icons";
import { Button, Collapse, Space, Form } from "antd";
import type { CollapseProps } from "antd";
import { Link } from "react-router-dom";
import PhoneInput from 'react-phone-number-input';
import 'react-phone-number-input/style.css';
import usePatientForm from "../hooks/usePatientForm";

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
  onSubmit,
}: PatientFormProps) {
  
  const {
    contextHolder,
    contactTypes,
    countryOptions,
    addressLocations,
    formRef,
    onFinish,
    onCancel,
    handleContactTypeChange,
    getPlaceholderByType,
    handleCountryChange,
    handleStateChange,
  } = usePatientForm(onSubmit, mode, initialValues);

  // Función para renderizar el input según el tipo de contacto
  const renderContactInput = (field: any, index: number) => {
    const contactType = contactTypes[index];
    
    if (contactType === "Email") {
      return (
        <ProFormText
          {...(mode === "create" ? field : {})}
          rules={[{ type: "email", message: "Ingrese un email válido" }]}
          name="value"
          label="Valor"
          placeholder="Ej. ejemplo@correo.com"
          width="md"
        />
      );
    } else if ((contactType === "Phone" || contactType === "Fax") && mode === "create") {
      // Solo usar PhoneInput en modo crear para Phone y Fax
      return (
        <Form.Item
          name={["telecom", index, "value"]}
          label="Valor"
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
            value={formRef.current?.getFieldValue(["telecom", index, "value"])}
            onChange={(value) => {
              formRef.current?.setFieldValue(["telecom", index, "value"], value || "");
            }}
            className="ant-input bg-white rounded px-3 py-2 border border-gray-300 hover:border-blue-400 focus:border-blue-400 focus:shadow-outline"
            style={{
              width: '100%',
              padding: '4px 11px',
            }}
          />
        </Form.Item>
      );
    } else if ((contactType === "Phone" || contactType === "Fax") && mode === "edit") {
      // En modo editar usar ProFormText normal para Phone y Fax
      return (
        <ProFormText
          {...field}
          name="value"
          label="Valor"
          placeholder={contactType === "Phone" ? "Ej. +504 1234-5678" : "Ej. +504 1234-5678"}
          width="md"
        />
      );
    } else {
      // Input por defecto para otros tipos
      return (
        <ProFormText
          {...(mode === "create" ? field : {})}
          name="value"
          label="Valor"
          placeholder={`Ej. ${getPlaceholderByType(contactType)}`}
          width="md"
        />
      );
    }
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
        fieldProps={{
          disabled: mode === "create"
        }}
        initialValue={mode === "create" ? 1 : undefined}
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
      {(field, index) => (
        <ProFormGroup key={field.key}>
          <ProFormSelect
            {...(mode === "create" ? field : {})}
            name="system"
            label="Tipo de Contacto"
            placeholder="Seleccionar"
            width="sm"
            options={[
              { label: "Teléfono", value: "Phone" },
              { label: "Correo electrónico", value: "Email" },
              { label: "Dirección web", value: "Url" },
              { label: "Pager", value: "Pager" },
              { label: "Fax", value: "Fax" },
              { label: "Mensaje de texto", value: "SMS" },
              { label: "Otro", value: "Other" },
            ]}
            fieldProps={{
              onChange: (value) => handleContactTypeChange(index, value as string),
            }}
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

          {renderContactInput(field, index)}
        </ProFormGroup>
      )}
    </ProFormList>
  );

  // Sección de direcciones
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
            name={mode === "create" ? ["line", 0] : ["line"]}
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
          <HomeOutlined style={{ color: "green" }} />
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