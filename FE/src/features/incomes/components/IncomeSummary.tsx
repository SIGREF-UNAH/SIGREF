// IncomeSummary.tsx
import { Card, Typography, Space, Select, Input, Divider, Spin } from "antd";
import { keycloak } from "../../../auth";
import { useInvoiceSeries } from "../hooks/useInvoiceSeries";


export const IncomeSummary = ({
  serie,
  setSerie,
  setSerieId,
  numeroRecibo,
  setNumeroRecibo,
  selectedPaciente,
  selectedServicio,
}: {
  serie: string;
  setSerie: (value: string) => void;
  setSerieId: (id: string) => void;
  numeroRecibo: string;
  setNumeroRecibo: (value: string) => void;
  selectedPaciente?: any;
  selectedServicio?: any;
}) => {
  const usuario = keycloak.tokenParsed?.name || "N/A";
  
  // Hook personalizado
  const { series, isLoading: loadingSeries } = useInvoiceSeries();

  const handleSerieChange = (prefix: string) => {
    const selected = series.find(s => s.prefix === prefix);
    if (selected) {
      setSerie(selected.prefix);     // Lo que ve el usuario (A, B, 001...)
      setSerieId(selected.id);       // El UUID real que necesita el backend
    }
  };

  const precioOriginal = selectedServicio?.cost || 0;
  const precioFinal = precioOriginal;

  return (
    <>
      <Typography.Title level={5}>Serie y Número de Recibo</Typography.Title>
      <Space>
        <Select
          loading={loadingSeries}
          value={serie}
          onChange={handleSerieChange}
          style={{ width: 140 }}
          placeholder="Seleccione serie"
          showSearch
          optionFilterProp="children"
          notFoundContent={loadingSeries ? <Spin size="small" /> : "No hay series disponibles"}
        >
          {series.map((s) => (
            <Select.Option key={s.id} value={s.prefix}>
              {s.prefix} - {s.name}
            </Select.Option>
          ))}
        </Select>

        <Input
          placeholder="Número de recibo"
          value={numeroRecibo}
          onChange={(e) => setNumeroRecibo(e.target.value)}
          style={{ width: 200 }}
          type="number"
          min={1}
        />
      </Space>

      {/* Resumen de Factura */}
      <Divider />
      <Typography.Title level={5}>Resumen de Factura</Typography.Title>
      <Card size="small" style={{ background: "#fafafa" }}>
        <Space direction="vertical" style={{ width: "100%" }}>
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text>Número:</Typography.Text>
            <Typography.Text strong>{numeroRecibo || "-"}</Typography.Text>
          </div>
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text>Serie:</Typography.Text>
            <Typography.Text strong>{serie || "Ninguna"}</Typography.Text>
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
              {selectedPaciente?.identificadorTipo}: {selectedPaciente?.identificador || "N/A"}
            </Typography.Text>
          </div>

          <Divider style={{ margin: "8px 0" }} />
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text>Servicio:</Typography.Text>
          </div>

          {selectedServicio ? (
            <>
              <div style={{ display: "flex", justifyContent: "space-between", paddingLeft: 16 }}>
                <Typography.Text type="secondary">
                  {selectedServicio.name || "Servicio sin nombre"}
                </Typography.Text>
                <Typography.Text>L.{precioOriginal.toFixed(2)}</Typography.Text>
              </div>
              {selectedServicio.abbreviation && (
                <div style={{ paddingLeft: 16 }}>
                  <Typography.Text type="secondary" style={{ fontSize: 11 }}>
                    Código: {selectedServicio.abbreviation}
                  </Typography.Text>
                </div>
              )}
            </>
          ) : (
            <div style={{ textAlign: "center", padding: "8px 0" }}>
              <Typography.Text type="secondary" italic>
                Sin servicio seleccionado
              </Typography.Text>
            </div>
          )}

          <Divider style={{ margin: "8px 0" }} />
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text strong>Total:</Typography.Text>
            <Typography.Text strong style={{ fontSize: 18, color: "#1890ff" }}>
              L.{precioFinal.toFixed(2)}
            </Typography.Text>
          </div>
        </Space>
      </Card>

      <Typography.Text type="secondary" style={{ fontSize: 12, display: "block", marginTop: 12 }}>
        Hospital de Occidente, Santa Rosa de Copán, Honduras
      </Typography.Text>
      <Typography.Text type="secondary" style={{ fontSize: 12 }}>
        Cajero: {usuario}
      </Typography.Text>
    </>
  );
};