import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormDatePicker,
  ProFormRadio,
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

export default function PatientForm({
  mode,
  initialValues,
  onSubmit,
  isSubmitting,
  error,
}: PatientFormProps) {
  const [messageApi, contextHolder] = message.useMessage();

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

  const onCancel = () => {
    messageApi.info("Operación cancelada");
  };

  const nacionalidadContent = (
    <ProFormGroup>
      <ProFormText
        name={mode === "create" ? "paisNacionalidad" : "nacionalidad"}
        label="País de Nacionalidad"
        placeholder="Honduras"
        width="md"
      />
      <ProFormSelect
        name="gender"
        label="Género"
        placeholder="Seleccione"
        width="sm"
        rules={[{ required: true, message: "Campo requerido" }]}
        options={[
          { label: "Masculino", value: 1 },
          { label: "Femenino", value: 2 },
          { label: "Otro", value: 3 },
          ...(mode === "edit" ? [{ label: "Desconocido", value: 0 }] : []),
        ]}
      />
      <ProFormSelect
        name="estadoCivil"
        label="Estado Civil"
        placeholder="Seleccione"
        width="sm"
        options={[
          { label: "Soltero/a", value: "soltero" },
          { label: "Casado/a", value: "casado" },
          { label: "Divorciado/a", value: "divorciado" },
          { label: "Viudo/a", value: "viudo" },
        ]}
      />
      <ProFormSelect
        name="estadoVital"
        label="Estado Vital"
        placeholder="VIVO"
        width="sm"
        options={[
          { label: "Vivo", value: 1 },
          { label: "Fallecido", value: 0 },
        ]}
      />
      <ProFormDatePicker
        name="fechanacimiento"
        label="Fecha de Nacimiento"
        placeholder="dd / mm / yyyy"
        width="md"
        rules={[{ required: true, message: "Campo requerido" }]}
      />
    </ProFormGroup>
  );

  const identificacionesContent = (
    <ProFormGroup>
      <ProFormSelect
        name="tipoIdentificacion"
        label="Tipo"
        placeholder="DNI HN"
        width="sm"
        options={[
          { label: "DNI HN", value: "DNI" },
          { label: "Pasaporte", value: "PPN" },
          { label: "Cédula", value: "NI" },
        ]}
      />
      <ProFormText
        name={["identifier", 0, "value"]}
        label="Número"
        placeholder="0000000000000"
        width="md"
      />
      <ProFormText name="emisor" label="Emisor" placeholder="SRNP" width="sm" />
      {mode === "create" && (
        <ProFormRadio.Group
          name="identificacionPreferida"
          label=" "
          options={[{ label: "Preferido", value: true }]}
        />
      )}
    </ProFormGroup>
  );

  const nombresContent = (
    <ProFormGroup>
      <ProFormSelect
        name="tipoNombre"
        label="Tipo"
        placeholder="Legal"
        width="sm"
        options={[
          { label: "Legal", value: 0 },
          { label: "Alias", value: 1 },
        ]}
      />
      <ProFormText
        name="primerNombre"
        label="Primer Nombre"
        placeholder="Nombre"
        width="md"
        rules={[{ required: true, message: "Campo requerido" }]}
      />
      <ProFormText
        name="segundoNombre"
        label="Segundo Nombre"
        placeholder="Nombre"
        width="md"
      />
      <ProFormText
        name="apellidos"
        label="Apellidos"
        placeholder="Apellido Díaz"
        width="md"
      />
    </ProFormGroup>
  );

  const contactoContent = (
    <ProFormList
      name="telecom"
      creatorButtonProps={{
        creatorButtonText: "Agregar contacto",
        icon: <PlusOutlined />,
      }}
    >
      {(field, index, action) => (
        <ProFormGroup key={field.key}>
          <ProFormSelect
            {...(mode === "create" ? field : {})}
            name="system"
            label="Tipo de contacto"
            placeholder="Seleccione"
            width="sm"
            options={[
              { label: "Teléfono", value: "phone" },
              { label: "Fax", value: "fax" },
              { label: "Email", value: "email" },
              { label: "Pager", value: "pager" },
              { label: "URL", value: "url" },
              { label: "SMS", value: "sms" },
              { label: "Otro", value: "other" },
            ]}
          />

          <ProFormSelect
            {...(mode === "create" ? field : {})}
            name="use"
            label="Uso"
            width="sm"
            options={[
              { label: "Casa", value: 0 },
              { label: "Trabajo", value: 1 },
              { label: "Móvil", value: 2 },
            ]}
          />

          <ProFormText
            {...(mode === "create" ? field : {})}
            name="value"
            label="Valor"
            placeholder="9999-9999 / ejemplo@correo.com"
            width="md"
          />

          <Button type="link" danger onClick={() => action.remove(index)}>
            Eliminar
          </Button>
        </ProFormGroup>
      )}
    </ProFormList>
  );

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
      {(field, index, action) => (
        <ProFormGroup key={field.key}>
          <ProFormSelect
            {...(mode === "create" ? field : {})}
            name="tipoDireccion"
            label="Tipo"
            placeholder="Casa"
            width="sm"
            options={[
              { label: "Casa", value: "home" },
              { label: "Trabajo", value: "work" },
              { label: "Temporal", value: "temp" },
              { label: "Antigua", value: "old" },
              { label: "Facturación", value: "billing" },
            ]}
          />
          <ProFormText
            {...(mode === "create" ? field : {})}
            name="country"
            label="País"
            placeholder="Honduras"
            width="md"
          />
          <ProFormText
            {...(mode === "create" ? field : {})}
            name="state"
            label="Departamento"
            placeholder={mode === "create" ? "Copan" : "Copán"}
            width="md"
          />
          <ProFormText
            {...(mode === "create" ? field : {})}
            name="city"
            label="Ciudad"
            placeholder="Santa Rosa"
            width="md"
          />
          <ProFormText
            {...(mode === "create" ? field : {})}
            name={mode === "create" ? ["line", 0] : "line"}
            label={
              mode === "create"
                ? "Detalle de Ubicación"
                : "Detalle de ubicación"
            }
            placeholder={
              mode === "create"
                ? "Ave 13, Calle 7, Casa 2"
                : "Ave 13, Calle 7, Casa 2, planta Azul"
            }
            width="xl"
            {...(mode === "edit"
              ? { fieldProps: { style: { width: "100%" } } }
              : {})}
          />
          <Button type="link" danger onClick={() => action.remove(index)}>
            Eliminar
          </Button>
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
