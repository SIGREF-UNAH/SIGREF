import { ProList } from "@ant-design/pro-components";
import { Space, Tag, Typography, Spin, Input } from "antd";
import { SearchOutlined, MedicineBoxOutlined } from "@ant-design/icons";
import { useState } from "react";
import type { HealthcareDto } from "../../../api/models";

interface ServiceIncomeProps {
  serviciosData: HealthcareDto[];
  servicioFilters: {
    searchServicios: string;
    pageServicio: number;
    pageSizeServicio: number;
  };
  setServicioFilter: (key: "searchServicios" | "pageServicio" | "pageSizeServicio", value: any) => void;
  setServicioFilters: (filters: any) => void;
  handleSelectServicio: (servicio: HealthcareDto) => void;
  selectedServicio: HealthcareDto | null;
  isLoading: boolean;
}

export const ServiceIncome = ({
  serviciosData,
  servicioFilters,
  setServicioFilter,
  setServicioFilters,
  handleSelectServicio,
  selectedServicio,
  isLoading,
}: ServiceIncomeProps) => {
  const [searchText, setSearchText] = useState(
    servicioFilters.searchServicios || "",
  );

  // Filtrar servicios segun el texto de busqueda
  const serviciosFiltrados = (serviciosData ?? []).filter((servicio) => {
    const term = searchText.toLowerCase().trim();
    if (!term) return true;

    const name = servicio.name?.toLowerCase() || "";
    const abbr = servicio.abbreviation?.toLowerCase() || "";

    return name.includes(term) || abbr.includes(term);
  });

  return (
    <>
      <div style={{ marginBottom: 16 }}>
        <Input
          placeholder="Buscar por nombre o código del servicio..."
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
        <ProList<HealthcareDto>
          rowKey="id"
          dataSource={serviciosFiltrados}
          pagination={{
            current: servicioFilters.pageServicio || 1,
            pageSize: servicioFilters.pageSizeServicio || 10,
            total: serviciosFiltrados.length,
            onChange: (page, pageSize) =>
              setServicioFilters({
                ...servicioFilters,
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
                    <MedicineBoxOutlined /> {record.abbreviation || "S/A"}
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
                  {record.cost !== null &&
                  record.cost !== undefined &&
                  record.cost > 0 ? (
                    <Typography.Text
                      strong
                      type="success"
                      style={{ fontSize: 18 }}
                    >
                      L. {record.cost}
                    </Typography.Text>
                  ) : (
                    <Typography.Text type="secondary" style={{ fontSize: 14 }}>
                      Sin costo
                    </Typography.Text>
                  )}
                </div>
              ),
            },
          }}
          onItem={(record) => ({
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
