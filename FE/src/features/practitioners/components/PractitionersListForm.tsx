import {
  AppstoreOutlined,
  DeleteOutlined,
  EditOutlined,
  ExclamationCircleOutlined,
  EyeOutlined,
  FilterOutlined,
  ManOutlined,
  QuestionCircleOutlined,
  UserOutlined,
  WomanOutlined,
} from "@ant-design/icons";
import {
  ProForm,
  ProFormSelect,
  ProFormText,
} from "@ant-design/pro-components";
import {
  Alert,
  Button,
  message,
  Popconfirm,
  Space,
  Spin,
  Table,
  Tag,
  Tooltip,
} from "antd";
import { useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router";
import {
  useDeletePractitionerById,
  useGetPractitionerList,
  getGetPractitionerListQueryKey,
} from "@endpoints/practitioners/practitioners";
import { useGetLocationList } from "@endpoints/locations/locations";
import { ROLE_OPTIONS } from "../../../shared/constants/RolesConstants";
import { useAbility } from "../../../config";
import { Can } from "@casl/react";
import type { NullableOfAdministrativeGender as AdministrativeGender } from "@types/shared";

interface Practitioner {
  id: string;
  name: string;
  email: string;
  positionText: string;
  positionCode: string;
  area: string;
  status: string;
  gender?: number; // 0: Desconocido, 1: Masculino, 2: Femenino, 3: Otro
}

const GENDER_API_MAP: Record<number, AdministrativeGender> = {
  0: "unknown",
  1: "male",
  2: "female",
  3: "other",
};

export const PractitionersListForm = () => {
  const navigate = useNavigate();
  const ability = useAbility();
  const queryClient = useQueryClient();
  const [searchName, setSearchName] = useState("");
  const [searchRole, setSearchRole] = useState<string | undefined>(undefined);
  const [searchArea, setSearchArea] = useState<string | undefined>(undefined);
  const [searchGender, setSearchGender] = useState<number | undefined>(
    undefined,
  );
  const [searchStatus, setSearchStatus] = useState<string | undefined>(
    undefined,
  );
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const { data: locations } = useGetLocationList<{
    items: { name: string }[];
  }>();

  const locationOptions =
    locations?.items?.map((loc) => ({
      label: loc.name,
      value: loc.name,
    })) ?? [];

  const handleNavigate = (id: string) => {
    navigate(`/practitioners/details/${id}`);
  };

  const activeFilter =
    searchStatus === "Activo"
      ? true
      : searchStatus === "Inactivo"
        ? false
        : undefined;
  const genderFilter =
    searchGender !== undefined ? GENDER_API_MAP[searchGender] : undefined;

  const queryParams = {
    Name: searchName || undefined,
    Active: activeFilter,
    Gender: genderFilter,
    PageNumber: pageNumber,
    PageSize: pageSize,
  };

  const { data, isLoading, isError } = useGetPractitionerList<{
    items: Practitioner[];
    pagination: {
      currentPage: number;
      hasNext: boolean;
      hasPrevious: boolean;
      pageSize: number;
      totalItems: number;
      totalPages: number;
    };
  }>(queryParams);

  const deleteMutation = useDeletePractitionerById({
    mutation: {
      onSuccess: () => {
        message.success("Empleado eliminado correctamente");
        queryClient.invalidateQueries({
          queryKey: getGetPractitionerListQueryKey(queryParams),
        });
      },
      onError: (error) => {
        message.error("Error al eliminar el empleado");
        console.error(error);
      },
    },
  });

  const GENDER_CONFIG: Record<
    number,
    { text: string; icon: React.ReactNode; color: string }
  > = {
    1: { text: "Masculino", icon: <ManOutlined />, color: "blue" },
    2: { text: "Femenino", icon: <WomanOutlined />, color: "magenta" },
    3: { text: "Otro", icon: <AppstoreOutlined />, color: "purple" },
    0: { text: "Desconocido", icon: <QuestionCircleOutlined />, color: "default" },
  };

  const practitioners: Practitioner[] =
    data?.items?.map((p: any, index: number) => {
      const role = p.roles?.[0]; // Tomar el primer rol asignado
      const positionCode = role?.code?.[0]?.coding?.[0]?.code ?? "-";
      const positionText = role?.code?.[0]?.text ?? "-";
      const area = role?.location?.[0]?.display ?? "-";

      return {
        id: p.id ?? String(index + 1),
        name: p.name?.[0]?.text ?? "-",
        email:
          p.telecom?.find((t: any) => t.system?.toLowerCase() === "email")
            ?.value ?? "-",
        positionText,
        positionCode,
        area,
        status: p.active ? "Activo" : "Inactivo",
        gender: p.gender,
      };
    }) ?? [];

  if (isLoading)
    return (
      <div className="flex items-center justify-center h-screen">
        <Spin size="large" />
      </div>
    );
  if (isError)
    return (
      <div className="flex items-center justify-center h-screen">
        <Alert message="Error al cargar empleados" type="error" showIcon />
      </div>
    );

  // Name, Active, Gender are filtered server-side; Role and Area are client-side only
  const filteredEmployees = practitioners.filter((e) => {
    const roleMatch = searchRole ? e.positionCode === searchRole : true;
    const areaMatch = searchArea ? e.area === searchArea : true;
    return roleMatch && areaMatch;
  });

  const handleEdit = (practitioner: Practitioner) => {
    navigate(`/practitioners/update/${practitioner.id}`);
  };

  const columns = [
    {
      title: "Nombre",
      dataIndex: "name",
      key: "name",
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
        <Tag color={status === "Activo" ? "green" : "red"}>
          {status === "Activo" ? "✓ Activo" : "✗ Inactivo"}
        </Tag>
      ),
    },
    {
      title: "Género",
      dataIndex: "gender",
      key: "gender",
      align: "center",
      render: (genderCode: number) => {
        const config = GENDER_CONFIG[genderCode] || GENDER_CONFIG[0];

        return (
          <Tooltip title={config.text}>
            <Tag
              color={config.color}
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                borderRadius: "50%",
                width: "32px",
                height: "32px",
                fontSize: "16px",
                margin: "0 auto",
              }}
            >
              {config.icon}
            </Tag>
          </Tooltip>
        );
      },
    },
    {
      title: "Acciones",
      key: "actions",
      align: "center",
      render: (_: any, record: Practitioner) => (
        <Space>
          <Can I="read" a="practitioners" ability={ability}>
            <Button
              type="text"
              icon={<EyeOutlined />}
              onClick={() => handleNavigate(record.id)}
              title="Ver detalles"
            />
          </Can>

          <Can I="update" a="practitioners" ability={ability}>
            <Button
              type="text"
              icon={<EditOutlined />}
              onClick={() => handleEdit(record)}
            />
          </Can>

          <Can I="delete" a="practitioners" ability={ability}>
            <Popconfirm
              title={`Eliminar a ${record.name}`}
              description={
                <div className="max-w-xs">
                  <p className="mb-2">
                    ¿Está seguro de que desea eliminar a este empleado?
                  </p>
                  <p className="text-gray-500 text-sm">
                    Esta acción no se puede deshacer.
                  </p>
                </div>
              }
              onConfirm={async () => {
                try {
                  await deleteMutation.mutateAsync({ id: record.id });
                } catch (error) {
                  message.error("No se pudo eliminar el empleado");
                }
              }}
              okText="Sí, eliminar"
              cancelText="Cancelar"
              okButtonProps={{
                danger: true,
              }}
              icon={<ExclamationCircleOutlined style={{ color: "red" }} />}
            >
              <Button type="text" danger icon={<DeleteOutlined />} />
            </Popconfirm>
          </Can>
        </Space>
      ),
    },
  ];

  return (
    <div className="primary-card">
      {/* {contextHolder} */}

      {/* Filtros */}
      <div>
        <div className="flex items-center gap-3 mb-4">
          <FilterOutlined className="text-primary! text-xl" />
          <span className="text-lg text-primary">Filtros de Búsqueda</span>
        </div>

        <ProForm submitter={false}>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
            <ProFormText
              name="name"
              label={<span className="text-general font-medium">Nombre</span>}
              placeholder="Buscar por nombre"
              fieldProps={{
                value: searchName,
                onChange: (e) => {
                  setSearchName(e.target.value);
                  setPageNumber(1);
                },
              }}
            />
            <ProFormSelect
              name="position"
              placeholder="Seleccionar"
              label={<span className="text-general font-medium">Cargo</span>}
              options={ROLE_OPTIONS.map((r) => ({
                label: r.label,
                value: r.value,
              }))}
              fieldProps={{
                value: searchRole,
                onChange: (value) => {
                  setSearchRole(value);
                  setPageNumber(1);
                },
              }}
            />
            <ProFormSelect
              name="area"
              placeholder="Seleccionar"
              label={
                <span className="text-general font-medium">Ubicación</span>
              }
              options={locationOptions}
              fieldProps={{
                value: searchArea,
                onChange: (value) => {
                  setSearchArea(value);
                  setPageNumber(1);
                },
              }}
            />
            <ProFormSelect
              name="gender"
              placeholder="Seleccionar"
              label={<span className="text-general font-medium">Género</span>}
              options={[
                { label: "Desconocido", value: 0 },
                { label: "Masculino", value: 1 },
                { label: "Femenino", value: 2 },
                { label: "Otro", value: 3 },
              ]}
              fieldProps={{
                value: searchGender,
                onChange: (value) => {
                  setSearchGender(value);
                  setPageNumber(1);
                },
              }}
            />
            <ProFormSelect
              name="status"
              placeholder="Seleccionar"
              label={<span className="text-general font-medium">Estado</span>}
              options={[
                { label: "Activo", value: "Activo" },
                { label: "Inactivo", value: "Inactivo" },
              ]}
              fieldProps={{
                value: searchStatus,
                onChange: (value) => {
                  setSearchStatus(value);
                  setPageNumber(1);
                },
              }}
            />
          </div>
        </ProForm>
      </div>

      {/* Tabla */}
      <div>
        <div className="flex items-center gap-3 mb-4 mt-2">
          <UserOutlined className="text-primary! text-xl" />
          <span className="text-lg text-primary">Lista de Empleados</span>
        </div>
        <Table
          columns={columns as any}
          dataSource={filteredEmployees}
          rowKey="id"
          loading={isLoading}
          pagination={{
            current: pageNumber,
            pageSize: pageSize,
            total: data?.pagination?.totalItems ?? 0,
            showSizeChanger: true,
            showQuickJumper: true,
            pageSizeOptions: ["5", "10", "20", "50"],
            onChange: (page, size) => {
              setPageNumber(page);
              setPageSize(size);
            },
            showTotal: (total, range) =>
              `${range[0]}-${range[1]} de ${total} empleados`,
          }}
          bordered
        />
      </div>
    </div>
  );
};
