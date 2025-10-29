import {
  CheckOutlined,
  CloseOutlined,
  PrinterOutlined,
  WarningOutlined,
} from "@ant-design/icons";
import { PageContainer, ProCard } from "@ant-design/pro-components";
import {
  Alert,
  Button,
  Col,
  Divider,
  InputNumber,
  Row,
  Select,
  Space,
  Typography,
} from "antd";
import { useState } from "react";

export const CashClosingPage = () => {
  const [amount, setAmount] = useState<number | null>(null);
  const [shift, setShift] = useState<string>();
  const [isLocked, setIsLocked] = useState(false);
  const [showConfirmation, setShowConfirmation] = useState(false);
  const [systemAmount] = useState(3800);
  const [showResult, setShowResult] = useState(false);

  const handleSave = () => {
    setShowConfirmation(true);
    setIsLocked(true);
  };

  const handleConfirm = () => {
    setShowConfirmation(false);
    setShowResult(true);
  };

  const handleCancel = () => {
    setShowConfirmation(false);
    setIsLocked(false);
  };

  const difference = amount !== null ? amount - systemAmount : 0;
  const isMatch = difference === 0;

  const shiftLabel =
    shift === "Día" ? "C: 9:00 AM - 9:00 PM" : "C: 9:00 PM - 9:00 AM";

  return (
    <PageContainer
      title={<Typography.Title level={3}>Cierre de Caja</Typography.Title>}
      subTitle={<Typography.Text>Cerrar Turno</Typography.Text>}
    >
      <ProCard
        title={
          <div className="flex justify-between items-center w-full">
            <img
              src="https://upload.wikimedia.org/wikipedia/commons/thumb/f/f1/Logo_de_SESAL.svg/1200px-Logo_de_SESAL.svg.png"
              alt="Logo Salud"
              className="h-12 absolute left-20 top-7"
            />

            <img
              src="https://krti.cl/wp-content/uploads/2021/04/Logo-Hospital-Final.png"
              className="h-12 absolute right-20 top-7"
            />
          </div>
        }
        bordered
      >
        <div className="text-center mb-4 text-gray-600">
          <div className="text-2xl font-bold text-general py-2">
            Hospital de Occidente
          </div>
          <div className="text-base font-medium text-general-secondary py-2">
            El Calvario, Santa Rosa de Copán, 41101 CA
          </div>
          <div className="text-base font-medium text-general-secondary py-2">
            Copán, Honduras
          </div>
          {!showResult && (
            <div className="text-2xl font-bold text-general py-4">
              Cierre de Caja
            </div>
          )}
        </div>

        <Divider className="my-4" />

        {!showResult && !showConfirmation && (
          <>
            <Row gutter={[16, 16]}>
              <Col span={12}>
                <Typography.Text strong style={{ color: "#7BA2D4" }}>
                  Auxiliar de Caja:
                </Typography.Text>{" "}
                <Typography.Text>Sebas Contreras</Typography.Text>
              </Col>
              <Col span={12}>
                <Typography.Text strong style={{ color: "#7BA2D4" }}>
                  Módulo:
                </Typography.Text>{" "}
                <Typography.Text style={{ color: "#7BA2D4" }}>
                  Emergencia
                </Typography.Text>
              </Col>

              <Col span={12}>
                <Typography.Text strong>
                  Ingrese el Monto Registrado en Caja
                </Typography.Text>
                <InputNumber
                  min={0}
                  value={amount ?? undefined}
                  disabled={isLocked}
                  onChange={(value) => setAmount(value ?? 0)}
                  style={{ width: "100%", marginTop: 0 }}
                  placeholder="0.00"
                  size="large"
                />
              </Col>

              <Col span={12}>
                <Typography.Text strong style={{ color: "#7BA2D4" }}>
                  Turno Actual
                </Typography.Text>
                <Select
                  placeholder="Seleccione un turno"
                  value={shift}
                  disabled={isLocked}
                  onChange={(value) => setShift(value)}
                  options={[
                    { label: "C: 9:00 am - 9:00 pm", value: "Día" },
                    { label: "C: 9:00 pm - 9:00 am", value: "Noche" },
                  ]}
                  className="w-full"
                  size="large"
                />
              </Col>
            </Row>

            <div className="flex justify-between mt-6">
              <div className="font-bold text-general-secondary">
                27 de Septiembre de 2025
              </div>
              <div className="text-secondary font-bold">04:36:12 pm</div>
            </div>

            <div className="text-center mt-6">
              <Button
                type="primary"
                onClick={handleSave}
                disabled={!amount || !shift}
                size="large"
                variant="solid"
                color="green"
              >
                Guardar Cierre
              </Button>
            </div>
          </>
        )}

        {showConfirmation && (
          <div className="py-6">
            <Row gutter={[16, 16]} className="mb-4">
              <Col span={12}>
                <Typography.Text strong style={{ color: "#7BA2D4" }}>
                  Auxiliar de Caja:
                </Typography.Text>{" "}
                <Typography.Text strong>Sebas Contreras</Typography.Text>
              </Col>
              <Col span={12}>
                <Typography.Text strong style={{ color: "#7BA2D4" }}>
                  Módulo:
                </Typography.Text>{" "}
                <Typography.Text style={{ color: "#7BA2D4" }}>
                  Emergencia
                </Typography.Text>
                <div>
                  <Typography.Text>{shiftLabel}</Typography.Text>
                </div>
              </Col>

              <Col span={12}>
                <Typography.Text strong>Monto Registrado:</Typography.Text>{" "}
                <Typography.Text
                  style={{ color: "#EF5350" }}
                  className="text-lg font-bold"
                >
                  Lp {amount?.toFixed(2)}
                </Typography.Text>
              </Col>
            </Row>
            <div className="mt-4 text-general-secondary text-md py-2">
              27 de Septiembre de 2025 04:36:12 pm
            </div>
            <div className="justify-center text-center">
              <div className="text-lg font-medium text-general-secondary py-2">
                Sistema de Gestion de Receptoria de Fondos
              </div>
              <div className="text-xl font-bold text-general py-2">
                ¿Estas Seguro de Cerrar Caja?
              </div>
            </div>

            <Alert
              showIcon
              type="warning"
              description="Una vez confirmado, no podrás modificar los datos del cierre actual."
              className="mb-6 "
            />
            <Divider className="my-4" />
            <div className="text-center mt-6">
              <Space size="middle">
                <Button
                  onClick={handleCancel}
                  size="large"
                  variant="solid"
                  color="danger"
                  icon={<CloseOutlined />}
                >
                  Cancelar
                </Button>
                <Button
                  type="primary"
                  onClick={handleConfirm}
                  size="large"
                  variant="solid"
                  color="green"
                  icon={<CheckOutlined />}
                >
                  Confirmar
                </Button>
              </Space>
            </div>
          </div>
        )}

        {showResult && (
          <div>
            <div className="text-center text-xl font-bold mb-6">
              Resumen de Cierre
            </div>

            <Divider className="my-4" />

            <Row gutter={[16, 16]} className="mb-6">
              <Col span={12}>
                <Typography.Text strong style={{ color: "#7BA2D4" }}>
                  Auxiliar de Caja:
                </Typography.Text>{" "}
                <Typography.Text strong>Sebas Contreras</Typography.Text>
              </Col>
              <Col span={12}>
                <Typography.Text strong style={{ color: "#7BA2D4" }}>
                  Módulo:
                </Typography.Text>{" "}
                <Typography.Text>Emergencia</Typography.Text>
                <div>
                  <Typography.Text>{shiftLabel}</Typography.Text>
                </div>
              </Col>
            </Row>

            <Row gutter={[16, 16]} className="mb-4">
              <Col span={12}>
                <Typography.Text strong>Cierre con</Typography.Text>{" "}
                <Typography.Text
                  style={{
                    fontSize: "18px",
                    color: isMatch ? "#52c41a" : "#ff4d4f",
                    fontWeight: "bold",
                  }}
                >
                  Lp {amount?.toFixed(2)}
                </Typography.Text>
              </Col>
              <Col span={12}>
                <Typography.Text strong>Sistema:</Typography.Text>{" "}
                <Typography.Text
                  style={{
                    color: "#4CAF50",
                    fontWeight: "bold",
                    fontSize: "18px",
                  }}
                >
                  Lp {systemAmount.toFixed(2)}
                </Typography.Text>
              </Col>
            </Row>

            <div className="mb-4 text-gray-400 text-xs">
              27 de Septiembre de 2025 &nbsp;&nbsp; 04:36:12 pm
            </div>

            {isMatch ? (
              <>
                <Alert
                  message="¡Cierre Correcto!"
                  description={`El monto ingresado coincide con el sistema: Lp ${amount?.toFixed(2)}`}
                  type="success"
                  showIcon
                  className="mb-4"
                />
                <div className="text-center text-general text-lg mt-4">
                  Sistema de Gestión de Receptoría de Fondos
                </div>
                <div className="text-right text-general-secondary text-lg mb-4">
                  ID:GJD8792JKDL303LD
                </div>
                <div className="text-center">
                  <Space size="large">
                    <Button size="large" icon={<PrinterOutlined />}>
                      Imprimir
                    </Button>
                    <Button
                      type="primary"
                      size="large"
                      color="green"
                      variant="solid"
                      icon={<CheckOutlined />}
                    >
                      Siguiente
                    </Button>
                  </Space>
                </div>
              </>
            ) : (
              <>
                <Alert
                  message="Diferencia Detectada"
                  description={
                    <div>
                      <p className="mb-2">
                        <strong>Diferencia:</strong>{" "}
                        <span className="text-red-500 text-lg font-bold">
                          Lp {Math.abs(difference).toFixed(2)}
                        </span>
                      </p>
                      <p className="mb-0">
                        El monto ingresado{" "}
                        {difference > 0 ? "es mayor" : "es menor"} que el
                        registrado en el sistema.
                      </p>
                    </div>
                  }
                  type="error"
                  showIcon
                  className="mb-4"
                />
                <div className="text-center mt-4">
                  <Button
                    type="primary"
                    size="large"
                    color="orange"
                    variant="solid"
                    icon={<WarningOutlined />}
                    onClick={() => {
                      console.log("Redirigir a página de corrección");
                    }}
                  >
                    Ir a Corrección
                  </Button>
                </div>
              </>
            )}
          </div>
        )}
      </ProCard>
    </PageContainer>
  );
};
