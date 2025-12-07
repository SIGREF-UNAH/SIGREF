import { useState } from "react";
import {
  Table,
  Button,
  Input,
  Select,
  Space,
  Popconfirm,
  Alert,
  Tag,
} from "antd";
import { useShiftsList } from "../hooks/useShiftsList";
import { useShiftFormData } from "../hooks/useShiftFormData";
import {
  EditOutlined,
  EyeOutlined,
  FilterOutlined,
  PlusOutlined,
  StopOutlined,
} from "@ant-design/icons";
import { PageHeaderTabs } from "../../../shared/components/ui";
import type { ColumnsType } from "antd/es/table";
import type { ShiftDto } from "../../../api/models";
import { CreateShiftModal, EditShiftModal, ShiftDetailsModal } from "../components";
import { useAbility } from "../../../config";
import { Can } from "@casl/react";

const { Search } = Input;

export const ShiftsPage = () => {
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const ability = useAbility();
  const [selectedShift, setSelectedShift] = useState<ShiftDto | null>(null);
  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);

  const { locations, isLoading: isLoadingLocations } = useShiftFormData();

  const {
    filters,
    shifts,
    paginationConfig,
    isLoading,
    isFetching,
    isError,
    searchInput,
    handleDelete,
    setFilter,
    handleSearchInputChange,
    handleSearch,
  } = useShiftsList();

  // Función para abrir el modal de edición
  const handleOpenEdit = (record: ShiftDto) => {
    setSelectedShift(record);
    setIsEditModalOpen(true);
  };

  const columns: ColumnsType<ShiftDto> = [
    {
      title: "Nombre del Turno",
      dataIndex: "name",
      key: "name",
      width: 220,
      align: "center" as const,
      render: (text) => text || "-",
    },
    {
      title: "Hora Inicio",
      dataIndex: "startTime",
      key: "startTime",
      width: 110,
      align: "center" as const,
      render: (t) => (t ? t.substring(0, 5) : "-"),
    },
    {
      title: "Hora Fin",
      dataIndex: "endTime",
      key: "endTime",
      width: 110,
      align: "center" as const,
      render: (t) => (t ? t.substring(0, 5) : "-"),
    },
    {
      title: "Área",
      dataIndex: "nameLocation",
      key: "nameLocation",
      width: 200,
      align: "center" as const,
      render: (text) => text || "Sin asignar",
    },
    {
      title: "Estado",
      dataIndex: "isActive",
      key: "isActive",
      width: 100,
      align: "center" as const,
      render: (active: boolean) => (
        <Tag color={active ? "green" : "red"}>
          {active ? "Activo" : "Inactivo"}
        </Tag>
      ),
    },
    {
      title: "Acciones",
      key: "actions",
      width: 160,
      align: "center" as const,
      fixed: "right" as const,
      render: (_, record) => (
        <Space size="small">
          <Can I="read" a="shifts" ability={ability}>
            <Button
              type="text"
              size="small"
              icon={<EyeOutlined className="text-blue-600" />}
              onClick={() => {
                setSelectedShift(record);
                setIsDetailsModalOpen(true);
              }}
              title="Ver detalles"
            />
          </Can>

          <Can I="update" a="shifts" ability={ability}>
            <Button
              type="text"
              size="small"
              icon={<EditOutlined className="text-green-600" />}
              onClick={() => handleOpenEdit(record)}
              title="Editar turno"
            />
          </Can>

          <Can I="delete" a="shifts" ability={ability}>
            <Popconfirm
              title="Desactivar turno"
              description="¿Estás seguro de desactivar este turno?"
              onConfirm={() => record.id && handleDelete(record.id)}
              okText="Sí"
              cancelText="No"
              okButtonProps={{ danger: true }}
            >
              <Button
                type="text"
                size="small"
                danger
                icon={<StopOutlined />}
                title="Desactivar turno"
              />
            </Popconfirm>
          </Can>
        </Space>
      ),
    },
  ];

  if (isError) {
    return (
      <Alert
        message="Error al cargar los turnos"
        description="Por favor, intenta de nuevo más tarde."
        type="error"
        showIcon
      />
    );
  }

  return (
    <div>
      <PageHeaderTabs title="Gestión de Turnos" tabs={[]} />

      <div className="primary-card">
        <div className="flex justify-between items-center mb-6">
          <Can I="create" a="shifts" ability={ability}>
            <Button
              className="ml-auto"
              type="primary"
              size="large"
              icon={<PlusOutlined />}
              onClick={() => setIsCreateModalOpen(true)}
              loading={isLoadingLocations}
            >
              Crear Turno
            </Button>
          </Can>
        </div>

        {/* Filtros */}
        <div className="flex justify-end gap-3 mb-4">
          <Search
            placeholder="Buscar por nombre"
            allowClear
            style={{ width: 300 }}
            value={searchInput}
            onChange={(e) => handleSearchInputChange(e.target.value)}
            onSearch={handleSearch}
            loading={isFetching}
          />
          <Select
            placeholder="Por Área"
            allowClear
            suffixIcon={<FilterOutlined />}
            style={{ width: 200 }}
            value={filters.location}
            onChange={(value) => setFilter("location", value)}
            options={locations.map((loc) => ({
              label: loc.name,
              value: loc.id,
            }))}
            showSearch
          />
          <Select
            placeholder="Por Estado"
            allowClear
            suffixIcon={<FilterOutlined />}
            style={{ width: 150 }}
            value={filters.status}
            onChange={(value) => setFilter("status", value)}
            options={[
              { label: "Activo", value: "active" },
              { label: "Inactivo", value: "inactive" },
            ]}
          />
        </div>

        {/* Tabla */}
        <Table
          columns={columns}
          dataSource={shifts}
          rowKey={(record) => record.id || Math.random().toString(36)}
          pagination={paginationConfig}
          bordered
          loading={isLoading || isFetching}
          scroll={{ x: 1000 }}
          locale={{
            emptyText: "No hay turnos registrados",
          }}
        />
      </div>

      {/* Modal Crear Turno */}
      <CreateShiftModal
        open={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        locations={locations}
      />

      {/* Modal Ver Detalles */}
      <ShiftDetailsModal
        open={isDetailsModalOpen}
        onClose={() => setIsDetailsModalOpen(false)}
        shift={selectedShift}
      />

      {/* Modal Editar*/}
      <EditShiftModal
        open={isEditModalOpen}
        onClose={() => {
          setIsEditModalOpen(false);
          setSelectedShift(null);
        }}
        shift={selectedShift}
        locations={locations}
      />
    </div>
  );
};
