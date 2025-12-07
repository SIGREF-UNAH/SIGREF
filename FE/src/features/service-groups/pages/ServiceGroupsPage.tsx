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
import { useServiceGroupsList } from "../hooks/useServiceGroupsList";
import {
  EditOutlined,
  DeleteOutlined,
  FilterOutlined,
  EyeOutlined,
} from "@ant-design/icons";
import { PageHeaderTabs } from "../../../shared/components/ui";
import type { ColumnsType } from "antd/es/table";
import type { ServiceGroupDto } from "../../../api/models";
import { useLocationSearch } from "../../../shared/hooks/useLocationSearch";
import { getListStatusLabel, getListStatusColor, getListStatusOptions } from "../../../shared/utils";
import { useAbility } from "../../../config";
import { Can } from "@casl/react";

const { Search } = Input;

export const ServiceGroupsPage = () => {
  const { options: locationOptions, loading: searchingLocations, searchLocations } = useLocationSearch();
  const ability = useAbility();
  
  const {
    filters,
    serviceGroups,
    paginationConfig,
    isLoading,
    isFetching,
    isError,
    searchInput,
    handleEdit,
    handleDelete,
    setFilter,
    handleSearchInputChange,
    handleSearch,
    handleClearSearch,
  } = useServiceGroupsList();

  // Columnas de la tabla
  const columns: ColumnsType<ServiceGroupDto & { abbreviation: string }> = [
    {
      title: "Abreviatura",
      dataIndex: "abbreviation",
      key: "abbreviation",
      width: 120,
    },
    {
      title: "Nombre",
      dataIndex: "title",
      key: "title",
      width: 200,
    },
    {
      title: "Área Asistencial",
      key: "locations",
      width: 250,
      render: (_, record) =>
        record.locations
          ?.map((loc) => loc?.name)
          .filter(Boolean)
          .join(", ") || "-",
    },
    {
      title: "Estado",
      dataIndex: "status",
      key: "status",
      width: 120,
      render: (status: string) => {
        const color = getListStatusColor(status);
        const label = getListStatusLabel(status);
        return <Tag color={color}>{label || "N/A"}</Tag>;
      },
    },
    {
      title: "Fecha",
      dataIndex: "date",
      key: "date",
      width: 120,
      render: (date: string) => {
        if (!date) return "-";
        return new Date(date).toLocaleDateString("es-HN");
      },
    },
    {
      title: "Acciones",
      key: "actions",
      width: 120,
      render: (_, record) => (
        <Space size="small">
          <Can I="read" a="service-groups" ability={ability}>
            <Button
              type="text"
              icon={<EyeOutlined />}
              onClick={() => console.log("Ver detalles")} // TODO: Implementar
              title="Ver detalles"
            />
          </Can>
          <Can I="update" a="service-groups" ability={ability}>            
            <Button
              type="text"
              icon={<EditOutlined />}
              onClick={() => handleEdit(record.id || "")}
            />
          </Can>
          <Can I="delete" a="service-groups" ability={ability}>
            <Popconfirm
              title="Eliminar paquete"
              description="¿Desea eliminar este paquete?"
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

  if (isError) {
    return (
      <div>
        <Alert
          message="Error al cargar los paquetes"
          description="No se pudieron cargar los paquetes. Por favor, intente nuevamente."
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
      <PageHeaderTabs
        title="Gestión de Paquetes"
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
        defaultActive="listar2"
      />

      <div className="primary-card">
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
            showSearch
            suffixIcon={<FilterOutlined />}
            style={{ width: 200, height: 36 }}
            value={filters.location}
            onChange={(value) => setFilter("location", value)}
            onSearch={searchLocations}
            loading={searchingLocations}
            filterOption={false}
            options={locationOptions}
            notFoundContent={searchingLocations ? <Spin size="small" /> : "Buscar ubicación..."}
          />
          <Select
            placeholder="Por Estado"
            allowClear
            suffixIcon={<FilterOutlined />}
            style={{ width: 200, height: 36 }}
            value={filters.status}
            onChange={(value) => setFilter("status", value)}
            options={getListStatusOptions()}
          />
        </div>
        <Table
          columns={columns}
          dataSource={serviceGroups}
          rowKey="id"
          pagination={paginationConfig}
          bordered
          loading={isFetching}
        />
      </div>
    </div>
  );
};
