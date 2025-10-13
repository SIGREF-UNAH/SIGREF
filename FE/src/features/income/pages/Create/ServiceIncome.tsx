import { ProList } from "@ant-design/pro-components";
import { Space, Tabs, Tag, Typography } from "antd";

export const ServiceIncome = ({
  serviciosData,
  servicioFilters,
  setServicioFilter,
  setServicioFilters,
  handleSelectServicio,
  selectedServicio,
}: any) => {
  const serviciosFiltrados = (serviciosData ?? []).filter(
    (servicio: { nombre: string; abreviatura: string; tipo: string }) => {
      const matchSearch =
        servicio.nombre
          .toLowerCase()
          .includes(servicioFilters.searchServicios.toLowerCase()) ||
        servicio.abreviatura
          .toLowerCase()
          .includes(servicioFilters.searchServicios.toLowerCase());
      const matchTipo =
        servicioFilters.tipoServicio === "todos" ||
        (servicioFilters.tipoServicio === "servicios" &&
          servicio.tipo === "servicio") ||
        (servicioFilters.tipoServicio === "paquetes" &&
          servicio.tipo === "paquete");
      return matchSearch && matchTipo;
    }
  );

  return (
    <>
      {/* Filtros de Servicios */}
      <Tabs
        activeKey={servicioFilters.tipoServicio}
        onChange={(key) => setServicioFilter("tipoServicio", key)}
        items={[
          { label: "Todos", key: "todos" },
          { label: "Servicios", key: "servicios" },
          { label: "Paquetes", key: "paquetes" },
        ]}
        style={{ marginBottom: 16 }}
      />

      {/* List para Servicios */}
      <ProList<any>
        rowKey="id"
        headerTitle={
          <Typography.Title level={5}>Lista de servicios</Typography.Title>
        }
        dataSource={serviciosFiltrados}
        pagination={{
          current: servicioFilters.page,
          pageSize: servicioFilters.pageSize,
          onChange: (page, pageSize) =>
            setServicioFilters({
              page,
              pageSize,
            }),
          showSizeChanger: true,
          pageSizeOptions: ["5", "10", "20"],
        }}
        metas={{
          title: {
            dataIndex: "nombre",
            search: true,
            render: (_, record) => (
              <Space>
                <Tag color={record.tipo === "paquete" ? "purple" : "blue"}>
                  {record.abreviatura}
                </Tag>
                <Typography.Text strong>{record.nombre}</Typography.Text>
              </Space>
            ),
          },
          description: {
            dataIndex: "area",
            render: (text) => (
              <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                {text}
              </Typography.Text>
            ),
          },
          subTitle: {
            render: (_, record) => (
              <Typography.Text strong style={{ fontSize: 16 }}>
                L.{record.precio.toFixed(2)}
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
                : undefined,
            borderRadius: 4,
          },
        })}
      />
    </>
  );
};
