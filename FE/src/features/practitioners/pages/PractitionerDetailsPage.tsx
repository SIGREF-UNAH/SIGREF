import { ModalForm, ProDescriptions, ProFormDatePicker, ProFormSelect, ProFormText } from "@ant-design/pro-components";
import { FaUser } from "react-icons/fa";
import { BsPersonVcardFill } from "react-icons/bs";
import { useParams, useNavigate } from "react-router-dom";
import { Button, Spin, Modal, message, Space } from "antd";
import { EditOutlined, DeleteOutlined } from "@ant-design/icons";
import { useDeleteApiPractitionerId, useGetApiPractitionerId } from "../../../api/practitioner/practitioner";
import { useQueryClient } from "@tanstack/react-query";
import { useGetApiOrganizations } from "../../../api/organizations/organizations";
import { useGetApiLocations } from "../../../api/locations/locations";
import { useState } from "react";
import { useGetApiPractitionerRolePractitionerId, usePostApiPractitionerRole } from "../../../api/practitioner-role/practitioner-role";
import { IdentifierUse } from "../../../api/models";


type EmployeeDetail = {
  id: string;
  identifier: {
    use: string;
    type?: { text?: string };
    system: string;
    value: string;
  }[];
  active: boolean;
  name: {
    use?: string;
    text?: string;
    family?: string;
    given?: string[];
    prefix?: string[];
    suffix?: string[];
  }[];
  telecom: {
    system: string;
    value: string;
    use?: string;
    rank?: number;
  }[];
  gender: number | string;
  birthDate: string;
  lastUpdated: string;
  roles: any[];
};


type PractitionerRoleDto = {
  id: string;
  active: boolean;
  code?: { display?: string; text?: string }[];
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
  const [modal, contextHolder] = Modal.useModal();
  const [roleModalOpen, setRoleModalOpen] = useState(false);

  const { data: orgs } = useGetApiOrganizations();
  const { data: locations } = useGetApiLocations();

  const { data: practitionerRole, isLoading: roleLoading } = useGetApiPractitionerRolePractitionerId<PractitionerRoleDto[]>(id || "");


  const createRoleMutation = usePostApiPractitionerRole({
  mutation: {
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['/api/practitionerRole', id]
      });
    }
  }
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

  

  const { data, isLoading } = useGetApiPractitionerId<EmployeeDetail, any>(id || "");

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-screen">
        <Spin size="large" tip="Cargando datos del empleado..." />
      </div>
    );
  }

  const employee = data;

const name = employee?.name?.[0];
const identifier = employee?.identifier?.[0];

const telecom = employee?.telecom ?? [];

const phone = telecom.find(t => t.system?.toLowerCase() === "phone")?.value ?? "-";
const email = telecom.find(t => t.system?.toLowerCase() === "email")?.value ?? "-";

// gender con mapa basado en números
const genderMap: Record<number, string> = {
  1: "Masculino",
  2: "Femenino",
  3: "Otro"
};

const genderValue = genderMap[employee?.gender] ?? "N/A";

// La fecha ya viene como string ISO
const birthDate = employee?.birthDate?.split("T")[0] ?? "-";


  // --- Función para editar ---
  const handleEdit = () => {
    if (id) navigate(`/practitioners/update/${id}`);
  };

  // --- Función para eliminar ---
  const handleDelete = () => {
    if (!id) {
      message.error("ID del empleado no válido");
      return;
    }

    modal.confirm({
      title: "¿Seguro que deseas eliminar este empleado?",
      content: "Esta acción no se puede deshacer.",
      okText: "Sí, eliminar",
      cancelText: "Cancelar",
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          await deleteMutation.mutateAsync({ id });
          message.success("Empleado eliminado correctamente");
          navigate("/practitioners/list");
        } catch (error) {
          message.error("Error al eliminar el empleado");
        }
      },
    });
  };

  return (
    <div className="bg-card rounded-lg border-2 border-[#D9D9D9] p-6">
      {contextHolder}
      {/* Encabezado */}
      <div className="flex items-center justify-between mb-8">
        <div className="flex items-center gap-3">
          <FaUser className="w-10 h-10 text-blue-500" />
          <span className="text-xl font-semibold text-general">
            Detalles del Empleado
          </span>
        </div>

        {/* Botones de acción */}
        <Space>
          <Button onClick={() => navigate("/practitioners/list")}>Volver</Button>
          {!practitionerRole ? (
            <Button type="default" onClick={() => setRoleModalOpen(true)}>
              Asignar Rol
            </Button>
          ) : (
            <Button type="default" onClick={() => setRoleModalOpen(true)}>
              Editar Rol
            </Button>
          )}
          <Button type="primary" icon={<EditOutlined />} onClick={handleEdit}>
            Editar
          </Button>
          <Button danger icon={<DeleteOutlined />} onClick={handleDelete}>
            Eliminar
          </Button>
        </Space>
      </div>

      {/* Sección de datos personales */}
      <section className="mb-8">
        <div className="flex items-center gap-3 mb-4">
          <BsPersonVcardFill className="w-8 h-8 text-blue-500" />
          <span className="text-lg font-semibold text-general">
            Datos Personales
          </span>
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

      {/* Sección de roles */}
      {!roleLoading && practitionerRole && practitionerRole.length > 0 && (
  <section className="mt-8">
    <div className="flex items-center gap-3 mb-4">
      <BsPersonVcardFill className="w-8 h-8 text-green-600" />
      <span className="text-lg font-semibold text-general">
        Rol Asignado
      </span>
    </div>

    {(() => {
      const role = practitionerRole[0];
      return (
        <ProDescriptions
          bordered
          column={3}
          dataSource={{
            role: role?.code?.[0]?.text ?? "-",
            organization:
              role?.organization?.display ??
              role?.organization?.reference?.replace("Organization/", "") ??
              "-",
            location:
              role?.location?.[0]?.display ??
              role?.location?.[0]?.reference?.replace("Location/", "") ??
              "-",
            start: role?.period?.start ?? "-",
            end: role?.period?.end ?? "-",
            active: role?.active ? "Sí" : "No",
          }}
        >
          <ProDescriptions.Item label="Rol" dataIndex="role" />
          <ProDescriptions.Item label="Organización" dataIndex="organization" />
          <ProDescriptions.Item label="Ubicación" dataIndex="location" />
          <ProDescriptions.Item label="Inicio" dataIndex="start" />
          <ProDescriptions.Item label="Fin" dataIndex="end" />
          <ProDescriptions.Item label="Activo" dataIndex="active" />
        </ProDescriptions>
      );
    })()}
  </section>
)}



      <ModalForm
  title="Asignar Rol a Practitioner"
  open={roleModalOpen}
  onOpenChange={setRoleModalOpen}
  modalProps={{
    destroyOnClose: true,
  }}
  onFinish={async (values) => {
  if (!employee) return false;

  await createRoleMutation.mutateAsync({
  data: {
    identifier: [
      {
        use: IdentifierUse.NUMBER_0,
        system: "https://hospitalpublico.hn/fhir/identifier/practitionerrole",
        value: `role-${id}-${Date.now()}`,
      },
    ],
    period: { start: values.startDate, end: values.endDate },
    practitioner: { reference: `Practitioner/${id}` },
    code: [
      {
        text: values.roleName,
        coding: [
          {
            system: "https://hospitalpublico.hn/fhir/CodeSystem/roles-admin",
            code: values.roleName,
          },
        ],
      },
    ],
    organization: values.organizationId
      ? { reference: `Organization/${values.organizationId}` }
      : undefined,
    location: values.locationId
      ? [{ reference: `Location/${values.locationId}` }]
      : undefined,
    active: true,
  },
});


  message.success("Rol asignado correctamente");
  setRoleModalOpen(false);
  queryClient.invalidateQueries({
    queryKey: ['/api/practitionerRole', id],
  });
  return true;
}}



>
  <ProFormText
    name="roleName"
    label="Nombre del Rol"
    placeholder="Ej: Médico General"
    rules={[{ required: true, message: "Este campo es obligatorio" }]}
  />

  <ProFormSelect
    name="organizationId"
    label="Organización"
    allowClear
    options={orgs?.items?.map((o: any) => ({
      label: o.name,
      value: o.id,
    }))}
  />

  <ProFormSelect
    name="locationId"
    label="Ubicación"
    allowClear
    options={locations?.items?.map((l: any) => ({
      label: l.name,
      value: l.id,
    }))}
  />

  <ProFormDatePicker
    name="startDate"
    label="Fecha de Inicio"
    rules={[{ required: true }]}
  />

  <ProFormDatePicker
    name="endDate"
    label="Fecha de Fin"
  />
</ModalForm>

    </div>
  );
}
