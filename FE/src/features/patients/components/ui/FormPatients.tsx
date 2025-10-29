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
import { ProFormList } from "@ant-design/pro-components";

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
            {...field}
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
            {...field}
            name="use"
            label="Uso"
            options={[
              { label: "Móvil", value: 2 },
              { label: "Casa", value: 0 },
              { label: "Trabajo", value: 1 },
            ]}
          /> 

          <ProFormText
            {...field}
            name="value"
            label="Valor"
            placeholder="9999-9999 / ejemplo@correo.com"
            width="md"
          />

          <Button type="link" danger onClick={() => action.remove(index)}>
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
    >
      {(field, index, action) => (
        <ProFormGroup key={field.key}>  
          <ProFormSelect
            {...field}
            name="tipoDireccion"
            label="Tipo"
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
            {...field}
            name="country"
            label="País"
            placeholder="Honduras"
            width="md"
          />
          <ProFormText
            {...field}
            name="state"
            label="Departamento"
            placeholder="Copan"
            width="md"
          />
          <ProFormText
            {...field}
            name="city"
            label="Ciudad"
            placeholder="Santa Rosa"
            width="md"
          />
          <ProFormText
            {...field}
            name={["line", 0]}
            label="Detalle de Ubicación"
            placeholder="Ave 13, Calle 7, Casa 2"
            width="xl"
          />
          <Button type="link" danger onClick={() => action.remove(index)}>
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
          <span>Identificación</span>
        </Space>
      ),
      children: identificacionesContent,
    },
    {
      key: "3",
      label: (
        <Space>
          <UserOutlined style={{ color: "#6366f1" }} />
          <span>Nombre</span>
        </Space>
      ),
      children: nombresContent,
    },
    {
      key: "4",
      label: (
        <Space>
          <PhoneOutlined style={{ color: "#f97316" }} />
          <span>Contactos</span>
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
