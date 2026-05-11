import { useState, useEffect, useCallback } from "react";
import {
  Table,
  Space,
  Tag,
  Button,
  Input,
  Select,
  Alert,
  Popconfirm,
} from "antd";
import type { LocationDto } from "../../../api/models";
import {
  LocationMode,
  LocationStatus,
} from "../../../api/models";
import {
  getGetLocationListQueryKey,
  useDeleteLocationById,
  useGetLocationList,
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
import { useAbility } from "../../../config";
import { useMessage } from "../../../shared/hooks";

const { Search } = Input;

export const LocationList = () => {
  const queryClient = useQueryClient();
  const ability = useAbility();
  const msg = useMessage();

  // Estados de búsqueda y filtros
  const [searchInputValue, setSearchInputValue] = useState<string>("");
  const [appliedSearchName, setAppliedSearchName] = useState<string>("");
  const [searchMode, setSearchMode] = useState<string | undefined>(undefined);
  const [searchStatus, setSearchStatus] = useState<string | undefined>(undefined);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  // Construir parámetros de forma estable
  const params = useCallback(() => {
    const p: Record<string, any> = {
      PageNumber: currentPage,
      PageSize: pageSize,
    };
    if (appliedSearchName) p.Name = appliedSearchName;
    if (searchMode !== undefined) p.Mode = searchMode;
    if (searchStatus !== undefined) p.Status = searchStatus;
    return p;
  }, [appliedSearchName, searchMode, searchStatus, currentPage, pageSize]);

  const { data: locations, isLoading, isError } = useGetLocationList(params());

  // Mutación para eliminar
  const { mutateAsync: deleteLocation } = useDeleteLocationById({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetLocationListQueryKey(),
        });
        msg.success("Ubicación eliminada correctamente");
      },
      onError: (error: any) => {
        const errorMessage =
          error?.response?.data?.detail ||
          error?.response?.data?.title ||
          "Error al eliminar la ubicación";
        msg.error(errorMessage);
      },
    },
  });

  // Resetear página al cambiar filtros
  useEffect(() => {
    setCurrentPage(1);
  }, [appliedSearchName, searchMode, searchStatus]);

  // Eliminar ubicación
  const handleDelete = async (id: string) => {
    await deleteLocation({ id });
  };

  // Columnas de la tabla
  const columns: ColumnsType<LocationDto> = [
    {
      title: "Nombre",
      dataIndex: "name",
      key: "name",
      width: 200,
      ellipsis: true,
    },
    {
      title: "Dirección",
      key: "address",
      width: 200,
      ellipsis: true,
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
      ellipsis: true,
      render: (_, record) => record.managingOrganization?.display || "-",
    },
    {
      title: "Modo",
      dataIndex: "mode",
      key: "mode",
      width: 120,
      render: (mode: string | undefined) => {
        if (!mode) return <Tag>-</Tag>;
        if (mode === LocationMode.instance)
          return <Tag color="blue">Instancia</Tag>;
        if (mode === LocationMode.kind)
          return <Tag color="purple">Tipo</Tag>;
        return <Tag>{mode}</Tag>;
      },
    },
    {
      title: "Estado",
      dataIndex: "status",
      key: "status",
      width: 120,
      render: (status: string | undefined) => {
        if (!status) return <Tag>-</Tag>;
        if (status === LocationStatus.active)
          return <Tag color="green">Activo</Tag>;
        if (status === LocationStatus.suspended)
          return <Tag color="orange">Suspendido</Tag>;
        if (status === LocationStatus.inactive)
          return <Tag color="red">Inactivo</Tag>;
        return <Tag>{status}</Tag>;
      },
    },
    {
      title: "Acciones",
      key: "actions",
      width: 150,
      fixed: "right",
      render: (_, record) => (
        <Space size="small">
          {ability.can("read", "locations") && (
            <Link to={`/locations/details/${record.id}`}>
              <Button
                type="text"
                icon={<EyeOutlined />}
                title="Ver detalles"
              />
            </Link>
          )}
          {ability.can("update", "locations") && (
            <Link to={`/locations/update/${record.id}`}>
              <Button
                type="text"
                icon={<EditOutlined />}
                title="Editar ubicación"
              />
            </Link>
          )}
          {ability.can("delete", "locations") && (
            <Popconfirm
              title="¿Eliminar ubicación?"
              description="Esta acción no se puede deshacer"
              onConfirm={() => handleDelete(record.id!)}
              okText="Sí, eliminar"
              cancelText="Cancelar"
              okButtonProps={{ danger: true }}
            >
              <Button
                type="text"
                danger
                icon={<DeleteOutlined />}
                title="Eliminar ubicación"
              />
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  // Pantalla de error
  if (isError) {
    return (
      <div className="primary-card">
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
      {/* Búsqueda y filtros */}
      <div className="flex flex-wrap justify-end gap-3 mb-4">
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
          onChange={(value) => setSearchMode(value)}
          options={[
            { label: "Tipo", value: LocationMode.kind },
            { label: "Instancia", value: LocationMode.instance },
          ]}
        />
        <Select
          placeholder="Por estado"
          allowClear
          suffixIcon={<FilterOutlined />}
          style={{ width: 150 }}
          value={searchStatus}
          onChange={(value) => setSearchStatus(value)}
          options={[
            { label: "Activo", value: LocationStatus.active },
            { label: "Suspendido", value: LocationStatus.suspended },
            { label: "Inactivo", value: LocationStatus.inactive },
          ]}
        />
      </div>

      {/* Tabla de ubicaciones */}
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
          onChange: (page, size) => {
            setCurrentPage(page);
            setPageSize(size);
          },
        }}
        scroll={{ x: "max-content" }}
        bordered
        locale={{
          emptyText: "No se encontraron ubicaciones",
        }}
      />
    </div>
  );
};

export default LocationList;