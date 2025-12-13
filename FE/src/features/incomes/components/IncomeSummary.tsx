import { Card, Typography, Space, Select, Input, Divider, Button, Alert } from "antd";
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
}: any) => {
  const usuario = keycloak.tokenParsed?.name || "N/A";
  const [modalOpen, setModalOpen] = useState(false);

  // Obtener series del backend
  const {
    series,
    isLoading: isLoadingSeries,
    isError,
    refetch,
    getSerieByPrefix,
    getNextNumber,
  } = useInvocesSeries();

  // Auto-abrir modal si no hay series
  useEffect(() => {
    if (!isLoadingSeries && series.length === 0) {
      setModalOpen(true);
    }
  }, [isLoadingSeries, series.length]);

  // Auto-seleccionar primera serie disponible
  useEffect(() => {
    if (series.length > 0 && !serie) {
      const firstSerie = series[0];
      const firstSerieId = (firstSerie as any).id; 
      setSerie(firstSerie.prefix || "");
      setSerieId(firstSerieId);
      
      // Auto-generar número de recibo
      if (!numeroRecibo) {
        const nextNum = getNextNumber(firstSerieId);        
        setNumeroRecibo(nextNum.toString());
      }
    }
  }, [series, serie, setSerie, setSerieId, numeroRecibo, setNumeroRecibo, getNextNumber]);

  // Manejar cambio de serie
  const handleSerieChange = (value: string) => {
    const selectedSerie = getSerieByPrefix(value);
    
    if (selectedSerie) {
      const selectedSerieId = (selectedSerie as any).id; // Cast para acceder al id
      
      if (!selectedSerieId) {
        console.error("ERROR: Serie seleccionada no tiene ID válido!");
        return;
      }
      
      setSerie(value);
      setSerieId(selectedSerieId);
      
      // Auto-generar número de recibo
      const nextNum = getNextNumber(selectedSerieId);
      setNumeroRecibo(nextNum.toString());
    }
  };

  // Calcular el precio actual
  const precioOriginal = selectedServicio?.cost || 0;
  const precioFinal = precioOriginal;

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

      {/* Alert de error */}
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

      {/* Alert de cargando */}
      {isLoadingSeries && (
        <Alert
          message="Cargando series..."
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
        />
      )}

      {/* Alert de sin series */}
      {!isLoadingSeries && !isError && series.length === 0 && (
        <Alert
          message="No hay series configuradas"
          description="Haz clic en el ícono de configuración para crear una serie antes de continuar."
          type="warning"
          showIcon
          action={
            <Button size="small" type="primary" onClick={() => setModalOpen(true)}>
              Crear Serie
            </Button>
          }
          style={{ marginBottom: 16 }}
        />
      )}

      {/* Alert de éxito */}
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
          placeholder="Número de Recibo"
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
              {selectedPaciente?.identificadorTipo || "N/A"}: {selectedPaciente?.identificador || "N/A"}
            </Typography.Text>
          </div>
          <Divider style={{ margin: "8px 0" }} />
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text>Servicio:</Typography.Text>
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
                  {selectedServicio.name || "Servicio sin nombre"}
                </Typography.Text>
                <Typography.Text>
                  L.{precioOriginal.toFixed(2)}
                </Typography.Text>
              </div>
              {selectedServicio.abbreviation && (
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
                Sin servicio seleccionado
              </Typography.Text>
            </div>
          )}
          <Divider style={{ margin: "8px 0" }} />
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text strong>Total:</Typography.Text>
            <Typography.Text strong style={{ fontSize: 16, color: "#1890ff" }}>
              L.{precioFinal.toFixed(2)}
            </Typography.Text>
          </div>
        </Space>
      </Card>

      <Typography.Text
        type="secondary"
        style={{ fontSize: 12, display: "block", marginTop: 8 }}
      >
        Hospital de Occidente, Santa Rosa de Copán, Honduras
      </Typography.Text>
      <Typography.Text type="secondary" style={{ fontSize: 12 }}>
        Cajero: {usuario}
      </Typography.Text>

      {/* Modal de gestión de series */}
      <SeriesManagementModal
        key={modalOpen ? 'open' : 'closed'} // Forzar remount
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