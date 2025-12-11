import {
  PageContainer,
  ProTable,
  type ProColumns,
} from "@ant-design/pro-components";
import {
  Button,
  DatePicker,
  Form,
  Tag,
  Space,
  Row,
  Col,
  Typography,
  Select,
  Input,
} from "antd";
import { SearchOutlined, EyeOutlined, UserOutlined } from "@ant-design/icons";
import dayjs, { Dayjs } from "dayjs";
import type { AuditLogDto } from "../../../api/models";
import { EventHistoryModal } from "../components";
import useEventHistory from "../hooks/useEventHistory";

const { RangePicker } = DatePicker;
const { Option } = Select;

export const EventHistoryPage = () => {
  const {
    data,
    form,
    formValues,
    filters,
    selectedRecord,
    modalOpen,
    pagination,
    isLoading,
    setFilters,
    handleSearch,
    handleClearFilters,
    handleViewDetails,
    getActionColor,
    getStatusColor,
    setFormValues,
    setModalOpen,
  } = useEventHistory();

  // Columnas de la tabla
  const columns: ProColumns<AuditLogDto>[] = [
    {
      title: "Usuario",
      dataIndex: "userName",
      key: "userName",
      width: 150,
      render: (_, record: any) => (
        <Space direction="vertical" size={0}>
          <Space>
            <UserOutlined />
            <span style={{ fontWeight: 500 }}>{record.userName || "N/A"}</span>
          </Space>
        </Space>
      ),
    },
    {
      title: "Acción",
      dataIndex: "action",
      key: "action",
      render: (_, record: any) => (
        <div>
          <div style={{ marginTop: 4, fontSize: 12 }}>
            <Tag color={getActionColor(record.action)}>
              {record.action || "N/A"}
            </Tag>
            <code style={{ fontSize: 11 }}>{record.endpoint}</code>
          </div>
        </div>
      ),
    },
    {
      title: "Código",
      dataIndex: "statusCode",
      key: "statusCode",
      width: 100,
      align: "center",
      render: (_, record: any) => (
        <Tag color={getStatusColor(record.statusCode)}>
          {record.statusCode || "N/A"}
        </Tag>
      ),
    },
    {
      title: "Resultado",
      dataIndex: "success",
      key: "success",
      width: 100,
      align: "center",
      render: (_, record: any) => (
        <Tag color={record.success ? "success" : "error"}>
          {record.success ? "Exitoso" : "Fallido"}
        </Tag>
      ),
    },
    {
      title: "Fecha",
      dataIndex: "timestamp",
      key: "timestamp",
      width: 180,
      render: (_, record: any) =>
        record.timestamp
          ? dayjs(record.timestamp).format("YYYY-MM-DD HH:mm:ss")
          : "N/A",
    },
    {
      title: "Acciones",
      key: "actions",
      width: 100,
      align: "center",
      render: (_, record) => (
        <Button
          type="link"
          icon={<EyeOutlined />}
          onClick={() => handleViewDetails(record)}
        >
          Ver
        </Button>
      ),
    },
  ];

  return (
    <PageContainer
      title={
        <Typography.Title level={2} style={{ margin: 0 }}>
          Historial de Eventos
        </Typography.Title>
      }
    >
      <div className="primary-card">
        {/* Filtros */}
        <Form layout="vertical" form={form}>
          <Row gutter={16}>
            {/* Nombre de Usuario */}
            <Col span={6}>
              <Form.Item label="Nombre de Usuario">
                <Input
                  placeholder="Buscar por usuario..."
                  value={formValues.userName}
                  onChange={(e) =>
                    setFormValues({ ...formValues, userName: e.target.value })
                  }
                  onPressEnter={handleSearch}
                  allowClear
                />
              </Form.Item>
            </Col>
            {/* Acción */}
            <Col span={6}>
              <Form.Item label="Acción">
                <Select
                  placeholder="Seleccionar tipo de acción"
                  value={formValues.action}
                  onChange={(value) =>
                    setFormValues({ ...formValues, action: value })
                  }
                  allowClear
                >
                  <Option value="read">Read</Option>
                  <Option value="create">Create</Option>
                  <Option value="update">Update</Option>
                  <Option value="delete">Delete</Option>
                </Select>
              </Form.Item>
            </Col>
            {/* Rango de Fechas */}
            <Col span={8}>
              <Form.Item label="Rango de Fechas">
                <RangePicker
                  showTime
                  format="YYYY-MM-DD HH:mm:ss"
                  value={formValues.dateRange}
                  onChange={(dates) =>
                    setFormValues({
                      ...formValues,
                      dateRange: dates as [Dayjs, Dayjs] | null,
                    })
                  }
                  style={{ width: "100%" }}
                />
              </Form.Item>
            </Col>
            {/* Botones */}
            <Col span={4}>
              <Form.Item label=" " colon={false}>
                <Space>
                  <Button
                    type="primary"
                    icon={<SearchOutlined />}
                    onClick={handleSearch}
                  >
                    Buscar
                  </Button>
                  <Button onClick={handleClearFilters}>Limpiar</Button>
                </Space>
              </Form.Item>
            </Col>
          </Row>
        </Form>

        {/* Contenido */}
        <ProTable<AuditLogDto>
          className="secondary-card"
          bordered
          rowKey="id"
          columns={columns}
          dataSource={data}
          search={false}
          loading={isLoading}
          pagination={{
            current: pagination?.currentPage || filters.page,
            pageSize: pagination?.pageSize || filters.pageSize,
            total: pagination?.totalItems || 0,
            showTotal: (total) => `Total ${total} registros`,
            showSizeChanger: true,
            pageSizeOptions: ["10", "20", "50", "100"],
            onChange: (page, pageSize) => {
              setFilters({ 
                ...filters,
                page, 
                pageSize 
              });
            },
            showQuickJumper: true,
          }}
        />
      </div>

      <EventHistoryModal
        selectedRecord={selectedRecord}
        setModalOpen={setModalOpen}
        modalOpen={modalOpen}
        getStatusColor={getStatusColor}
        getActionColor={getActionColor}
        dayjs={dayjs}
      />
    </PageContainer>
  );
};