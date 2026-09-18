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
  Switch,
} from "antd";
import {
  EditOutlined,
  DeleteOutlined,
  FilterOutlined,
  EyeOutlined,
} from "@ant-design/icons";
import type { ColumnsType } from "antd/es/table";
import type {
  HealthcareDto,
} from "@models";
import type { HealthcareScope as HealthcareScopeType } from "../../../api/generated/schemas/types/healthcare-services/healthcareScope";
import { HealthcareScope } from "../../../api/generated/schemas/types/healthcare-services/healthcareScope";
import { useHealthcaresList } from "../hooks/useHealthcaresList";
import { PageHeaderTabs } from "../../../shared/components/ui";
import { HealthcareDetailsModal } from "../components";
import { useAbility } from "../../../config";

const { Search } = Input;
const { Option } = Select;

export const HealthcaresListPage = () => {
  const ability = useAbility();

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

  // Columnas de la tabla
  const columns: ColumnsType<HealthcareDto> = [
    {
      title: "Abreviatura",
      dataIndex: "abbreviation",
      key: "abbreviation",
      width: 120,
      ellipsis: true,
    },
    {
      title: "Nombre",
      dataIndex: "name",
      key: "name",
      width: 200,
      ellipsis: true,
    },
    {
      title: "Tipo",
      dataIndex: "scope",
      key: "scope",
      width: 120,
      render: (scope: HealthcareScopeType) => {
        if (!scope) return <Tag>-</Tag>;

        const isInternal = scope === HealthcareScope.internal;

        return (
          <Tag color={isInternal ? "blue" : "orange"}>
            {isInternal ? "Interno" : "Externo"}
          </Tag>
        );
      },
    },
    {
      title: "Ubicación(es)",
      key: "location",
      width: 200,
      render: (_, record) => {
        const locationNames = record.location
          ?.map((loc) => loc?.display)
          .filter(Boolean);

        if (!locationNames?.length) return <Tag>-</Tag>;

        return (
          <Space direction="vertical" size={0}>
            {locationNames.map((loc, index) => (
              <Tag key={index} color="purple">
                {loc}
              </Tag>
            ))}
          </Space>
        );
      },
    },
    ...(filters.includeCost
      ? [
          {
            title: "Costo",
            dataIndex: "cost",
            key: "cost",
            width: 120,
            align: "right" as const,
            render: (cost: number) =>
              cost != null ? `L. ${cost.toFixed(2)}` : <Tag>-</Tag>,
          },
        ]
      : []),
    {
      title: "Estado",
      dataIndex: "active",
      key: "active",
      width: 100,
      align: "center" as const,
      render: (status: boolean) => {
        const isActive = status === true;
        return (
          <Tag color={isActive ? "success" : "error"}>
            {isActive ? "Activo" : "Inactivo"}
          </Tag>
        );
      },
    },
    {
      title: "Acciones",
      key: "actions",
      width: 150,
      fixed: "right" as const,
      render: (_, record) => (
        <Space size="small">
          {ability.can("read", "healthcares") && (
            <Button
              type="text"
              icon={<EyeOutlined />}
              onClick={() => handleViewDetails(record)}
              title="Ver detalles"
            />
          )}
          {ability.can("update", "healthcares") && (
            <Button
              type="text"
              icon={<EditOutlined />}
              onClick={() => handleEdit(record.id || "")}
              title="Editar servicio"
            />
          )}
          {ability.can("delete", "healthcares") && (
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
                title="Eliminar servicio"
              />
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  // Pantalla de error
  if (isError) {
    return (
      <div className="primary-card">
        <Alert
          message="Error al cargar los servicios"
          description="No se pudieron cargar los servicios médicos. Por favor, intente nuevamente."
          type="error"
          showIcon
          action={
            <Button size="small" onClick={() => window.location.reload()}>
              Reintentar
            </Button>
          }
        />
      </div>
    );
  }

  // Pantalla de carga
  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-100">
        <Spin size="large" tip="Cargando servicios médicos..." />
      </div>
    );
  }

  return (
    <div>
      {/* Encabezado */}
      <PageHeaderTabs
        title="Gestión de Servicios"
        tabs={[
          ...(ability.can("read", "healthcares")
            ? [
                {
                  key: "listar1",
                  label: "Lista de Servicios",
                  path: "/healthcares/list",
                },
              ]
            : []),
          ...(ability.can("create", "healthcares")
            ? [
                {
                  key: "crear1",
                  label: "Crear Servicio",
                  path: "/healthcares/create",
                },
              ]
            : []),
          ...(ability.can("read", "service-groups")
            ? [
                {
                  key: "listar2",
                  label: "Lista de Paquetes",
                  path: "/service-groups/list",
                },
              ]
            : []),
          ...(ability.can("create", "service-groups")
            ? [
                {
                  key: "crear2",
                  label: "Crear Paquete",
                  path: "/service-groups/create",
                },
              ]
            : []),
        ]}
        defaultActive="listar1"
      />

      {/* Contenido */}
      <div className="primary-card">
        {/* Búsqueda y filtros */}
        <div className="flex flex-wrap justify-end gap-3 mb-4">
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
            showSearch
            allowClear
            suffixIcon={<FilterOutlined />}
            style={{ width: 225 }}
            value={filters.location}
            onChange={(value) => setFilter("location", value)}
            filterOption={(input, option) =>
              (option?.label as string)
                ?.toLowerCase()
                .includes(input.toLowerCase())
            }
          >
            {locations.map((location: any) => (
              <Option
                key={location.id}
                value={location.id}
                label={location.name || location.display || "Sin nombre"}
              >
                {location.name || location.display || "Sin nombre"}
              </Option>
            ))}
          </Select>
          <Select
            placeholder="Por Tipo"
            allowClear
            suffixIcon={<FilterOutlined />}
            style={{ width: 150 }}
            value={filters.scope}
            onChange={(value) => setFilter("scope", value)}
            options={[
              {
                label: "Interno",
                value: HealthcareScope.internal,
              },
              {
                label: "Externo",
                value: HealthcareScope.external,
              },
            ]}
          />
          <Select
            placeholder="Por Estado"
            allowClear
            suffixIcon={<FilterOutlined />}
            style={{ width: 150 }}
            value={filters.active}
            onChange={(value) => setFilter("active", value)}
            options={[
              { label: "Activo", value: true },
              { label: "Inactivo", value: false },
            ]}
          />
          <div className="flex items-center gap-2 px-3 border border-gray-300 bg-white rounded">
            <span className="text-sm text-gray-600">Mostrar Costo</span>
            <Switch
              checked={filters.includeCost}
              onChange={(checked) => setFilter("includeCost", checked)}
              size="small"
            />
          </div>
        </div>

        {/* Tabla de servicios */}
        <Table
          columns={columns}
          dataSource={healthcares}
          rowKey="id"
          pagination={paginationConfig}
          bordered
          loading={isFetching}
          scroll={{ x: 1000 }}
          locale={{
            emptyText: "No se encontraron servicios médicos",
          }}
        />
      </div>

      {/* Modal de detalles */}
      <HealthcareDetailsModal
        open={isModalOpen}
        healthcare={selectedHealthcare}
        filters={filters}
        onClose={handleCloseModal}
      />
    </div>
  );
};
