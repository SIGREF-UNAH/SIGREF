import {
  Card,
  Typography,
  Space,
  Select,
  Input,
  Divider,
  Button,
  Alert,
  Tooltip,
} from "antd";
import { InfoCircleOutlined, ReloadOutlined } from "@ant-design/icons";
import { keycloak } from "../../../auth";
import { useInvoiceSeriesManager } from "../hooks";


interface IncomeSummaryProps {
  selectedPaciente?: any;
  selectedServicio?: any;
  aPagarEfectivo?: number;
  exonerado?: boolean;
  tramiteEmergencia?: boolean;
  onSerieChange?: (serieId: string, seriePrefix: string) => void;
  onNumeroReciboChange?: (numero: string) => void;
}

export const IncomeSummary = ({
  selectedPaciente,
  selectedServicio,
  aPagarEfectivo,
  exonerado,
  tramiteEmergencia,
  onSerieChange,
  onNumeroReciboChange,
}: IncomeSummaryProps) => {
  const usuario = keycloak.tokenParsed?.name || "N/A";

  const {
    series,
    seriePrefix,
    numeroRecibo,
    currentSerie,
    isLoading,
    isError,
    isNumberInRange,
    handleSerieChange,
    setNumeroRecibo,
    refetchSeries,
  } = useInvoiceSeriesManager();

  // Notificar cambios al padre
  const handleSerieChangeInternal = (prefix: string) => {
    handleSerieChange(prefix);
    const selected = series.find((s) => s.prefix === prefix);
    if (selected?.id && selected?.prefix) {
      onSerieChange?.(selected.id, selected.prefix);
    }
  };

  const handleNumeroChange = (value: string) => {
    setNumeroRecibo(value);
    onNumeroReciboChange?.(value);
  };

  // Calcular precios correctamente
  const precioOriginal = selectedServicio?.precio || 0;

  // Calcular el descuento basado en el estado
  let descuento = 0;
  let razonDescuento = "";

  if (exonerado) {
    descuento = precioOriginal;
    razonDescuento = "Exonerado";
  } else if (tramiteEmergencia) {
    descuento = precioOriginal;
    razonDescuento = "Trámite de Emergencia";
  } else if (aPagarEfectivo !== undefined && aPagarEfectivo < precioOriginal) {
    descuento = precioOriginal - aPagarEfectivo;
    razonDescuento = "Ajuste manual";
  }

  const precioFinal =
    aPagarEfectivo !== undefined ? aPagarEfectivo : precioOriginal;

  return (
    <>
      {/* Alertas de estado */}
      {isError && (
        <Alert
          type="error"
          message="Error al cargar series"
          description="Por favor, intenta recargar o contacta a soporte técnico"
          showIcon
          closable
          action={
            <Button size="small" type="text" onClick={() => refetchSeries()}>
              <ReloadOutlined /> Reintentar
            </Button>
          }
        />
      )}

      {isLoading && (
        <Alert type="info" message="Cargando series..." showIcon />
      )}

      {!isLoading && !isError && series.length === 0 && (
        <Alert
          type="warning"
          message="No hay series configuradas"
          description="Contacta al administrador del sistema para configurar series"
          showIcon
        />
      )}

      {!isLoading && !isError && series.length > 0 && (
        <Alert
          type="success"
          message={`${series.length} serie(s) disponible(s)`}
          showIcon
          closable
        />
      )}

      {/* Selección de Serie y Número */}
      <Space direction="vertical" style={{ width: "100%", marginTop: 16 }} size="small">
        <Space>
          <div>
            <Typography.Text type="secondary" style={{ fontSize: 14 }}>
              Serie de Facturación {" "}
            </Typography.Text>
            <Space>
              <Select
                value={seriePrefix}
                onChange={handleSerieChangeInternal}
                placeholder="Seleccionar serie"
                loading={isLoading}
                disabled={isLoading || series.length === 0}
                style={{ width: 240 }}
              >
                {series.map((s) => (
                  <Select.Option key={s.id} value={s.prefix ?? ""}>
                    <Space>
                      <span>{s.name || "Sin nombre"}</span>
                      <Typography.Text type="secondary" style={{ fontSize: 11 }}>
                        ({s.prefix || "—"})
                      </Typography.Text>
                    </Space>
                  </Select.Option>
                ))}
              </Select>

              {currentSerie && (
                <Tooltip
                  title={
                    <div>
                      <div>Rango permitido:</div>
                      <div>
                        Inicio: {currentSerie.startNumber ?? "N/A"}
                      </div>
                      <div>
                        Fin: {currentSerie.endNumber ?? "N/A"}
                      </div>
                      <div>
                        Actual: {currentSerie.currentNumber ?? "N/A"}
                      </div>
                    </div>
                  }
                >
                  <InfoCircleOutlined style={{ color: "#1890ff" }} />
                </Tooltip>
              )}
            </Space>
          </div>

          <div>
            <Typography.Text type="secondary" style={{ fontSize: 14 }}>
              Número de Recibo {" "}
            </Typography.Text>
            <Input
              value={numeroRecibo}
              onChange={(e) => handleNumeroChange(e.target.value)}
              placeholder="Número"
              type="number"
              disabled={!seriePrefix}
              style={{ width: 160 }}
              status={
                numeroRecibo && !isNumberInRange ? "error" : undefined
              }
            />
          </div>
        </Space>

        {/* Advertencia si el número está fuera de rango */}
        {numeroRecibo && !isNumberInRange && (
          <Alert
            type="warning"
            message="Número fuera de rango"
            description={`El número debe estar entre ${currentSerie?.startNumber ?? "?"} y ${currentSerie?.endNumber ?? "?"}`}
            showIcon
            style={{ marginTop: 8 }}
          />
        )}
      </Space>

      {/* Resumen de Factura */}
      <Divider />
      <Typography.Title level={5}>Resumen de Factura</Typography.Title>
      <Card size="small" style={{ background: "#fafafa" }}>
        <Space direction="vertical" style={{ width: "100%" }}>
          {/* Información de la Factura */}
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text>Número:</Typography.Text>
            <Typography.Text strong>
              {seriePrefix && numeroRecibo
                ? `${seriePrefix}-${numeroRecibo.padStart(8, "0")}`
                : "Sin número"}
            </Typography.Text>
          </div>

          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text>Serie:</Typography.Text>
            <Typography.Text strong>
              {currentSerie?.name || seriePrefix || "N/A"}
            </Typography.Text>
          </div>

          <Divider style={{ margin: "8px 0" }} />

          {/* Información del Paciente */}
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text>Paciente:</Typography.Text>
            <Typography.Text strong>
              {selectedPaciente?.nombre || "Sin seleccionar"}
            </Typography.Text>
          </div>

          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text>Identificador:</Typography.Text>
            <Typography.Text strong>
              {selectedPaciente?.identificadorTipo || "N/A"}:{" "}
              {selectedPaciente?.identificador || "N/A"}
            </Typography.Text>
          </div>

          <Divider style={{ margin: "8px 0" }} />

          {/* Información del Servicio/Paquete */}
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text>Servicio/Paquete:</Typography.Text>
          </div>

          {selectedServicio ? (
            <>
              <div
                style={{
                  display: "flex",
                  justifyContent: "space-between",
                  paddingLeft: 16,
                }}
              >
                <Typography.Text type="secondary">
                  {selectedServicio.nombre || "Sin nombre"}
                </Typography.Text>
                <Typography.Text>L.{precioOriginal.toFixed(2)}</Typography.Text>
              </div>

              {/* Mostrar detalles del paquete */}
              {selectedServicio.tipo === "paquete" &&
                selectedServicio.items?.length > 0 && (
                  <div style={{ paddingLeft: 24, marginTop: 8 }}>
                    <Typography.Text
                      type="secondary"
                      style={{ fontSize: 11, fontWeight: "bold" }}
                    >
                      Servicios incluidos ({selectedServicio.items.length}):
                    </Typography.Text>
                    {selectedServicio.items.map((item: any, idx: number) => (
                      <div
                        key={idx}
                        style={{
                          paddingLeft: 8,
                          fontSize: 11,
                          color: "#8c8c8c",
                          marginTop: 2,
                        }}
                      >
                        • {item.name || "Servicio"}
                      </div>
                    ))}
                  </div>
                )}

              {/* Mostrar código solo para servicios individuales */}
              {selectedServicio.tipo === "servicio" &&
                selectedServicio.abbreviation && (
                  <div
                    style={{
                      display: "flex",
                      justifyContent: "space-between",
                      paddingLeft: 16,
                    }}
                  >
                    <Typography.Text type="secondary" style={{ fontSize: 11 }}>
                      Código: {selectedServicio.abbreviation}
                    </Typography.Text>
                  </div>
                )}

              {/* Mostrar código del paquete */}
              {selectedServicio.tipo === "paquete" && selectedServicio.code && (
                <div
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    paddingLeft: 16,
                  }}
                >
                  <Typography.Text type="secondary" style={{ fontSize: 11 }}>
                    Código: {selectedServicio.code}
                  </Typography.Text>
                </div>
              )}

              {/* Mostrar descuento si existe */}
              {descuento > 0 && (
                <>
                  <Divider style={{ margin: "8px 0" }} dashed />
                  <div
                    style={{
                      display: "flex",
                      justifyContent: "space-between",
                      paddingLeft: 16,
                    }}
                  >
                    <Typography.Text type="warning">
                      Descuento ({razonDescuento}):
                    </Typography.Text>
                    <Typography.Text type="warning">
                      -L.{descuento.toFixed(2)}
                    </Typography.Text>
                  </div>
                </>
              )}
            </>
          ) : (
            <div
              style={{
                display: "flex",
                justifyContent: "center",
                paddingLeft: 16,
              }}
            >
              <Typography.Text type="secondary" italic>
                Sin servicio/paquete seleccionado
              </Typography.Text>
            </div>
          )}

          <Divider style={{ margin: "8px 0" }} />

          {/* Total a Pagar */}
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text strong>Total a Pagar:</Typography.Text>
            <Typography.Text strong style={{ fontSize: 16, color: "#1890ff" }}>
              L.{precioFinal.toFixed(2)}
            </Typography.Text>
          </div>

          {/* Indicador visual si está exonerado o es emergencia */}
          {(exonerado || tramiteEmergencia) && (
            <div style={{ marginTop: 8 }}>
              <Alert
                message={
                  exonerado ? "✓ Servicio Exonerado" : "✓ Trámite de Emergencia"
                }
                type="info"
                showIcon={false}
                style={{
                  padding: "4px 8px",
                  fontSize: 11,
                  backgroundColor: exonerado ? "#f6ffed" : "#fff7e6",
                  borderColor: exonerado ? "#b7eb8f" : "#ffd591",
                }}
              />
            </div>
          )}
        </Space>
      </Card>

      {/* Footer */}
      <Typography.Text
        type="secondary"
        style={{ fontSize: 12, display: "block", marginTop: 12 }}
      >
        Hospital de Occidente, Santa Rosa de Copán, Honduras
      </Typography.Text>
      <Typography.Text type="secondary" style={{ fontSize: 12 }}>
        Cajero: {usuario}
      </Typography.Text>
    </>
  );
};