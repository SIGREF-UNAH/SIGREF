import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormDatePicker,
  ProFormRadio,
  ProFormGroup,
} from "@ant-design/pro-components"
import { Button, Collapse, Space, message } from "antd"
import {
  GlobalOutlined,
  IdcardOutlined,
  UserOutlined,
  PhoneOutlined,
  HomeOutlined,
  PlusOutlined,
} from "@ant-design/icons"
import type { CollapseProps } from "antd"
import { useNavigate } from 'react-router-dom';

const { Panel } = Collapse

export default function EditFormPatient() {
  const [messageApi, contextHolder] = message.useMessage()
  const navigate = useNavigate();


  const handleFinish = async (values: any) => {
    console.log("Form values:", values)
    messageApi.success("Paciente guardado exitosamente")
    return true
  }

  const handleCancel = () => {
    messageApi.info("Operación cancelada")
    navigate("/patients/list");  
  }

  const nacionalidadContent = (
    <ProFormGroup>
      <ProFormText
        name="paisNacionalidad"
        label="País de Nacionalidad"
        placeholder="Honduras"
        width="md"
        rules={[{ required: true, message: "Campo requerido" }]}
      />
      <ProFormSelect
        name="genero"
        label="Género"
        placeholder="Seleccione"
        width="sm"
        options={[
          { label: "Masculino", value: "1" },
          { label: "Femenino", value: "2" },
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
          { label: "Vivo", value: "vivo" },
          { label: "Fallecido", value: "fallecido" },
        ]}
      />
      <ProFormDatePicker name="fechanacimiento" label="Fecha de Nacimiento" placeholder="dd / mm / yyyy" width="md" />
    </ProFormGroup>  
  )

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
        name="numeroIdentificacion"
        label="Número"
        placeholder="0000000000000"
        width="md"
        rules={[{ required: true, message: "Campo requerido" }]}
      />
      <ProFormText name="emisor" label="Emisor" placeholder="SRNP" width="sm" />
      <ProFormDatePicker name="fechaExpedicion" label="Fecha Expedición" placeholder="dd / mm / yyyy" width="md" />
      <ProFormRadio.Group name="identificacionPreferida" label=" " options={[{ label: "Preferido", value: true }]} />
    </ProFormGroup>
  )

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
      <ProFormText name="segundoNombre" label="Segundo Nombre" placeholder="Nombre" width="md" />
      <ProFormText
        name="apellidos"
        label="Apellidos"
        placeholder="Apellido Díaz"
        width="md"
        rules={[{ required: true, message: "Campo requerido" }]}
      />
      <ProFormDatePicker name="fechaInicioNombre" label="Fecha de Inicio" placeholder="dd / mm / yyyy" width="md" />
      <ProFormDatePicker
        name="fechaExpiracionNombre"
        label="Fecha de Expiración"
        placeholder="dd / mm / yyyy"
        width="md"
      />
    </ProFormGroup>
  )

  const contactoContent = (
    <ProFormGroup>
      <ProFormSelect
        name="tipoContacto"
        label="Tipo"
        placeholder="Celular"
        width="sm"
        options={[
          { label: "Celular", value: "celular" },
          { label: "Teléfono", value: "telefono" },
          { label: "Email", value: "email" },
        ]}
      />
      <ProFormText
        name="codigoPais"
        label="Codigo de País"
        placeholder="+504"
        width="md"
        rules={[{ required: true, message: "Campo requerido" }]}
      />
      <ProFormText
        name="valor"
        label="Valor"
        placeholder="9999-9999"
        width="md"
        rules={[{ required: true, message: "Campo requerido" }]}
      />
      <ProFormDatePicker name="fechaInicioContacto" label="Fecha de Inicio" placeholder="dd / mm / yyyy" width="md" />
      <ProFormDatePicker
        name="fechaExpiracionContacto"
        label="Fecha de Expiración"
        placeholder="dd / mm / yyyy"
        width="md"
      />
      <ProFormRadio.Group name="contactoPreferido" label=" " options={[{ label: "Preferido", value: true }]} />
    </ProFormGroup>
  )

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
      <ProFormSelect
        name="pais"
        label="País"
        placeholder="Honduras"
        width="md"
        options={[
          { label: "Honduras", value: "honduras" },
          { label: "Guatemala", value: "guatemala" },
          { label: "El Salvador", value: "el_salvador" },
        ]}
      />
      <ProFormSelect
        name="departamento"
        label="Departamento"
        placeholder="Copán"
        width="md"
        options={[
          { label: "Copán", value: "copan" },
          { label: "Cortés", value: "cortes" },
          { label: "Francisco Morazán", value: "francisco_morazan" },
        ]}
      />
      <ProFormSelect
        name="ciudad"
        label="Ciudad"
        placeholder="Santa Rosa"
        width="md"
        options={[
          { label: "Santa Rosa", value: "santa_rosa" },
          { label: "San Pedro Sula", value: "san_pedro_sula" },
          { label: "Tegucigalpa", value: "tegucigalpa" },
        ]}
      />
      <ProFormText
        name="detalleDireccion"
        label="Detalle de Ubicación"
        placeholder="Ave 13, Calle 7, Casa 2 planta Azul"
        width="xl"
        fieldProps={{
          style: { width: "100%" },
        }}
      />
      <ProFormDatePicker name="fechaRegistro" label="Fecha de Registro" placeholder="dd / mm / yyyy" width="md" />
      <ProFormDatePicker name="fechaFinalizacion" label="Fecha Finalización" placeholder="dd / mm / yyyy" width="md" />
    </ProFormGroup>
  )

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
      extra: (
        <Button
          type="primary"
          size="small"
          icon={<PlusOutlined />}
          onClick={(e) => {
            e.stopPropagation()
            messageApi.info("Agregar nacionalidad")
          }}
        >
          Agregar
        </Button>
      ),
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
      extra: (
        <Button
          type="primary"
          size="small"
          icon={<PlusOutlined />}
          onClick={(e) => {
            e.stopPropagation()
            messageApi.info("Agregar identificación")
          }}
        >
          Agregar
        </Button>
      ),
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
      extra: (
        <Button
          type="primary"
          size="small"
          icon={<PlusOutlined />}
          onClick={(e) => {
            e.stopPropagation()
            messageApi.info("Agregar nombre")
          }}
        >
          Agregar
        </Button>
      ),
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
            e.stopPropagation()
            messageApi.info("Agregar contacto")
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
            e.stopPropagation()
            messageApi.info("Agregar dirección")
          }}
        >
          Agregar
        </Button>
      ),
    },
  ]

  return (
  <div className="flex flex-col min-h-screen bg-gray-50">
  {contextHolder}

  <main className="flex-1 w-full p-8">
    <h1 className="text-3xl font-bold text-[#333333] mb-8">Gestión de Pacientes</h1>

    <ProForm
      onFinish={handleFinish}
      submitter={{
        render: (_, dom) => (
          <div className="flex justify-end gap-3 mt-6"> 
            <Button size="large" onClick={handleCancel}>
              Cancelar
            </Button>
            <Button type="primary" size="large" htmlType="submit">
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

