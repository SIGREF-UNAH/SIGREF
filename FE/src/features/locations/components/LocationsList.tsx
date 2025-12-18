import React, { useState, useEffect } from "react";
import {
  Table,
  Space,
  Tag,
  message,
  Button,
  Input,
  Select,
  Alert,
  Popconfirm,
} from "antd";
import {
  type LocationDto,
  LocationMode,
  LocationStatus,
} from "../../../api/models";
import {
  getGetApiLocationsQueryKey,
  useDeleteApiLocationsId,
  useGetApiLocations,
} from "../../../api/locations/locations";
import {
  FilterOutlined,
  EyeOutlined,
  EditOutlined,
  DeleteOutlined,
} from "@ant-design/icons";
import type { ColumnsType } from "antd/es/table";
import { Link } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import { Can } from "@casl/react";
import { useAbility } from "../../../config";

const { Search } = Input;
const { Option } = Select;

export const LocationList: React.FC = () => {
  const queryClient = useQueryClient();
  const ability = useAbility();
  const [searchInputValue, setSearchInputValue] = useState<string>("");
  const [appliedSearchName, setAppliedSearchName] = useState<string>("");
  const [searchMode, setSearchMode] = useState<LocationMode | undefined>(undefined);
  const [searchStatus, setSearchStatus] = useState<LocationStatus | undefined>(undefined);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

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
        message.success("Ubicación eliminada correctamente");
      },
      onError: () => message.error("Error al eliminar la ubicación"),
    },
  });

  // Actualizar filtros
  useEffect(() => {
    setCurrentPage(1);
  }, [appliedSearchName, searchMode, searchStatus]);

  // Eliminar ubicación
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
        if (normalized === "suspended")
          return <Tag color="orange">⚠︎ Suspendido</Tag>;
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
          <Can I="read" a="locations" ability={ability}>
            <Link to={`/locations/details/${record.id}`}>
              <Button
                type="text"
                icon={<EyeOutlined />}
                title="Ver detalles"
              ></Button>
            </Link>
          </Can>
          <Can I="update" a="locations" ability={ability}>
            <Link to={`/locations/update/${record.id}`}>
              <Button type="text" icon={<EditOutlined />}></Button>
            </Link>
          </Can>
          <Can I="delete" a="locations" ability={ability}>
            <Popconfirm
              title="¿Eliminar ubicación?"
              description="Esta acción no se puede deshacer"
              onConfirm={() => handleDelete(Number(record.id!))}
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
            style={{ width: 200 }}
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
            style={{ width: 150 }}
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
    </div>
  );
};

export default LocationList;
