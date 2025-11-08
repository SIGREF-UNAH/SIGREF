import React, { useState, useEffect } from "react";
import { Table, Space, Tag, message, Button, Input, Select, Alert } from "antd";
import type { ColumnsType } from "antd/es/table";
import {
  FilterOutlined,
  EyeOutlined,
  EditOutlined,
  DeleteOutlined,
} from "@ant-design/icons";
import {
  useGetApiLocations,
  useDeleteApiLocationsId,
  getGetApiLocationsQueryKey,
} from "../../../../api/locations/locations";
import {
  type LocationDto,
  LocationMode,
  LocationStatus,
} from "../../../../api/models";
import { Link } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import DeleteLocationModal from "../modals/DeleteLocationModal";

const { Search } = Input;
const { Option } = Select;

const LocationList: React.FC = () => {
  const queryClient = useQueryClient();
  const [searchInputValue, setSearchInputValue] = useState<string>("");
  const [appliedSearchName, setAppliedSearchName] = useState<string>("");
  const [searchMode, setSearchMode] = useState<LocationMode | undefined>(undefined);
  const [searchStatus, setSearchStatus] = useState<LocationStatus | undefined>(undefined);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [deleteModalVisible, setDeleteModalVisible] = useState(false);
  const [selectedLocation, setSelectedLocation] = useState<{id: number;name: string;} | null>(null);

  // Parametros de la consulta
  const params = {
    ...(appliedSearchName && { Name: appliedSearchName }),
    ...(searchMode !== undefined && { Mode: searchMode }),
    ...(searchStatus !== undefined && { status: searchStatus }),
    pageNumber: currentPage,
    pageSize: pageSize,
  };

  const { data: locations, isLoading, isError } = useGetApiLocations(params);

  const { mutate: deleteLocation } = useDeleteApiLocationsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiLocationsQueryKey(params),
        });
        message.success("Ubicación eliminada exitosamente");
      },
      onError: () => message.error("Error al eliminar la ubicación"),
    },
  });

  // Actualizar filtros
  useEffect(() => {
    setCurrentPage(1);
  }, [appliedSearchName, searchMode, searchStatus]);

  
  // Abrir modal de eliminación
  const handleDeleteClick = (id: number, name: string) => {
    setSelectedLocation({ id, name });
    setDeleteModalVisible(true);
  };
  
  // Eliminar
  const handleDelete = async (id: number) => {
    try {
      await deleteLocation({ id });
      return true;
    } catch {
      return false;
    }
  };
  
  // Columnas de la tabla
  const columns: ColumnsType<LocationDto> = [
    {
      title: "Nombre",
      dataIndex: "name",
      key: "name",
      width: 200,
    },
    {
      title: "Dirección",
      key: "address",
      width: 200,
      render: (_, record) =>
        Array.isArray(record.address?.line)
          ? record.address.line.join(", ")
          : record.address?.line || "-",
    },
    {
      title: "Organización",
      dataIndex: "managingOrganization",
      key: "managingOrganization",
      width: 200,
      render: (_, record) => record.managingOrganization?.display || "-",
    },
    {
      title: "Modo",
      dataIndex: "mode",
      key: "mode",
      width: 120,
      render: (mode) => {
        if (mode.toLowerCase() === "instance") return "Instancia";
        if (mode.toLowerCase() === "kind") return "Tipo";
        return "-";
      },
    },
    {
      title: "Estado",
      dataIndex: "status",
      key: "status",
      width: 120,
      render: (status) => {
        const normalized = status.toLowerCase();
        if (normalized === "active") return <Tag color="green">✓ Activo</Tag>;
        if (normalized === "suspended") return <Tag color="orange">⚠︎ Suspendido</Tag>;
        if (normalized === "inactive") return <Tag color="red">✗ Inactivo</Tag>; 
        return "-";
      },
    },
    {
      title: "Acciones",
      key: "actions",
      width: 150,
      render: (_, record) => (
        <Space size="small">
          <Link to={`/locations/details/${record.id}`}>
            <Button
              type="text"
              icon={<EyeOutlined />}
              title="Ver detalles"
            ></Button>
          </Link>
          <Link to={`/locations/update/${record.id}`}>
            <Button type="text" icon={<EditOutlined />}></Button>
          </Link>
          <Button
            type="text"
            danger
            icon={<DeleteOutlined />}
            onClick={() => handleDeleteClick(Number(record.id!), record.name)}
          />
        </Space>
      ),
    },
  ];

  // Manejo de error
  if (isError) {
    return (
      <div>
        <Alert
          message="Error al cargar las ubicaciones"
          description="No se pudieron cargar las ubicaciones. Por favor, intente nuevamente."
          type="error"
          showIcon
        />
      </div>
    );
  }

  return (
    <div className="primary-card">
      <div>
        {/* Búsqueda y filtros */}
        <div className="flex justify-end gap-3 mb-4">
          <Search
            placeholder="Buscar por nombre/alias"
            allowClear
            style={{ width: 300 }}
            value={searchInputValue}
            onChange={(e) => setSearchInputValue(e.target.value)}
            onSearch={(value) => {
              setAppliedSearchName(value.trim());
            }}
          />
          <Select
            placeholder="Por modo"
            allowClear
            suffixIcon={<FilterOutlined />}
            style={{ width: 200, height: 36 }}
            value={searchMode}
            onChange={(value) => setSearchMode(value as LocationMode)}
          >
            <Option value={LocationMode.NUMBER_0}>Tipo</Option>
            <Option value={LocationMode.NUMBER_1}>Instancia</Option>
          </Select>
          <Select
            placeholder="Por estado"
            allowClear
            suffixIcon={<FilterOutlined />}
            style={{ width: 150, height: 36 }}
            value={searchStatus}
            onChange={(value) => setSearchStatus(value as LocationStatus)}
          >
            <Option value={LocationStatus.NUMBER_0}>Activo</Option>
            <Option value={LocationStatus.NUMBER_1}>Suspendido</Option>
            <Option value={LocationStatus.NUMBER_2}>Inactivo</Option>
          </Select>
        </div>
        {/* Lista de ubicaciones */}
        <Table
          columns={columns}
          dataSource={locations?.items || []}
          rowKey="id"
          loading={isLoading}
          pagination={{
            current: currentPage,
            pageSize: pageSize,
            total: locations?.pagination?.totalItems || 0,
            showSizeChanger: true,
            showQuickJumper: true,
            pageSizeOptions: ["10", "20", "50", "100"],
            showTotal: (total, range) => `${range[0]}-${range[1]} de ${total}`,
            onChange: (page, pageSize) => {
              setCurrentPage(page);
              setPageSize(pageSize);
            },
          }}
          scroll={{ x: "max-content" }}
          bordered
        />
      </div>

      {/* Modal de Eliminación */}
      <DeleteLocationModal
        visible={deleteModalVisible}
        onVisibleChange={setDeleteModalVisible}
        locationId={selectedLocation?.id || null}
        locationName={selectedLocation?.name}
        onDelete={handleDelete}
      />
    </div>
  );
};

export default LocationList;
