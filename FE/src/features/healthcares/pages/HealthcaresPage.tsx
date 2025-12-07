import {
  Table,
  Button,
  Input,
  Select,
  Space,
  Popconfirm,
  Alert,
  Tag,
  Spin,
} from "antd";
import { useHealthcaresList } from "../hooks";
import {
  EditOutlined,
  DeleteOutlined,
  FilterOutlined,
  EyeOutlined,
} from "@ant-design/icons";
import { PageHeaderTabs } from "../../../shared/components/ui";
import type { ColumnsType } from "antd/es/table";
import type { HealthcareDto } from "../../../api/models";
import { HealthcareDetailsModal } from "../components";
import { Can } from "@casl/react";
import { useAbility } from "../../../config";

const { Search } = Input;

export const HealthcaresPage = () => {
  const {
    filters,
    locations,
    healthcares,
    paginationConfig,
    isLoading,
    isFetching,
    isError,
    selectedHealthcare,
    isModalOpen,
    searchInput,
    handleEdit,
    handleDelete,
    handleViewDetails,
    setFilter,
    handleCloseModal,
    handleSearchInputChange,
    handleSearch,
    handleClearSearch,
  } = useHealthcaresList();

  const ability = useAbility();

  // Columnas de la tabla
  const columns: ColumnsType<HealthcareDto> = [
    {
      title: "Abreviatura",
      dataIndex: "abbreviation",
      key: "abbreviation",
      width: 120,
    },
    {
      title: "Nombre",
      dataIndex: "name",
      key: "name",
      width: 200,
    },
    {
      title: "Ubicación(es)",
      key: "area",
      width: 200,
      render: (_, record) =>
        record.location
          ?.map((loc) => loc?.display)
          .filter(Boolean)
          .join(", ") || "-",
    },
    {
      title: "Costo",
      dataIndex: "cost",
      key: "cost",
      width: 120,
      render: (cost: number) => `L. ${cost?.toFixed(2) || "0.00"}`,
    },
    {
      title: "Estado",
      dataIndex: "active",
      key: "active",
      width: 60,
      render: (status: boolean) => {
        const color = status === true ? "green" : "error";
        const text = status === true ? "✓ Activo" : "✗ Inactivo";
        return <Tag color={color}>{text}</Tag>;
      },
    },
    {
      title: "Acciones",
      key: "actions",
      width: 120,
      render: (_, record) => (
        <Space size="small">
          <Can I="read" a="healthcares" ability={ability}>
            <Button
              type="text"
              icon={<EyeOutlined />}
              onClick={() => handleViewDetails(record)}
              title="Ver detalles"
            />
          </Can>
          <Can I="update" a="healthcares" ability={ability}>
            <Button
              type="text"
              icon={<EditOutlined />}
              onClick={() => handleEdit(record.id || "")}
            />
          </Can>
          <Can I="delete" a="healthcares" ability={ability}>
            <Popconfirm
              title="Eliminar servicio"
              description="¿Desea eliminar este servicio médico?"
              onConfirm={() => handleDelete(record.id || "")}
              okText="Sí, eliminar"
              cancelText="Cancelar"
              okButtonProps={{ danger: true }}
            >
              <Button type="text" danger icon={<DeleteOutlined />} />
            </Popconfirm>
          </Can>
        </Space>
      ),
    },
  ];

  // Manejo de error
  if (isError) {
    return (
      <div>
        <Alert
          message="Error al cargar los servicios"
          description="No se pudieron cargar los servicios médicos. Por favor, intente nuevamente."
          type="error"
          showIcon
        />
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-100">
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div>
      {/* Encabezado */}
      <PageHeaderTabs
        title="Gestión de Servicios"
        tabs={[
          ...(ability.can("read", "healthcares") ? [{
            key: "listar1",
            label: "Lista de Servicios",
            path: "/healthcares/list",
          }] : []),
          ...(ability.can("create", "healthcares") ? [{
            key: "crear1",
            label: "Crear Servicio",
            path: "/healthcares/create",
          }] : []),
          ...(ability.can("read", "service-groups") ? [{
            key: "listar2",
            label: "Lista de Paquetes",
            path: "/service-groups/list",
          }] : []),
          ...(ability.can("create", "service-groups") ? [{
            key: "crear2",
            label: "Crear Paquete",
            path: "/service-groups/create",
          }] : []),
        ]}
        defaultActive="listar1"
      />

      {/* Contenido */}
      <div className="primary-card">
        {/* Búsqueda y filtros */}
        <div className="flex justify-end gap-3 mb-4">
          <Search
            placeholder="Buscar por nombre"
            allowClear
            style={{ width: 300 }}
            value={searchInput}
            onChange={(e) => handleSearchInputChange(e.target.value)}
            onSearch={handleSearch}
            onClear={handleClearSearch}
          />
          <Select
            placeholder="Por Ubicación"
            allowClear
            suffixIcon={<FilterOutlined />}
            style={{ width: 200, height: 36 }}
            value={filters.location}
            onChange={(value) => setFilter("location", value)}
            options={locations.map((loc) => ({
              label: loc.display,
              value: loc.reference,
            }))}
          />
          <Select
            placeholder="Por Estado"
            allowClear
            suffixIcon={<FilterOutlined />}
            style={{ width: 150, height: 36 }}
            value={filters.status}
            onChange={(value) => setFilter("status", value)}
            options={[
              { label: "Activo", value: "active" },
              { label: "Inactivo", value: "inactive" },
            ]}
          />
        </div>
        {/* Lista de servicios */}
        <Table
          columns={columns}
          dataSource={healthcares}
          rowKey="id"
          pagination={paginationConfig}
          bordered
          loading={isFetching}
        />
      </div>

      {/* Modal de detalles */}
      <HealthcareDetailsModal
        open={isModalOpen}
        healthcare={selectedHealthcare}
        onClose={handleCloseModal}
      />
    </div>
  );
};
