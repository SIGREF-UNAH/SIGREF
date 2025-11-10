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
  console.log(organizations);

  const columns = [
    {
      title: "Nombre",
      dataIndex: "name",
      key: "name",
    },
    {
      title: "Identificador",
      dataIndex: "identifiers",
      key: "identifier",
      render: (identifiers: any[]) =>
        Array.isArray(identifiers)
          ? identifiers[0]?.value || "N/A"
          : identifiers?.value || "N/A",
    },
    {
      title: "Tipo",
      dataIndex: "types",
      key: "type",
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
      key: "active",
      render: (active: boolean) => (
        <Tag color={active ? "green" : "red"}>
          {active ? "Activo" : "Inactivo"}
        </Tag>
      ),
    },
    {
      title: "Descripción",
      dataIndex: "description",
      key: "description",
      render: (text: string) => (
        <div
          className="whitespace-normal break-words max-w-xs overflow-hidden text-ellipsis"
          title={text} 
        >
          {text}
        </div>
      ),
    },
    {
      title: "Acciones",
      key: "actions",
      render: (_, record) => (
        <Space size="small">
          {/* ver detalles de organizacion */}
          <Button
            type="link"
            icon={<EyeOutlined className="!text-black" />}
            onClick={() => handleViewDetails(record)}
          ></Button>
          <Button
            type="link"
            icon={<EditOutlined className="!text-black" />}
            onClick={() => handleEdit(record.id)}
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
    <div className="bg-white rounded-lg border border-gray-200 p-6 shadow-sm">
      <div className="flex justify-end gap-3 mb-4">
        {/* Búsqueda por nombre */}
        <Input
          placeholder="Buscar por nombre"
          value={searchInput}
          onChange={(e) => handleSearchInputChange(e.target.value)}
          onPressEnter={handleSearch}
          allowClear
          style={{ width: 200 }}
          suffix={
            <SearchOutlined
              onClick={handleSearch}
              style={{ cursor: "pointer", color: "#black" }}
            />
          }
        />
        {/* Filtro por ubicación/tipo */}
        <Select
          placeholder="Por Ubicación"
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

      <Table
        columns={columns}
        dataSource={organizations}
        rowKey="id"
        loading={isLoading}
        pagination={paginationConfig}
      />
      <OrganizationDetailsModal
        open={isModalOpen}
        organization={selectedOrganization}
        onClose={() => setIsModalOpen(false)}
      />
    </div>
  );
}
