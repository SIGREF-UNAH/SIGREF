import { ProList } from "@ant-design/pro-components";
import { Space, Tag, Typography, Spin, Input } from "antd";
import { SearchOutlined } from "@ant-design/icons";
import { useState } from "react";

export const ServiceIncomeList = ({
  serviciosData,
  servicioFilters,
  setServicioFilter,
  setServicioFilters,
  handleSelectServicio,
  selectedServicio,
  isLoading,
}: any) => {
  const [searchText, setSearchText] = useState(servicioFilters.searchServicios || "");

  // Filtrado local en tiempo real mientras escribes
  const serviciosFiltrados = (serviciosData ?? []).filter((servicio: any) => {
    const term = searchText.toLowerCase().trim();
    if (!term) return true;

    const name = servicio.name?.toLowerCase() || "";
    const abbr = servicio.abbreviation?.toLowerCase() || "";

    return name.includes(term) || abbr.includes(term);
  });

  return (
    <>
      {/* Barra de busqueda prominente */}
      <div style={{ marginBottom: 16 }}>
        <Input
          placeholder="Buscar por nombre o abreviatura del servicio..."
          prefix={<SearchOutlined style={{ color: "#aaa" }} />}
          size="large"
          allowClear
          value={searchText}
          onChange={(e) => {
            const value = e.target.value;
            setSearchText(value);
            setServicioFilter("searchServicios", value);
          }}
          style={{ borderRadius: 8 }}
        />
      </div>

      {/* Lista de Servicios */}
      {isLoading ? (
        <div style={{ textAlign: "center", padding: "60px 0" }}>
          <Spin size="large" tip="Cargando servicios..." />
        </div>
      ) : serviciosFiltrados.length === 0 ? (
        <div style={{ textAlign: "center", padding: "60px 0", color: "#999" }}>
          <Typography.Text type="secondary">
            {searchText
              ? `No se encontraron servicios con "${searchText}"`
              : "No hay servicios disponibles"}
          </Typography.Text>
        </div>
      ) : (
        <ProList<any>
          rowKey="id"
          dataSource={serviciosFiltrados}
          pagination={{
            current: servicioFilters.pageServicio || 1,
            pageSize: servicioFilters.pageSizeServicio || 10,
            total: serviciosFiltrados.length,
            onChange: (page, pageSize) =>
              setServicioFilters({
                pageServicio: page,
                pageSizeServicio: pageSize || 10,
              }),
            showSizeChanger: true,
            pageSizeOptions: ["10", "20", "50"],
            showTotal: (total) => `Total: ${total} servicios`,
          }}
          metas={{
            title: {
              render: (_, record) => (
                <Space>
                  <Tag color="blue" style={{ fontWeight: "bold" }}>
                    {record.abbreviation || "S/A"}
                  </Tag>
                  <Typography.Text strong>
                    {record.name || "Servicio sin nombre"}
                  </Typography.Text>
                </Space>
              ),
            },
            description: {
              render: (_, record) => {
                const specialty =
                  record.specialty?.[0]?.coding?.[0]?.display ||
                  record.specialty?.[0]?.text ||
                  "Sin especialidad";
                return (
                  <Typography.Text type="secondary" style={{ fontSize: 13 }}>
                    {specialty}
                  </Typography.Text>
                );
              },
            },
            subTitle: {
              render: (_, record) => (
                <div style={{ textAlign: "right" }}>
                  <Typography.Text strong type="success" style={{ fontSize: 18 }}>
                    L. {(record.cost || 0).toFixed(2)}
                  </Typography.Text>
                </div>
              ),
            },
          }}
          onItem={(record: any) => ({
            onClick: () => handleSelectServicio(record),
            style: {
              cursor: "pointer",
              border:
                selectedServicio?.id === record.id
                  ? "2px solid #1890ff"
                  : "1px solid #f0f0f0",
              borderRadius: 8,
              marginBottom: 12,
              padding: "12px 16px",
              transition: "all 0.3s",
              backgroundColor:
                selectedServicio?.id === record.id ? "#e6f7ff" : "white",
              boxShadow:
                selectedServicio?.id === record.id
                  ? "0 4px 12px rgba(24, 144, 255, 0.15)"
                  : "none",
            },
          })}
        />
      )}
    </>
  );
};