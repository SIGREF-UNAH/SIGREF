import {
  Card,
  Typography,
  Space,
  Select,
  Input,
  Divider,
  Button,
  Alert,
} from "antd";
import { SettingOutlined } from "@ant-design/icons";
import { useState, useEffect } from "react";
import { keycloak } from "../../../auth";
import { SeriesManagementModal } from "./modals";
import { useInvocesSeries } from "../hooks";

export const IncomeSummary = ({
  serie,
  setSerie,
  setSerieId,
  numeroRecibo,
  setNumeroRecibo,
  selectedPaciente,
  selectedServicio,
  aPagarEfectivo,
  exonerado,
  tramiteEmergencia,
}: {
  serie: string;
  setSerie: (value: string) => void;
  setSerieId: (id: string) => void;
  numeroRecibo: string;
  setNumeroRecibo: (value: string) => void;
  selectedPaciente?: any;
  selectedServicio?: any;
  aPagarEfectivo?: number;
  exonerado?: boolean;
  tramiteEmergencia?: boolean;
}) => {
  const usuario = keycloak.tokenParsed?.name || "N/A";
  const [modalOpen, setModalOpen] = useState(false);

  const {
    series,
    isLoading: isLoadingSeries,
    isError,
    refetch,
    getSerieByPrefix,
    getNextNumber,
  } = useInvocesSeries();

  useEffect(() => {
    if (!isLoadingSeries && series.length === 0) {
      setModalOpen(true);
    }
  }, [isLoadingSeries, series.length]);

  useEffect(() => {
    if (series.length > 0 && !serie) {
      const firstSerie = series[0];
      const firstSerieId = (firstSerie as any).id;
      setSerie(firstSerie.prefix || "");
      setSerieId(firstSerieId);
      if (!numeroRecibo) {
        const nextNum = getNextNumber(firstSerieId);
        setNumeroRecibo(nextNum.toString());
      }
    }
  }, [
    series,
    serie,
    setSerie,
    setSerieId,
    numeroRecibo,
    setNumeroRecibo,
    getNextNumber,
  ]);

  // Manejar cambio de serie
  const handleSerieChange = (value: string) => {
    const selectedSerie = getSerieByPrefix(value);

    if (selectedSerie) {
      const selectedSerieId = (selectedSerie as any).id;

      if (!selectedSerieId) {
        console.error("ERROR: Serie seleccionada no tiene ID válido!");
        return;
      }

      setSerie(value);
      setSerieId(selectedSerieId);

      const nextNum = getNextNumber(selectedSerieId);
      setNumeroRecibo(nextNum.toString());
    }
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
      <Typography.Title level={5}>
        <Space>
          Serie y Número de Recibo
          <Button
            type="text"
            size="small"
            icon={<SettingOutlined />}
            onClick={() => setModalOpen(true)}
          />
        </Space>
      </Typography.Title>

      {isError && (
        <Alert
          message="Error al cargar series"
          description="No se pudieron cargar las series. Intenta recargar la página."
          type="error"
          showIcon
          closable
          style={{ marginBottom: 16 }}
        />
      )}

      {isLoadingSeries && (
        <Alert
          message="Cargando series..."
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
        />
      )}

      {!isLoadingSeries && !isError && series.length === 0 && (
        <Alert
          message="No hay series configuradas"
          description="Haz clic en el ícono de configuración para crear una serie antes de continuar."
          type="warning"
          showIcon
          action={
            <Button
              size="small"
              type="primary"
              onClick={() => setModalOpen(true)}
            >
              Crear Serie
            </Button>
          }
          style={{ marginBottom: 16 }}
        />
      )}
      {!isLoadingSeries && !isError && series.length > 0 && (
        <Alert
          message={`${series.length} serie(s) disponible(s)`}
          type="success"
          showIcon
          closable
          style={{ marginBottom: 16 }}
        />
      )}

      <Space>
        <Select
          value={serie}
          onChange={handleSerieChange}
          style={{ width: 200 }}
          loading={isLoadingSeries}
          disabled={series.length === 0 || isLoadingSeries}
          placeholder="Seleccionar serie"
        >
          {series.map((s: any) => (
            <Select.Option key={s.id} value={s.prefix || ""}>
              {s.name} ({s.prefix})
            </Select.Option>
          ))}
        </Select>

        <Input
          placeholder="Número de recibo"
          value={numeroRecibo}
          onChange={(e) => setNumeroRecibo(e.target.value)}
          style={{ width: 200 }}
          type="number"
          disabled={!serie}
        />
      </Space>

      {/* Resumen de Factura */}
      <Divider />
      <Typography.Title level={5}>Resumen de Factura</Typography.Title>
      <Card size="small" style={{ background: "#fafafa" }}>
        <Space direction="vertical" style={{ width: "100%" }}>
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text>Número:</Typography.Text>
            <Typography.Text strong>
              {numeroRecibo || "Sin número"}
            </Typography.Text>
          </div>
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text>Serie:</Typography.Text>
            <Typography.Text strong>{serie || "N/A"}</Typography.Text>
          </div>
          <Divider style={{ margin: "8px 0" }} />
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

              {/* Mostrar codigo solo para servicios individuales */}
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

              {/* Mostrar codigo del paquete */}
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
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text strong>Total a Pagar:</Typography.Text>
            <Typography.Text strong style={{ fontSize: 16, color: "#1890ff" }}>
              L.{precioFinal.toFixed(2)}
            </Typography.Text>
          </div>

          {/* Indicador visual si esta exonerado o es emergencia */}
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

      <Typography.Text
        type="secondary"
        style={{ fontSize: 12, display: "block", marginTop: 12 }}
      >
        Hospital de Occidente, Santa Rosa de Copán, Honduras
      </Typography.Text>
      <Typography.Text type="secondary" style={{ fontSize: 12 }}>
        Cajero: {usuario}
      </Typography.Text>

      {/* Modal de gestion de series */}
      <SeriesManagementModal
        key={modalOpen ? "open" : "closed"}
        open={modalOpen}
        onClose={() => {
          setModalOpen(false);
          console.log("Modal cerrado, refetching series...");
          refetch();
        }}
        series={series}
        refetch={refetch}
      />
    </>
  );
};
