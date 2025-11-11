import { DeleteOutlined, EditOutlined, ExclamationCircleOutlined, FilterOutlined } from "@ant-design/icons";
import {
  ProForm,
  ProFormSelect,
  ProFormText,
} from "@ant-design/pro-components";
import { Button, Card, message, Modal, Space, Table, Tag } from "antd";
import { useState } from "react";
import { useDeleteApiPractitionerId, useGetApiPractitioner } from "../../../../api/practitioner/practitioner";
import { useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router";

interface Practitioner {
  id: string;
  name: string;
  email: string;
  position: string;
  area: string;
  status: string;
}

export const PractitionersListForm = () => {
  const [searchName, setSearchName] = useState("");
  const [searchRole, setSearchRole] = useState<string | undefined>(undefined);
  const [searchArea, setSearchArea] = useState<string | undefined>(undefined);
  const [searchStatus, setSearchStatus] = useState<string | undefined>(undefined);

  const navigate = useNavigate();

  const handleNavigate = (id: string) => {
  navigate(`/practitioners/details/${id}`);
};

  const { data, isLoading, isError } = useGetApiPractitioner<{
  items: Practitioner[];
  pagination: {
    currentPage: number;
    hasNext: boolean;
    hasPrevious: boolean;
    pageSize: number;
    totalItems: number;
    totalPages: number;
  };
}>();

  const queryClient = useQueryClient();
  const deleteMutation = useDeleteApiPractitionerId({
  mutation: {
    onSuccess: () => {
      message.success("Empleado eliminado correctamente");
      queryClient.invalidateQueries({ queryKey: ["/api/Practitioner"] });
    },
    onError: (error) => {
      message.error("Error al eliminar el empleado");
      console.error(error);
    },
  },
});

const [modal, contextHolder] = Modal.useModal();

 const practitioners: Practitioner[] =
  data?.items?.map((p: any, index: number) => ({
    id: p.id ?? String(index + 1),
    name: p.name?.[0]?.text ?? "Sin nombre",
    email:
      p.telecom?.find(
        (t: any) => t.system?.toLowerCase() === "email"
      )?.value ?? "Sin correo",
    position: p.qualification?.[0]?.code?.text ?? "No especificado",
    area: p.address?.[0]?.text ?? "Sin área",
    status: p.active ? "Activo" : "Inactivo",
  })) ?? [];


    if (isLoading) return <p>Cargando empleados...</p>;
  if (isError) return <p>Error al cargar empleados.</p>;

  const filteredEmployees = practitioners.filter((e) => {
    const nameMatch = e.name.toLowerCase().includes(searchName.toLowerCase());
    const roleMatch = searchRole ? e.position === searchRole : true;
    const areaMatch = searchArea ? e.area === searchArea : true;
    const statusMatch = searchStatus ? e.status === searchStatus : true;
    return nameMatch && roleMatch && areaMatch && statusMatch;
  });

  const handleEdit = (practitioner: Practitioner) => {
  navigate(`/practitioners/edit/${practitioner.id}`);
};

  const handleDelete = (practitioner: Practitioner) => {
  modal.confirm({
  title: "¿Eliminar empleado?",
  icon: <ExclamationCircleOutlined />,
  content: `¿Estás seguro de que deseas eliminar a ${practitioner.name}? Esta acción no se puede deshacer.`,
  okText: "Eliminar",
  okType: "danger",
  cancelText: "Cancelar",
  onOk: async () => {
    try {
      await deleteMutation.mutateAsync({ id: practitioner.id });
    } catch (error) {
      message.error("No se pudo eliminar el empleado");
    }
  },
});
};

  const columns = [
    {
  title: "Nombre",
  dataIndex: "name",
  key: "name",
  render: (text: string, record: Practitioner) => (
    <span
      className="text-blue-600 hover:underline cursor-pointer"
      onClick={() => handleNavigate(record.id)}
    >
      {text}
    </span>
  ),
},
    {
      title: "Correo",
      dataIndex: "email",
      key: "email",
    },
    {
      title: "Puesto",
      dataIndex: "position",
      key: "position",
    },
    {
      title: "Área",
      dataIndex: "area",
      key: "area",
    },
    {
      title: "Estado",
      dataIndex: "status",
      key: "status",
      align: "center",
      render: (status: string) => (
        <Tag color={status === "Activo" ? "green" : "red"}>{status}</Tag>
      ),
    },
    {
  title: "Acciones",
  key: "actions",
  align: "center",
  render: (_: any, record: Practitioner) => (
    <Space>
      {/* Botón de editar */}
      <Button
        type="text"
        icon={<EditOutlined />}
        onClick={() => handleEdit(record)}
      />

      {/* Botón de eliminar */}
      <Button
        type="text"
        danger
        icon={<DeleteOutlined />}
        onClick={() => handleDelete(record)}
      />
    </Space>
  ),
},

  ];

  return (
    <div className="bg-[#FAFAFA] rounded-lg border-2 border-[#D9D9D9] p-6">
      {contextHolder}
    {/* Filtros */}
    <Card
      style={{ marginBottom: 16, borderRadius: 8 }}
      bodyStyle={{ padding: 24 }}
    >
      <div className="flex items-center gap-3 mb-6">
        <FilterOutlined className="text-blue-500 text-xl" />
        <span className="text-lg font-semibold text-[#333333]">
          Filtros de Búsqueda
        </span>
      </div>

      <ProForm submitter={false}>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
          <ProFormText
              name="name"
              label={<span className="text-[#616161] font-medium">Nombre</span>}
              placeholder="Buscar por nombre"
              fieldProps={{
                value: searchName,
                onChange: (e) => setSearchName(e.target.value),
              }}
            />
          <ProFormSelect
            name="position"
            label={<span className="text-[#616161] font-medium">Cargo</span>}
            options={[
              {
                label: "Auxiliar de Receptoría",
                value: "Auxiliar de Receptoría",
              },
              { label: "Médico", value: "Médico" },
              { label: "Enfermero", value: "Enfermero" },
            ]}
            fieldProps={{
              value: searchRole,
              onChange: (value) => setSearchRole(value),
            }}
          />
          <ProFormSelect
            name="area"
            label={
              <span className="text-[#616161] font-medium">
                Área Asistencial
              </span>
            }
            options={[
              { label: "Consulta Externa", value: "Consulta Externa" },
              { label: "Emergencia", value: "Emergencia" },
              { label: "Pediatría", value: "Pediatría" },
            ]}
            fieldProps={{
              value: searchArea,
              onChange: (value) => setSearchArea(value),
            }}
          />
          <ProFormSelect
            name="status"
            label={<span className="text-[#616161] font-medium">Estado</span>}
            options={[
              { label: "Activo", value: "Activo" },
              { label: "Inactivo", value: "Inactivo" },
              { label: "Bloqueado", value: "Bloqueado" },
            ]}
            fieldProps={{
              value: searchStatus,
              onChange: (value) => setSearchStatus(value),
            }}
          />
        </div>
      </ProForm>
    </Card>

    {/* Tabla */}
        <Card style={{ borderRadius: 8 }}>
          <Table
        columns={columns}
        dataSource={filteredEmployees}
        rowKey="id"
        loading={isLoading}
        pagination={{
          pageSize: 10,
          showSizeChanger: true,
          showQuickJumper: true,
          total: filteredEmployees.length,
          showTotal: (total, range) =>
            `${range[0]}-${range[1]} de ${total} empleados`,
        }}
        bordered
      />
        </Card>
        </div>
  );
};
