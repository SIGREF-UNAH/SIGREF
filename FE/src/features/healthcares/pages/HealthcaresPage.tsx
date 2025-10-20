import { Table, Button, Input, Select, Space, Popconfirm, Alert, Tag } from "antd";
import type { ColumnsType } from "antd/es/table";
import { useHealthcaresList } from "../hooks";
import { HealthcareHeader } from "../components/ui";
import type { HealthcareDto } from "../../../api/models";
import { HealthcaresPageSkeleton } from "../components/skeletons";
import { HealthcareDetailsModal } from "../components/modals";
import { PageHeaderTabs } from "../../../shared/components/ui";
import {
  EditOutlined,
  DeleteOutlined,
  PlusOutlined,
  FilterOutlined,
  EyeOutlined,
} from "@ant-design/icons";

const { Search } = Input;

export const HealthcaresPage = () => {
  const {
    filters,
    departments,
    filteredData,
    paginationConfig,
    isLoading,
    isError,
    selectedHealthcare,
    isModalOpen,
    handleCreate,
    handleEdit,
    handleDelete,
    handleViewDetails,
    setFilter,
    handleCloseModal,
  } = useHealthcaresList();

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
      render: (_, record) => record.location?.map(loc => 
        loc?.display).filter(Boolean).join(", ") || "-",
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
          <Button
            type="text"
            icon={<EyeOutlined />}
            onClick={() => handleViewDetails(record)}
            title="Ver detalles"
          />
          <Button
            type="text"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record.id || "")}
          />
          <Popconfirm
            title="Eliminar servicio"
            description="¿Desea eliminar este servicio médico?"
            onConfirm={() => handleDelete(record.id || "")}
            okText="Sí, eliminar"
            cancelText="Cancelar"
            okButtonProps={{ danger: true }}
          >
            <Button
              type="text"
              danger
              icon={<DeleteOutlined />}
            />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  // Manejo de error
  if (isError) {
    return (
      <div>
        <div className="flex mb-4 items-start justify-between">
          <HealthcareHeader />
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={handleCreate}
            size="large"
            style={{ backgroundColor: "var(--color-primary)" }}
          >
            Nuevo Servicio
          </Button>
        </div>
        <Alert
          message="Error al cargar los servicios"
          description="No se pudieron cargar los servicios médicos. Por favor, intente nuevamente."
          type="error"
          showIcon
        />
      </div>
    );
  }

  return (
    <div>
      {/* Encabezado */}
      <PageHeaderTabs
        title="Gestión de Servicios"
        tabs={[
          { key: "listar", label: "Lista de Servicios", path: "/healthcares/list" },
          { key: "crear", label: "Crear Servicio", path: "/healthcares/create" },
        ]}
        defaultActive="crear"
      />

      {/* Contenido */}
      {isLoading ? (
        <HealthcaresPageSkeleton />
      ) : (
        <div className="p-4 border-2 bg-card border-gray-300 shadow-md rounded-lg">
          {/* Busqueda y filtros */}
          <div className="flex justify-end gap-3 mb-4">
            <Search
              placeholder="Buscar por nombre o abreviatura"
              allowClear
              style={{ width: 300 }}
              value={filters.search}
              onChange={(e) => setFilter("search", e.target.value)}
              onSearch={(value) => setFilter("search", value)}
            />
            <Select
              placeholder="Por Departamento"
              allowClear
              suffixIcon={<FilterOutlined />}
              style={{ width: 200, height: 36 }}
              value={filters.department}
              onChange={(value) => setFilter("department", value)}
              options={departments.map((dept) => ({ label: dept, value: dept }))}
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
            dataSource={filteredData}
            rowKey="id"
            pagination={paginationConfig}
            bordered
            loading={isLoading}
          />
        </div>
      )}

      {/* Modal de detalles */}
      <HealthcareDetailsModal
        open={isModalOpen}
        healthcare={selectedHealthcare}
        onClose={handleCloseModal}
      />
    </div>
  );
};