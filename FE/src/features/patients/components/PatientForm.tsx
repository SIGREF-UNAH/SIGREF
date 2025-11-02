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
import { Button, Collapse, message, Space } from "antd";
import type { CollapseProps } from "antd";
import { Link } from "react-router-dom";

interface PatientFormProps {
  mode: "create" | "edit";
  initialValues?: any;
  onSubmit: (values: any) => Promise<boolean>;
  isSubmitting: boolean;
  error?: string | null;
}

// TODO: Implementar una librería para seleccionar el país
// TODO: Implementar una librería para el ingreso de un numero de telefono

export default function PatientForm({
  mode,
  initialValues,
  isSubmitting,
  error,
  onSubmit,
}: PatientFormProps) {
  const [messageApi, contextHolder] = message.useMessage();

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
      <ProFormText
        name={mode === "create" ? "paisNacionalidad" : "nacionalidad"}
        label="País de Nacionalidad"
        placeholder="Ej. Honduras"
        width="md"
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
        ]}
      />
      <ProFormSelect
        name="estadoCivil"
        label="Estado Civil"
        placeholder="Seleccionar"
        width="sm"
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
        options={[
          { label: "Vivo", value: 1 },
          { label: "Fallecido", value: 0 },
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
        label="Tipo"
        placeholder="Seleccionar"
        width="sm"
        options={[
          { label: "Legal", value: 0 },
          { label: "Alias", value: 1 },
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
            label="Tipo de contacto"
            placeholder="Seleccionar"
            width="sm"
            options={[
              { label: "Teléfono", value: "phone" },
              { label: "Email", value: "email" },
              { label: "URL", value: "url" },
              { label: "Fax", value: "fax" },
              { label: "Otro", value: "other" },
            ]}
          />

          <ProFormSelect
            {...(mode === "create" ? field : {})}
            name="use"
            label="Uso"
            placeholder="Seleccionar"
            width="sm"
            options={[
              { label: "Personal", value: "personal" },
              { label: "Casa", value: "home" },
              { label: "Trabajo", value: "work" },
              { label: "Temporal", value: "temp" },
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
      {(field) => (
        <ProFormGroup key={field.key}>
          <ProFormSelect
            {...(mode === "create" ? field : {})}
            name="tipoDireccion"
            label="Tipo"
            placeholder="Seleccionar"
            width="sm"
            options={[
              { label: "Casa", value: "home" },
              { label: "Trabajo", value: "work" },
              { label: "Antigua", value: "old" },
              { label: "Temporal", value: "temp" },
            ]}
          />
          <ProFormText
            {...(mode === "create" ? field : {})}
            name="country"
            label="País"
            placeholder="Ej. Honduras"
            width="md"
          />
          <ProFormText
            {...(mode === "create" ? field : {})}
            name="state"
            label="Departamento"
            placeholder="Ej. Copán"
            width="md"
          />
          <ProFormText
            {...(mode === "create" ? field : {})}
            name="city"
            label="Ciudad"
            placeholder="Ej. Santa Rosa de Copán"
            width="md"
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
