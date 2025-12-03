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
} from "antd";
import { SearchOutlined, EyeOutlined, UserOutlined } from "@ant-design/icons";
import dayjs, { Dayjs } from "dayjs";
import { useState, useMemo } from "react";
import { useUrlFilters } from "../../../shared/hooks";
import { EventHistoryModal } from "../components/modals";
import type { AuditLogDto } from "../../../api/models";
import { useGetApiAudit } from "../../../api/audit/audit";
import { Option } from "antd/es/mentions";

const { RangePicker } = DatePicker;

export const EventHistoryPage = () => {
  const [form] = Form.useForm();
  const [selectedRecord, setSelectedRecord] = useState<AuditLogDto | null>(null);
  const [modalOpen, setModalOpen] = useState(false);

  // Filtros en la URL
  const { filters, setFilters, resetFilters } = useUrlFilters({
    defaultValues: {
      action: undefined as string | undefined,
      from: undefined as string | undefined,
      to: undefined as string | undefined,
      page: 1,
      pageSize: 10,
    },
  });

  const [formValues, setFormValues] = useState({
    action: filters.action,
    dateRange:
      filters.from && filters.to
        ? ([dayjs(filters.from), dayjs(filters.to)] as [Dayjs, Dayjs])
        : null,
  });

  // Query params para backend
  const queryParams = useMemo(() => ({
    page: filters.page,
    pageSize: filters.pageSize,
    action: filters.action,
    from: filters.from,
    to: filters.to,
  }), [filters]);

  // Fetch data from backend
  const { data: response, isLoading } = useGetApiAudit(queryParams);

  const responseData = response as any;
  const data = responseData?.data || [];
  const pagination = responseData?.pagination;

  const handleSearch = () => {
    const newFilters: any = {
      action: formValues.action,
      page: 1, // Resetear a página 1 al buscar
    };

    if (formValues.dateRange) {
      newFilters.from = formValues.dateRange[0].toISOString();
      newFilters.to = formValues.dateRange[1].toISOString();
    } else {
      newFilters.from = undefined;
      newFilters.to = undefined;
    }

    setFilters(newFilters);
  };

  const handleClearFilters = () => {
    // Limpiar formulario
    setFormValues({
      action: undefined,
      dateRange: null,
    });
    form.resetFields();
    // Limpiar filtros de URL
    resetFilters();
  };

  const handleViewDetails = (record: AuditLogDto) => {
    setSelectedRecord(record);
    setModalOpen(true);
  };

  const getActionColor = (action: string | null | undefined) => {
    if (!action) return "default";
    const actionLower = action.toLowerCase();
    if (actionLower.includes("create")) return "cyan";
    if (actionLower.includes("read")) return "green";
    if (actionLower.includes("update")) return "orange";
    if (actionLower.includes("delete")) return "red";
    return "blue";
  };

  const getStatusColor = (statusCode: number | undefined) => {
    if (!statusCode) return "default";
    if (statusCode >= 200 && statusCode < 300) return "success";
    if (statusCode >= 400 && statusCode < 500) return "warning";
    if (statusCode >= 500) return "error";
    return "default";
  };

  const columns: ProColumns<AuditLogDto>[] = [
    {
      title: "Usuario",
      dataIndex: "userId",
      key: "userId",
      render: (_, record) => (
        <Space>
          <UserOutlined />
          <div>
            <div style={{ fontSize: 12, color: "#999" }}>
              {record.userId ? `${record.userId.substring(0, 8)}...` : "N/A"}
            </div>
          </div>
        </Space>
      ),
    },
    {
      title: "Acción",
      dataIndex: "action",
      key: "action",
      render: (_, record) => (
        <div>
          <div style={{ marginTop: 4 }}>
            <Tag color={getActionColor(record.action)}>{record.action || "N/A"}</Tag>
            <span style={{ fontWeight: 500 }}>{record.httpMethod}</span>{" "}
            <code style={{ fontSize: 12 }}>{record.endpoint}</code>
          </div>
        </div>
      ),
    },
    // {
    //   title: "Tipo de Recurso",
    //   dataIndex: "resourceType",
    //   key: "resourceType",
    //   render: (text) => text || "-",
    // },
    {
      title: "Código",
      dataIndex: "statusCode",
      key: "statusCode",
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
      render: (_, record) => (
        <Tag color={record.success ? "success" : "error"}>
          {record.success ? "Exitoso" : "Fallido"}
        </Tag>
      ),
    },
    {
      title: "Fecha",
      dataIndex: "timestamp",
      key: "timestamp",
      render: (_, record) =>
        record.timestamp
          ? dayjs(record.timestamp).format("YYYY-MM-DD HH:mm:ss")
          : "N/A",
    },
    {
      title: "Acciones",
      key: "actions",
      width: 120,
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
            {/* Acción */}
            <Col span={8}>
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
            <Col span={10}>
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
            <Col span={6}>
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
            current: filters.page,
            pageSize: filters.pageSize,
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