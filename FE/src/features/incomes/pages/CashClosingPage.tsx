import {
  CheckOutlined,
  CloseOutlined,
  PrinterOutlined,
  FilePdfOutlined,
  FileImageOutlined,
} from "@ant-design/icons";
import {
  Alert,
  Button,
  Col,
  Divider,
  Dropdown,
  InputNumber,
  Row,
  Space,
  Typography,
  message,
  Spin,
} from "antd";
import { useRef, useState } from "react";
import { useNavigate } from "react-router";
import { useKeycloak } from "@react-keycloak/web";
import { PageHeaderTabs } from "../../../shared/components";
import { useGetApiHospitalPropertiesDetails } from "../../../api/hospital-properties/hospital-properties";
import dayjs from "dayjs";
import { useCashierSessionStore } from "../../cashier-sessions/store";
import { usePostApiCashierSessionsSessionIdClose } from "../../../api/cashier-sessions/cashier-sessions";
import useMediaFiles from "../../media-files/hooks/useMediaFiles";
import { useExport } from "../../../shared/utils";

export const CashClosingPage = () => {
  const navigate = useNavigate();
  const { keycloak } = useKeycloak();
  const { getMediaUrl } = useMediaFiles();
  const [amount, setAmount] = useState<number | null>(null);
  const [isLocked, setIsLocked] = useState(false);
  const [showConfirmation, setShowConfirmation] = useState(false);
  const [showResult, setShowResult] = useState(false);
  const [closedSessionId, setClosedSessionId] = useState<string>("");
  
  //* systemAmount es el monto del sistema que se debe comparar con el monto declarado por el cashier
  const [systemAmount] = useState(0); //! Este valor debe venir del backend
  
  // Store
  const { session, clearSession } = useCashierSessionStore();
  
  // Queries
  const { data: hospitalResponse, isLoading: isLoadingHospital } = useGetApiHospitalPropertiesDetails();
  
  // Mutation para cerrar sesión
  const { mutate: closeSession, isPending: isClosingSession } = usePostApiCashierSessionsSessionIdClose({
    mutation: {
      onSuccess: (response: any) => {
        setClosedSessionId(response?.data?.id || session?.id || "");
        message.success(response?.message || 'Sesión cerrada exitosamente');
        setShowConfirmation(false);
        setShowResult(true);
        // Limpiar la sesión del store DESPUÉS de mostrar el resultado
        // No limpiamos inmediatamente para que se pueda ver el ID en el resumen
      },
      onError: (error: any) => {
        message.error(
          error?.response?.data?.message || 'Error al cerrar la sesión'
        );
        setIsLocked(false);
      },
    },
  });

  // Datos del hospital
  const hospitalResponseData = hospitalResponse as any;
  const hospitalData = hospitalResponseData?.data;

  const logoHealthUrl = hospitalData?.urlLogoHealth 
    ? getMediaUrl(hospitalData.urlLogoHealth) 
    : "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f1/Logo_de_SESAL.svg/1200px-Logo_de_SESAL.svg.png";
  
  const logoHospitalUrl = hospitalData?.urlLogo 
    ? getMediaUrl(hospitalData.urlLogo) 
    : "https://krti.cl/wp-content/uploads/2021/04/Logo-Hospital-Final.png";

  const hospitalName = hospitalData?.name || "Hospital";
  const hospitalAddress = hospitalData?.ubication || "";
  const hospitalCurrency = hospitalData?.currency || "LPS";

  const userName = keycloak.tokenParsed?.name || "Usuario";

  // Fecha y hora actual
  const currentDateTime = dayjs().format('DD [de] MMMM [de] YYYY hh:mm:ss A');
  const currentDate = dayjs().format('DD [de] MMMM [de] YYYY');
  const currentTime = dayjs().format('hh:mm:ss A');

  const handleSave = () => {
    if (!session?.id) {
      message.error('No hay sesión activa para cerrar');
      return;
    }
    setShowConfirmation(true);
    setIsLocked(true);
  };

  const handleConfirm = () => {
    if (!session?.id || amount === null) {
      message.error('Datos incompletos para cerrar la sesión');
      return;
    }

    closeSession({
      sessionId: session.id,
      data: {
        declaredAmount: amount,
      },
    });
  };

  const handleCancel = () => {
    setShowConfirmation(false);
    setIsLocked(false);
  };

  const handleFinish = () => {
    navigate('/');
    clearSession();
  };

  const difference = amount !== null ? amount - systemAmount : 0;
  const isMatch = difference === 0;

  // Ref para exportación
  const printRef = useRef<HTMLDivElement>(null);
  const { exportData } = useExport();

  // Opciones de exportación
  const printMenuItems = [
    {
      key: "pdf",
      label: "Exportar como PDF",
      icon: <FilePdfOutlined />,
      onClick: () =>
        exportData("print-area", "pdf", { fileName: "Cierre-de-caja-" + currentDateTime.replace(/[: ]/g, "-") + ".pdf" }),
    },
    {
      key: "image",
      label: "Exportar como Imagen",
      icon: <FileImageOutlined />,
      onClick: () =>
        exportData("print-area", "png", { fileName: "Cierre-de-caja-" + currentDateTime.replace(/[: ]/g, "-") + ".png" }),
    },
  ];

  if (isLoadingHospital) {
    return (
      <div className="flex items-center justify-center h-screen">
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div>
      {/* Encabezado */}
      <PageHeaderTabs title="Cierre de Caja" tabs={[]} />

      {/* Contenido */}
      <div className="primary-card">
        {/* Area de exportación */}
        <div className="secondary-card">
          <div
            ref={printRef}
            id="print-area"
            style={{
              padding: "20px",
              backgroundColor: "#FFFFFF",
              position: "relative",
            }}
          >
            {/* Header con logos */}
            <div className="flex justify-between items-center w-full mb-6">
              <img
                src={logoHealthUrl}
                alt="Logo Salud"
                style={{ height: "48px", objectFit: "contain" }}
                onError={(e) => {
                  e.currentTarget.src = "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f1/Logo_de_SESAL.svg/1200px-Logo_de_SESAL.svg.png";
                }}
              />
              <div className="flex-1"></div>
              <img
                src={logoHospitalUrl}
                alt={hospitalName}
                style={{ height: "48px", objectFit: "contain" }}
                onError={(e) => {
                  e.currentTarget.src = "https://krti.cl/wp-content/uploads/2021/04/Logo-Hospital-Final.png";
                }}
              />
            </div>

            {/* Información */}
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
                {hospitalName}
              </div>
              <div
                style={{
                  fontSize: "16px",
                  fontWeight: "500",
                  color: "#4b5563",
                  padding: "8px 0",
                }}
              >
                {hospitalAddress}
              </div>

              {!showResult && (
                <>
                  <Divider className="my-4" />
                  <div
                    style={{
                      textAlign: "center",
                      fontSize: "20px",
                      fontWeight: "bold",
                      marginBottom: "24px",
                      color: "#1f2937",
                    }}
                  >
                    Cierre de Caja
                  </div>
                </>
              )}
            </div>

            <Divider className="my-4" />

            {/* Formulario de cierre */}
            {!showResult && !showConfirmation && (
              <div>
                <Row gutter={[16, 16]}>
                  <Col span={12}>
                    <Typography.Text strong>
                        Auxiliar de Caja:
                    </Typography.Text>{" "}
                    <Typography.Text strong className="text-secondary!">{userName}</Typography.Text>
                  </Col>

                  <Col span={24}>
                    <Typography.Text strong>
                      Ingrese el monto registrado en caja:
                    </Typography.Text>
                    <InputNumber
                      min={0}
                      value={amount ?? undefined}
                      disabled={isLocked}
                      onChange={(value) => setAmount(value ?? 0)}
                      style={{ width: "100%", marginTop: 8 }}
                      placeholder="0.00"
                      size="large"
                      prefix={hospitalCurrency}
                    />
                  </Col>
                </Row>

                <div className="flex justify-between mt-6">
                  <div className="font-bold text-general-secondary">
                    {currentDate}
                  </div>
                  <div className="text-secondary font-bold">{currentTime}</div>
                </div>

                <div className="text-center mt-6">
                  <Button
                    type="primary"
                    onClick={handleSave}
                    // disabled={!amount || isClosingSession}
                    loading={isClosingSession}
                    size="large"
                    variant="solid"
                    color="green"
                  >
                    Guardar Cierre
                  </Button>
                </div>
              </div>
            )}

            {/* Confirmación de cierre */}
            {showConfirmation && (
              <div className="pb-6 text-center">
                <div className="mb-4">
                  <Typography.Text strong>
                      Auxiliar de Caja:
                  </Typography.Text>{" "}
                  <Typography.Text strong className="text-secondary!">{userName}</Typography.Text>
                </div>
                <div className="mb-4">
                  <Typography.Text strong>Monto Registrado:</Typography.Text>{" "}
                  <Typography.Text
                    style={{ color: "#EF5350" }}
                    className="text-lg font-bold"
                  >
                    {hospitalCurrency} {amount?.toFixed(2)}
                  </Typography.Text>
                </div>
                <div className="mt-4 text-general-secondary text-md py-2">
                  {currentDateTime}
                </div>
                <div className="justify-center text-center mt-6">
                  <div className="text-xl font-bold text-general py-2">
                    ¿Está seguro de realizar el cierre de caja?
                  </div>
                </div>
                <div className="mt-4 flex items-center justify-center">
                  <Alert
                    style={{width: "50%"}}
                    showIcon
                    type="warning"
                    description="Una vez confirmado, no podrá modificar los datos del cierre actual."
                  />
                </div>
                <Divider className="my-4" />
                <div className="text-center mt-6">
                  <Space size="middle">
                    <Button
                      onClick={handleCancel}
                      size="large"
                      variant="solid"
                      color="danger"
                      icon={<CloseOutlined />}
                      disabled={isClosingSession}
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
                      loading={isClosingSession}
                      disabled={isClosingSession}
                    >
                      Confirmar
                    </Button>
                  </Space>
                </div>
              </div>
            )}

            {/* Resumen de cierre */}
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

                <div className="text-center pb-6">
                  <div className="mb-4">
                    <Typography.Text strong>
                        Auxiliar de Caja:
                    </Typography.Text>{" "}
                    <Typography.Text strong className="text-secondary!">{userName}</Typography.Text>
                  </div>
                  <div className="mb-4">
                    <Typography.Text strong>Monto Registrado:</Typography.Text>{" "}
                    <Typography.Text
                      style={{
                        color: isMatch ? "#52c41a" : "#ff4d4f",
                        fontWeight: "bold",
                      }}
                    >
                      {hospitalCurrency} {amount?.toFixed(2)}
                    </Typography.Text>
                  </div>
                  <div className="mb-4">
                    <Typography.Text strong>Monto del Sistema:</Typography.Text>{" "}
                    <Typography.Text
                      style={{
                        color: "#4CAF50",
                        fontWeight: "bold",
                      }}
                    >
                      {hospitalCurrency} {systemAmount.toFixed(2)}
                    </Typography.Text>
                  </div>
                  <div className="mt-4 text-general-secondary text-md py-2">
                    {currentDateTime}
                  </div>
                </div>

                {isMatch ? ( // Si el monto es correcto
                  <>
                    <div className="mt-2 flex items-center justify-center">
                      <Alert
                        message="¡Cierre correcto!"
                        description={`El monto ingresado coincide con el sistema: ${hospitalCurrency} ${amount?.toFixed(2)}`}
                        type="success"
                        showIcon
                        style={{ marginBottom: "16px" }}
                      />
                    </div>
                    <div className="text-general-secondary text-center my-4 italic">
                      Sistema de Gestión de Receptoría de Fondos
                    </div>
                    <div className="text-general-secondary text-xs text-center my-4 mb-24 italic">
                      ID: {closedSessionId}
                    </div>
                  </>
                ) : ( // Si hay diferencia
                  <>
                    <div className="my-2 flex items-center justify-center">
                      <Alert
                        message="Diferencia detectada"
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
                                {hospitalCurrency} {Math.abs(difference).toFixed(2)}
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
                    </div>
                    <div className="text-general-secondary text-center my-4 italic">
                      Sistema de Gestión de Receptoría de Fondos
                    </div>
                    <div className="text-general-secondary text-xs text-center my-4 mb-24 italic">
                      ID: {closedSessionId}
                    </div>
                  </>
                )}
              </div>
            )}
          </div>
        </div>

        {/* Botones de acción */}
        {showResult && (
          <div className="text-center mt-6">
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
                  Exportar
                </Button>
              </Dropdown>
              <Button
                type="primary"
                size="large"
                color="green"
                variant="solid"
                icon={<CheckOutlined />}
                onClick={handleFinish}
              >
                Finalizar
              </Button>
            </Space>
          </div>
        )}
      </div>
    </div>
  );
};