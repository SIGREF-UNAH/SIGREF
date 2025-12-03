import {
  ProForm,
  ProFormText,
  ProFormSelect,
  type ProFormInstance,
} from "@ant-design/pro-components";
import { Card, List, Tag, Space, message } from "antd";
import { useEffect, useRef, useState } from "react";
import { PageHeaderTabs } from "../../../shared/components";
import { useGetApiPractitioner } from "../../../api/practitioner/practitioner";
import { ROLE_OPTIONS } from "../../../shared/constants/RolesConstants";
import { useGetApiLocations } from "../../../api/locations/locations";
import { USER_ROLE_OPTIONS } from "../../../shared/constants/UserRolesConstants";
import { FaCheck } from "react-icons/fa";
import { usePostApiKeycloakSeederCreateUser } from "../../../api/keycloak-seeder/keycloak-seeder";
import { useAbility } from "../../../config";
import { useKeycloak } from "@react-keycloak/web";

type Practitioner = {
  id: number;
  name: string;
  dni: string;
  role: string;
  positionCode: string;
  area: string;
  status: "Activo" | "Inactivo";
};

function generarBaseUsername(nombreCompleto: string) {
  const partes = nombreCompleto.trim().split(/\s+/);

  const primerNombre = partes[0] ?? "";
   const segundoNombre = (partes.length >= 3 ? partes[1][0] : "");
  const apellido = partes[partes.length - 1][0] ?? "";

  return `${primerNombre}${segundoNombre}${apellido}`;
}

function generarUsernameUnico(base: string, existentes: string[]) {
  let numero = 1;

  while (existentes.includes(`${base}${numero}`)) {
    numero++;
  }

  return `${base}${numero}`;
}


export default function CreateUsersPage() {
  const [selected, setSelected] = useState<Practitioner | null>(null);
  const formRef = useRef<ProFormInstance | null>(null);
  const [searchName, setSearchName] = useState("");
  const [searchRole, setSearchRole] = useState<string | undefined>(undefined);
  const [searchArea, setSearchArea] = useState<string | undefined>(undefined);
  const [searchStatus, setSearchStatus] = useState<string | undefined>(undefined);
  const { keycloak } = useKeycloak();
  const ability = useAbility();

  const createUserMutation = usePostApiKeycloakSeederCreateUser();

  const currentUserRole = keycloak.tokenParsed?.realm_access?.roles || [];

  const { data } = useGetApiPractitioner<{
      items: Practitioner[];
      pagination: {
        currentPage: number;
        hasNext: boolean;
        hasPrevious: boolean;
        pageSize: number;
        totalItems: number;
        totalPages: number;
      };
    }>();

    const practitioners: Practitioner[] =
  data?.items?.map((p: any, index: number) => {
    const role = p.roles?.[0];
    const positionCode = role?.code?.[0]?.coding?.[0]?.code ?? "sin-código";
    const positionText = role?.code?.[0]?.text ?? "Sin puesto";
    const area = role?.location?.[0]?.display ?? "Sin área";

    return {
      id: p.id ?? index + 1,
      name: p.name?.[0]?.text ?? "",
      dni: p.identifier?.[0]?.value ?? "",
      role: positionText,
      status: p.active ? "Activo" : "Inactivo",
      positionCode,
      area,
      raw: p,
    };
  }) ?? [];

  const filtered = practitioners.filter((e) => e.status === "Activo").filter((e) => {
    const nameMatch = e.name.toLowerCase().includes(searchName.toLowerCase());
    const roleMatch = searchRole ? e.positionCode === searchRole : true;
    const areaMatch = searchArea ? e.area === searchArea : true;
    const statusMatch = searchStatus ? e.status === searchStatus : true;
    return nameMatch && roleMatch && areaMatch && statusMatch;
  }
  );

  const existingUsernames = ["juanclopez1", "juanclopez2", "anamtorres1"];
  // const existingUsernames = ["isaacv1", "milcajr1", "annerjh1"];

  const { data: locations } = useGetApiLocations<{ items: { name: string }[] }>();

  const locationOptions = locations?.items?.map((loc) => ({
    label: loc.name,
    value: loc.name,
  })) ?? [];

  useEffect(() => {
  if (!selected) return;

  // Obtener valores de telecom del practitioner original
  const phone = selected.raw?.telecom?.find(t => t.system === "Phone")?.value;
  const email = selected.raw?.telecom?.find(t => t.system === "Email")?.value;

  const base = generarBaseUsername(selected.name);
  const usernameFinal = generarUsernameUnico(base, existingUsernames);

  formRef.current?.setFieldsValue({
    practitionerName: selected.name,
    dni: selected.dni,
    phone: phone ?? "",
    email: email ?? "",
    username: usernameFinal,
    role: undefined,
  });
}, [selected]);

    const allRoles = ["admin", "ti", "cashier", "auditor"];

// Filtra los roles que puede crear el usuario actual
const allowedRoles = allRoles.filter((r) => {
  // Admin no puede crear admin ni ti
  if (ability.can("create", "users")) {
    if (currentUserRole.includes("admin")) {
      return r === "cashier" || r === "auditor";
    }
    if (currentUserRole.includes("ti")) {
      return true; // TI puede crear todos
    }
  }
  return false;
})



  return (
    <div>
      {/* Header */}
      <PageHeaderTabs
        title="Gestión de Usuarios"
        tabs={[
          {
            key: "crear",
            label: "Crear Usuario",
            path: "/users/create",
          },
        ]}
        defaultActive="crear"
      />

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* ===================== PANEL IZQUIERDO ===================== */}
        <Card title="Elija un Empleado" className="primary-card">
          <Space direction="vertical" style={{ width: "100%" }}>
            <ProForm submitter={false}>
              <ProFormText
                name="name"
                label={<span className="text-general font-medium">Nombre</span>}
                placeholder="Buscar por nombre"
                  fieldProps={{
                    value: searchName,
                    onChange: (e) => setSearchName(e.target.value),
                  }}
              />
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <ProFormSelect
                  name="position"
                  placeholder="Seleccionar"
                  label={
                    <span className="text-general font-medium">Cargo</span>
                  }
                    options={ROLE_OPTIONS.map((r) => ({ label: r.label, value: r.value }))}
                    fieldProps={{
                      value: searchRole,
                      onChange: (value) => setSearchRole(value),
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
                      onChange: (value) => setSearchArea(value),
                    }}
                />
                <ProFormSelect
                  name="status"
                  placeholder="Seleccionar"
                  label={
                    <span className="text-general font-medium">Estado</span>
                  }
                  options={[
                    { label: "Activo", value: "Activo" },
                    { label: "Inactivo", value: "Inactivo" },
                  ]}
                    fieldProps={{
                      value: searchStatus,
                      onChange: (value) => setSearchStatus(value),
                    }}
                />
              </div>
            </ProForm>

            <List
              bordered
              dataSource={filtered}
              pagination={{
                pageSize: 4,
              }}
              renderItem={(item) => (
                <List.Item
                  className="cursor-pointer hover:bg-gray-50"
                  onClick={() => setSelected(item)}
                  style={{
                    background:
                      selected?.id === item.id ? "#e6f7ff" : "transparent",
                  }}
                >
                  <List.Item.Meta
                    title={item.name}
                    description={
                      <>
                        Identificación: {item.dni}
                        <br />
                        <span className="text-gray-500 text-sm">
                          {item.role}
                        </span>
                      </>
                    }
                  />
                  <Tag color={item.status === "Activo" ? "green" : "red"}>{item.status === "Activo" ? "✓ Activo" : "✗ Inactivo"}</Tag>
                </List.Item>
              )}
            />
          </Space>
        </Card>

        {/* ===================== PANEL DERECHO ===================== */}
        <Card title="Crear Usuario" className="primary-card">
          <ProForm
            formRef={formRef}
            submitter={{
              searchConfig: {
                submitText: "Crear usuario",
                resetText: "Limpiar",
              },
              submitButtonProps: {
                type: "primary",
                icon: <FaCheck className="w-4 h-4" />,
                className: `px-6 py-2 text-white font-medium rounded-md transition-colors duration-200 flex items-center gap-2`,
                loading: createUserMutation.isPending,
              },
              resetButtonProps: { 
                className: "px-6 py-2 bg-gray-300 hover:bg-gray-400 text-gray-800 font-medium rounded-md transition-colors duration-200" 
              },
              render: (_, dom) => {
                return (
                  <div className="flex justify-end w-full mt-4 gap-3">
                    {dom}
                  </div>
                );
              },
            }}
            onFinish={async (values) => {
              if (!selected) {
                message.error("Debe seleccionar un empleado");
                return;
              }

              if (values.password !== values.confirmPassword) {
                message.error("Las contraseñas no coinciden");
                return;
              }

              const payload = {
                username: values.username,
                practitionerId: String(selected.id),
                email: values.email,
                password: values.password,
                roles: [values.role], 
              };

              console.log("payload:", payload);
              

              try {
                await createUserMutation.mutateAsync({ data: payload });

                message.success({
                  content: "Usuario creado correctamente",
                  duration: 2,
                });
                formRef.current?.resetFields();
              } catch (error) {
                console.error(error);
                message.error({
                  content: "Error al crear el usuario",
                  duration: 2,
                });
              }
            }}
          >
            {/* ===================== DATOS DEL EMPLEADO ===================== */}
            <div className="primary-card mb-6">
              <h3 className="text-lg font-semibold text-gray-700 mb-2">
                Datos del Empleado
              </h3>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <ProFormText
                  name="practitionerName"
                  label="Nombre Completo"
                  disabled
                />
                <ProFormText
                  name="dni"
                  label="DNI"
                  disabled
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <ProFormText
                  name="phone"
                  label="Número de Teléfono"
                  disabled
                />
                <ProFormText
                  name="email"
                  label="Correo Electrónico"
                  disabled
                />
              </div>
            </div>

            {/* ===================== DATOS DEL USUARIO ===================== */}
            <div className="primary-card mb-10">
              <h3 className="text-lg font-semibold text-gray-700 mb-2">
                Datos del Usuario
              </h3>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <ProFormText
                  name="username"
                  label="Nombre de Usuario"
                  placeholder="Ej. JuanP1"
                />

                <ProFormSelect
                  name="role"
                  label="Rol del Usuario"
                  placeholder="Seleccione un rol"
                  options={USER_ROLE_OPTIONS.filter(r => allowedRoles.includes(r.value))}
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <ProFormText.Password
                  name="password"
                  label="Contraseña"
                  placeholder="Ingrese una contraseña"
                />

                <ProFormText.Password
                  name="confirmPassword"
                  label="Confirmar Contraseña"
                  placeholder="Repita la contraseña"
                />
              </div>
            </div>
          </ProForm>
        </Card>
      </div>
    </div>
  );
}
