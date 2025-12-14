import {
  CheckOutlined,
  CloseOutlined,
  PrinterOutlined,
  WarningOutlined,
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
import { useExport } from "../../../shared/utils";
import { PageHeaderTabs } from "../../../shared/components";
import { useGetApiHospitalPropertiesDetails } from "../../../api/hospital-properties/hospital-properties";
import { USER_ROLE_OPTIONS } from "../../../shared/constants";
import dayjs from "dayjs";
import { useCashierSessionStore } from "../../cashier-sessions/store";
import { usePostApiCashierSessionsSessionIdClose } from "../../../api/cashier-sessions/cashier-sessions";

// Función helper para construir URLs de media
const getMediaUrl = (relativePath?: string | null): string => {
  if (!relativePath) return '';
  
  const API_BASE_URL = import.meta.env.VITE_API_URL || window.location.origin;
  const baseUrl = API_BASE_URL.endsWith('/') 
    ? API_BASE_URL.slice(0, -1) 
    : API_BASE_URL;
  
  if (relativePath.startsWith('http://') || relativePath.startsWith('https://')) {
    return relativePath;
  }
  
  if (relativePath.startsWith('/files/') || relativePath.startsWith('/media/')) {
    return `${baseUrl}${relativePath}`;
  }
  
  return `${baseUrl}${relativePath.startsWith('/') ? '' : '/'}${relativePath}`;
};

export const CashClosingPage = () => {
  const navigate = useNavigate();
  const { keycloak } = useKeycloak();
  const [amount, setAmount] = useState<number | null>(null);
  const [isLocked, setIsLocked] = useState(false);
  const [showConfirmation, setShowConfirmation] = useState(false);
  const [showResult, setShowResult] = useState(false);
  const [closedSessionId, setClosedSessionId] = useState<string>("");
  
  const [systemAmount] = useState(0); //! Este valor debería venir del backend
  
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

  // Datos del usuario
  const roles = keycloak.tokenParsed?.realm_access?.roles || [];
  const rolesValidos = roles
    .map((rol) => {
      const roleOption = USER_ROLE_OPTIONS.find(option => option.value === rol);
      return roleOption ? roleOption.label : undefined;
    })
    .filter((rolMapeado) => rolMapeado !== undefined);

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
    // Limpiar la sesión del store al finalizar
    clearSession();
    // Redirigir al home
    navigate('/');
  };

  const handleGoToCorrection = () => {
    // NO limpiar la sesión aquí porque la corrección aún la necesita
    navigate('/cashier/correction');
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

  if (isLoadingHospital) {
    return (
      <div className="flex items-center justify-center h-screen">
        <Spin size="large" />
      </div>
    );
  }

  if (!session?.id && !showResult) {
    return (
      <div className="flex items-center justify-center h-screen">
        <Alert
          message="No hay sesión activa"
          description="Debe abrir una sesión de caja antes de poder cerrarla"
          type="warning"
          showIcon
          action={
            <Button type="primary" onClick={() => navigate('/cashier/open-session')}>
              Abrir Sesión
            </Button>
          }
        />
      </div>
    );
  }

  return (
    <div>
      <PageHeaderTabs title="Cierre de Caja" tabs={[]} />
      <div className="primary-card">
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
                  <Typography.Text>{userName}</Typography.Text>
                </Col>
                <Col span={12}>
                  <Typography.Text strong style={{ color: "#7BA2D4" }}>
                    Rol:
                  </Typography.Text>{" "}
                  <Typography.Text style={{ color: "#7BA2D4" }}>
                    {rolesValidos[0] || "Cajero"}
                  </Typography.Text>
                </Col>

                <Col span={24}>
                  <Typography.Text strong>
                    Ingrese el Monto Registrado en Caja
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

          {showConfirmation && (
            <div className="py-6">
              <Row gutter={[16, 16]} className="mb-4">
                <Col span={12}>
                  <Typography.Text strong style={{ color: "#7BA2D4" }}>
                    Auxiliar de Caja:
                  </Typography.Text>{" "}
                  <Typography.Text strong>{userName}</Typography.Text>
                </Col>
                <Col span={12}>
                  <Typography.Text strong style={{ color: "#7BA2D4" }}>
                    Rol:
                  </Typography.Text>{" "}
                  <Typography.Text style={{ color: "#7BA2D4" }}>
                    {rolesValidos[0] || "Cajero"}
                  </Typography.Text>
                </Col>

                <Col span={12}>
                  <Typography.Text strong>Monto Registrado:</Typography.Text>{" "}
                  <Typography.Text
                    style={{ color: "#EF5350" }}
                    className="text-lg font-bold"
                  >
                    {hospitalCurrency} {amount?.toFixed(2)}
                  </Typography.Text>
                </Col>
              </Row>
              <div className="mt-4 text-general-secondary text-md py-2">
                {currentDateTime}
              </div>
              <div className="justify-center text-center">
                <div className="text-lg font-medium text-general-secondary py-2">
                  Sistema de Gestión de Receptoría de Fondos
                </div>
                <div className="text-xl font-bold text-general py-2">
                  ¿Estás Seguro de Cerrar Caja?
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
                  <Typography.Text strong>{userName}</Typography.Text>
                </Col>
                <Col span={12}>
                  <Typography.Text strong style={{ color: "#7BA2D4" }}>
                    Rol:
                  </Typography.Text>{" "}
                  <Typography.Text>{rolesValidos[0] || "Cajero"}</Typography.Text>
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
                    {hospitalCurrency} {amount?.toFixed(2)}
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
                    {hospitalCurrency} {systemAmount.toFixed(2)}
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
                {currentDateTime}
              </div>

              {isMatch ? (
                <>
                  <Alert
                    message="¡Cierre Correcto!"
                    description={`El monto ingresado coincide con el sistema: ${hospitalCurrency} ${amount?.toFixed(2)}`}
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
                    ID: {closedSessionId}
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
                  onClick={handleFinish}
                >
                  Finalizar
                </Button>
              </Space>
            ) : (
              <Button
                type="primary"
                size="large"
                color="orange"
                variant="solid"
                icon={<WarningOutlined />}
                onClick={handleGoToCorrection}
              >
                Ir a Corrección
              </Button>
            )}
          </div>
        )}
      </div>
    </div>
  );
};