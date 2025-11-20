import { Table, Button, Input, Space, Popconfirm, Tag, Select } from "antd";
import {
  EditOutlined,
  DeleteOutlined,
  SearchOutlined,
  EyeOutlined,
  FilterOutlined,
} from "@ant-design/icons";
import { useOrganizationsList } from "../../hooks";
import { OrganizationDetailsModal } from "../modals/OrganizationsDetailsModal";

const { Search } = Input;

export default function OrganizationsList() {
  const {
    organizations,
    filters,
    paginationConfig,
    isLoading,
    searchInput,
    handleEdit,
    handleDelete,
    handleViewDetails,
    isModalOpen,
    selectedOrganization,
    setIsModalOpen,
    handleSearchInputChange,
    handleSearch,
    handleTypeChange,
    handleStatusChange,
  } = useOrganizationsList();
  
  //console.log(organizations);

  const columns = [
    {
      title: "Nombre",
      dataIndex: "name",
      key: "name",
      width: 300,
    },
    {
      title: "Identificador",
      dataIndex: "identifier",
      key: "identifier",
      window: 100,
      render: (identifier: any[]) =>
        Array.isArray(identifier)
          ? identifier[0]?.value || "N/A"
          : identifier?.value || "N/A",
    },
    {
      title: "Tipo",
      dataIndex: "types",
      key: "type",
      width: 300,
      render: (types: any) => {
        if (Array.isArray(types)) {
          return types[0]?.text || types[0]?.coding?.[0]?.display || "N/A";
        }
        if (types?.coding) {
          return types?.coding?.[0]?.display || "N/A";
        }
        return "N/A";
      },
    },
    {
      title: "Estado",
      dataIndex: "active",
      width: 140,
      key: "active",
      render: (status: boolean) => {
        const color = status === true ? "green" : "error";
        const text = status === true ? "✓ Activo" : "✗ Inactivo";
        return <Tag color={color}>{text}</Tag>;
      },
    },
    {
      title: "Acciones",
      key: "actions",
      width: 190,
      render: (_, record) => (
        <Space size="small">
          {/* ver detalles de organizacion */}
          <Button
            onClick={() => handleViewDetails(record)}
            type="text"
            icon={<EyeOutlined />}
            title="Ver detalles"
          ></Button>
          <Button
            type="text"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record.id || "")}
          ></Button>
          {/* eliminar organizacion */}
          <Popconfirm
            title="¿Eliminar organización?"
            description="Esta acción no se puede deshacer"
            onConfirm={() => handleDelete(record.id)}
            okText="Sí, eliminar"
            cancelText="Cancelar"
            okButtonProps={{ danger: true }}
          >
            <Button type="link" danger icon={<DeleteOutlined />}></Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div className="primary-card">
      {/* Búsqueda y Filtros*/}
      <div className="flex justify-end gap-3 mb-4">
        {/* Búsqueda por nombre */}
        <Search
          placeholder="Buscar por nombre"
          value={searchInput}
          onChange={(e) => handleSearchInputChange(e.target.value)}
          allowClear
          style={{ width: 300 }}
          onSearch={handleSearch}
        />
        {/* Filtro por ubicación/tipo */}
        <Select
          placeholder="Por Tipo"
          allowClear
          suffixIcon={<FilterOutlined />}
          style={{ width: 200, height: 36 }}
          value={filters.type}
          onChange={handleTypeChange}
          options={[
            { label: "Proveedor de salud", value: "prov" },
            { label: "Departamento", value: "dept" },
            { label: "Equipo", value: "team" },
            { label: "Gobierno", value: "govt" },
            { label: "Aseguradora", value: "ins" },
            { label: "Pagador", value: "pay" },
            { label: "Educativo", value: "edu" },
            { label: "Religioso", value: "reli" },
            { label: "Investigación clínica", value: "crs" },
            { label: "Comunidad", value: "cg" },
            { label: "Negocio no médico", value: "bus" },
            { label: "Otro", value: "other" },
          ]}
        />
        {/* Filtro por estado */}
        <Select
          placeholder="Por Estado"
          allowClear
          suffixIcon={<FilterOutlined />}
          style={{ width: 150, height: 36 }}
          value={filters.status}
          onChange={handleStatusChange}
          options={[
            { label: "Activo", value: "active" },
            { label: "Inactivo", value: "inactive" },
          ]}
        />
      </div>

      {/* Lista de organizaciones */}
      <Table
        columns={columns}
        dataSource={organizations}
        rowKey="id"
        bordered
        loading={isLoading}
        pagination={paginationConfig}
      />

      {/* Modal de detalles */}
      <OrganizationDetailsModal
        open={isModalOpen}
        organization={selectedOrganization}
        onClose={() => setIsModalOpen(false)}
      />
    </div>
  );
}
