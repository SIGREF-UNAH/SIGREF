import PractitionerRoleModal from "../components/PractitionerRoleModal";
import { ProDescriptions } from "@ant-design/pro-components";
import { FaUser, FaBriefcase } from "react-icons/fa";
import { BsPersonVcardFill } from "react-icons/bs";
import { Button, Spin, Space, Popconfirm, Empty, Alert, Tag } from "antd";
import { ROLE_OPTIONS } from "../../../shared/constants/RolesConstants";
import { PageHeaderTabs } from "../../../shared/components";
import { Can } from "@casl/react";
import {
  EditOutlined,
  DeleteOutlined,
  ExclamationCircleOutlined,
  PlusOutlined,
} from "@ant-design/icons";
import usePractitionerInfo from "../hooks/usePractitionerInfo";

export default function PractitionerDetailsPage() {
  const {
    practitioner,
    practitionerLoading,
    practitionerError,
    rolesLoading,
    practitionerRoles,
    roleModalOpen,
    editingRole,  
    formRef,
    ability,
    orgsData,
    locationsData,
    formatDate,
    getGenderLabel,
    getRoleFormValues,
    handleRoleSubmit,
    handleEdit,
    handleDeleteRole,
    handleDeletePractitioner,
    setRoleModalOpen,
    setEditingRole,
  } = usePractitionerInfo();

  // Loading state
  if (practitionerLoading) {
    return (
      <div className="flex items-center justify-center h-screen">
        <Spin size="large" />
      </div>
    );
  }

  // Error state
  if (practitionerError || !practitioner) {
    return (
      <div>
        <Alert
          message="Error al cargar la información del empleado"
          description="No se pudieron cargar los datos. Por favor, intente nuevamente."
          type="error"
          showIcon
        />
      </div>
    );
  }

  // Extraer campos
  const name = practitioner.name?.[0];
  const identifier = practitioner.identifier?.[0];
  const telecom = practitioner.telecom ?? [];
  const phone = telecom.find((t) => String(t.system)?.toLowerCase() === "phone")?.value || "-";
  const email = telecom.find((t) => String(t.system)?.toLowerCase() === "email")?.value || "-";
  const fullName = name?.text || `${name?.given?.join(" ")} ${name?.family}`.trim();
  const activeRole = practitionerRoles?.[0];

  return (
    <div>
      {/* Encabezado */}
      <PageHeaderTabs
        title="Gestión de Empleados"
        tabs={[
          ...(ability.can("read", "practitioners")
            ? [
                {
                  key: "listar",
                  label: "Lista de Empleados",
                  path: "/practitioners/list",
                },
              ]
            : []),
          ...(ability.can("create", "practitioners")
            ? [
                {
                  key: "crear",
                  label: "Crear Empleado",
                  path: "/practitioners/create",
                },
              ]
            : []),
        ]}
        defaultActive="null"
      />

      {/* Contenido */}
      <div className="primary-card">
        {/* Titulo con botones */}
        <div className="flex items-center justify-between mb-4 pb-6 border-b border-gray-200">
          <div>
            <div className="flex items-center gap-3">
              <FaUser className="size-8 text-blue-600" />
              <h1 className="text-2xl font-bold text-gray-800">{fullName}</h1>
            </div>
          </div>
          <Space>
            <Can I="update" a="practitioners" ability={ability}>
              <Button type="primary" icon={<EditOutlined />} onClick={handleEdit}>
                Editar Información
              </Button>
            </Can>
            <Can I="delete" a="practitioners" ability={ability}>
              <Popconfirm
                title="Eliminar empleado"
                description={
                  <div className="max-w-xs">
                    <p className="mb-2">
                      ¿Está seguro de que desea eliminar a <strong>{fullName}</strong>?
                    </p>
                    <p className="text-gray-500 text-sm">
                      Esta acción no se puede deshacer.
                    </p>
                  </div>
                }
                onConfirm={handleDeletePractitioner}
                okText="Sí, eliminar"
                cancelText="Cancelar"
                okButtonProps={{ danger: true }}
                icon={<ExclamationCircleOutlined style={{ color: "red" }} />}
              >
                <Button className="bg-red-500! text-white! hover:bg-red-400!" danger icon={<DeleteOutlined />}>
                  Eliminar empleado
                </Button>
              </Popconfirm>
            </Can>
          </Space>
        </div>

        {/* Información */}
        <section className="mb-4">
          <div className="flex items-center gap-3 mb-6 pb-3 border-b border-gray-200">
            <BsPersonVcardFill className="size-6 text-blue-600" />
            <h2 className="text-lg font-semibold text-gray-800">
              Datos Personales
            </h2>
          </div>

          <ProDescriptions
            bordered
            column={{ xs: 1, sm: 2, md: 3 }}
            dataSource={{
              firstName: name?.given?.[0] || "-",
              middleName: name?.given?.[1] || "-",
              lastName: name?.family || "-",
              dni: identifier?.value || "-",
              idType: identifier?.type?.text || "-",
              phone,
              email,
              gender: getGenderLabel(practitioner.gender),
              birthDate: formatDate(practitioner.birthDate),
              active: practitioner.active ? "Sí" : "No",
            }}
          >
            <ProDescriptions.Item label="Primer Nombre" dataIndex="firstName" />
            <ProDescriptions.Item label="Segundo Nombre" dataIndex="middleName" />
            <ProDescriptions.Item label="Apellidos" dataIndex="lastName" />
            <ProDescriptions.Item label="Identificador" dataIndex="dni" />
            <ProDescriptions.Item label="Tipo de ID" dataIndex="idType" />
            <ProDescriptions.Item label="Teléfono" dataIndex="phone" />
            <ProDescriptions.Item label="Correo Electrónico" dataIndex="email" />
            <ProDescriptions.Item label="Género" dataIndex="gender" />
            <ProDescriptions.Item label="Fecha de Nacimiento" dataIndex="birthDate" />
            <ProDescriptions.Item label="Estado" dataIndex="active" 
              render={
                (active) => active !== "No" ? <Tag color="green">✓ Activo</Tag> : <Tag color="red">✗ Inactivo</Tag>
              }
            />
          </ProDescriptions>
        </section>

        {/* Cargo */}
        <section>
          <div className="flex items-center justify-between mb-6 pb-3 border-b border-gray-200">
            <div className="flex items-center gap-3">
              <FaBriefcase className="size-5.5 text-green-600" />
              <h2 className="text-lg font-semibold text-gray-800">
                Cargo Desempeñado
              </h2>
            </div>

            {activeRole && (
              <Space>
                <Can I="update" a="practitioner-roles" ability={ability}>
                  <Button
                    type="primary" 
                    icon={<EditOutlined />}
                    onClick={() => {
                      setEditingRole(activeRole);
                      setRoleModalOpen(true);
                    }}
                  >
                    Editar cargo
                  </Button>
                </Can>
                <Can I="delete" a="practitioner-roles" ability={ability}>
                  <Popconfirm
                    title="Eliminar cargo"
                    description="¿Está seguro de que desea eliminar este cargo?"
                    okText="Sí, eliminar"
                    cancelText="Cancelar"
                    okType="danger"
                    onConfirm={() => handleDeleteRole(activeRole.id!)}
                  >
                    <Button className="bg-red-500! text-white! hover:bg-red-400!" danger icon={<DeleteOutlined />}>
                      Eliminar cargo
                    </Button>
                  </Popconfirm>
                </Can>
              </Space>
            )}
          </div>

          {rolesLoading ? (
            <div className="text-center py-8">
              <Spin />
            </div>
          ) : activeRole ? (
            <ProDescriptions
              bordered
              column={{ xs: 1, sm: 2, md: 3 }}
              dataSource={{
                role: activeRole.code?.[0]?.text || "-",
                roleCode: activeRole.code?.[0]?.coding?.[0]?.display || "-",
                organization: activeRole.organization?.display || "-",
                location: activeRole.location?.[0]?.display || "-",
                start: formatDate(activeRole.period?.start),
                end: formatDate(activeRole.period?.end),
                active: activeRole.active ? "Sí" : "No",
              }}
            >
              <ProDescriptions.Item label="Titulo" dataIndex="role" />
              <ProDescriptions.Item label="Tipo de Cargo" dataIndex="roleCode" />
              <ProDescriptions.Item label="Ubicación" dataIndex="location" />
              <ProDescriptions.Item label="Fecha de Inicio" dataIndex="start" />
              <ProDescriptions.Item label="Fecha de Fin" dataIndex="end" />
              <ProDescriptions.Item label="Organización" dataIndex="organization" />
            </ProDescriptions>
          ) : (
            <Empty
              description="No hay cargo asignado"
              image={Empty.PRESENTED_IMAGE_SIMPLE}
            >
              <Can I="create" a="practitioner-roles" ability={ability}>
                <Button
                  type="primary"
                  icon={<PlusOutlined />}
                  onClick={() => setRoleModalOpen(true)}
                >
                  Asignar Cargo
                </Button>
              </Can>
            </Empty>
          )}
        </section>
      </div>

      {/* Modal de asignación de cargo */}
      <PractitionerRoleModal
        open={roleModalOpen}
        onOpenChange={setRoleModalOpen}
        onSubmit={handleRoleSubmit}
        roleOptions={ROLE_OPTIONS}
        orgOptions={
          orgsData?.items?.map((o) => ({
            label: o.name || "",
            value: o.id || "",
          })) || []
        }
        locationOptions={
          locationsData?.items?.map((l) => ({
            label: l.name || "",
            value: l.id || "",
          })) || []
        }
        initialValues={editingRole ? getRoleFormValues(editingRole) : undefined}
        formRef={formRef}
        title={editingRole ? "Actualizar cargo del empleado" : "Asignar cargo del empleado"}
      />
    </div>
  );
}