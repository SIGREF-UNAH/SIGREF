import { Table, Button, Input, Space, Popconfirm, message, Tag } from "antd";
import { EditOutlined, DeleteOutlined, PlusOutlined, SearchOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import { useDeleteApiOrganizationsId } from "../../../../api/organizations/organizations";
import { useQueryClient } from "@tanstack/react-query";
import type { OrganizationDto } from "../../../../api/models";
import useOrganizationList from "../../hooks/useListOrganization";

export default function OrganizationsList() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const {
    organizations,
    total,
    page,
    pageSize,
    isLoading,
    handlePageChange,
    handleSearch,
  } = useOrganizationList();

  const { mutateAsync: deleteOrganization } = useDeleteApiOrganizationsId();

  const handleDelete = async (id: string) => {
    try {
      await deleteOrganization({ id });
      message.success("Organización eliminada exitosamente");
      queryClient.invalidateQueries({ queryKey: ["/api/Organizations"] });
    } catch (error) {
      message.error("Error al eliminar la organización");
    }
  };

  const columns = [
    {
      title: "Nombre",
      dataIndex: "name",
      key: "name",
      width: "25%",
    },
    {
      title: "Identificador",
      dataIndex: "identifier",
      key: "identifier",
      width: "15%",
      render: (identifiers: any[]) => identifiers?.[0]?.value || "N/A",
    },
    {
      title: "Tipo",
      dataIndex: "type",
      key: "type",
      width: "15%",
      render: (types: any[]) => types?.[0]?.text || "N/A",
    },
    {
      title: "Estado",
      dataIndex: "active",
      key: "active",
      width: "10%",
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
      width: "25%",
      ellipsis: true,
      render: (text: string) => text || "Sin descripción",
    },
    {
      title: "Acciones",
      key: "actions",
      width: "10%",
      render: (_: any, record: OrganizationDto) => (
        <Space size="small">
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => navigate(`/organizations/update/${record.id}`)}
          >
            Editar
          </Button>
          <Popconfirm
            title="¿Eliminar organización?"
            description="Esta acción no se puede deshacer"
            onConfirm={() => handleDelete(record.id!)}
            okText="Sí"
            cancelText="No"
          >
            <Button type="link" danger icon={<DeleteOutlined />}>
              Eliminar
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div className="bg-white rounded-lg border border-gray-200 p-6 shadow-sm">
      <div className="flex justify-between items-center mb-6">
        <Input
          placeholder="Buscar organizaciones..."
          prefix={<SearchOutlined />}
          style={{ width: 300 }}
          onChange={(e) => handleSearch(e.target.value)}
          allowClear
        />
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => navigate("/organizations/create")}
        >
          Nueva Organización
        </Button>
      </div>

      <Table
        columns={columns}
        dataSource={organizations}
        rowKey="id"
        loading={isLoading}
        pagination={{
          current: page,
          pageSize: pageSize,
          total: total,
          showSizeChanger: true,
          showTotal: (total) => `Total: ${total} organizaciones`,
          onChange: handlePageChange,
        }}
      />
    </div>
  );
}