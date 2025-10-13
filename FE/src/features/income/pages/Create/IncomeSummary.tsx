import { Card, Typography, Space, Select, Input, Divider } from "antd";
import { keycloak } from "../../../../auth";

export const IncomeSummary = ({
  serie,
  setSerie,
  numeroRecibo,
  setNumeroRecibo,
  selectedPaciente,
  selectedServicio,
}: any) => {
  const usuario = keycloak.tokenParsed?.name || "N/A";

  return (
    <>
      <Typography.Title level={5}>Serie y Número de Recibo</Typography.Title>
      <Space>
        <Select value={serie} onChange={setSerie} style={{ width: 100 }}>
          <Select.Option value="A">Serie A</Select.Option>
          <Select.Option value="B">Serie B</Select.Option>
          <Select.Option value="C">Serie C</Select.Option>
        </Select>
        <Input
          placeholder="Número de Recibo"
          value={numeroRecibo}
          onChange={(e) => setNumeroRecibo(e.target.value)}
          style={{ width: 200 }}
        />
      </Space>

      {/* Resumen de Factura */}
      <Divider />
      <Typography.Title level={5}>Resumen de Factura</Typography.Title>
      <Card size="small" style={{ background: "#fafafa" }}>
        <Space direction="vertical" style={{ width: "100%" }}>
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text>{numeroRecibo}</Typography.Text>
            <Typography.Text strong>{serie}</Typography.Text>
          </div>
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text>Nombre:</Typography.Text>
            <Typography.Text strong>
              {selectedPaciente?.nombre || "Sin seleccionar"}
            </Typography.Text>
          </div>
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text>DNI:</Typography.Text>
            <Typography.Text strong>
              {selectedPaciente?.identificador || "N/A"}
            </Typography.Text>
          </div>
          <Divider style={{ margin: "8px 0" }} />
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text>Servicio:</Typography.Text>
          </div>
          {selectedServicio && (
            <div
              style={{
                display: "flex",
                justifyContent: "space-between",
                paddingLeft: 16,
              }}
            >
              <Typography.Text type="secondary">
                {selectedServicio.nombre}
              </Typography.Text>
              <Typography.Text>
                L.{selectedServicio.precio.toFixed(2)}
              </Typography.Text>
            </div>
          )}
          <Divider style={{ margin: "8px 0" }} />
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <Typography.Text strong>Total:</Typography.Text>
            <Typography.Text strong style={{ fontSize: 16 }}>
              L.
              {selectedServicio ? selectedServicio.precio.toFixed(2) : "0.00"}
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
    </>
  );
};
