import React, { useState, useEffect } from "react";
import { Table, Card, Space, Tag, message, Button, Input, Select } from "antd";
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
  const [searchMode, setSearchMode] = useState<LocationMode | undefined>(
    undefined
  );
  const [searchStatus, setSearchStatus] = useState<LocationStatus | undefined>(
    undefined
  );
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [deleteModalVisible, setDeleteModalVisible] = useState(false);
  const [selectedLocation, setSelectedLocation] = useState<{
    id: number;
    name: string;
  } | null>(null);

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

  useEffect(() => {
    setCurrentPage(1);
  }, [appliedSearchName, searchMode, searchStatus]);

  if (isError) {
    return <div>Error al cargar datos</div>;
  }

  const handleDeleteClick = (id: number, name: string) => {
    setSelectedLocation({ id, name });
    setDeleteModalVisible(true);
  };

  const handleDelete = async (id: number) => {
    try {
      await deleteLocation({ id });
      return true;
    } catch {
      return false;
    }
  };

  const columns: ColumnsType<LocationDto> = [
    {
      title: (
        <span style={{ textAlign: "center", display: "block" }}>Nombre</span>
      ),
      dataIndex: "name",
      key: "name",
      width: 220,
      align: "center",
    },
    {
      title: (
        <span style={{ textAlign: "center", display: "block" }}>Estado</span>
      ),
      dataIndex: "status",
      key: "status",
      width: 150,
      align: "center",
      render: (status) => {
        const normalized = status.toLowerCase();
        if (normalized === "active") return <Tag color="#52c41a">Activo</Tag>;
        if (normalized === "suspended")
          return <Tag color="#faad14">Suspendido</Tag>;
        if (normalized === "inactive")
          return <Tag color="#f5222d">Inactivo</Tag>;
        return "-";
      },
    },
    {
      title: (
        <span style={{ textAlign: "center", display: "block" }}>Modo</span>
      ),
      dataIndex: "mode",
      key: "mode",
      width: 150,
      align: "center",
      render: (mode) => {
        if (mode.toLowerCase() === "instance") return "Instancia";
        if (mode.toLowerCase() === "kind") return "Tipo";
        return "-";
      },
    },
    {
      title: (
        <span style={{ textAlign: "center", display: "block" }}>Dirección</span>
      ),
      key: "address",
      width: 250,
      align: "center",
      render: (_, record) =>
        Array.isArray(record.address?.line)
          ? record.address.line.join(", ")
          : record.address?.line || "-",
    },
    {
      title: (
        <span style={{ textAlign: "center", display: "block" }}>
          Organización responsable
        </span>
      ),
      dataIndex: "managingOrganization",
      key: "managingOrganization",
      width: 220,
      align: "center",
      render: (text, record) => record.managingOrganization?.display || "-",
    },
    {
      title: (
        <span style={{ textAlign: "center", display: "block" }}>Acciones</span>
      ),
      key: "actions",
      width: 220,
      render: (_, record) => (
        <Space
          size="middle"
          style={{ display: "flex", justifyContent: "center" }}
        >
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
          <Link to={`/locations/update/${record.id}`}>
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
            onClick={() => handleDeleteClick(Number(record.id!), record.name)}
          >
            Eliminar
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <div className="primary-card">
      <div>
        <Table<LocationDto>
          title={() => (
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
          )}
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
