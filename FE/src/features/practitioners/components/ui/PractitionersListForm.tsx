import {
  DeleteOutlined,
  EditOutlined,
  // ExclamationCircleOutlined,
  FilterOutlined,
  UserOutlined,
} from "@ant-design/icons";
import {
  ProForm,
  ProFormSelect,
  ProFormText,
} from "@ant-design/pro-components";
import { Button, message, Popconfirm, Space, Table, Tag } from "antd";
import { useState } from "react";
import {
  useDeleteApiPractitionerId,
  useGetApiPractitioner,
} from "../../../../api/practitioner/practitioner";
import { useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router";
import { useGetApiLocations } from "../../../../api/locations/locations";
import { ROLE_OPTIONS } from "../../../../shared/constants/RolesConstants";

interface Practitioner {
  id: string;
  name: string;
  email: string;
  positionText: string;
  positionCode: string;
  area: string;
  status: string;
}

export const PractitionersListForm = () => {
  const navigate = useNavigate();
  const [searchName, setSearchName] = useState("");
  const [searchRole, setSearchRole] = useState<string | undefined>(undefined);
  const [searchArea, setSearchArea] = useState<string | undefined>(undefined);
  const [searchStatus, setSearchStatus] = useState<string | undefined>(undefined);
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

  // const [modal, contextHolder] = Modal.useModal();

  const practitioners: Practitioner[] =
    data?.items?.map((p: any, index: number) => {
      const role = p.roles?.[0]; // Tomar el primer rol asignado
      const positionCode = role?.code?.[0]?.coding?.[0]?.code ?? "sin-codigo";
      const positionText = role?.code?.[0]?.text ?? "Sin puesto";
      const area = role?.location?.[0]?.display ?? "Sin área";

      return {
        id: p.id ?? String(index + 1),
        name: p.name?.[0]?.text ?? "Sin nombre",
        email:
          p.telecom?.find((t: any) => t.system?.toLowerCase() === "email")
            ?.value ?? "Sin correo",
        positionText,
        positionCode,
        area,
        status: p.active ? "Activo" : "Inactivo",
      };
    }) ?? [];

  if (isLoading) return <p>Cargando empleados...</p>;
  if (isError) return <p>Error al cargar empleados.</p>;

  const filteredEmployees = practitioners.filter((e) => {
    const nameMatch = e.name.toLowerCase().includes(searchName.toLowerCase());
    const roleMatch = searchRole ? e.positionCode === searchRole : true;
    const areaMatch = searchArea ? e.area === searchArea : true;
    const statusMatch = searchStatus ? e.status === searchStatus : true;
    return nameMatch && roleMatch && areaMatch && statusMatch;
  });


  const handleEdit = (practitioner: Practitioner) => {
    navigate(`/practitioners/update/${practitioner.id}`);
  };

  // const handleDelete = (practitioner: Practitioner) => {
  //   modal.confirm({
  //     title: "¿Eliminar empleado?",
  //     icon: <ExclamationCircleOutlined />,
  //     content: `¿Estás seguro de que deseas eliminar a ${practitioner.name}? Esta acción no se puede deshacer.`,
  //     okText: "Eliminar",
  //     okType: "danger",
  //     cancelText: "Cancelar",
  //     onOk: async () => {
  //       try {
  //         await deleteMutation.mutateAsync({ id: practitioner.id });
  //       } catch (error) {
  //         message.error("No se pudo eliminar el empleado");
  //       }
  //     },
  //   });
  // };

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
      title: "Cargo",
      dataIndex: "positionText",
      key: "position",
    },
    {
      title: "Ubicación",
      dataIndex: "area",
      key: "area",
    },
    {
      title: "Estado",
      dataIndex: "status",
      key: "status",
      render: (status: string) => (
        <Tag color={status === "Activo" ? "green" : "red"}>{status === "Activo" ? "✓ Activo" : "✗ Inactivo"}</Tag>
      ),
    },
    {
  title: "Acciones",
  key: "actions",
  render: (_: any, record: Practitioner) => (
    <Space>
      {/* Botón de editar */}
      <Button
        type="text"
        icon={<EditOutlined />}
        onClick={() => handleEdit(record)}
      />

      {/* Botón de eliminar con Popconfirm */}
      <Popconfirm
        title={`¿Estás seguro de que deseas eliminar a ${record.name}? Esta acción no se puede deshacer.`}
        onConfirm={async () => {
          try {
            await deleteMutation.mutateAsync({ id: record.id });
          } catch (error) {
            message.error("No se pudo eliminar el empleado");
          }
        }}
        okText="Eliminar"
        okType="danger"
        cancelText="Cancelar"
      >
        <Button type="text" danger icon={<DeleteOutlined />} />
      </Popconfirm>
    </Space>
  ),
}

  ];

  

  const { data: locations } = useGetApiLocations<{ items: { name: string }[] }>();

  const locationOptions = locations?.items?.map((loc) => ({
    label: loc.name,
    value: loc.name,
  })) ?? [];

  return (
    <div className="primary-card">
      {/* {contextHolder} */}

      {/* Filtros */}
      <div>
        <div className="flex items-center gap-3 mb-4">
          <FilterOutlined className="text-primary! text-xl" />
          <span className="text-lg text-primary">
            Filtros de Búsqueda
          </span>
        </div>

        <ProForm submitter={false}>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
            <ProFormText
              name="name"
              label={
                <span className="text-general font-medium">
                  Nombre
                </span>
              }
              placeholder="Buscar por nombre"
              fieldProps={{
                value: searchName,
                onChange: (e) => setSearchName(e.target.value),
              }}
            />
            <ProFormSelect
              name="position"
              placeholder="Seleccionar"
              label={<span className="text-general font-medium">Cargo</span>}
              options={ROLE_OPTIONS.map((r) => ({ label: r.label, value: r.value }))}
              fieldProps={{
                value: searchRole,
                onChange: (value) => setSearchRole(value),
              }}
            />
            <ProFormSelect
              name="area"
              placeholder="Seleccionar"
              label={<span className="text-general font-medium">Ubicación</span>}
              options={locationOptions}
              fieldProps={{
                value: searchArea,
                onChange: (value) => setSearchArea(value),
              }}
            />
            <ProFormSelect
              name="status"
              placeholder="Seleccionar"
              label={
                <span className="text-general font-medium">
                  Estado
                </span>
              }
              options={[
                { label: "Activo", value: "Activo" },
                { label: "Inactivo", value: "Inactivo" },
              ]}
              fieldProps={{
                value: searchStatus,
                onChange: (value) => setSearchStatus(value),
              }}
            />
          </div>
        </ProForm>
      </div>

      {/* Tabla */}
      <div>
        <div className="flex items-center gap-3 mb-4 mt-2">
          <UserOutlined className="text-primary! text-xl" />
          <span className="text-lg text-primary">
            Lista de Empleados
          </span>
        </div>
        <Table
          columns={columns as any}
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
      </div>
    </div>
  );
};
