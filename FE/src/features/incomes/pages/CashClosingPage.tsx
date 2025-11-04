import {
  CheckOutlined,
  CloseOutlined,
  PrinterOutlined,
  WarningOutlined,
  FilePdfOutlined,
  FileImageOutlined,
} from "@ant-design/icons";
import { PageContainer, ProCard } from "@ant-design/pro-components";
import {
  Alert,
  Button,
  Col,
  Divider,
  Dropdown,
  InputNumber,
  Row,
  Select,
  Space,
  Typography,
} from "antd";
import { useRef, useState } from "react";
import { useExport } from "../../../shared/utils";

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
    shift === "Día" ? "C: 9:00 AM - 9:00 PM" : "B: 9:00 PM - 9:00 AM";

  // Ref para exportación
  const printRef = useRef<HTMLDivElement>(null);

  const { exportData } = useExport();

  // Menú de opciones
  const printMenuItems = [
    {
      key: "pdf",
      label: "Exportar como PDF",
      icon: <FilePdfOutlined />,
      onClick: () =>
        exportData("print-area", "pdf", { fileName: "cierre_de_caja.pdf" }),
    },
    {
      key: "image",
      label: "Exportar como Imagen",
      icon: <FileImageOutlined />,
      onClick: () =>
        exportData("print-area", "png", { fileName: "cierre_de_caja.png" }),
    },
  ];

  return (
    <PageContainer
      title={<Typography.Title level={3}>Cierre de Caja</Typography.Title>}
      subTitle={<Typography.Text>Cerrar Turno</Typography.Text>}
    >
      <ProCard bordered>
        {/* ÁREA COMPLETA PARA EXPORTACIÓN */}
        <div
          ref={printRef}
          id="print-area"
          style={{
            padding: "20px",
            backgroundColor: "#FFFFFF",
            minHeight: "800px",
            width: "100%",
            position: "relative",
          }}
        >
          {/* Header con logos */}
          <div className="flex justify-between items-center w-full mb-6">
            <img
              src="/src/public/Logo_de_Salud.svg.png"
              alt="Logo Salud"
              style={{ height: "48px", objectFit: "contain" }}
            />
            <div className="flex-1"></div>
            <img
              src="/src/public/Logo-Hospital-Occiedente.png"
              alt="Logo Hospital de Occidente"
              style={{ height: "48px", objectFit: "contain" }}
            />
          </div>

          {/* Información del hospital */}
          <div
            style={{
              textAlign: "center",
              marginBottom: "16px",
              color: "#6b7280",
            }}
          >
            <div
              style={{
                fontSize: "24px",
                fontWeight: "bold",
                color: "#1f2937",
                padding: "8px 0",
              }}
            >
              Hospital de Occidente
            </div>
            <div
              style={{
                fontSize: "16px",
                fontWeight: "500",
                color: "#4b5563",
                padding: "8px 0",
              }}
            >
              El Calvario, Santa Rosa de Copán, 41101 CA
            </div>
            <div
              style={{
                fontSize: "16px",
                fontWeight: "500",
                color: "#4b5563",
                padding: "8px 0",
              }}
            >
              Copán, Honduras
            </div>
            {!showResult && (
              <div
                style={{
                  fontSize: "24px",
                  fontWeight: "bold",
                  color: "#1f2937",
                  padding: "16px 0",
                }}
              >
                Cierre de Caja
              </div>
            )}
          </div>

          <Divider className="my-4" />

          {!showResult && !showConfirmation && (
            <div>
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
                      { label: "B: 9:00 pm - 9:00 am", value: "Noche" },
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
            </div>
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
              <div
                style={{
                  textAlign: "center",
                  fontSize: "20px",
                  fontWeight: "bold",
                  marginBottom: "24px",
                }}
              >
                Resumen de Cierre
              </div>

              <Divider className="my-4" />

              <Row gutter={[16, 16]} style={{ marginBottom: "24px" }}>
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

              <Row gutter={[16, 16]} style={{ marginBottom: "16px" }}>
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

              <div
                style={{
                  marginBottom: "16px",
                  color: "#9ca3af",
                  fontSize: "12px",
                }}
              >
                27 de Septiembre de 2025 &nbsp;&nbsp; 04:36:12 pm
              </div>

              {isMatch ? (
                <>
                  <Alert
                    message="¡Cierre Correcto!"
                    description={`El monto ingresado coincide con el sistema: Lp ${amount?.toFixed(2)}`}
                    type="success"
                    showIcon
                    style={{ marginBottom: "16px" }}
                  />
                  <div
                    style={{
                      textAlign: "center",
                      fontSize: "18px",
                      marginTop: "16px",
                    }}
                  >
                    Sistema de Gestión de Receptoría de Fondos
                  </div>
                  <div
                    style={{
                      textAlign: "right",
                      fontSize: "18px",
                      marginBottom: "16px",
                      color: "#4b5563",
                    }}
                  >
                    ID:GJD8792JKDL303LD
                  </div>
                </>
              ) : (
                <>
                  <Alert
                    message="Diferencia Detectada"
                    description={
                      <div>
                        <p style={{ marginBottom: "8px" }}>
                          <strong>Diferencia:</strong>{" "}
                          <span
                            style={{
                              color: "#ef4444",
                              fontSize: "18px",
                              fontWeight: "bold",
                            }}
                          >
                            Lp {Math.abs(difference).toFixed(2)}
                          </span>
                        </p>
                        <p style={{ marginBottom: 0 }}>
                          El monto ingresado{" "}
                          {difference > 0 ? "es mayor" : "es menor"} que el
                          registrado en el sistema.
                        </p>
                      </div>
                    }
                    type="error"
                    showIcon
                    style={{ marginBottom: "16px" }}
                  />
                </>
              )}
            </div>
          )}
        </div>

        {/* Botones de acción FUERA del área de impresión */}
        {showResult && (
          <div className="text-center mt-6">
            {isMatch ? (
              <Space size="large">
                <Dropdown
                  menu={{ items: printMenuItems }}
                  trigger={["click"]}
                  placement="topCenter"
                >
                  <Button
                    type="primary"
                    size="large"
                    icon={<PrinterOutlined />}
                  >
                    Imprimir / Exportar
                  </Button>
                </Dropdown>
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
            ) : (
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
            )}
          </div>
        )}
      </ProCard>
    </PageContainer>
  );
};
