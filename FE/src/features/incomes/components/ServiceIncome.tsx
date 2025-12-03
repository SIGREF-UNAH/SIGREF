import { ProList } from "@ant-design/pro-components";
import { Space, Tabs, Tag, Typography, Spin } from "antd";

export const ServiceIncome = ({
  serviciosData,
  servicioFilters,
  setServicioFilter,
  setServicioFilters,
  handleSelectServicio,
  selectedServicio,
  isLoading,
}: any) => {
  // Filtrar servicios basados en los filtros actuales
  const serviciosFiltrados = (serviciosData ?? []).filter((servicio: any) => {
    const searchTerm = servicioFilters.searchServicios?.toLowerCase() || "";
    
    // Búsqueda por nombre o abreviatura
    const matchSearch =
      servicio.name?.toLowerCase().includes(searchTerm) ||
      servicio.abbreviation?.toLowerCase().includes(searchTerm);

    // Filtro por tipo (si existe en specialty o alguna otra propiedad)
    // Por ahora solo filtramos por búsqueda ya que no hay un campo "tipo" explícito
    const matchTipo = servicioFilters.tipoServicio === "todos";

    return matchSearch && matchTipo;
  });

  return (
    <>
      {/* Filtros de Servicios */}
      <Tabs
        activeKey={servicioFilters.tipoServicio}
        onChange={(key) => setServicioFilter("tipoServicio", key)}
        items={[
          { label: "Todos", key: "todos" },
        ]}
        style={{ marginBottom: 16 }}
      />

      {/* Lista de Servicios */}
      {isLoading ? (
        <div style={{ textAlign: "center", padding: "40px 0" }}>
          <Spin size="large" />
        </div>
      ) : (
        <ProList<any>
          rowKey="id"
          dataSource={serviciosFiltrados}
          pagination={{
            current: servicioFilters.pageServicio,
            pageSize: servicioFilters.pageSizeServicio,
            onChange: (page, pageSize) =>
              setServicioFilters({
                pageServicio: page,
                pageSizeServicio: pageSize,
              }),
            showSizeChanger: true,
            pageSizeOptions: ["5", "10", "20"],
          }}
          search={{
            filterType: "light",
          }}
          metas={{
            title: {
              dataIndex: "name",
              search: true,
              render: (_, record) => (
                <Space>
                  <Tag color="blue">
                    {record.abbreviation || "N/A"}
                  </Tag>
                  <Typography.Text strong>{record.name || "Sin nombre"}</Typography.Text>
                </Space>
              ),
            },
            description: {
              render: (_, record) => {
                // Mostrar specialty si existe
                const specialty = record.specialty?.[0]?.coding?.[0]?.display || 
                                record.specialty?.[0]?.text || 
                                "Sin especialidad";
                return (
                  <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                    {specialty}
                  </Typography.Text>
                );
              },
            },
            subTitle: {
              render: (_, record) => (
                <Typography.Text strong style={{ fontSize: 16, color: '#1890ff' }}>
                  L.{(record.cost || 0).toFixed(2)}
                </Typography.Text>
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
              borderRadius: 4,
              marginBottom: 8,
              transition: "all 0.3s",
              backgroundColor:
                selectedServicio?.id === record.id
                  ? "#e6f7ff"
                  : "white",
            },
          })}
        />
      )}
    </>
  );
};