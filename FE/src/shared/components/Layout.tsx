import { ProLayout } from "@ant-design/pro-components";
import { Link, Outlet, useNavigate } from "react-router";
import { Button, Dropdown } from "antd";
import { useKeycloak } from "@react-keycloak/web";
import { RoutesByRole } from "../../config";
import { validRoles } from "../../auth";
import {
  BookOutlined,
  LogoutOutlined,
  PhoneOutlined,
  QuestionCircleOutlined,
} from "@ant-design/icons";
import { ShortcutsGuideModal } from "./modals";
import { useState } from "react";

export const Layout = () => {
  const navigate = useNavigate();
  const { keycloak } = useKeycloak();
  const [isModalOpen, setIsModalOpen] = useState(false);

  // Obtener todos los roles del token
  const roles = keycloak.tokenParsed?.realm_access?.roles || [];

  // Filtrar roles para mostrar solo los que nos interesan
  const rolesValidos = roles
    .map((rol) => validRoles[rol])
    .filter((rolMapeado) => rolMapeado !== undefined);

  // Obtener el nombre del usuario
  const name = keycloak.tokenParsed?.name || "Usuario";

  // Filtrar iniciales del primer y segundo nombre
  const getInitials = (fullName: string): string => {
    const names = fullName.split(" ");
    if (names.length > 1) {
      return names[0][0] + names[1][0];
    }
    return fullName.charAt(0);
  };

  return (
    <div style={{ height: "100vh", display: "flex", flexDirection: "column" }}>
      <ProLayout
        title={`SIGREF - Panel de ${rolesValidos}`}
        logo="https://upload.wikimedia.org/wikipedia/commons/thumb/f/f1/Logo_de_SESAL.svg/1200px-Logo_de_SESAL.svg.png"
        layout="top"
        fixedHeader
        style={{
          height: "100%",
          minHeight: "100vh",
        }}
        contentStyle={{
          height: "100%",
          minHeight: "calc(100vh - 128px)",
          display: "flex",
          flexDirection: "column",
        }}
        // Pie de página
        footerRender={() => (
          <div
            style={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
              padding: "16px 24px",
              backgroundColor: "#163C65",
              borderTop: "1px solid #d9d9d9",
              fontSize: "14px",
              color: "#ffffff",
              marginTop: "auto",
            }}
          >
            <span>SIGREF - Sistema de Gestión de Receptoría de Fondos</span>
            <span>© 2025 Ingeniería en Sistemas - UNAH Campus Copán</span>
          </div>
        )}
        // Encabezado / Titulo
        headerTitleRender={(logo) => (
          <div
            className="flex items-center gap-2 md:gap-4 hover:cursor-pointer"
            onClick={() => navigate("/")}
          >
            {logo}
            <div className="flex">
              <div className="flex items-center gap-2">
                <img
                  src="https://krti.cl/wp-content/uploads/2021/04/Logo-Hospital-Final.png"
                  alt="Hospital de Occidente"
                  className="h-6 md:h-8"
                />
              </div>
              <div className="ml-4 mr-8 text-xs md:text-xl font-semibold text-general truncate max-w-[150px] md:max-w-none">
                {`SIGREF - Panel de ${rolesValidos}`}{" "}
              </div>
            </div>
          </div>
        )}
        token={{
          header: {
            colorBgHeader: "#fff",
            colorBgMenuItemHover: "rgba(0,0,0,0.04)",
            colorTextMenuSelected: "#163C65",
            colorTextMenu: "#666",
            colorTextMenuActive: "#163C65",
            heightLayoutHeader: 64,
          },
        }}
        
        actionsRender={() => {
          return [
            <Button 
              icon={<QuestionCircleOutlined />} 
              type="default" 
              onClick={() => setIsModalOpen(true)}
            >
              Atajos
            </Button>
          ];
        }}

        // Avatar / Acciones
        avatarProps={{
          src: undefined,
          size: "default",
          style: {
            backgroundColor: "#163C65",
            fontSize: "16px",
            fontWeight: "600",
          },
          icon: <span className="text-xs font-thin text-white">{getInitials(name)}</span>,
          render: (_props, dom) => {
            const userMenu = [
              {
                key: "1",
                label: <Link to="/documentation">Documentación</Link>,
                icon: <BookOutlined />,
              },
              {
                key: "2",
                label: <Link to="/support">Soporte</Link>,
                icon: <PhoneOutlined />,
              },
              {
                key: "3",
                label: "Cerrar Sesión",
                onClick: () => keycloak.logout(),
                danger: true,
                icon: <LogoutOutlined />,
              },
            ];

            return (
              <Dropdown
                menu={{
                  items: userMenu,
                }}
                trigger={["click"]}
                placement="bottomRight"
              >
                <div className="flex items-center space-x-2 cursor-pointer hover:opacity-80 transition-opacity">
                  {dom}
                  <div className="flex flex-col leading-none">
                    <span className="text-xs text-general">{rolesValidos}</span>
                    <span className="font-medium">{name}</span>
                  </div>
                </div>
              </Dropdown>
            );
          },
        }}
        menuHeaderRender={undefined}
        menuDataRender={() =>
          Object.entries(RoutesByRole[rolesValidos[0]] || {}).map(
            ([key, items]) => ({
              path: `/${key}`,
              name: key.charAt(0).toUpperCase() + key.slice(1),
              children: items,
            })
          )
        }
        menuItemRender={(item, dom) => <Link to={item.path || "/"}>{dom}</Link>}
      >
        <div style={{ flex: 1, display: "flex", flexDirection: "column" }}>
          <Outlet />
        </div>
        {/* Modal de Atajos */}
        <ShortcutsGuideModal
          open={isModalOpen}
          onClose={() => setIsModalOpen(false)}
        />
      </ProLayout>
    </div>
  );
};
