import {
  Modal,
  Steps,
  Typography,
  Button,
  Form,
  Input,
  Select,
  Card,
  Descriptions,
  Space,
  Alert,
} from "antd";
import { useState } from "react";
import {
  CheckCircleOutlined,
  FormOutlined,
  AccountBookOutlined,
  SafetyOutlined,
} from "@ant-design/icons";

export const EditIncomeModal = ({
  modalVisible,
  setModalVisible,
  selectedIncome,
}: any) => {
  const [currentStep, setCurrentStep] = useState(0);
  const [form] = Form.useForm();
  const [formValues, setFormValues] = useState<any>({});

  const steps = [
    {
      title: "Verificar Ingreso",
      icon: <CheckCircleOutlined />,
      content: (
        <Space direction="vertical" size="large" style={{ width: "100%" }}>
          <Alert
            message="Verificación de Ingreso"
            description="Revise cuidadosamente los datos del ingreso que desea invalidar"
            type="info"
            showIcon
          />
          {selectedIncome ? (
            <Card size="small" style={{ backgroundColor: "#fafafa" }}>
              <Descriptions column={1} size="small" bordered>
                <Descriptions.Item label="Fecha">
                  {selectedIncome.fecha}
                </Descriptions.Item>
                <Descriptions.Item label="Recibo">
                  {selectedIncome.recibo}
                </Descriptions.Item>
                <Descriptions.Item label="Serie">
                  {selectedIncome.serie}
                </Descriptions.Item>
                <Descriptions.Item label="Módulo">
                  {selectedIncome.modulo}
                </Descriptions.Item>
                <Descriptions.Item label="Servicio">
                  {selectedIncome.servicio}
                </Descriptions.Item>
                <Descriptions.Item label="Monto Cobrado">
                  <Typography.Text
                    strong
                    style={{ color: "#4CAF50", fontSize: "16px" }}
                  >
                    L. {selectedIncome.monto.toFixed(2)}
                  </Typography.Text>
                </Descriptions.Item>
                <Descriptions.Item label="Paciente">
                  {selectedIncome.paciente}
                </Descriptions.Item>
                <Descriptions.Item label="Auxiliar Recepcionista">
                  {selectedIncome.auxiliar}
                </Descriptions.Item>
              </Descriptions>
            </Card>
          ) : (
            <Alert
              message="No se ha seleccionado ningún ingreso"
              type="error"
              showIcon
            />
          )}
        </Space>
      ),
    },
    {
      title: "Motivo de Anulación",
      icon: <FormOutlined />,
      content: (
        <Space direction="vertical" size="large" style={{ width: "100%" }}>
          <Alert
            message="Motivo de Anulación"
            description="Ingrese el motivo y una descripción detallada de la anulación"
            type="warning"
            showIcon
          />
          <Form form={form} layout="vertical" size="large">
            <Form.Item
              label="Motivo de Anulación"
              name="motivo"
              rules={[{ required: true, message: "Seleccione un motivo" }]}
            >
              <Select placeholder="Seleccione un motivo" size="large">
                <Select.Option value="error_monto">
                  Error en el monto
                </Select.Option>
                <Select.Option value="duplicado">Cobro duplicado</Select.Option>
                <Select.Option value="error_paciente">
                  Error en paciente
                </Select.Option>
                <Select.Option value="otro">Otro motivo</Select.Option>
              </Select>
            </Form.Item>
            <Form.Item
              label="Descripción Detallada"
              name="descripcion"
              rules={[
                {
                  required: true,
                  message: "Describa el motivo de la anulación",
                },
              ]}
            >
              <Input.TextArea
                rows={4}
                placeholder="Describa detalladamente el motivo de la anulación..."
                showCount
                maxLength={500}
              />
            </Form.Item>
          </Form>
        </Space>
      ),
    },
    {
      title: "Contrapartida",
      icon: <AccountBookOutlined />,
      content: (
        <Space direction="vertical" size="large" style={{ width: "100%" }}>
          <Alert
            message="Asiento Contable Automático"
            description="El sistema generará automáticamente el asiento de contrapartida para registrar la anulación del ingreso en la contabilidad."
            type="success"
            showIcon
          />
          <Card
            style={{
              textAlign: "center",
              backgroundColor: "#FAFAFA",
              borderColor: "#333333",
            }}
          >
            <Typography.Title
              level={4}
              style={{ color: "#EF5350", marginBottom: 8 }}
            >
              Se anula
            </Typography.Title>
            <Typography.Title
              level={2}
              style={{ color: "#EF5350", marginTop: 0 }}
            >
              L. {selectedIncome?.monto.toFixed(2)}
            </Typography.Title>
            <Typography.Text type="secondary">
              Este monto será registrado en el sistema como anulación de ingreso
            </Typography.Text>
          </Card>
        </Space>
      ),
    },
    {
      title: "Confirmar",
      icon: <SafetyOutlined />,
      content: (
        <Space direction="vertical" size="large" style={{ width: "100%" }}>
          <Alert
            message="Resumen de la operación"
            description="Revise cuidadosamente toda la información antes de confirmar la invalidación"
            type="warning"
            showIcon
          />
          <Card
            size="small"
            title="Datos de la Anulación"
            style={{ backgroundColor: "#fafafa" }}
          >
            <Descriptions column={1} size="small" bordered>
              <Descriptions.Item label="Ingreso a invalidar">
                {selectedIncome?.recibo}
              </Descriptions.Item>
              <Descriptions.Item label="Monto">
                <Typography.Text
                  strong
                  style={{ color: "#ff4d4f", fontSize: "16px" }}
                >
                  L. {selectedIncome?.monto.toFixed(2)}
                </Typography.Text>
              </Descriptions.Item>
              <Descriptions.Item label="Motivo">
                {formValues.motivo === "error_monto" && "Error en el monto"}
                {formValues.motivo === "duplicado" && "Cobro duplicado"}
                {formValues.motivo === "error_paciente" && "Error en paciente"}
                {formValues.motivo === "otro" && "Otro motivo"}
                {!formValues.motivo && "-"}
              </Descriptions.Item>
              <Descriptions.Item label="Paciente">
                {selectedIncome?.paciente}
              </Descriptions.Item>
              <Descriptions.Item label="Descripción">
                {formValues.descripcion || "-"}
              </Descriptions.Item>
            </Descriptions>
          </Card>
        </Space>
      ),
    },
  ];

  const next = async () => {
    if (currentStep === 1) {
      try {
        await form.validateFields();
        setFormValues(form.getFieldsValue());
      } catch {
        return;
      }
    }
    setCurrentStep(currentStep + 1);
  };

  const prev = () => setCurrentStep(currentStep - 1);

  const handleFinish = () => {
    setModalVisible(false);
    setCurrentStep(0);
    form.resetFields();
    setFormValues({});
  };

  return (
    <Modal
      open={modalVisible}
      title="Invalidar Ingreso"
      width={900}
      footer={null}
      onCancel={() => setModalVisible(false)}
      keyboard
      maskClosable={false}
      afterClose={() => {
        setCurrentStep(0);
        form.resetFields();
        setFormValues({});
      }}
    >
      {/* Form wrapper para conectar el form en todos los steps */}
      <Form form={form} component={false}>
        <Steps
          current={currentStep}
          items={steps.map((step) => ({ title: step.title, icon: step.icon }))}
          style={{ marginBottom: 32 }}
          responsive={true}
        />

        <div style={{ minHeight: 280, marginBottom: 24 }}>
          {steps[currentStep].content}
        </div>

        <div
          style={{
            marginTop: 24,
            textAlign: "right",
            borderTop: "1px solid #f0f0f0",
            paddingTop: 16,
          }}
        >
          {currentStep > 0 && (
            <Button size="large" style={{ marginRight: 8 }} onClick={prev}>
              Anterior
            </Button>
          )}
          {currentStep < steps.length - 1 && (
            <Button type="primary" size="large" onClick={next}>
              Siguiente
            </Button>
          )}
          {currentStep === steps.length - 1 && (
            <Button type="primary" danger size="large" onClick={handleFinish}>
              Confirmar Invalidación
            </Button>
          )}
        </div>
      </Form>
    </Modal>
  );
};