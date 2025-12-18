import {
  EditOutlined,
  DeleteOutlined,
  EyeOutlined,
  FilterOutlined,
} from "@ant-design/icons";
import { Table, Button, Input, Space, Popconfirm, Tag, Select } from "antd";
import { useOrganizationsList } from "../hooks";
import type { OrganizationDto } from "../../../api/models";
import { OrganizationDetailsModal } from "./OrganizationsDetailsModal";
import { useAbility } from "../../../config";
import { Can } from "@casl/react";

const { Search } = Input;

export default function OrganizationsList() {
  const {
    organizations,
    filters,
    paginationConfig,
    isLoading,
    isModalOpen,
    selectedOrganization,
    searchInput,
    handleEdit,
    handleDelete,
    handleViewDetails,
    setIsModalOpen,
    handleSearchInputChange,
    handleSearch,
    handleTypeChange,
    handleStatusChange,
  } = useOrganizationsList();

  const ability = useAbility();

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
          : identifier || "N/A",
    },
    {
      title: "Tipo",
      dataIndex: "type",
      key: "type",
      width: 300,
      render: (types: any) => {
    if (Array.isArray(types) && types.length > 0) {
      return types.map(type => {
        return type?.text?.value || 
               type?.coding?.[0]?.display?.value || 
               type?.text || 
               type?.coding?.[0]?.display || 
               "N/A";
      }).join(', ');
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
      render: (record : OrganizationDto) => (
        <Space size="small">
          <Can I="read" a="organizations" ability={ability}>
            <Button
              onClick={() => handleViewDetails(record)}
              type="text"
              icon={<EyeOutlined />}
              title="Ver detalles"
            ></Button>
          </Can>
          <Can I="update" a="organizations" ability={ability}>
            <Button
              type="text"
              icon={<EditOutlined />}
              onClick={() => handleEdit(record.id || "")}
            ></Button>
          </Can>
          <Can I="delete" a="organizations" ability={ability}>
            <Popconfirm
              title="¿Eliminar organización?"
              description="Esta acción no se puede deshacer"
              onConfirm={() => handleDelete(record?.id || "")}
              okText="Sí, eliminar"
              cancelText="Cancelar"
              okButtonProps={{ danger: true }}
            >
              <Button type="link" danger icon={<DeleteOutlined />}></Button>
            </Popconfirm>
          </Can>
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
          style={{ width: 200 }}
          value={filters.type}
          onChange={handleTypeChange}
          options={[
            { label: "Proveedor de salud", value: "Provider" },
            { label: "Departamento", value: "Department" },
            { label: "Equipo", value: "Team" },
            { label: "Gobierno", value: "Government" },
            { label: "Aseguradora", value: "Insurer" },
            { label: "Pagador", value: "Payer" },
            { label: "Educativo", value: "Educational" },
            { label: "Religioso", value: "Regligious" },
            { label: "Investigación clínica", value: "ClinicalResearchSponsor" },
            { label: "Comunidad", value: "CommunityGroup" },
            { label: "Negocio no médico", value: "NonHealthcareBusiness" },
            // { label: "Otro", value: "Other" },
          ]}
        />

        {/* Filtro por estado */}
        <Select
          placeholder="Por Estado"
          allowClear
          suffixIcon={<FilterOutlined />}
          style={{ width: 150 }}
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
