import {
  Button,
  Table,
  Space,
  Input,
  Tag,
  Modal,
  Switch,
  Tooltip,
  Typography,
  Alert,
  Tabs,
  Badge,
  Card,
  Statistic,
  Row,
  Col,
} from "antd";
import {
  SearchOutlined,
  EditOutlined,
  ClearOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  BarChartOutlined,
} from "@ant-design/icons";
import type { ColumnsType } from "antd/es/table";
import type { SerieDto, UpdateSeriesDto } from "../../../api/models";
import { useSeriesList } from "../hooks/useSeriesList";
import { useToggleSerieActive } from "../hooks/useToggleSerieActive";
import { useState } from "react";
import { SeriesForm } from "../components";
import { useUpdateSerie } from "../hooks";
import { PageHeaderTabs } from "../../../shared/components";
import { useAbility } from "../../../config";
import type { SeriesStatusFilter } from "../hooks/useSeriesList";

const { Text } = Typography;

export const SeriesListPage = () => {
  const ability = useAbility();

  // Estados para modal de edición
  const [editModalVisible, setEditModalVisible] = useState(false);
  const [editingSerie, setEditingSerie] = useState<SerieDto | null>(null);

  const {
    series,
    stats,            
    statusFilter,       
    paginationConfig,
    isLoading,
    isFetching,
    isError,
    searchInput,
    handleStatusFilterChange,
    handleSearchInputChange,
    handleSearch,
    handleClearSearch,
  } = useSeriesList();

  const { handleToggle, isPending: isToggling } = useToggleSerieActive();
  const { handleEdit: updateSerie, isPending: isUpdating } = useUpdateSerie();

  // modal de edición
  const handleEditClick = (record: SerieDto) => {
    setEditingSerie(record);
    setEditModalVisible(true);
  };

  const handleEditFinish = async (values: any) => {
    if (!editingSerie?.id) return;

    const payload: UpdateSeriesDto = {
      name: values.name ?? editingSerie.name,
      prefix: values.prefix ?? editingSerie.prefix,
      startNumber: values.startNumber ?? editingSerie.startNumber,
      endNumber: values.endNumber ?? editingSerie.endNumber,
      isActive: editingSerie.isActive, 
    };

    const success = await updateSerie(editingSerie.id, payload);

    if (success) {
      setEditModalVisible(false);
      setEditingSerie(null);
    }
  };

  const handleEditCancel = () => {
    setEditModalVisible(false);
    setEditingSerie(null);
  };

  // tablas y columnas
  const columns: ColumnsType<SerieDto> = [
    {
      title: "Nombre",
      dataIndex: "name",
      key: "name",
      width: 200,
      render: (text) => <Text strong>{text}</Text>,
    },
    {
      title: "Prefijo",
      dataIndex: "prefix",
      key: "prefix",
      width: 120,
      render: (text) => (
        <Tag color="blue" style={{ fontSize: 13 }}>
          {text}
        </Tag>
      ),
    },
    {
      title: "Rango",
      key: "range",
      width: 180,
      render: (_, record) => (
        <Space>
          <Tag color="green">{record.startNumber}</Tag>
          <Text type="secondary">-</Text>
          <Tag color="orange">{record.endNumber}</Tag>
        </Space>
      ),
    },
    {
      title: "Número Actual",
      dataIndex: "currentNumber",
      key: "currentNumber",
      width: 130,
      align: "center",
      render: (num) => (
        <Tag color="purple" style={{ fontSize: 13 }}>
          {num || 0}
        </Tag>
      ),
    },
    {
      title: "Disponibles",
      key: "available",
      width: 120,
      align: "center",
      render: (_, record) => {
        const available = (record.endNumber || 0) - (record.currentNumber || 0);
        const isLow = available < 10;
        return (
          <Tooltip title={isLow ? "⚠️ Quedan pocos números disponibles" : ""}>
            <Tag color={isLow ? "red" : "cyan"} style={{ fontSize: 13 }}>
              {available}
            </Tag>
          </Tooltip>
        );
      },
    },
    {
      title: "Estado",
      dataIndex: "isActive",
      key: "isActive",
      width: 150,
      align: "center",
      render: (isActive, record) => (
        <Tooltip title={isActive ? "Click para desactivar" : "Click para activar"}>
          <Switch
            checked={isActive ?? true}
            loading={isToggling}
            checkedChildren="Activa"
            unCheckedChildren="Inactiva"
            onChange={() =>
              handleToggle(record.id!, {
                name: record.name!,
                prefix: record.prefix!,
                startNumber: record.startNumber!,
                endNumber: record.endNumber!,
                currentNumber: record.currentNumber,
                isActive: record.isActive,
              })
            }
          />
        </Tooltip>
      ),
    },
    {
      title: "Acciones",
      key: "actions",
      width: 100,
      align: "center",
      fixed: "right",
      render: (_, record) => (
        <Space>
          <Tooltip title="Editar serie">
            <Button
              type="primary"
              ghost
              icon={<EditOutlined />}
              size="small"
              onClick={() => handleEditClick(record)}
              disabled={!record.isActive} // ⭐ Solo editar si está activa
            />
          </Tooltip>
        </Space>
      ),
    },
  ];

  // ============ ITEMS DE TABS ============
  const tabItems = [
    {
      key: "active",
      label: (
        <Badge count={stats.active} offset={[10, 0]} showZero>
          <Space>
            <CheckCircleOutlined />
            Activas
          </Space>
        </Badge>
      ),
    },
    {
      key: "inactive",
      label: (
        <Badge count={stats.inactive} offset={[10, 0]} showZero>
          <Space>
            <CloseCircleOutlined />
            Inactivas
          </Space>
        </Badge>
      ),
    },
    {
      key: "all",
      label: (
        <Badge count={stats.total} offset={[10, 0]} showZero>
          <Space>
            <BarChartOutlined />
            Todas
          </Space>
        </Badge>
      ),
    },
  ];

  return (
    <div>
      {/* Navegación */}
      <PageHeaderTabs
        title="Gestión de Series"
        tabs={[
          ...(ability.can("read", "series")
            ? [
                {
                  key: "list",
                  label: "Lista de Series",
                  path: "/series/list",
                },
              ]
            : []),
          ...(ability.can("create", "series")
            ? [
                {
                  key: "create",
                  label: "Crear Serie",
                  path: "/series/create",
                },
              ]
            : []),
        ]}
        defaultActive="list"
      />

      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col span={8}>
          <Card>
            <Statistic
              title="Series Activas"
              value={stats.active}
              valueStyle={{ color: "#52c41a" }}
              prefix={<CheckCircleOutlined />}
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card>
            <Statistic
              title="Series Inactivas"
              value={stats.inactive}
              valueStyle={{ color: "#ff4d4f" }}
              prefix={<CloseCircleOutlined />}
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card>
            <Statistic
              title="Total de Series"
              value={stats.total}
              valueStyle={{ color: "#1890ff" }}
              prefix={<BarChartOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {/* Contenido */}
      <div className="primary-card">
        <Tabs
          activeKey={statusFilter}
          items={tabItems}
          onChange={(key) => handleStatusFilterChange(key as SeriesStatusFilter)}
          style={{ marginBottom: 16 }}
        />

        {/* Barra de búsqueda */}
        <Space style={{ marginBottom: 16, width: "100%" }} direction="vertical">
          <Space style={{ width: "100%", justifyContent: "space-between" }}>
            <Space>
              <Input
                placeholder="Buscar por nombre..."
                value={searchInput}
                onChange={(e) => handleSearchInputChange(e.target.value)}
                onPressEnter={handleSearch}
                style={{ width: 300 }}
                prefix={<SearchOutlined />}
                allowClear
              />
              <Button
                type="primary"
                icon={<SearchOutlined />}
                onClick={handleSearch}
              >
                Buscar
              </Button>
              {searchInput && (
                <Button icon={<ClearOutlined />} onClick={handleClearSearch}>
                  Limpiar
                </Button>
              )}
            </Space>
          </Space>
        </Space>

        {/* Mensaje de error */}
        {isError && (
          <Alert
            message="Error al cargar las series"
            description="Ocurrió un error al intentar cargar la lista de series. Por favor, intente nuevamente."
            type="error"
            showIcon
            style={{ marginBottom: 16 }}
          />
        )}

        {/* Tabla */}
        <Table
          columns={columns}
          dataSource={series}
          rowKey="id"
          loading={isLoading || isFetching}
          pagination={paginationConfig}
          bordered
          size="middle"
        />
      </div>

      {/* Modal de edición */}
      <Modal
        title={
          <Space>
            <EditOutlined />
            <span>Editar Serie: {editingSerie?.name}</span>
          </Space>
        }
        open={editModalVisible}
        onCancel={handleEditCancel}
        footer={null}
        width={600}
        destroyOnClose
      >
        {editingSerie && (
          <SeriesForm
            mode="edit"
            initialValues={editingSerie}
            onFinish={handleEditFinish}
            loading={isUpdating}
          />
        )}
      </Modal>
    </div>
  );
};