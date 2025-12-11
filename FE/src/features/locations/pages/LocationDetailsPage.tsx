import React from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import { ProDescriptions } from "@ant-design/pro-components";
import { Card, Spin, Typography, Space, Button, message, Tag, Popconfirm } from "antd";
import {
  EditOutlined,
  DeleteOutlined,
  ArrowLeftOutlined,
} from "@ant-design/icons";
import { useGetApiLocationsId } from "../../../api/locations/locations";
import {
  useDeleteApiLocationsId,
  getGetApiLocationsQueryKey,
} from "../../../api/locations/locations";
import { useQueryClient } from "@tanstack/react-query";
import {
  BsBuilding,
  BsGeoAltFill,
  BsPersonFill,
  BsPinMapFill,
} from "react-icons/bs";
import { ContactPointSystem } from "../../../api/models";
import { PageHeaderTabs } from "../../../shared/components/ui";
import { useAbility } from "../../../config";
import { Can } from "@casl/react";

// Mapeo de ContactPointSystem a etiquetas legibles
const TelecomLabels: Record<number, string> = {
  [ContactPointSystem.NUMBER_0]: "Teléfono",
  [ContactPointSystem.NUMBER_1]: "Fax",
  [ContactPointSystem.NUMBER_2]: "Correo Electrónico",
  [ContactPointSystem.NUMBER_3]: "Pager",
  [ContactPointSystem.NUMBER_4]: "URL",
  [ContactPointSystem.NUMBER_5]: "SMS",
  [ContactPointSystem.NUMBER_6]: "Otro",
};

// Paleta de colores para los alias
const ALIAS_COLORS = [
  "blue",
  "green",
  "geekblue",
  "purple",
  "orange",
  "cyan",
  "gold",
  "lime",
  "volcano",
  "magenta",
];

const LocationDetailsPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const ability = useAbility();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const {
    data: location,
    isLoading,
    isError,
  } = useGetApiLocationsId(Number(id));

  const { mutate: deleteLocation } = useDeleteApiLocationsId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: getGetApiLocationsQueryKey(),
        });
        message.success("Ubicación eliminada exitosamente");
        navigate("/locations/list");
      },
      onError: () => message.error("Error al eliminar la ubicación"),
    },
  });

  const handleDelete = async (locationId: number) => {
    try {
      await deleteLocation({ id: locationId });
      return true;
    } catch {
      return false;
    }
  };

  const getModeLabel = (mode?: string | null): string => {
    if (!mode) return "—";
    return mode === "Kind" ? "Tipo" : mode === "Instance" ? "Instancia" : mode;
  };

  const renderStatusTag = (status?: string | null) => {
    if (status === undefined || status === null) return <Tag>—</Tag>;
    switch (status) {
      case "Active":
        return <Tag color="success">Activo</Tag>;
      case "Suspended":
        return <Tag color="warning">Suspendido</Tag>;
      case "Inactive":
        return <Tag color="error">Inactivo</Tag>;
      default:
        return <Tag color="default">Desconocido</Tag>;
    }
  };

  const renderModeTag = (mode?: string | null) => {
    const label = getModeLabel(mode);
    if (label === "Tipo") return <Tag color="blue">Tipo</Tag>;
    if (label === "Instancia") return <Tag color="geekblue">Instancia</Tag>;
    return <Tag color="default">{label}</Tag>;
  };

  if (isError) {
    return (
      <div className="min-h-screen bg-white p-6">
        <Card style={{ borderRadius: 8 }} bodyStyle={{ padding: 24 }}>
          <Typography.Text type="danger">
            Error al cargar los detalles de la ubicación
          </Typography.Text>
        </Card>
      </div>
    );
  }

  if (isLoading || !location) {
    return (
      <div className="min-h-screen bg-white p-6">
        <Card style={{ borderRadius: 8 }} bodyStyle={{ padding: 24 }}>
          <Spin tip="Cargando detalles de la ubicación..." />
        </Card>
      </div>
    );
  }

  return (
    <div>
      <main>
        <PageHeaderTabs
          title="Gestión de Ubicaciones"
          tabs={[
            ...(ability.can("read", "locations") ? [{
              key: "listar", label: "Lista de Ubicaciones", path: "/locations/list",
            }] : []),
            ...(ability.can("create", "locations") ? [{
              key: "crear", label: "Crear Ubicación", path: "/locations/create",
            }] : []),
          ]}
          defaultActive="null"
        />

        <div className="primary-card">
          {/* Header */}
          <div className="flex items-center gap-3 mb-8">
            <BsPinMapFill className="text-blue-500 text-2xl" />
            <h1 className="text-2xl font-bold text-general">
              {location.name || "Ubicación"}
            </h1>
          </div>

          {/* Información Básica */}
          <ProDescriptions
            title={
              <div className="flex items-center gap-2">
                <BsBuilding className="text-blue-500" />
                <span className="font-semibold">Información Básica</span>
              </div>
            }
            column={{ xs: 1, sm: 2, md: 2 }}
            size="middle"
            bordered
            dataSource={location}
            className="mb-8"
            labelStyle={{ fontWeight: 600, backgroundColor: "#fafafa" }}
          >
            <ProDescriptions.Item label="Nombre de la ubicación" span={2}>
              {location.name || "—"}
            </ProDescriptions.Item>

            <ProDescriptions.Item label="Alias" span={2}>
              {location.alias && location.alias.length > 0 ? (
                <div className="flex flex-wrap gap-2">
                  {location.alias.map((alias, index) => (
                    <Tag
                      key={index}
                      color={ALIAS_COLORS[index % ALIAS_COLORS.length]}
                      className="text-sm px-2.5 py-1"
                    >
                      {alias}
                    </Tag>
                  ))}
                </div>
              ) : (
                "—"
              )}
            </ProDescriptions.Item>

            <ProDescriptions.Item label="Estado">
              {renderStatusTag(location.status)}
            </ProDescriptions.Item>

            <ProDescriptions.Item label="Modo">
              {renderModeTag(location.mode)}
            </ProDescriptions.Item>

            {/* <ProDescriptions.Item label="Tipo de función" span={2}>
              {location.type || "—"}
            </ProDescriptions.Item> */}

            <ProDescriptions.Item label="Descripción" span={2}>
              <div className="whitespace-pre-wrap text-gray-700">
                {location.description || "—"}
              </div>
            </ProDescriptions.Item>
          </ProDescriptions>

          <hr className="border-black my-8" />

          {/* Dirección física */}
          <ProDescriptions
            title={
              <div className="flex items-center gap-2">
                <BsGeoAltFill className="text-blue-500" />
                <span className="font-semibold">Dirección física</span>
              </div>
            }
            column={{ xs: 1, sm: 2, md: 3 }}
            size="middle"
            bordered
            dataSource={location}
            className="mb-8"
            labelStyle={{ fontWeight: 600, backgroundColor: "#fafafa" }}
          >
            <ProDescriptions.Item label="País">
              {location.address?.country ? (
                <Tag color="blue">{location.address.country}</Tag>
              ) : (
                "—"
              )}
            </ProDescriptions.Item>

            <ProDescriptions.Item label="Estado/Provincia">
              {location.address?.state ? (
                <Tag color="blue">{location.address.state}</Tag>
              ) : (
                "—"
              )}
            </ProDescriptions.Item>

            <ProDescriptions.Item label="Ciudad">
              {location.address?.city ? (
                <Tag color="blue">{location.address.city}</Tag>
              ) : (
                "—"
              )}
            </ProDescriptions.Item>
            <ProDescriptions.Item label="Dirección" span={3}>
              {location.address?.line?.[0] || "—"}
            </ProDescriptions.Item>
          </ProDescriptions>

          <hr className="border-black my-8" />

          {/* Información de contacto */}
          <div className="mb-8">
            <div className="flex items-center gap-2 mb-4">
              <BsPersonFill className="text-blue-500" />
              <span className="font-semibold">Información de contacto</span>
            </div>

            {location.telecom && location.telecom.length > 0 ? (
              <ProDescriptions
                column={{ xs: 1, sm: 2 }}
                size="middle"
                bordered
                dataSource={{}}
                labelStyle={{ fontWeight: 600, backgroundColor: "#fafafa" }}
              >
                {location.telecom.map((t, index) => {
                  const label =
                    t.system !== undefined && TelecomLabels[t.system]
                      ? TelecomLabels[t.system]
                      : `Contacto (${t.system ?? "desconocido"})`;
                  return (
                    <ProDescriptions.Item key={index} label={label}>
                      {t.value || "—"}
                    </ProDescriptions.Item>
                  );
                })}
              </ProDescriptions>
            ) : (
              <div className="text-gray-500 text-sm pl-2">
                No hay información de contacto disponible
              </div>
            )}
          </div>

          <hr className="border-black my-8" />

          {/* Organización y jerarquía */}
          <ProDescriptions
            title={
              <div className="flex items-center gap-2">
                <BsBuilding className="text-blue-500" />
                <span className="font-semibold">Organización y jerarquía</span>
              </div>
            }
            column={{ xs: 1, sm: 2 }}
            size="middle"
            bordered
            dataSource={location}
            className="mb-8"
            labelStyle={{ fontWeight: 600, backgroundColor: "#fafafa" }}
          >
            <ProDescriptions.Item label="Organización responsable">
              <Tag color="purple">
                {location.managingOrganization?.display ||
                  "No disponible"}
              </Tag>
            </ProDescriptions.Item>

            <ProDescriptions.Item label="Parte de (ubicación padre)">
              <Tag color="purple">{location.partOf?.display || "Ninguna"}</Tag>
            </ProDescriptions.Item>
          </ProDescriptions>

          {/* Botones de acciones */}
          <div className="flex justify-end pt-10">
            <Space size="middle">
              <Can I="read" a="locations" ability={ability}>
                <Link to="/locations/list">
                  <Button
                    size="large"
                    style={{
                      borderRadius: 6,
                      boxShadow: "0 2px 8px rgba(0, 0, 0, 0.1)",
                      color: "#163C65",
                      borderColor: "#163C65",
                    }}
                  >
                    <ArrowLeftOutlined /> Volver
                  </Button>
                </Link>
              </Can>
              <Can I="update" a="locations" ability={ability}>
                <Link to={`/locations/update/${location.id}`}>
                  <Button
                    type="primary"
                    size="large"
                    icon={<EditOutlined />}
                    style={{
                      backgroundColor: "#52c41a",
                      borderColor: "#52c41a",
                      borderRadius: 6,
                      boxShadow: "0 2px 8px rgba(82, 196, 26, 0.3)",
                    }}
                  >
                    Editar
                  </Button>
                </Link>
              </Can>
              <Can I="delete" a="locations" ability={ability}>
                <Popconfirm
                  title="¿Eliminar ubicación?"
                  description="Esta acción no se puede deshacer"
                  onConfirm={() => handleDelete(Number(location.id!))}
                  okText="Sí, eliminar"
                  cancelText="Cancelar"
                  okButtonProps={{ danger: true }}
                >
                  <Button
                  type="primary"
                  size="large"
                  icon={<DeleteOutlined />}
                  danger
                  style={{
                    borderRadius: 6,
                    boxShadow: "0 2px 8px rgba(245, 34, 45, 0.3)",
                  }}
                >
                  Eliminar
                </Button>
                </Popconfirm>
              </Can>
            </Space>
          </div>
        </div>
      </main>
    </div>
  );
};

export default LocationDetailsPage;
