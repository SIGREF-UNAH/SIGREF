import { ProDescriptions, type ProFormInstance } from "@ant-design/pro-components";
import { FaUser } from "react-icons/fa";
import { BsPersonVcardFill } from "react-icons/bs";
import { useParams, useNavigate } from "react-router-dom";
import { Button, Spin, Modal, message, Space, Popconfirm } from "antd";
import { EditOutlined, DeleteOutlined } from "@ant-design/icons";
import {
  useDeleteApiPractitionerId,
  useGetApiPractitionerId,
} from "../../../api/practitioner/practitioner";
import { useQueryClient } from "@tanstack/react-query";
import { useGetApiOrganizations } from "../../../api/organizations/organizations";
import { useGetApiLocations } from "../../../api/locations/locations";
import { useState, useRef, useEffect } from "react";
import {
  useDeleteApiPractitionerRoleId,
  useGetApiPractitionerRolePractitionerId,
  usePostApiPractitionerRole,
  usePutApiPractitionerRoleId,
} from "../../../api/practitioner-role/practitioner-role";
import { IdentifierUse } from "../../../api/models";
import { PageHeaderTabs } from "../../../shared/components";
import PractitionerRoleModal from "../components/PractitionerRoleModal";
import { ROLE_OPTIONS } from "../../../shared/constants/RolesConstants";
import { useAbility } from "../../../config";
import { Can } from "@casl/react";

type EmployeeDetail = {
  id: string;
  identifier: { use: string; type?: { text?: string }; system: string; value: string }[];
  active: boolean;
  name: { use?: string; text?: string; family?: string; given?: string[] }[];
  telecom: { system: string; value: string; use?: string; rank?: number }[];
  gender: number | string;
  birthDate: string;
  lastUpdated: string;
  roles: any[];
};

type PractitionerRoleDto = {
  id: string;
  active: boolean;
  code?: { display?: string; text?: string; coding?: { code?: string; display?: string }[] }[];
  identifier?: { value: string; type?: { text?: string } }[];
  location?: { display?: string; reference?: string }[];
  organization?: { reference?: string; display?: string; type?: string | null; identifier?: any };
  period?: { start?: string; end?: string };
  practitioner?: { reference?: string; display?: string };
};

export default function PractitionerDetailsPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [contextHolder] = Modal.useModal();
  const ability = useAbility();
  const [roleModalOpen, setRoleModalOpen] = useState(false);
  const [editingRole, setEditingRole] = useState<PractitionerRoleDto | null>(null);
  const formRef = useRef<ProFormInstance>(null);

  const { data: orgs } = useGetApiOrganizations();
  const { data: locations } = useGetApiLocations();

  const { data: practitionerRole, isLoading: roleLoading } =
    useGetApiPractitionerRolePractitionerId<PractitionerRoleDto[]>(id || "");

  const createRoleMutation = usePostApiPractitionerRole({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["/api/practitionerRole", id] });
      },
    },
  });

  const updateRoleMutation = usePutApiPractitionerRoleId({
    mutation: {
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ["/api/practitionerRole", id] });
      },
    },
  });

  const deleteMutation = useDeleteApiPractitionerId({
    mutation: {
      onSuccess: () => {
        message.success("Empleado eliminado correctamente");
        queryClient.invalidateQueries({ queryKey: ["/api/Practitioner"] });
      },
      onError: (error) => {
        message.error("Error al eliminar el empleado");
        console.error(error);
      },
    },
  });

  useEffect(() => {
    if (!roleModalOpen) return;

    if (editingRole) {
      formRef.current?.setFieldsValue(initialRoleValues);
    } else {
      formRef.current?.resetFields();
    }
  }, [roleModalOpen, editingRole]);

  const deleteRoleMutation = useDeleteApiPractitionerRoleId({
  mutation: {
    onSuccess: () => {
      message.success("Cargo eliminado correctamente");
      queryClient.invalidateQueries({ queryKey: ["/api/practitionerRole", id] });
    },
    onError: () => {
      message.error("Error al eliminar el cargo");
    },
  },
});

  const { data: employee, isLoading } = useGetApiPractitionerId<EmployeeDetail, any>(id || "");

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-screen">
        <Spin size="large" tip="Cargando datos del empleado..." />
      </div>
    );
  }

  const name = employee?.name?.[0];
  const identifier = employee?.identifier?.[0];
  const telecom = employee?.telecom ?? [];
  const phone = telecom.find((t) => t.system?.toLowerCase() === "phone")?.value ?? "-";
  const email = telecom.find((t) => t.system?.toLowerCase() === "email")?.value ?? "-";
  const genderMap: Record<number, string> = { 1: "Masculino", 2: "Femenino", 3: "Otro" };
  const genderValue = genderMap[Number(employee?.gender)] ?? "N/A";
  const birthDate = employee?.birthDate?.split("T")[0] ?? "-";  

  const handleEdit = () => { if (id) navigate(`/practitioners/update/${id}`); };

  const handleRoleSubmit = async (values: any) => {
    if (!employee) return false;
    const selectedRole = ROLE_OPTIONS.find((r) => r.value === values.role);

    const payload = {
      identifier: editingRole ? editingRole.identifier : [{
        use: IdentifierUse.NUMBER_0,
        system: "https://hospitalpublico.hn/fhir/identifier/practitionerrole",
        value: `role-${id}-${Date.now()}`,
      }],
      period: {
        start: values.period?.[0] ?? null,
        end: values.period?.[1] ?? null,
      },
      practitioner: { reference: `Practitioner/${id}` },
      code: [{
        coding: [{ system: "https://hospitalpublico.hn/fhir/CodeSystem/roles-admin", code: selectedRole?.value, display: selectedRole?.label }],
        text: values.roleName,
      }],
      organization: values.organizationId ? {
        reference: `Organization/${values.organizationId}`,
        display: orgs?.items?.find(o => o.id === values.organizationId)?.name ?? ""
      } : undefined,
      location: values.locationId ? [{
        reference: `Location/${values.locationId}`,
        display: locations?.items?.find(l => l.id === values.locationId)?.name ?? ""
      }] : undefined,
      active: true,
    };

    try {
    if (editingRole) {
      await updateRoleMutation.mutateAsync({ id: editingRole.id, data: payload });
      message.success("Rol actualizado correctamente");
    } else {
      await createRoleMutation.mutateAsync({ data: payload });
      message.success("Rol asignado correctamente");
    }

    setRoleModalOpen(false);
    setEditingRole(null);
    queryClient.invalidateQueries({ queryKey: ["/api/practitionerRole", id] });

    window.location.reload();
    return true;
  } catch (error) {
    message.error("Ocurrió un error al guardar el rol");
    console.error(error);
    return false;
  }
  };
  

  const initialRoleValues = editingRole
    ? {
        roleName: editingRole.code?.[0]?.text,
        role: editingRole.code?.[0]?.coding?.[0]?.code,
        organizationId: editingRole.organization?.reference?.replace("Organization/", ""),
        locationId: editingRole.location?.[0]?.reference?.replace("Location/", ""),
        period: [
          editingRole.period?.start ? new Date(editingRole.period.start) : null,
          editingRole.period?.end ? new Date(editingRole.period.end) : null
        ]
      }
    : {};

    const formatDate = (dateString?: string) => {
      if (!dateString) return "-";
      const date = new Date(dateString);
      return date.toLocaleDateString("es-ES", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      });
    };

  return (
    <div>
      <PageHeaderTabs
        title="Gestión de Empleados"
        tabs={[
          ...(ability.can("read", "practitioners") ? [{
            key: "listar",
            label: "Lista de Empleados",
            path: "/practitioners/list",
          }] : []),
          ...(ability.can("create", "practitioners") ? [{
            key: "crear",
            label: "Crear Empleado",
            path: "/practitioners/create",
          }] : []),
        ]}
        defaultActive="null"
      />

      <div className="primary-card">

        <div className="flex items-center justify-between mb-8">
          <div className="flex items-center gap-3">
            <FaUser className="w-10 h-10 text-blue-500" />
            <span className="text-xl font-semibold text-general">Detalles del Empleado</span>
          </div>

          <Space>
            <Can I="create" a="practitioner-roles" ability={ability}>
              <Button
                onClick={() => {
                  setEditingRole(practitionerRole?.[0] ?? null);
                  setRoleModalOpen(true);
                }}
              >
                {practitionerRole?.[0] ? "Editar Cargo" : "Asignar Cargo"}
              </Button>
            </Can>
            {practitionerRole?.[0] && (
              <Can I="delete" a="practitioner-roles" ability={ability}>
                <Popconfirm
                  title="¿Deseas eliminar el cargo asignado?"
                  okText="Eliminar"
                  okType="danger"
                  cancelText="Cancelar"
                  onConfirm={async () => {
                    try {
                      await deleteRoleMutation.mutateAsync({ id: practitionerRole[0].id });
                      window.location.reload();
                    } catch (err) {
                      message.error("Error al eliminar el cargo");
                    }
                  }}
                >
                  <Button danger icon={<DeleteOutlined />}>Eliminar Cargo</Button>
                </Popconfirm>
              </Can>
            )}
            <Can I="update" a="practitioners" ability={ability}>
              <Button type="primary" icon={<EditOutlined />} onClick={handleEdit}>Editar</Button>
            </Can>
            <Can I="delete" a="practitioners" ability={ability}>
              <Popconfirm
                title={`¿Estás seguro de que deseas eliminar a ${name?.text}? Esta acción no se puede deshacer.`}
                onConfirm={async () => {
                  try {
                    await deleteMutation.mutateAsync({ id: id?? "" });
                  } catch (error) {
                    message.error("No se pudo eliminar el empleado");
                  }
                }}
                okText="Eliminar"
                okType="danger"
                cancelText="Cancelar"
              >
                <Button type="text" danger icon={<DeleteOutlined />} >Eliminar</Button>
              </Popconfirm>
            </Can>
          </Space>
        </div>

        {/* Datos personales */}
        <section className="mb-8">
          <div className="flex items-center gap-3 mb-4">
            <BsPersonVcardFill className="w-8 h-8 text-blue-500" />
            <span className="text-lg font-semibold text-general">Datos Personales</span>
          </div>

          <ProDescriptions
            bordered
            column={3}
            dataSource={{
              firstName: name?.given?.[0],
              middleName: name?.given?.[1] ?? "-",
              lastName: name?.family,
              dni: identifier?.value,
              idType: identifier?.type?.text,
              phone,
              email,
              gender: genderValue,
              birthDate,
              active: employee?.active ? "Sí" : "No",
            }}
          >
            <ProDescriptions.Item label="Primer Nombre" dataIndex="firstName" />
            <ProDescriptions.Item label="Segundo Nombre" dataIndex="middleName" />
            <ProDescriptions.Item label="Apellidos" dataIndex="lastName" />
            <ProDescriptions.Item label="Identificador" dataIndex="dni" />
            <ProDescriptions.Item label="Tipo de Identificador" dataIndex="idType" />
            <ProDescriptions.Item label="Teléfono" dataIndex="phone" />
            <ProDescriptions.Item label="Correo Electrónico" dataIndex="email" />
            <ProDescriptions.Item label="Género" dataIndex="gender" />
            <ProDescriptions.Item label="Fecha de Nacimiento" dataIndex="birthDate" />
            <ProDescriptions.Item label="Activo" dataIndex="active" />
          </ProDescriptions>
        </section>

        {/* Cargo asignado */}
        {!roleLoading && practitionerRole?.[0] && (
          <section className="mt-8">
            <div className="flex items-center gap-3 mb-4">
              <BsPersonVcardFill className="w-8 h-8 text-green-600" />
              <span className="text-lg font-semibold text-general">Cargo Asignado</span>
            </div>

            <ProDescriptions
              bordered
              column={3}
              dataSource={{
                role: practitionerRole[0].code?.[0]?.text ?? "-",
                organization: practitionerRole[0].organization?.display ?? "-",
                location: practitionerRole[0].location?.[0]?.display ?? "-",
                start: formatDate(practitionerRole[0].period?.start),
                end: formatDate(practitionerRole[0].period?.end),
                active: practitionerRole[0].active ? "Sí" : "No",
              }}
            >
              <ProDescriptions.Item label="Rol" dataIndex="role" />
              <ProDescriptions.Item label="Organización" dataIndex="organization" />
              <ProDescriptions.Item label="Ubicación" dataIndex="location" />
              <ProDescriptions.Item label="Inicio" dataIndex="start" />
              <ProDescriptions.Item label="Fin" dataIndex="end" />
              <ProDescriptions.Item label="Activo" dataIndex="active" />
            </ProDescriptions>
          </section>
        )}

        <PractitionerRoleModal
          open={roleModalOpen}
          onOpenChange={setRoleModalOpen}
          onSubmit={handleRoleSubmit}
          roleOptions={ROLE_OPTIONS}
          orgOptions={orgs?.items?.map((o: any) => ({ label: o.name, value: o.id })) ?? []}
          locationOptions={locations?.items?.map((l: any) => ({ label: l.name, value: l.id })) ?? []}
          initialValues={initialRoleValues}
          formRef={formRef}
          title={editingRole ? "Editar Rol" : "Asignar Rol"}
        />
      </div>
    </div>
  );
}
