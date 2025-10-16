import { ProLayout } from "@ant-design/pro-components";
import { Badge } from "antd/lib";
import { Link, Outlet, useNavigate } from "react-router";
import { Dropdown } from "antd";
import { useKeycloak } from "@react-keycloak/web";
import { MenusPorRol } from "../../config";
import { validRoles } from "../../auth";
import {
  BellOutlined,
  BookOutlined,
  FileSyncOutlined,
  LogoutOutlined,
  PhoneOutlined,
  QuestionCircleOutlined,
  SettingOutlined,
  UserOutlined,
  WarningOutlined,
} from "@ant-design/icons";

export const Layout = () => {
  const navigate = useNavigate();
  const { keycloak } = useKeycloak();

  // Obtener todos los roles del token
  const roles = keycloak.tokenParsed?.realm_access?.roles || [];

  // Filtrar roles para mostrar solo los que nos interesan
  const rolesValidos = roles
    .map((rol) => validRoles[rol])
    .filter((rolMapeado) => rolMapeado !== undefined);

  const name = keycloak.tokenParsed?.name || "Usuario";

  const getInitials = (fullName: string): string => {
    const names = fullName.split(" ");
    if (names.length > 1) {
      return names[0][0] + names[1][0];
    }
    return fullName.charAt(0);
  };

  return (
    <ProLayout
      title={`SIGREF - Panel de ${rolesValidos}`}
      logo="https://upload.wikimedia.org/wikipedia/commons/thumb/f/f1/Logo_de_SESAL.svg/1200px-Logo_de_SESAL.svg.png"
      layout="top"
      fixedHeader
      
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
        const helpMenu = [
          {
            key: "1",
            label: <Link to="/">Documentación</Link>,
            icon: <BookOutlined />,
          },
          {
            key: "2",
            label: <Link to="/">Tutorial</Link>,
            icon: <FileSyncOutlined />,
          },
          {
            key: "3",
            label: <Link to="/">Contactar Soporte</Link>,
            icon: <PhoneOutlined />,
          },
        ];

        const notificationsMenu = [
          {
            key: "1",
            label: (
              <div className="px-2 py-1">
                <strong>Nueva factura generada</strong>
                <br />
                <span className="text-xs text-general-secondary">
                  Hace 5 minutos
                </span>
              </div>
            ),
            icon: <WarningOutlined style={{ color: "#FFD54F" }} />,
          },
          {
            key: "2",
            label: (
              <div className="px-2 py-1">
                <strong>Cierre de caja pendiente</strong>
                <br />
                <span className="text-xs text-general-secondary">
                  Hace 1 hora
                </span>
              </div>
            ),
            icon: <WarningOutlined style={{ color: "#FFD54F" }} />,
          },
          {
            key: "3",
            label: (
              <div className="px-2 py-1">
                <strong>Nuevo paciente registrado</strong>
                <br />
                <span className="text-xs text-general-secondary">
                  Hace 2 horas
                </span>
              </div>
            ),
            icon: <WarningOutlined style={{ color: "#FFD54F" }} />,
          },
        ];

        return [
          <Dropdown
            key="help"
            menu={{
              items: helpMenu,
            }}
            trigger={["click"]}
          >
            <QuestionCircleOutlined className="text-lg text-general-secondary cursor-pointer hover:text-general" />
          </Dropdown>,

          <Dropdown
            key="notif"
            menu={{
              items: notificationsMenu,
            }}
            trigger={["click"]}
            placement="bottomRight"
          >
            <Badge count={3} size="small">
              <BellOutlined className="text-lg text-general-secondary cursor-pointer hover:text-general" />
            </Badge>
          </Dropdown>,
        ];
      }}
      avatarProps={{
        src: undefined,
        size: "default",
        style: {
          backgroundColor: "#163C65",
          fontSize: "16px",
          fontWeight: "600",
        },
        title: getInitials(name),
        render: (_props, dom) => {
          const userMenu = [
            {
              key: "1",
              label: <Link to="/">Mi Perfil</Link>,
              icon: <UserOutlined />,
            },
            {
              key: "2",
              label: <Link to="/">Configuración</Link>,
              icon: <SettingOutlined />,
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
        Object.entries(MenusPorRol[rolesValidos[0]] || {}).map(
          ([key, items]) => ({
            path: `/${key}`,
            name: key.charAt(0).toUpperCase() + key.slice(1),
            children: items,
          })
        )
      }
      menuItemRender={(item, dom) => <Link to={item.path || "/"}>{dom}</Link>}
    >
      <Outlet />
    </ProLayout>
  );
};
