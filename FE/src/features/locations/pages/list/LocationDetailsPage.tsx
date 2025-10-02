import React, { useState } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormTextArea,
} from "@ant-design/pro-components";
import { Card, Spin, Typography, Space, Button, message } from "antd";
import {
  EnvironmentOutlined,
  EditOutlined,
  DeleteOutlined,
  ArrowLeftOutlined,
} from "@ant-design/icons";
import { useGetApiLocationsId } from "../../../../api/locations/locations";
import { LocationMode, LocationStatus } from "../../../../api/models";
import DeleteLocationModal from "../../components/modals/DeleteLocationModal";
import {
  useDeleteApiLocationsId,
  getGetApiLocationsQueryKey,
} from "../../../../api/locations/locations";
import { useQueryClient } from "@tanstack/react-query";
import { BsBuilding, BsGeoAltFill, BsPersonFill } from "react-icons/bs";
import { BiChevronDown } from "react-icons/bi";

const LocationDetailsPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
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
  const [deleteModalVisible, setDeleteModalVisible] = useState(false);

  const handleDeleteClick = () => {
    setDeleteModalVisible(true);
  };

  const handleDeleteCancel = () => {
    setDeleteModalVisible(false);
  };

  const handleDelete = async (locationId: number) => {
    try {
      await deleteLocation({ id: locationId });
      return true;
    } catch {
      return false;
    }
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

  const renderStatusOptions = [
    { label: "Seleccionar estado", value: "" },
    { label: "Activo", value: LocationStatus.NUMBER_0 },
    { label: "Inactivo", value: LocationStatus.NUMBER_1 },
    { label: "Suspendido", value: LocationStatus.NUMBER_2 },
  ];

  const getModeLabel = (mode: number | undefined) => {
    if (mode === LocationMode.NUMBER_0) return "Kind";
    if (mode === LocationMode.NUMBER_1) return "Instance";
    return "";
  };

  const initialValues = {
    name: location.name || "",
    alias: location.alias?.join(", ") || "",
    description: location.description || "",
    status: location.status,
    mode: location.mode,
    type: location.type || "",
    address: {
      line: location.address?.line?.[0] || "",
      city: location.address?.city || "",
      state: location.address?.state || "",
      postalCode: location.address?.postalCode || "",
      country: location.address?.country || "",
    },
    managingOrganizationIds: location.managingOrganizationIds || "",
    partOfId: location.partOfId || "",
  };

  return (
    <div className="min-h-screen bg-white">
      <main className="p-6">
        {/* Header */}
        <div className="relative mb-6">
          <div className="flex justify-between items-center relative z-10">
            <h1 className="text-3xl font-bold text-[#333333]">
              Detalles de la Ubicación
            </h1>
          </div>
        </div>

        <div className="bg-[#FAFAFA] rounded-lg border-2 border-[#D9D9D9] p-6">
          {/* Header */}
          <div className="flex items-center gap-3 mb-8">
            <EnvironmentOutlined className="w-10 h-10 text-blue-500" />
            <span className="text-xl font-semibold text-[#333333]">
              {location.name || "Ubicación"}
            </span>
          </div>

          <ProForm initialValues={initialValues} submitter={false}>
            <div className="max-w-[105rem] mx-auto">
              {/* Información Básica */}
              <section>
                <div className="flex items-center gap-3 mb-6">
                  <BsBuilding className="w-8 h-8 text-blue-500" />
                  <span className="text-lg font-semibold text-[#333333]">
                    Información Básica
                  </span>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 sm:gap-8 md:gap-16 lg:gap-32 mb-2">
                  <ProFormText
                    name="name"
                    label={
                      <span className="text-[#616161] font-medium">
                        Nombre de la ubicación
                      </span>
                    }
                    readonly
                    fieldProps={{
                      value: location.name || "",
                    }}
                  />
                  <ProFormText
                    name="alias"
                    label={
                      <span className="text-[#616161] font-medium">Alias</span>
                    }
                    readonly
                    fieldProps={{
                      value: location.alias?.join(", ") || "",
                    }}
                  />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 sm:gap-8 md:gap-16 lg:gap-32 mb-2">
                  <ProFormSelect
                    name="status"
                    label={
                      <span className="text-[#616161] font-medium">Estado</span>
                    }
                    options={renderStatusOptions}
                    readonly
                    fieldProps={{
                      value: location.status,
                      suffixIcon: (
                        <BiChevronDown className="w-4 h-4 text-[#616161]" />
                      ),
                    }}
                  />
                  <ProFormText
                    name="mode"
                    label={
                      <span className="text-[#616161] font-medium">Modo</span>
                    }
                    readonly
                    fieldProps={{
                      value: getModeLabel(location.mode) || "",
                    }}
                  />
                  <ProFormText
                    name="type"
                    label={
                      <span className="text-[#616161] font-medium">
                        Tipo de función
                      </span>
                    }
                    readonly
                    fieldProps={{
                      value: location.type || "",
                    }}
                  />
                </div>

                <ProFormTextArea
                  name="description"
                  label={
                    <span className="text-[#616161] font-medium">
                      Descripción
                    </span>
                  }
                  readonly
                  fieldProps={{
                    value: location.description || "",
                    rows: 2,
                  }}
                />
              </section>

              <hr className="border-[#000] my-8" />

              {/* Dirección física */}
              <section>
                <div className="flex items-center gap-3 mb-6">
                  <BsGeoAltFill className="w-8 h-8 text-blue-500" />
                  <span className="text-lg font-semibold text-[#333333]">
                    Dirección física
                  </span>
                </div>

                <ProFormText
                  name="address.line"
                  label={
                    <span className="text-[#616161] font-medium">
                      Dirección
                    </span>
                  }
                  readonly
                  fieldProps={{
                    value: location.address?.line?.[0] || "",
                  }}
                />

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 sm:gap-8 md:gap-16 lg:gap-32 mb-2">
                  <ProFormText
                    name="address.city"
                    label={
                      <span className="text-[#616161] font-medium">Ciudad</span>
                    }
                    readonly
                    fieldProps={{
                      value: location.address?.city || "",
                    }}
                  />
                  <ProFormText
                    name="address.state"
                    label={
                      <span className="text-[#616161] font-medium">
                        Estado/Provincia
                      </span>
                    }
                    readonly
                    fieldProps={{
                      value: location.address?.state || "",
                    }}
                  />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 sm:gap-8 md:gap-16 lg:gap-32">
                  <ProFormText
                    name="address.postalCode"
                    label={
                      <span className="text-[#616161] font-medium">
                        Código Postal
                      </span>
                    }
                    readonly
                    fieldProps={{
                      value: location.address?.postalCode || "",
                    }}
                  />
                  <ProFormText
                    name="address.country"
                    label={
                      <span className="text-[#616161] font-medium">País</span>
                    }
                    readonly
                    fieldProps={{
                      value: location.address?.country || "",
                    }}
                  />
                </div>
              </section>

              <hr className="border-[#000] mt-2 mb-8" />

              {/* Información de contacto */}
              <section>
                <div className="flex items-center gap-3 mb-6">
                  <BsPersonFill className="w-8 h-8 text-blue-500" />
                  <span className="text-lg font-semibold text-[#333333]">
                    Información de contacto
                  </span>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 sm:gap-8 md:gap-16 lg:gap-32">
                  <ProFormText
                    name="managingOrganizationIds"
                    label={
                      <span className="text-[#616161] font-medium">
                        Nombre de contacto
                      </span>
                    }
                    readonly
                    fieldProps={{
                      value: location.managingOrganizationIds || "",
                    }}
                  />
                  <ProFormText
                    name="phone"
                    label={
                      <span className="text-[#616161] font-medium">
                        Teléfono
                      </span>
                    }
                    readonly
                    fieldProps={{
                      value:
                        (location.telecom || []).find(
                          (t) => t.system?.toLowerCase() === "phone"
                        )?.value || "No disponible",
                    }}
                  />
                  <ProFormText
                    name="email"
                    label={
                      <span className="text-[#616161] font-medium">
                        Correo Electrónico
                      </span>
                    }
                    readonly
                    fieldProps={{
                      type: "email",
                      value:
                        (location.telecom || []).find(
                          (t) => t.system?.toLowerCase() === "email"
                        )?.value || "No disponible",
                    }}
                  />
                </div>
              </section>

              <hr className="border-[#000] mt-2 mb-8" />

              {/* Organización y jerarquía */}
              <section>
                <div className="flex items-center gap-3 mb-6">
                  <BsBuilding className="w-8 h-8 text-blue-500" />
                  <span className="text-lg font-semibold text-[#333333]">
                    Organización y jerarquía
                  </span>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 sm:gap-8 md:gap-16 lg:gap-32">
                  <ProFormText
                    name="managingOrganizationIds"
                    label={
                      <span className="text-[#616161] font-medium">
                        Organización responsable
                      </span>
                    }
                    readonly
                    fieldProps={{
                      value: location.managingOrganizationIds || "",
                    }}
                  />
                  <ProFormText
                    name="partOfId"
                    label={
                      <span className="text-[#616161] font-medium">
                        Parte de (ubicación padre)
                      </span>
                    }
                    readonly
                    fieldProps={{
                      value: location.partOfId || "",
                    }}
                  />
                </div>
              </section>

              {/* Botones de acciones */}
              <div className="flex justify-end pt-2 pb-2">
                <Space size="middle">
                  <Link to="/locations">
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
                  <Link to={`/locations/edit/${location.id}`}>
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
                  <Button
                    type="primary"
                    size="large"
                    icon={<DeleteOutlined />}
                    style={{
                      backgroundColor: "#f5222d",
                      borderColor: "#f5222d",
                      borderRadius: 6,
                      boxShadow: "0 2px 8px rgba(245, 34, 45, 0.3)",
                    }}
                    onClick={handleDeleteClick}
                  >
                    Eliminar
                  </Button>
                </Space>
              </div>
            </div>
          </ProForm>
        </div>

        <DeleteLocationModal
          visible={deleteModalVisible}
          onVisibleChange={setDeleteModalVisible}
          locationId={location.id}
          locationName={location.name}
          onDelete={handleDelete}
        />
      </main>
    </div>
  );
};

export default LocationDetailsPage;