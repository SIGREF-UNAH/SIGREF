import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormDatePicker,
  ProFormGroup,
  ProFormList,
} from "@ant-design/pro-components";
import { Button, Collapse, Space } from "antd";
import {
  GlobalOutlined,
  IdcardOutlined,
  UserOutlined,
  PhoneOutlined,
  HomeOutlined,
  PlusOutlined,
} from "@ant-design/icons";
import type { CollapseProps } from "antd";
import { useEditPatient } from "../../hooks";

export default function EditFormPatient() {
  const {
    contextHolder,
    handleFinish,
    handleCancel,
    initialValues,
    isPending,
  } = useEditPatient();

  const nacionalidadContent = (
    <ProFormGroup>
      <ProFormText
        name="nacionalidad"
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
      initialValue={initialValues.address}
    >
      {(field, index, action) => (
        <ProFormGroup key={field.key}>
          <ProFormSelect
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

          <ProFormText name="country" label="País" placeholder="Honduras" />

          <ProFormText name="state" label="Departamento" placeholder="Copán" />

          <ProFormText name="city" label="Ciudad" placeholder="Santa Rosa" />

          <ProFormText
            name="line"
            label="Detalle de ubicación"
            placeholder="Ave 13, Calle 7, Casa 2, planta Azul"
            width="xl"
            fieldProps={{ style: { width: "100%" } }}
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

      <main className="flex-1 w-full p-8">
        <h1 className="text-3xl font-bold text-[#333333] mb-8">
          Gestión de Pacientes
        </h1>

        <ProForm
          onFinish={handleFinish}
          initialValues={initialValues}
          submitter={{
            render: (_) => (
              <div className="flex justify-end gap-3 mt-6">
                <Button size="large" onClick={handleCancel}>
                  Cancelar
                </Button>
                <Button
                  type="primary"
                  size="large"
                  htmlType="submit"
                  loading={isPending}
                >
                  Guardar Cambios
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
