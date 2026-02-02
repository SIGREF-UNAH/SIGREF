import { PageContainer } from "@ant-design/pro-components";
import {
  Button,
  Card,
  Table,
  Space,
  Input,
  Tag,
  Modal,
  Switch,
  Tooltip,
  Typography,
  Alert,
} from "antd";
import {
  PlusOutlined,
  SearchOutlined,
  EditOutlined,
  DeleteOutlined,
  ClearOutlined,
  ExclamationCircleOutlined,
} from "@ant-design/icons";
import type { ColumnsType } from "antd/es/table";
import type { SerieDto, UpdateSeriesDto } from "../../../api/models";
import { useSeriesList } from "../hooks/useSeriesList";
import { useToggleSerieActive } from "../hooks/useToggleSerieActive";
import { useState } from "react";
import { SeriesForm } from "../components";
import { useUpdateSerie } from "../hooks";

const { Text } = Typography;

export const SeriesListPage = () => {
  // Estados para modal de edición
  const [editModalVisible, setEditModalVisible] = useState(false);
  const [editingSerie, setEditingSerie] = useState<SerieDto | null>(null);

  // Estados para modal de eliminación
  const [deleteModalVisible, setDeleteModalVisible] = useState(false);
  const [deletingSerie, setDeletingSerie] = useState<SerieDto | null>(null);

  const {
    series,
    paginationConfig,
    isLoading,
    isFetching,
    isError,
    isDeleting,
    searchInput,
    handleCreate,
    handleDelete,
    handleSearchInputChange,
    handleSearch,
    handleClearSearch,
  } = useSeriesList();

  const { handleToggle, isPending: isToggling } = useToggleSerieActive();
  const { handleEdit: updateSerie, isPending: isUpdating } = useUpdateSerie();

  // Funciones para modal de eliminación
  const showDeleteConfirm = (record: SerieDto) => {
    setDeletingSerie(record);
    setDeleteModalVisible(true);
  };

  const handleDeleteConfirm = () => {
    if (deletingSerie?.id) {
      handleDelete(deletingSerie.id);
      setDeleteModalVisible(false);
      setDeletingSerie(null);
    }
  };

  const handleDeleteCancel = () => {
    setDeleteModalVisible(false);
    setDeletingSerie(null);
  };

  // Funciones para modal de edición
  const handleEditClick = (record: SerieDto) => {
    setEditingSerie(record);
    setEditModalVisible(true);
  };

const handleEditFinish = async (values: any) => {
  if (!editingSerie?.id) return;

  // Construir payload completo
  const payload: UpdateSeriesDto = {
    name: values.name ?? editingSerie.name,
    prefix: values.prefix ?? editingSerie.prefix,
    startNumber: values.startNumber ?? editingSerie.startNumber,
    endNumber: values.endNumber ?? editingSerie.endNumber,
    isActive: values.isActive ?? editingSerie.isActive,  
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
          <Tag color={isLow ? "red" : "cyan"} style={{ fontSize: 13 }}>
            {available}
          </Tag>
        );
      },
    },
    {
      title: "Estado",
      dataIndex: "isActive",
      key: "isActive",
      width: 100,
      align: "center",
      render: (isActive, record) => (
        <Tooltip title={isActive ? "Desactivar" : "Activar"}>
          <Switch
            checked={isActive ?? true}
            loading={isToggling}
            onChange={(checked) =>
              handleToggle(record.id!, checked, {
                name: record.name,
                prefix: record.prefix,
                startNumber: record.startNumber,
                endNumber: record.endNumber,
                currentNumber: record.currentNumber,
              })
            }
          />
        </Tooltip>
      ),
    },
    {
      title: "Acciones",
      key: "actions",
      width: 150,
      align: "center",
      fixed: "right",
      render: (_, record) => (
        <Space>
          <Tooltip title="Editar">
            <Button
              type="primary"
              ghost
              icon={<EditOutlined />}
              size="small"
              onClick={() => handleEditClick(record)}
            />
          </Tooltip>
          <Tooltip title="Desactivar">
            <Button
              danger
              icon={<DeleteOutlined />}
              size="small"
              loading={isDeleting}
              onClick={() => showDeleteConfirm(record)}
            />
          </Tooltip>
        </Space>
      ),
    },
  ];

  return (
    <PageContainer
      header={{
        title: "Gestión de Series",
        subTitle: "Administre las series de numeración para sus documentos",
      }}
    >
      <Card>
        {/* Barra de búsqueda y acciones */}
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
            <Button
              type="primary"
              icon={<PlusOutlined />}
              size="large"
              onClick={handleCreate}
            >
              Nueva Serie
            </Button>
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
          scroll={{ x: 1200 }}
          bordered
          size="middle"
        />
      </Card>

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

      {/* Modal de eliminación */}
      <Modal
        title={
          <Space>
            <ExclamationCircleOutlined style={{ color: "#ff4d4f" }} />
            <span>Desactivar Serie</span>
          </Space>
        }
        open={deleteModalVisible}
        onOk={handleDeleteConfirm}
        onCancel={handleDeleteCancel}
        okText="Sí, desactivar"
        okType="danger"
        cancelText="Cancelar"
        confirmLoading={isDeleting}
        width={500}
      >
        <div style={{ marginTop: 16 }}>
          <p style={{ fontSize: 16, marginBottom: 16 }}>
            ¿Está seguro de <strong>desactivar</strong> la serie:{" "}
            <Text strong style={{ fontSize: 16 }}>
              {deletingSerie?.name}
            </Text>
            ?
          </p>

          <Alert
            message="Importante"
            description="La serie no se eliminará permanentemente, solo quedará inactiva y no se podrá usar para nuevos documentos."
            type="warning"
            showIcon
          />

          <div
            style={{
              marginTop: 16,
              padding: 12,
              backgroundColor: "#f5f5f5",
              borderRadius: 4,
            }}
          >
            <Space direction="vertical" size="small">
              <Text type="secondary">Detalles de la serie:</Text>
              <Text>
                <strong>Prefijo:</strong> {deletingSerie?.prefix}
              </Text>
              <Text>
                <strong>Rango:</strong> {deletingSerie?.startNumber} -{" "}
                {deletingSerie?.endNumber}
              </Text>
              <Text>
                <strong>Número actual:</strong>{" "}
                {deletingSerie?.currentNumber || 0}
              </Text>
            </Space>
          </div>
        </div>
      </Modal>
    </PageContainer>
  );
};
