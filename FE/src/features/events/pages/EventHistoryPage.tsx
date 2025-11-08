import {
  PageContainer,
  ProCard,
  ProTable,
  type ProColumns,
} from "@ant-design/pro-components";
import {
  Button,
  DatePicker,
  Form,
  Tag,
  Space,
  Statistic,
  Row,
  Col,
  Input,
  Select,
  Typography,
} from "antd";
import { UserOutlined, SearchOutlined, EyeOutlined } from "@ant-design/icons";
import dayjs, { Dayjs } from "dayjs";
import { useState, useEffect, useMemo } from "react";
import { useUrlFilters } from "../../../shared/hooks";
import { EventHistoryModal } from "../components/modals";

const { RangePicker } = DatePicker;
const { Option } = Select;

type AuditLog = {
  _id: string;
  UserId: string;
  Username: string;
  Action: string;
  ActionType: string;
  Endpoint: string;
  HttpMethod: string;
  StatusCode: number;
  ErrorMessage: string | null;
  RequestBody: string | null;
  ResponseBody: string | null;
  IpAddress: string;
  UserAgent: string;
  Timestamp: string;
  AdditionalData: string | null;
};

const STATIC_DATA: AuditLog[] = [
  {
    _id: "1",
    UserId: "d6289016-8787-479a-9a40-6d2c1fe377e7",
    Username: "juanti",
    Action: "Inicio de sesión exitoso - Roles: ti, cashier, auditor, admin",
    ActionType: "LOGIN",
    Endpoint: "/api/AuditLog/stats/summary",
    HttpMethod: "POST",
    StatusCode: 200,
    ErrorMessage: null,
    RequestBody: null,
    ResponseBody: null,
    IpAddress: "::1",
    UserAgent: "bruno-runtime/2.2.0",
    Timestamp: "2025-10-22T03:42:31.890Z",
    AdditionalData: "Roles asignados: ti, cashier, auditor, admin",
  },
  {
    _id: "2",
    UserId: "d6289016-8787-479a-9a40-6d2c1fe377e7",
    Username: "juanti",
    Action: "Consulta de estadísticas",
    ActionType: "READ",
    Endpoint: "/api/AuditLog/stats/summary",
    HttpMethod: "GET",
    StatusCode: 200,
    ErrorMessage: null,
    RequestBody: null,
    ResponseBody: null,
    IpAddress: "::1",
    UserAgent: "Mozilla/5.0",
    Timestamp: "2025-10-22T04:15:20.123Z",
    AdditionalData: null,
  },
  {
    _id: "3",
    UserId: "a1234567-1234-1234-1234-123456789abc",
    Username: "maria.lopez",
    Action: "Actualización de configuración",
    ActionType: "UPDATE",
    Endpoint: "/api/Configuration/update",
    HttpMethod: "PUT",
    StatusCode: 200,
    ErrorMessage: null,
    RequestBody: '{"setting": "timezone", "value": "UTC-6"}',
    ResponseBody: null,
    IpAddress: "192.168.1.100",
    UserAgent: "Mozilla/5.0",
    Timestamp: "2025-10-22T05:30:45.678Z",
    AdditionalData: "Configuración actualizada exitosamente",
  },
  {
    _id: "4",
    UserId: "b2345678-2345-2345-2345-234567890bcd",
    Username: "carlos.admin",
    Action: "Intento de acceso denegado",
    ActionType: "LOGIN_FAILED",
    Endpoint: "/api/Auth/login",
    HttpMethod: "POST",
    StatusCode: 401,
    ErrorMessage: "Credenciales inválidas",
    RequestBody: null,
    ResponseBody: null,
    IpAddress: "192.168.1.105",
    UserAgent: "PostmanRuntime/7.32.0",
    Timestamp: "2025-10-22T06:45:12.456Z",
    AdditionalData: "Intento #2 de inicio de sesión fallido",
  },
  {
    _id: "5",
    UserId: "d6289016-8787-479a-9a40-6d2c1fe377e7",
    Username: "juanti",
    Action: "Exportación de reportes",
    ActionType: "EXPORT",
    Endpoint: "/api/Reports/export",
    HttpMethod: "POST",
    StatusCode: 200,
    ErrorMessage: null,
    RequestBody: '{"format": "xlsx", "dateRange": "2025-10-01:2025-10-22"}',
    ResponseBody: null,
    IpAddress: "::1",
    UserAgent: "bruno-runtime/2.2.0",
    Timestamp: "2025-10-22T07:20:33.789Z",
    AdditionalData: "Reporte exportado: ventas_octubre.xlsx",
  },
  {
    _id: "6",
    UserId: "e3456789-3456-3456-3456-345678901cde",
    Username: "laura.smith",
    Action: "Eliminación de usuario",
    ActionType: "DELETE",
    Endpoint: "/api/Users/delete",
    HttpMethod: "DELETE",
    StatusCode: 200,
    ErrorMessage: null,
    RequestBody: '{"userId": "f4567890-4567-4567-4567-456789012def"}',
    ResponseBody: null,
    IpAddress: "192.192.21.2",
    UserAgent: "Mozilla/5.0",
    Timestamp: "2025-10-22T08:55:47.321Z",
    AdditionalData: "Usuario eliminado exitosamente",
  },
  {
    _id: "7",
    UserId: "f4567890-4567-4567-4567-456789012def",
    Username: "miguel.torres",
    Action: "Creación de nuevo proyecto",
    ActionType: "CREATE",
    Endpoint: "/api/Projects/create",
    HttpMethod: "POST",
    StatusCode: 200,
    ErrorMessage: null,
    RequestBody: '{"projectName": "Proyecto X", "deadline": "2025-12-31"}',
    ResponseBody: null,
    IpAddress: "2312",
    UserAgent: "Mozilla/5.0",
    Timestamp: "2025-10-22T09:10:05.654Z",
    AdditionalData: "Proyecto creado con ID: 7890",
  },
  {
    _id: "8",
    UserId: "g5678901-5678-5678-5678-567890123efg",
    Username: "ana.garcia",
    Action: "Error al procesar solicitud",
    ActionType: "ERROR",
    Endpoint: "/api/Orders/process",
    HttpMethod: "POST",
    StatusCode: 500,
    ErrorMessage: "Error interno del servidor",
    RequestBody: '{"orderId": "12345"}',
    ResponseBody: null,
    IpAddress: "129.12.02.13",
    UserAgent: "Mozilla/5.0",
    Timestamp: "2025-10-22T10:25:18.987Z",
    AdditionalData: "Stack trace disponible en los logs del servidor",
  },
];

export const EventHistoryPage = () => {
  const [form] = Form.useForm();
  const [loading] = useState(false);
  const [data, setData] = useState<AuditLog[]>(STATIC_DATA);
  const [selectedRecord, setSelectedRecord] = useState<AuditLog | null>(null);
  const [modalOpen, setModalOpen] = useState(false);

  // Filtros en la URL
  const { filters, setFilters, resetFilters } = useUrlFilters({
    defaultValues: {
      search: "",
      username: undefined as string | undefined,
      actionType: undefined as string | undefined,
      startDate: undefined as string | undefined,
      endDate: undefined as string | undefined,
      page: 1,
      pageSize: 10,
    },
  });

  const [formValues, setFormValues] = useState({
    search: filters.search || "",
    username: filters.username,
    actionType: filters.actionType,
    dateRange:
      filters.startDate && filters.endDate
        ? ([dayjs(filters.startDate), dayjs(filters.endDate)] as [Dayjs, Dayjs])
        : null,
  });

  const stableFilters = useMemo(
    () => ({
      search: filters.search,
      username: filters.username,
      actionType: filters.actionType,
      startDate: filters.startDate,
      endDate: filters.endDate,
      page: filters.page,
      pageSize: filters.pageSize,
    }),
    [
      filters.search,
      filters.username,
      filters.actionType,
      filters.startDate,
      filters.endDate,
      filters.page,
      filters.pageSize,
    ]
  );

  useEffect(() => {
    let filtered = [...STATIC_DATA];

    if (stableFilters.search) {
      const searchLower = stableFilters.search.toLowerCase();
      filtered = filtered.filter(
        (item) =>
          item.Username.toLowerCase().includes(searchLower) ||
          item.Action.toLowerCase().includes(searchLower) ||
          item.Endpoint.toLowerCase().includes(searchLower) ||
          item.IpAddress.toLowerCase().includes(searchLower)
      );
    }

    if (stableFilters.username) {
      filtered = filtered.filter(
        (item) => item.Username === stableFilters.username
      );
    }

    if (stableFilters.actionType) {
      filtered = filtered.filter(
        (item) => item.ActionType === stableFilters.actionType
      );
    }

    if (stableFilters.startDate && stableFilters.endDate) {
      filtered = filtered.filter((item) => {
        const itemDate = dayjs(item.Timestamp);
        return (
          itemDate.isAfter(dayjs(stableFilters.startDate)) &&
          itemDate.isBefore(dayjs(stableFilters.endDate))
        );
      });
    }

    setData(filtered);
  }, [stableFilters]);

  const handleSearch = () => {
    const newFilters: any = {
      search: formValues.search || "",
      username: formValues.username,
      actionType: formValues.actionType,
      page: 1, // Resetear a página 1 al buscar
    };

    if (formValues.dateRange) {
      newFilters.startDate = formValues.dateRange[0].toISOString();
      newFilters.endDate = formValues.dateRange[1].toISOString();
    } else {
      newFilters.startDate = undefined;
      newFilters.endDate = undefined;
    }

    setFilters(newFilters);
  };

  const handleClearFilters = () => {
    // Limpiar formulario
    setFormValues({
      search: "",
      username: undefined,
      actionType: undefined,
      dateRange: null,
    });
    form.resetFields();
    // Limpiar filtros de URL
    resetFilters();
  };

  const handleViewDetails = (record: AuditLog) => {
    setSelectedRecord(record);
    setModalOpen(true);
  };

  const getActionTypeColor = (actionType: string) => {
    const colors: Record<string, string> = {
      LOGIN: "blue",
      READ: "green",
      UPDATE: "orange",
      DELETE: "red",
      EXPORT: "purple",
      CREATE: "cyan",
      LOGIN_FAILED: "red",
      ERROR: "red",
    };
    return colors[actionType] || "default";
  };

  const getStatusColor = (statusCode: number) => {
    if (statusCode >= 200 && statusCode < 300) return "success";
    if (statusCode >= 400 && statusCode < 500) return "warning";
    if (statusCode >= 500) return "error";
    return "default";
  };
  const columns: ProColumns<AuditLog>[] = [
    {
      title: "Usuario",
      dataIndex: "Username",
      key: "Username",
      render: (_, record) => (
        <Space>
          <UserOutlined />
          <div>
            <div style={{ fontWeight: 500 }}>{record.Username}</div>
            <div style={{ fontSize: 12, color: "#999" }}>
              {record.UserId.substring(0, 8)}...
            </div>
          </div>
        </Space>
      ),
    },
    {
      title: "Acción",
      dataIndex: "Action",
      key: "Action",
      render: (_, record) => (
        <div>
          <div>{record.Action}</div>
          {record.AdditionalData && (
            <div style={{ fontSize: 12, color: "#666", marginTop: 4 }}>
              {record.AdditionalData}
            </div>
          )}
        </div>
      ),
    },
    {
      title: "Tipo",
      dataIndex: "ActionType",
      key: "ActionType",
      render: (_, record) => (
        <Tag color={getActionTypeColor(record.ActionType)}>
          {record.ActionType}
        </Tag>
      ),
    },
    {
      title: "Estado",
      dataIndex: "StatusCode",
      key: "StatusCode",
      render: (_, record) => (
        <Tag color={getStatusColor(record.StatusCode)}>{record.StatusCode}</Tag>
      ),
    },
    {
      title: "Fecha",
      dataIndex: "Timestamp",
      key: "Timestamp",
      render: (_, record) =>
        dayjs(record.Timestamp).format("YYYY-MM-DD HH:mm:ss"),
    },
    {
      title: "Acciones",
      key: "actions",
      width: 100,
      render: (_, record) => (
        <Button
          type="link"
          icon={<EyeOutlined />}
          onClick={() => handleViewDetails(record)}
        >
          Ver Detalles
        </Button>
      ),
    },
  ];

  const totalEvents = data.length;
  const successEvents = data.filter(
    (d) => d.StatusCode >= 200 && d.StatusCode < 300
  ).length;
  const errorEvents = data.filter((d) => d.StatusCode >= 400).length;
  const uniqueUsers = new Set(data.map((d) => d.UserId)).size;

  return (
    <PageContainer
      title={
        <Typography.Title level={2} style={{ margin: 0 }}>
          Historial de Eventos
        </Typography.Title>
      }
    >
      <ProCard style={{ marginBottom: 16 }}>
        <Row gutter={16}>
          <Col span={6}>
            <Statistic title="Total de Eventos" value={totalEvents} />
          </Col>
          <Col span={6}>
            <Statistic
              title="Exitosos"
              value={successEvents}
              valueStyle={{ color: "#3f8600" }}
            />
          </Col>
          <Col span={6}>
            <Statistic
              title="Con Errores"
              value={errorEvents}
              valueStyle={{ color: "#cf1322" }}
            />
          </Col>
          <Col span={6}>
            <Statistic title="Usuarios" value={uniqueUsers} />
          </Col>
        </Row>
      </ProCard>

      <ProCard>
        <Form layout="vertical" form={form} style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={6}>
              <Form.Item label="Búsqueda General">
                <Input
                  placeholder="Buscar..."
                  prefix={<SearchOutlined />}
                  value={formValues.search}
                  onChange={(e) =>
                    setFormValues({ ...formValues, search: e.target.value })
                  }
                  onPressEnter={handleSearch}
                  allowClear
                />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item label="Tipo de Acción">
                <Select
                  placeholder="Todos los tipos"
                  value={formValues.actionType}
                  onChange={(value) =>
                    setFormValues({ ...formValues, actionType: value })
                  }
                  allowClear
                >
                  {[
                    "LOGIN",
                    "READ",
                    "UPDATE",
                    "DELETE",
                    "EXPORT",
                    "CREATE",
                    "ERROR",
                    "LOGIN_FAILED",
                  ].map((type) => (
                    <Option key={type} value={type}>
                      {type}
                    </Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item label="Rango de Fechas">
                <RangePicker
                  format="YYYY-MM-DD"
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

        <ProTable<AuditLog>
          rowKey="_id"
          columns={columns}
          dataSource={data}
          search={false}
          loading={loading}
          pagination={{
            current: stableFilters.page,
            pageSize: stableFilters.pageSize,
            showTotal: (total) => `Total ${total} registros`,
            showSizeChanger: true,
            pageSizeOptions: ["10", "20", "50", "100"],
            onChange: (page, pageSize) => setFilters({ page, pageSize }),
          }}
        />
      </ProCard>
      <EventHistoryModal
        selectedRecord={selectedRecord}
        setModalOpen={setModalOpen}
        modalOpen={modalOpen}
        getActionTypeColor={getActionTypeColor}
        getStatusColor={getStatusColor}
        dayjs={dayjs}
      />
    </PageContainer>
  );
};
