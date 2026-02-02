import { ProDescriptions } from "@ant-design/pro-components";
import type { ProDescriptionsItemProps } from "@ant-design/pro-components";
import {
  Tag,
  Typography,
  Space,
  Button,
  Spin,
  Alert,
  Divider,
  Popconfirm,
} from "antd";
import {
  EditOutlined,
  DeleteOutlined,
  EnvironmentOutlined,
  LeftOutlined,
} from "@ant-design/icons";
import { PageHeaderTabs } from "../../../shared/components";
import { useServiceGroupDetails, useServiceGroupsList } from "../hooks";
import { getListStatusLabel, getListStatusColor } from "../../../shared/utils";

const { Title, Text } = Typography;

export const ServiceGroupDetailsPage = () => {
  const {
    data: serviceGroup,
    isLoading,
    error,
    ability,
    navigate,
  } = useServiceGroupDetails();
  const { handleDelete } = useServiceGroupsList();

  // Pantalla de carga
  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-100">
        <Spin size="large" />
      </div>
    );
  }

  // Pantalla de error
  if (error || !serviceGroup) {
    return (
      <div>
        <Alert
          message="Error al cargar el paquete de servicios"
          description="No se pudió cargar el paquete de servicios. Por favor, intente nuevamente."
          type="error"
          showIcon
        />
      </div>
    );
  }

  // Columnas de la tabla
  const columns: ProDescriptionsItemProps[] = [
    {
      title: "Nombre",
      dataIndex: "title",
      copyable: false,
      ellipsis: true,
      render: () => (
        <Text strong className="text-base">
          {serviceGroup.title || "Sin título"}
        </Text>
      ),
    },
    {
      title: "Descripción",
      dataIndex: "description",
      ellipsis: true,
      render: () =>
        serviceGroup.description || (
          <Text type="secondary">Sin descripción</Text>
        ),
    },
    {
      title: "Abreviatura",
      dataIndex: "abbreviation",
      key: "abbreviation",
      render: () =>
        serviceGroup.code?.coding?.[0]?.code ? (
          <Text>{serviceGroup.code?.coding?.[0]?.code}</Text>
        ) : (
          <Text type="secondary">Sin abreviatura</Text>
        ),
    },
    {
      title: "Precio Total",
      dataIndex: "totalPrice",
      render: () => (
        <Text strong className="text-base">
          L{" "}
          {(serviceGroup.totalPrice || 0).toLocaleString("es-HN", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
          })}
        </Text>
      ),
    },
    {
      title: "Estado",
      dataIndex: "status",
      key: "status",
      render: () => {
        const color = getListStatusColor(serviceGroup.status);
        const label = getListStatusLabel(serviceGroup.status);
        return <Tag color={color}>{label || "N/A"}</Tag>;
      },
    },
    {
      title: "Fecha de Creación",
      dataIndex: "date",
      render: () =>
        serviceGroup.date ? (
          new Date(serviceGroup.date).toLocaleDateString("es-HN", {
            year: "numeric",
            month: "long",
            day: "numeric",
          })
        ) : (
          <Text type="secondary">Sin fecha</Text>
        ),
    },
  ];

  return (
    <div>
      {/* Navegación */}
      <PageHeaderTabs
        title="Gestión de Paquetes"
        tabs={[
          ...(ability.can("read", "healthcares")
            ? [
                {
                  key: "listar1",
                  label: "Lista de Servicios",
                  path: "/healthcares/list",
                },
              ]
            : []),
          ...(ability.can("create", "healthcares")
            ? [
                {
                  key: "crear1",
                  label: "Crear Servicio",
                  path: "/healthcares/create",
                },
              ]
            : []),
          ...(ability.can("read", "service-groups")
            ? [
                {
                  key: "listar2",
                  label: "Lista de Paquetes",
                  path: "/service-groups/list",
                },
              ]
            : []),
          ...(ability.can("create", "service-groups")
            ? [
                {
                  key: "crear2",
                  label: "Crear Paquete",
                  path: "/service-groups/create",
                },
              ]
            : []),
        ]}
        defaultActive="null"
      />

      {/* Contenido */}
      <div className="primary-card">
        {/* Encabezado y botones */}
        <div className="flex flex-col mb-3 sm:flex-row sm:items-center sm:justify-between gap-4">
          <Title level={3}>
            Información General
          </Title>
          {ability.can("update", "service-groups") && (
            <Space>
              {/* Editar */}
              <Button 
                onClick={() => navigate(`/service-groups/update/${serviceGroup.id}`)} 
                type="primary" 
                icon={<EditOutlined />}
              >
                Editar
              </Button>
              {/* Eliminar */}
              {ability.can("delete", "service-groups") && (
                <Popconfirm
                  title="Eliminar paquete"
                  description="¿Desea eliminar este paquete?"
                  onConfirm={async () => {
                    await handleDelete(serviceGroup.id || "");
                    navigate("/service-groups/list");
                  }}
                  okText="Sí, eliminar"
                  cancelText="Cancelar"
                  okButtonProps={{ danger: true }}
                >
                  <Button 
                    type="primary" 
                    danger 
                    icon={<DeleteOutlined />}
                    className="bg-red-600! hover:bg-red-500!"
                  >
                    Eliminar
                  </Button>
                </Popconfirm>
              )}
              {/* Volver */}
              <Button 
                type="default"
                icon={<LeftOutlined />}
                onClick={() => navigate("/service-groups/list")}
              >
                Regresar
              </Button>
            </Space>
          )}
        </div>

        <ProDescriptions
          column={{
            xs: 1,
            sm: 1,
            md: 2,
            lg: 2,
            xl: 2,
          }}
          columns={columns}
          dataSource={serviceGroup}
          bordered
        />

        {/* Servicios */}
        {serviceGroup.items && serviceGroup.items.length > 0 && (
          <>
            <Divider />
            <Title level={4} className="mb-4!">
              Servicios Incluidos ({serviceGroup.items.length})
            </Title>
            <div className="space-y-3">
              {serviceGroup.items.map((item, index) => (
                <div
                  key={item.id || index}
                  className="flex flex-col sm:flex-row sm:items-center sm:justify-between p-4 bg-gray-50 rounded-lg border border-gray-200"
                >
                  <div className="flex-1 mb-2 sm:mb-0">
                    <Text strong className="text-base block">
                      {item.name}
                    </Text>
                  </div>
                  <div className="text-left sm:text-right">
                    <Text className="text-base font-semibold text-blue-600">
                      L{" "}
                      {(item.price || 0).toLocaleString("es-HN", {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2,
                      })}
                    </Text>
                  </div>
                </div>
              ))}

              {/* Total */}
              <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between p-4 bg-blue-50 rounded-lg border border-blue-200">
                <Text strong className="text-base mb-2 sm:mb-0">
                  Total:
                </Text>
                <Text strong className="text-lg text-blue-600">
                  L{" "}
                  {serviceGroup.items
                    .reduce((sum, item) => sum + (item.price || 0), 0)
                    .toLocaleString("es-HN", {
                      minimumFractionDigits: 2,
                      maximumFractionDigits: 2,
                    })}
                </Text>
              </div>
            </div>
          </>
        )}

        {/* Ubicaciones */}
        {serviceGroup.locations && serviceGroup.locations.length > 0 && (
          <>
            <Divider />
            <Title level={4} className="mb-4!">
              Ubicaciones donde se encuentra disponible este paquete:
            </Title>
            <div className="flex flex-wrap gap-2">
              {serviceGroup.locations.map((location, index) => (
                <Tag
                  key={location.id || index}
                  icon={<EnvironmentOutlined />}
                  color="blue"
                  className="px-3 py-1 text-base!"
                >
                  {location.name}
                </Tag>
              ))}
            </div>
          </>
        )}
      </div>
    </div>
  );
};
