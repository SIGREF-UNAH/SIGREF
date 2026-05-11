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
  Tooltip,
} from "antd";
import {
  SearchOutlined,
  EyeOutlined,
  UserOutlined,
  LinkOutlined,
  ApiOutlined,
  WarningOutlined,
} from "@ant-design/icons";
import dayjs, { Dayjs } from "dayjs";
import type { AuditLog } from "../../../api/models/auditLog";
import { EventHistoryModal } from "../components";
import {useEventHistory} from "../hooks/useEventHistory";

const { RangePicker } = DatePicker;
const { Option } = Select;
const { Text } = Typography;

export const EventHistoryPage = () => {
  const {
    data,
    form,
    formValues,
    filters,
    selectedRecord,
    modalOpen,
    isLoading,
    setFilters,
    handleSearch,
    handleClearFilters,
    handleViewDetails,
    getActionColor,
    getStatusColor,
    getHttpMethodColor,
    setFormValues,
    setModalOpen,
  } = useEventHistory();

  // Columnas de la tabla
  const columns: ProColumns<AuditLog>[] = [
    {
      title: "Trace ID",
      dataIndex: "traceId",
      key: "traceId",
      width: 120,
      ellipsis: true,
      render: (_, record) => (
        <Tooltip title={record.traceId}>
          <Space>
            <LinkOutlined style={{ fontSize: 12 }} />
            <Text
              copyable={{ text: record.traceId ?? "" }}
              style={{ fontSize: 12 }}
              ellipsis
            >
              {record.traceId
                ? record.traceId.length > 12
                  ? `${record.traceId.substring(0, 12)}...`
                  : record.traceId
                : "N/A"}
            </Text>
          </Space>
        </Tooltip>
      ),
    },
    {
      title: "Usuario",
      dataIndex: "userName",
      key: "userName",
      width: 170,
      render: (_, record) => (
        <Space direction="vertical" size={0}>
          <Space>
            <UserOutlined />
            <span style={{ fontWeight: 500 }}>
              {record.userName || "N/A"}
            </span>
          </Space>
          {record.userId && (
            <Text
              type="secondary"
              style={{ fontSize: 11, marginLeft: 24 }}
              ellipsis
            >
              {record.userId.substring(0, 20)}...
            </Text>
          )}
        </Space>
      ),
    },
    {
      title: "HTTP",
      dataIndex: "httpMethod",
      key: "httpMethod",
      width: 80,
      align: "center",
      render: (_, record) =>
        record.httpMethod ? (
          <Tag color={getHttpMethodColor?.(record.httpMethod) || "default"}>
            {record.httpMethod.toUpperCase()}
          </Tag>
        ) : (
          <Tag>N/A</Tag>
        ),
    },
    {
      title: "Acción / Endpoint",
      dataIndex: "action",
      key: "action",
      width: 220,
      render: (_, record) => (
        <div>
          <div>
            <Tag color={getActionColor(record.action || "")}>
              {record.action || "N/A"}
            </Tag>
            {record.resourceType && (
              <Tag color="geekblue">{record.resourceType}</Tag>
            )}
          </div>
          <Tooltip title={record.endpoint}>
            <code
              style={{
                fontSize: 10,
                color: "#8c8c8c",
                display: "block",
                marginTop: 4,
                overflow: "hidden",
                textOverflow: "ellipsis",
                whiteSpace: "nowrap",
                maxWidth: 220,
              }}
            >
              {record.httpMethod?.toUpperCase() || ""} {record.endpoint || ""}
            </code>
          </Tooltip>
        </div>
      ),
    },
    {
      title: "Recurso",
      dataIndex: "resourceType",
      key: "resourceType",
      width: 140,
      render: (_, record) => (
        <Space direction="vertical" size={0}>
          {record.resourceType && (
            <Tag color="geekblue" icon={<ApiOutlined />}>
              {record.resourceType}
            </Tag>
          )}
          {record.resourceId && (
            <Text
              type="secondary"
              style={{ fontSize: 11 }}
              ellipsis
              copyable={{ text: record.resourceId }}
            >
              {record.resourceId.length > 20
                ? `${record.resourceId.substring(0, 20)}...`
                : record.resourceId}
            </Text>
          )}
        </Space>
      ),
    },
    {
      title: "IP",
      dataIndex: "ipAddress",
      key: "ipAddress",
      width: 130,
      ellipsis: true,
      render: (_, record) => (
        <Text
          copyable={!!record.ipAddress}
          style={{ fontSize: 12, fontFamily: "monospace" }}
        >
          {record.ipAddress || "N/A"}
        </Text>
      ),
    },
    {
      title: "Código",
      dataIndex: "statusCode",
      key: "statusCode",
      width: 100,
      align: "center",
      render: (_, record) => (
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
      render: (_, record) =>
        record.success ? (
          <Tag color="success">Exitoso</Tag>
        ) : (
          <Space size={4}>
            <Tag color="error">Fallido</Tag>
            {record.errorMessage && (
              <Tooltip title={record.errorMessage}>
                <WarningOutlined style={{ color: "#ff4d4f", fontSize: 14 }} />
              </Tooltip>
            )}
          </Space>
        ),
    },
    {
      title: "Fecha",
      dataIndex: "timestamp",
      key: "timestamp",
      width: 180,
      sorter: true,
      render: (_, record) =>
        record.timestamp
          ? dayjs(record.timestamp).format("YYYY-MM-DD HH:mm:ss")
          : "N/A",
    },
    {
      title: "Acciones",
      key: "actions",
      width: 80,
      align: "center",
      fixed: "right",
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
            <Col xs={24} sm={12} md={6} lg={5}>
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
            {/* User ID */}
            <Col xs={24} sm={12} md={6} lg={5}>
              <Form.Item label="User ID">
                <Input
                  placeholder="Buscar por ID..."
                  value={formValues.userId}
                  onChange={(e) =>
                    setFormValues({ ...formValues, userId: e.target.value })
                  }
                  onPressEnter={handleSearch}
                  allowClear
                />
              </Form.Item>
            </Col>
            {/* Acción */}
            <Col xs={24} sm={12} md={6} lg={4}>
              <Form.Item label="Acción">
                <Select
                  placeholder="Seleccionar tipo"
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
            {/* HTTP Method */}
            <Col xs={24} sm={12} md={6} lg={4}>
              <Form.Item label="HTTP Method">
                <Select
                  placeholder="Método HTTP"
                  value={formValues.httpMethod}
                  onChange={(value) =>
                    setFormValues({ ...formValues, httpMethod: value })
                  }
                  allowClear
                >
                  <Option value="GET">GET</Option>
                  <Option value="POST">POST</Option>
                  <Option value="PUT">PUT</Option>
                  <Option value="PATCH">PATCH</Option>
                  <Option value="DELETE">DELETE</Option>
                </Select>
              </Form.Item>
            </Col>
            {/* Rango de Fechas */}
            <Col xs={24} sm={12} md={8} lg={6}>
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
          </Row>
          {/* Segunda fila de filtros */}
          <Row gutter={16}>
            {/* Resource Type */}
            <Col xs={24} sm={12} md={6} lg={5}>
              <Form.Item label="Tipo de Recurso">
                <Input
                  placeholder="Ej: Patient, Observation..."
                  value={formValues.resourceType}
                  onChange={(e) =>
                    setFormValues({
                      ...formValues,
                      resourceType: e.target.value,
                    })
                  }
                  onPressEnter={handleSearch}
                  allowClear
                />
              </Form.Item>
            </Col>
            {/* Trace ID */}
            <Col xs={24} sm={12} md={6} lg={5}>
              <Form.Item label="Trace ID">
                <Input
                  placeholder="Trace ID..."
                  value={formValues.traceId}
                  onChange={(e) =>
                    setFormValues({ ...formValues, traceId: e.target.value })
                  }
                  onPressEnter={handleSearch}
                  allowClear
                />
              </Form.Item>
            </Col>
            {/* IP Address */}
            <Col xs={24} sm={12} md={6} lg={4}>
              <Form.Item label="Dirección IP">
                <Input
                  placeholder="IP..."
                  value={formValues.ipAddress}
                  onChange={(e) =>
                    setFormValues({ ...formValues, ipAddress: e.target.value })
                  }
                  onPressEnter={handleSearch}
                  allowClear
                />
              </Form.Item>
            </Col>
            {/* Resultado */}
            <Col xs={24} sm={12} md={6} lg={4}>
              <Form.Item label="Resultado">
                <Select
                  placeholder="Éxito/Fallo"
                  value={formValues.success}
                  onChange={(value) =>
                    setFormValues({ ...formValues, success: value })
                  }
                  allowClear
                >
                  <Option value={true}>Exitoso</Option>
                  <Option value={false}>Fallido</Option>
                </Select>
              </Form.Item>
            </Col>
            {/* Botones */}
            <Col
              xs={24}
              sm={24}
              md={8}
              lg={6}
              style={{
                display: "flex",
                alignItems: "flex-end",
                justifyContent: "flex-start",
              }}
            >
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
        <ProTable<AuditLog>
          className="secondary-card"
          bordered
          rowKey="id"
          columns={columns}
          dataSource={data.items || []}
          search={false}
          loading={isLoading}
          scroll={{ x: 1600 }}
          pagination={{
            current: data.pagination?.currentPage || filters.CurrentPage,
            pageSize: data.pagination?.pageSize || filters.PageSize,
            total: data.pagination?.totalItems || 0,
            showTotal: (total) => `Total ${total} registros`,
            showSizeChanger: true,
            pageSizeOptions: ["10", "20", "50", "100"],
            onChange: (page, pageSize) => {
              setFilters({
                ...filters,
                CurrentPage: page,
                PageSize: pageSize,
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