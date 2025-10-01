import { Form, Input, Select, Button, Card, Row, Col, Typography, Space, Divider, Spin } from "antd";
import { EnvironmentOutlined, ContactsOutlined, SnippetsOutlined, CheckOutlined } from "@ant-design/icons";
import { useParams } from "react-router-dom";   
import useEditLocationForm from "../../hooks/useEditLocationForm";
import { LocationStatus, LocationMode } from "../../../../api/models";

const { Title } = Typography;
const { TextArea } = Input;
const { Option } = Select;

export default function LocationManagementForm() {
  const [form] = Form.useForm();
  const { id } = useParams<{ id: string }>();

  const {
    formData,
    setField,
    handleSubmit,
    isSubmitting,   
    error,
  } = useEditLocationForm(id!); 

  const initialValues = {
    name: formData?.name || "",
    type: formData?.type || "",
    description: formData?.description || "",
    status: formData?.status?.toString() || LocationStatus.NUMBER_0.toString(),
    direccion: formData?.address?.line?.[0] || "",
    ciudad: formData?.address?.city || "",
    estadoProvincia: formData?.address?.state || "",
    codigoPostal: formData?.address?.postalCode || "",
    pais: formData?.address?.country || "",
    phone: formData?.telecom?.find(t => t.system === "phone")?.value || "",
    email: formData?.telecom?.find(t => t.system === "email")?.value || "",
  };

  if (isSubmitting) {    
    return (
      <div className="flex justify-center items-center h-64">
        <Spin size="large" />
      </div>
    );
  }
  return (
    <div className="bg-[#FAFAFA] rounded-lg border-2 border-[#D9D9D9] p-6">
      <Card>  
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
          initialValues={initialValues}
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
                  label={<span className="text-[#616161]">Nombre de la Ubicación</span>}
                  name="name"
                  rules={[{ required: true, message: "Por favor ingrese el nombre" }]}
                >
                  <Input 
                    className="!bg-[#ffffff]"
                    placeholder="Ej. Sala de emergencias"
                    onChange={(e) => setField("name", e.target.value)} 
                  />
                </Form.Item>
              </Col>
              <Col xs={24} sm={8}>
                <Form.Item 
                  label={<span className="text-[#616161]">Tipo de Función</span>} 
                  name="type" 
                >
                  <Input 
                    className="!bg-[#ffffff]"
                    placeholder="Ej. Emergencias, ROOM"
                    onChange={(e) => setField("type", e.target.value)} 
                  />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={16}>
              <Col xs={24} sm={8}>
                <Form.Item
                  label={<span className="text-[#616161]">Estado</span>}
                  name="status"
                >
                  <Select 
                    className="[&_.ant-select-selector]:!border-gray-400"
                    placeholder="Seleccionar estado"
                    onChange={(value) => setField("status", parseInt(value))}
                  >
                    <Option value={LocationStatus.NUMBER_0.toString()}>Activo</Option>
                    <Option value={LocationStatus.NUMBER_1.toString()}>Inactivo</Option>
                    <Option value={LocationStatus.NUMBER_2.toString()}>Suspendido</Option>
                  </Select>
                </Form.Item>
              </Col>
              <Col xs={24} sm={8}>
                <Form.Item
                  label={<span className="text-[#616161]">Modo</span>}
                  name="mode"
                >
                  <Select 
                    className="[&_.ant-select-selector]:!border-gray-400"
                    placeholder="Seleccionar modo"
                    onChange={(value) => setField("mode", parseInt(value))}
                  >
                    <Option value={LocationMode.NUMBER_0.toString()}>Instancia</Option>
                    <Option value={LocationMode.NUMBER_1.toString()}>Clase</Option>
                  </Select>
                </Form.Item>
              </Col>
            </Row>

            <Form.Item 
              label={<span className="text-[#616161]">Descripción</span>} 
              name="description"
            >
              <TextArea 
                rows={3}  
                className="!bg-[#ffffff]" 
                placeholder="Descripción adicional de la ubicación"
                onChange={(e) => setField("description", e.target.value)}
              />
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
            >
              <Input 
                className="!bg-[#ffffff]"
                placeholder="Ej. Avenida principal 123"
                onChange={(e) => setField("address.line", e.target.value)}
              />
            </Form.Item>

            <Row gutter={16}>
              <Col xs={24} sm={12}>
                <Form.Item
                  label={<span className="text-[#616161]">Ciudad</span>}    
                  name="ciudad"
                >
                  <Input 
                    className="!bg-[#ffffff]"
                    placeholder="Ej. San José"
                    onChange={(e) => setField("address.city", e.target.value)}
                  />
                </Form.Item>
              </Col>
              <Col xs={24} sm={12}>
                <Form.Item
                  label={<span className="text-[#616161]">Estado/Provincia</span>}
                  name="estadoProvincia"
                >
                  <Input 
                    className="!bg-[#ffffff]"
                    placeholder="Ej. San José" 
                    onChange={(e) => setField("address.state", e.target.value)}
                  />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={16}>
              <Col xs={24} sm={12}>
                <Form.Item 
                  label={<span className="text-[#616161]">Código postal</span>} 
                  name="codigoPostal"
                >
                  <Input 
                    className="!bg-[#ffffff]"
                    placeholder="Ej. 10110"
                    onChange={(e) => setField("address.postalCode", e.target.value)} 
                  />
                </Form.Item>
              </Col>
              <Col xs={24} sm={12}>
                <Form.Item 
                  label={<span className="text-[#616161]">País</span>} 
                  name="pais" 
                >
                  <Input 
                    className="!bg-[#ffffff]"
                    placeholder="Ej. Costa Rica"
                    onChange={(e) => setField("address.country", e.target.value)} 
                  />
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
                  label={<span className="text-[#616161]">Teléfono</span>}
                  name="phone"
                >
                  <Input 
                    className="!bg-[#ffffff]"
                    placeholder="Ej. +504 4665-5945"
                    onChange={(e) => setField("phone", e.target.value)} 
                  />
                </Form.Item>
              </Col>
              <Col xs={24} sm={8}>
                <Form.Item
                  label={<span className="text-[#616161]">Correo electrónico</span>}
                  name="email"
                >
                  <Input 
                    className="!bg-[#ffffff]"
                    placeholder="Ej. contacto@hospital.cr"
                    onChange={(e) => setField("email", e.target.value)} 
                  />
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
                loading={isSubmitting}
                icon={<CheckOutlined className="!text-stone-100 !mr-1" />} 
                className="!bg-[#4CAF50] hover:!bg-green-700 !border-green-500 hover:!border-green-600"
              >
                {isSubmitting ? 'Guardando...' : 'Editar Ubicación'}
              </Button>
            </Form.Item>
          </div>

          {error && (
            <div className="mt-4 p-3 bg-red-50 border border-red-200 rounded-md">
              <span className="text-red-600">Error: {error}</span>
            </div>
          )}
        </Form>
      </Card>
    </div>
  )
}