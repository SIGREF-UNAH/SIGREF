import { Form, Input, Select, Button, Card, Row, Col, Typography, Space, Divider } from "antd"
import { EnvironmentOutlined,  ContactsOutlined, TeamOutlined, SnippetsOutlined, CheckOutlined } from "@ant-design/icons"
import { Footer } from "../../../shared/components/layout/footer"
 
const { Title } = Typography
const { TextArea } = Input
const { Option } = Select

export default function LocationManagementForm() {
  const [form] = Form.useForm()

  const handleSubmit = (values: any) => {
    console.log("Form values:", values)
  }

  return (
    <>
    <div className="w-full min-h-screen p-6">
      {/* Header */}
      <div className="mb-6">
        <div className="flex justify-between items-center mb-4">
          <Title level={2} className="!mb-0 !text-[#333333]">
            Gestión de Ubicaciones
          </Title>
          <Space>
            <Button className="!text-[#163C65] !font-semibold" type="link">Listar Ubicación</Button>
            <Button className="!text-[#163C65] !font-semibold" type="link">Crear Ubicación</Button>
            <Button className="!text-[#163C65] !font-semibold" type='link'>Editar Ubicación</Button>
          </Space>
        </div>
      </div>

      {/* Main Form Card */}
      <Card className="shadow-sm !border-gray-800">  
        <div className="mb-6">
          <Space align="center" className="mb-4">
            <EnvironmentOutlined className="!text-[#7BA2D4] !text-xl" />
            <Title level={4} className="!mb-0 !text-[#333333]">
              Actualizar Ubicación
            </Title>
          </Space>
        </div>

        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          initialValues={{
            identificador: "",
            nombreUbicacion: "",
            alias: "",
            tipoFuncion: "",
            descripcion: "",
            direccion: "",
            ciudad: "",
            estadoProvincia: "",
            codigoPostal: "",
            pais: "",
            nombreContacto: "",
            telefono: "",
            correoElectronico: "",
            organizacionResponsable: "",
            parteDe: "",    
          }}
        >
          {/* Información Básica */}
          <div className="mb-8">
            <Space align="center" className="mb-4">
              <SnippetsOutlined className="!text-[#7BA2D4] !text-xl" />
              <Title level={5} className="!mb-0 !text-[#333333]">
                Información Básica
              </Title>  
            </Space>

            <Row gutter={16}>
              <Col xs={24} sm={8}>
                <Form.Item
                  label={<span className="text-[#616161]">Identificador</span>}  
                  name="identificador"
                  rules={[{ required: true, message: "Por favor ingrese el identificador" }]}
                  required={false}
                >
                  <Input 
                  className="!bg-[#d9d9d9]"
                  placeholder="Ej. LOC-001" />
                </Form.Item>
              </Col>
              <Col xs={24} sm={8}>
                <Form.Item
                  label={<span className="text-[#616161]">Nombre de la Ubicación</span>}
                  name="nombreUbicacion"
                  rules={[{ required: true, message: "Por favor ingrese el nombre" }]}
                  required={false}
                >
                  <Input 
                   className="!bg-[#d9d9d9]"
                   placeholder="Ej. Sala de emergencias" />
                </Form.Item>
              </Col>
              <Col xs={24} sm={8}>
                <Form.Item 
                  label={<span className="text-[#616161]">Alias</span>} 
                  name="alias"
                  rules={[{ required: true, message: "Por favor ingrese el alias" }]}
                  required={false}
                >   
                  <Input 
                   className="!bg-[#d9d9d9]"
                   placeholder="Ej. Emergencias, ER" />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={16}>
              <Col xs={24} sm={8}>
                <Form.Item
                  label={<span className="text-[#616161]">Estado</span>}
                  name="estado"
                  rules={[{ required: true, message: "Por favor seleccione el estado" }]}
                  required={false}
                >
                  <Select 
                    className="[&_.ant-select-selector]:!border-gray-400"
                    placeholder="Seleccionar estado">
                    <Option value="activo">Activo</Option>
                    <Option value="inactivo">Inactivo</Option>
                    <Option value="suspendido">Suspendido</Option>
                  </Select>
                </Form.Item>
              </Col>
              <Col xs={24} sm={8}>
                <Form.Item
                  label={<span className="text-[#616161]">Modo</span>}
                  name="modo"
                  rules={[{ required: true, message: "Por favor seleccione el modo" }]}
                  required={false}
                >
                  <Select 
                    className="[&_.ant-select-selector]:!border-gray-400"
                    placeholder="Seleccionar estado">
                    <Option value="instancia">Instancia</Option>
                    <Option value="clase">Clase</Option>
                  </Select>
                </Form.Item>
              </Col>
              <Col xs={24} sm={8}>
                <Form.Item 
                  label={<span className="text-[#616161]">Tipo de Función</span>} 
                  name="tipoFuncion"
                  rules={[{ required: true, message: "Por favor ingrese el tipo de función" }]}
                  required={false}  
                >
                  <Input 
                   className="!bg-[#d9d9d9]"
                   placeholder="Ej. Emergencias, ROOM" />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item 
                label={<span className="text-[#616161]">Descripción</span>} 
                name="descripcion">
              <TextArea 
                rows={3}  
                className="!bg-[#d9d9d9]" 
                placeholder="Descripción adicional de la ubicación" />
            </Form.Item>
          </div>

          <Divider className="!border-gray-800 !mt-14"/>

          {/* Dirección Física */}
          <div className="mb-8">
            <Space align="center" className="mb-4">
              <EnvironmentOutlined className="!text-[#7BA2D4] !text-xl" />
              <Title level={5} className="!mb-0 !text-[#333333]">
                Dirección Física
              </Title>
            </Space>

            <Form.Item
              label={<span className="text-[#616161]">Dirección</span>}
              name="direccion"
              rules={[{ required: true, message: "Por favor ingrese la dirección" }]}
              required={false}
            >
              <Input 
               className="!bg-[#d9d9d9]"
               placeholder="Ej. Avenida principal 123" />
            </Form.Item>

            <Row gutter={16}>
              <Col xs={24} sm={12}>
                <Form.Item
                  label={<span className="text-[#616161]">Ciudad</span>}    
                  name="ciudad"
                  rules={[{ required: true, message: "Por favor ingrese la ciudad" }]}
                  required={false}
                >
                  <Input 
                   className="!bg-[#d9d9d9]"
                   placeholder="Ej. San José" />
                </Form.Item>
              </Col>
              <Col xs={24} sm={12}>
                <Form.Item
                  label={<span className="text-[#616161]">Estado/Provincia</span>}
                  name="estadoProvincia"
                  rules={[{ required: true, message: "Por favor ingrese el estado/provincia" }]}
                  required={false}
                >
                  <Input 
                   className="!bg-[#d9d9d9]"
                   placeholder="Ej. San José" />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={16}>
              <Col xs={24} sm={12}>
                <Form.Item 
                  label={<span className="text-[#616161]">Código postal</span>} 
                  name="codigoPostal"
                  rules={[{ required: true, message: "Por favor ingrese el código postal" }]}
                  required={false}
                >
                  <Input 
                   className="!bg-[#d9d9d9]"
                   placeholder="Ej. 10110" />
                </Form.Item>
              </Col>
              <Col xs={24} sm={12}>
                <Form.Item label={<span className="text-[#616161]">País</span>} 
                            name="pais" 
                            rules={[{ required: true, message: "Por favor ingrese el país" }]}
                            required={false}
                >
                  <Input 
                   className="!bg-[#d9d9d9]"
                   placeholder="Ej. Costa Rica" />
                </Form.Item>
              </Col>
            </Row>
          </div>    

          <Divider className="!border-gray-800"/>

          {/* Información de Contacto */}
          <div className="mb-8">
            <Space align="center" className="mb-4">
              <ContactsOutlined className="!text-[#7BA2D4] !text-xl" />
              <Title level={5} className="!mb-0 !text-[#333333]">
                Información de Contacto
              </Title>
            </Space>

            <Row gutter={16}>
              <Col xs={24} sm={8}>
                <Form.Item 
                  label={<span className="text-[#616161]">Nombre de contacto</span>} 
                  name="nombreContacto"
                  rules={[{ required: true, message: "Por favor ingrese el nombre de contacto" }]}
                  required={false}
                >
                  <Input 
                   className="!bg-[#d9d9d9]"
                   placeholder="Ej. Juan Pérez" />
                </Form.Item>
              </Col>
              <Col xs={24} sm={8}>
                <Form.Item label={<span className="text-[#616161]">Teléfono</span>} 
                            name="telefono"
                            rules={[{ required: true, message: "Por favor ingrese el teléfono" }]}
                            required={false}
                >
                  <Input 
                   className="!bg-[#d9d9d9]"
                   placeholder="Ej. +504 4665-5945" />
                </Form.Item>
              </Col>
              <Col xs={24} sm={8}>
                <Form.Item
                  label={<span className="text-[#616161]">Correo electrónico</span>}
                  name="correoElectronico"
                  rules={[{ type: "email", message: "Por favor ingrese un email válido" }]}
                  required={false}
                >
                  <Input 
                   className="!bg-[#d9d9d9]"
                   placeholder="Ej. contacto@hospital.cr" />
                </Form.Item>
              </Col>
            </Row>
          </div>

          <Divider className="!border-gray-800" />

          {/* Organización y Jerarquía */}
          <div className="mb-8">
            <Space align="center" className="mb-4">
              <TeamOutlined className="!text-[#7BA2D4] !text-xl" />
              <Title level={5} className="!mb-0 !text-[#333333]">
                Organización y Jerarquía
              </Title>
            </Space>

            <Row gutter={16}>
              <Col xs={24} sm={12}>
                <Form.Item
                  label={<span className="text-[#616161]">Organización Responsable</span>}
                  name="organizacionResponsable"
                  rules={[{ required: true, message: "Por favor ingrese la organización responsable" }]}
                  required={false}
                >
                  <Input 
                   className="!bg-[#d9d9d9]"
                   placeholder="Ej. Hospital Nacional" />
                </Form.Item>
              </Col>
              <Col xs={24} sm={12}>
                <Form.Item 
                  label={<span className="text-[#616161]">Parte de</span>}
                  name="parteDe"
                  rules={[{ required: true, message: "Por favor ingrese la ubicación padre" }]}
                  required={false}>
                  <Input 
                   className="!bg-[#d9d9d9]"
                   placeholder="Ej. Edificio Principal" />
                </Form.Item>
              </Col>
            </Row>
          </div>

          {/* Submit Button */}
          <div className="flex justify-end">
            <Form.Item>
              <Button
                type="primary"
                htmlType="submit"
                size="large"
                icon={<CheckOutlined className="!text-stone-100 !mr-1" />} 
                className="!bg-[#4CAF50] hover:!bg-green-700 !border-green-500 hover:!border-green-600"
              >
                Editar Ubicación
              </Button>
            </Form.Item>
          </div>
        </Form>
      </Card>
    </div>
    <Footer/>
    </>
  )
}