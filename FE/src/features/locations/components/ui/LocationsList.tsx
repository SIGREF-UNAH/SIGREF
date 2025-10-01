import React, { useState } from "react";
import { ProForm, ProFormText, ProFormSelect } from "@ant-design/pro-components";
import { Table, Card, Button, Space, Tag, message, Modal } from "antd";
import type { ColumnsType } from "antd/es/table";
import { FilterOutlined, EditOutlined, DeleteOutlined, BookOutlined, EyeOutlined } from "@ant-design/icons";
import { useGetApiLocations, useDeleteApiLocationsId, getGetApiLocationsQueryKey } from "../../../../api/locations/locations";
import { type LocationDto, LocationMode, LocationStatus } from "../../../../api/models";
import { BiChevronDown } from "react-icons/bi";
import { Link } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";

const LocationList: React.FC = () => {
  const queryClient = useQueryClient();
  const { data: locations, isLoading, isError } = useGetApiLocations({});
  const { mutate: deleteLocation } = useDeleteApiLocationsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: getGetApiLocationsQueryKey() });
        message.success("Ubicación eliminada exitosamente");
      },
      onError: () => message.error("Error al eliminar la ubicación"),
    },
  });
  const [searchName, setSearchName] = useState<string>("");
  const [searchMode, setSearchMode] = useState<LocationMode | undefined>(undefined);
  const [searchStatus, setSearchStatus] = useState<LocationStatus | undefined>(undefined);

  if (isError) {
    message.error("Error al cargar las ubicaciones");
    return <div>Error al cargar datos</div>;
  }

  // Filtrar datos localmente
  const filteredData = locations?.filter((loc) => {
    const nameMatch = loc.name.toLowerCase().includes(searchName.toLowerCase());
    const modeMatch = searchMode === undefined || loc.mode === searchMode;
    const statusMatch = searchStatus === undefined || loc.status === searchStatus;
    return nameMatch && modeMatch && statusMatch;
  }) || [];

  // Renderizar tag para status
  const renderStatusTag = (status: LocationStatus) => {
    const colorMap = {
      [LocationStatus.NUMBER_0]: "#52c41a",
      [LocationStatus.NUMBER_1]: "#faad14",
      [LocationStatus.NUMBER_2]: "#f5222d",
    };
    const labelMap = {
      [LocationStatus.NUMBER_0]: "Activo",
      [LocationStatus.NUMBER_1]: "Inactivo",
      [LocationStatus.NUMBER_2]: "Suspendido",
    };

    return <Tag color={colorMap[status]}>{labelMap[status]}</Tag>;
  };

  // Confirmación para eliminar
  const handleDelete = (id: string) => {
    Modal.confirm({
      title: "¿Estás seguro de eliminar esta ubicación?",
      content: "Esta acción no se puede deshacer.",
      okText: "Eliminar",
      okType: "danger",
      cancelText: "Cancelar",
      onOk: () => deleteLocation({ id: Number(id) }),
    });
  };

  const columns: ColumnsType<LocationDto> = [
    {
      title: <span style={{ textAlign: "center", display: "block" }}>Nombre</span>,
      dataIndex: "name",
      key: "name",
      width: 220,
      align: "center",
    },
    {
      title: <span style={{ textAlign: "center", display: "block" }}>Estado</span>,
      dataIndex: "status",
      key: "status",
      width: 150,
      align: "center",
      render: (status) => renderStatusTag(status as LocationStatus),
    },
    {
      title: <span style={{ textAlign: "center", display: "block" }}>Modo</span>,
      dataIndex: "mode",
      key: "mode",
      width: 150,
      align: "center",
      render: (mode) => (mode === LocationMode.NUMBER_0 ? "Kind" : "Instance"),
    },
    {
      title: <span style={{ textAlign: "center", display: "block" }}>Dirección</span>,
      key: "address",
      width: 250,
      align: "center",
      render: (_, record) => (Array.isArray(record.address?.line) ? record.address.line.join(", ") : record.address?.line || "-"),
    },
    {
      title: <span style={{ textAlign: "center", display: "block" }}>Organización responsable</span>,
      dataIndex: "managingOrganizationIds",
      key: "managingOrganizationIds",
      width: 200,
      align: "center",
      render: (org) => org || "-",
    },
    {
      title: <span style={{ textAlign: "center", display: "block" }}>Acciones</span>,
      key: "actions",
      width: 220,
      render: (_, record) => (
        <Space size="middle" style={{ display: "flex", justifyContent: "center" }}>
          <Link to={`/locations/details/${record.id}`}>
            <Button
              type="primary"
              size="small"
              icon={<EyeOutlined />}
              style={{ backgroundColor: "#1890ff", borderColor: "#1890ff" }}
            >
              Detalles
            </Button>
          </Link>
          <Link to={`/locations/edit/${record.id}`}>
            <Button
              type="primary"
              size="small"
              icon={<EditOutlined />}
              style={{ backgroundColor: "#52c41a", borderColor: "#52c41a" }}
            >
              Editar
            </Button>
          </Link>
          <Button
            type="primary"
            size="small"
            icon={<DeleteOutlined />}
            style={{ backgroundColor: "#f5222d", borderColor: "#f5222d" }}
            onClick={() => handleDelete(record.id!)}
          >
            Eliminar
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <div className="bg-[#FAFAFA] rounded-lg border-2 border-[#D9D9D9] p-6">
      {/* Filtros de Búsqueda */}
      <Card style={{ marginBottom: "16px", borderRadius: "8px" }} bodyStyle={{ padding: "24px" }}>
        <div className="flex items-center gap-3 mb-6">
          <FilterOutlined className="w-8 h-8 text-blue-500" />
          <span className="text-lg font-semibold text-[#333333]">
            Filtros de Búsqueda
          </span>
        </div>

        <ProForm
          submitter={false}
          onValuesChange={(changedValues) => {
            if (changedValues.name) setSearchName(changedValues.name);
            if (changedValues.mode !== undefined) setSearchMode(changedValues.mode);
            if (changedValues.status !== undefined) setSearchStatus(changedValues.status);
          }}
        >
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 sm:gap-8 md:gap-16 lg:gap-32">
            <ProFormText
              name="name"
              label={<span className="text-[#616161] font-medium">Nombre/Alias de la ubicación</span>}
              placeholder="Nombre/Alias ubicación"
              fieldProps={{
                value: searchName,
                onChange: (e) => setSearchName(e.target.value),
              }}
            />
            <ProFormSelect
              name="mode"
              label={<span className="text-[#616161] font-medium">Modo</span>}
              options={[
                { label: "Todos", value: undefined },
                { label: "Kind", value: LocationMode.NUMBER_0 },
                { label: "Instance", value: LocationMode.NUMBER_1 },
              ]}
              fieldProps={{
                value: searchMode,
                onChange: (value) => setSearchMode(value),
                suffixIcon: <BiChevronDown className="w-4 h-4 text-[#616161]" />,
              }}
            />
            <ProFormSelect
              name="status"
              label={<span className="text-[#616161] font-medium">Estado</span>}
              options={[
                { label: "Todos", value: undefined },
                { label: "Activo", value: LocationStatus.NUMBER_0 },
                { label: "Inactivo", value: LocationStatus.NUMBER_1 },
                { label: "Suspendido", value: LocationStatus.NUMBER_2 },
              ]}
              fieldProps={{
                value: searchStatus,
                onChange: (value) => setSearchStatus(value),
                suffixIcon: <BiChevronDown className="w-4 h-4 text-[#616161]" />,
              }}
            />
          </div>
        </ProForm>
      </Card>

      {/* Tabla de Registro de ubicaciones */}
      <Card style={{ borderRadius: "8px" }} bodyStyle={{ padding: "24px" }}>
        <div className="flex items-center gap-3 mb-6">
          <BookOutlined className="w-8 h-8 text-blue-500" />
          <span className="text-lg font-semibold text-[#333333]">
            Registro de ubicaciones
          </span>
        </div>

        <Table<LocationDto>
          columns={columns}
          dataSource={filteredData}
          rowKey="id"
          loading={isLoading}
          pagination={{
            pageSize: 10,
            total: filteredData.length,
            showSizeChanger: true,
            showQuickJumper: true,
            pageSizeOptions: ["10", "20", "50", "100"],
            showTotal: (total, range) => `${range[0]}-${range[1]} de ${total}`,
          }}
          scroll={{ x: "max-content" }}
          style={{ marginTop: "16px" }}
          bordered
        />
      </Card>
    </div>
  );
};

export default LocationList;