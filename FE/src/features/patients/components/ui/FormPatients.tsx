import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormDatePicker,
  ProFormRadio,
  ProFormGroup,
} from "@ant-design/pro-components";
import { Button, Collapse, message, Space } from "antd";
import {
  GlobalOutlined,
  IdcardOutlined,
  UserOutlined,
  PhoneOutlined,
  HomeOutlined,
  PlusOutlined,
} from "@ant-design/icons";
import type { CollapseProps } from "antd";
import useCreatePatientForm from "../../hooks/useCreatePatient";
import { Link } from "react-router-dom";

export default function CreateFormPatient() {
  const { handleSubmit, isSubmitting, error } = useCreatePatientForm();

  const [messageApi, contextHolder] = message.useMessage();

  const onFinish = async (values: any) => {
    const success = await handleSubmit(values);
    if (success) {
      messageApi.success("Paciente creado exitosamente");
    } else {
      messageApi.error(error ?? "Error al crear el paciente");
    }
  };

  const onCancel = () => {
    messageApi.info("Operación cancelada");
  };

  const nacionalidadContent = (
    <ProFormGroup>
      <ProFormText
        name="paisNacionalidad"
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
          { label: "Desconocido", value: 0 },
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
          { label: "DNI HN", value: "dni_hn" },
          { label: "Pasaporte", value: "pasaporte" },
          { label: "Cédula", value: "cedula" },
        ]}
      />
      <ProFormText
        name={["identifier", 0, "value"]}
        label="Número"
        placeholder="0000000000000"
        width="md"
      />
      <ProFormText name="emisor" label="Emisor" placeholder="SRNP" width="sm" />
      <ProFormDatePicker
        name="fechaExpedicion"
        label="Fecha Expedición"
        placeholder="dd / mm / yyyy"
        width="md"
      />
      <ProFormRadio.Group
        name="identificacionPreferida"
        label=" "
        options={[{ label: "Preferido", value: true }]}
      />
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
          { label: "Legal", value: "legal" },
          { label: "Alias", value: "alias" },
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
    <ProFormGroup>
      <ProFormSelect
        name={["telecom", 0, "system"]}
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
        name={["telecom", 0, "use"]}
        label="Uso"
        options={[
          { label: "Móvil", value: "mobile" },
          { label: "Casa", value: "home" },
          { label: "Trabajo", value: "work" },
        ]}
      />
      <ProFormText
        name="codigoPais"
        label="Codigo de País"
        placeholder="+504"
        width="md"
      />
      <ProFormText
        name={["telecom", 0, "value"]}
        label="Valor"
        placeholder="9999-9999 / ejemplo@correo.com"
        width="md"
      />
      <ProFormRadio.Group
        name="contactoPreferido"
        label=" "
        options={[{ label: "Preferido", value: true }]}
      />
    </ProFormGroup>
  );

  const direccionesContent = (
    <ProFormGroup>
      <ProFormSelect
        name="tipoDireccion"
        label="Tipo"
        placeholder="Casa"
        width="sm"
        options={[
          { label: "Casa", value: "casa" },
          { label: "Trabajo", value: "trabajo" },
          { label: "Otro", value: "otro" },
        ]}
      />
      <ProFormText
        name={["address", 0, "country"]}
        label="Pais"
        placeholder="Honduras"
        width="md"
      />
      <ProFormText
        name={["address", 0, "state"]}
        label="Departamento"
        placeholder="Copan"
        width="md"
      />
      <ProFormText
        name={["address", 0, "city"]}
        label="Ciudad"
        placeholder="Santa Rosa"
        width="md"
      />
      <ProFormText
        name={["address", 0, "line", 0]}
        label="Detalle de Ubicación"
        placeholder="Ave 13, Calle 7, Casa 2 planta Azul"
        width="xl"
        fieldProps={{
          style: { width: "100%" },
        }}
      />
    </ProFormGroup>
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
          <span>Identificaciones</span>
        </Space>
      ),
      children: identificacionesContent,
    },
    {
      key: "3",
      label: (
        <Space>
          <UserOutlined style={{ color: "#6366f1" }} />
          <span>Nombres</span>
        </Space>
      ),
      children: nombresContent,
    },
    {
      key: "4",
      label: (
        <Space>
          <PhoneOutlined style={{ color: "#f97316" }} />
          <span>Contacto</span>
        </Space>
      ),
      children: contactoContent,
      extra: (
        <Button
          type="primary"
          size="small"
          icon={<PlusOutlined />}
          onClick={(e) => {
            e.stopPropagation();
          }}
        >
          Agregar
        </Button>
      ),
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
      extra: (
        <Button
          type="primary"
          size="small"
          icon={<PlusOutlined />}
          onClick={(e) => {
            e.stopPropagation();
          }}
        >
          Agregar
        </Button>
      ),
    },
  ];

  return (
    <div className="flex flex-col min-h-screen bg-gray-50">
      {contextHolder}

      <main className="flex-1 w-full p-8">
        <ProForm
          onFinish={onFinish}
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
                  Guardar
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
