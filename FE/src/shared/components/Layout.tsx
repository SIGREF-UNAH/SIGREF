import { ProLayout } from "@ant-design/pro-components";
import { Link, Outlet, useNavigate } from "react-router";
import { Button, Dropdown } from "antd";
import { useKeycloak } from "@react-keycloak/web";
import { RoutesByRole, useAbility } from "../../config";
import { ShortcutsGuideModal } from "./modals";
import { useState } from "react";
import { useGetApiHospitalPropertiesDetails } from "../../api/hospital-properties/hospital-properties";
import { USER_ROLE_OPTIONS } from "../constants";
import useMediaFiles from "../../features/media-files/hooks/useMediaFiles";
import {
  BankOutlined,
  BookOutlined,
  LogoutOutlined,
  PhoneOutlined,
  QuestionCircleOutlined,
} from "@ant-design/icons";

export const Layout = () => {
  const navigate = useNavigate();
  const { keycloak } = useKeycloak();
  const { getMediaUrl } = useMediaFiles();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const ability = useAbility();

  // Obtener información del hospital con los logos
  const { data: hospitalResponse } = useGetApiHospitalPropertiesDetails();
  const hospitalResponseData = hospitalResponse as any;
  const hospitalData = hospitalResponseData?.data;

  // Construir URLs de los logos
  const logoHealthUrl = hospitalData?.urlLogoHealth 
    ? getMediaUrl(hospitalData.urlLogoHealth) 
    : "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f1/Logo_de_SESAL.svg/1200px-Logo_de_SESAL.svg.png";
  
  const logoHospitalUrl = hospitalData?.urlLogo 
    ? getMediaUrl(hospitalData.urlLogo) 
    : "https://krti.cl/wp-content/uploads/2021/04/Logo-Hospital-Final.png";

  const hospitalName = hospitalData?.name || "Hospital";

  // Obtener todos los roles del token
  const roles = keycloak.tokenParsed?.realm_access?.roles || [];

  // Filtrar roles para mostrar solo los que nos interesan
  const rolesValidos = roles
    .map((rol) => {
      const roleOption = USER_ROLE_OPTIONS.find(option => option.value === rol);
      return roleOption ? roleOption.label : undefined;
    })
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
        logo={logoHealthUrl}
        layout="top"
        fixedHeader
        // Estilos 
        style={{
          height: "100%",
          minHeight: "100vh",
        }}
        // Estilos para el contenido
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
                  src={logoHospitalUrl}
                  alt={hospitalName}
                  className="h-6 md:h-8 object-contain"
                  onError={(e) => {
                    // Fallback en caso de error al cargar la imagen
                    e.currentTarget.src = "https://krti.cl/wp-content/uploads/2021/04/Logo-Hospital-Final.png";
                  }}
                />
              </div>
              <div className="ml-4 mr-8 text-xs md:text-xl font-semibold text-general truncate max-w-37.5 md:max-w-none">
                {`SIGREF - Panel de ${rolesValidos}`}
              </div>
            </div>
          </div>
        )}
        // Estilos para el token
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
        // Botones
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
        // Menu Desplegable y Avatar
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
              ...(ability.can("read", "hospital") ? [
              {
                key: "1",
                label: <Link to="/hospital">Hospital</Link>,
                icon: <BankOutlined />,
              }] : []),
              {
                key: "2",
                label: <Link to="/documentation">Documentación</Link>,
                icon: <BookOutlined />,
              },
              ...(ability.can("read", "support") ? [
              {
                key: "3",
                label: <Link to="/support">Soporte</Link>,
                icon: <PhoneOutlined />,
              }] : []),
              {
                key: "4",
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