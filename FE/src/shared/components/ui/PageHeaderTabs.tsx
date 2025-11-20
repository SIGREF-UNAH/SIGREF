import { useState, useEffect } from "react";
import { Link, useLocation } from "react-router-dom";

interface TabItem {
  key: string;
  label: string;
  path: string;
}

interface PageHeaderTabsProps {
  /** Título del encabezado */
  title: string;
  /** Lista de pestañas disponibles */
  tabs: TabItem[];
  /** Clave activa inicial */
  defaultActive?: string;
}

/**
 * Componente reutilizable de encabezado con pestañas de navegación.
 */
export const PageHeaderTabs: React.FC<PageHeaderTabsProps> = ({
  title,
  tabs,
  defaultActive,
}) => {
  const location = useLocation();
  const [activeTab, setActiveTab] = useState<string>(
    defaultActive || tabs[0]?.key || ""
  );

  // Sincronizar la pestaña activa con la ruta actual
  useEffect(() => {
    const currentTab = tabs.find(tab => tab.path === location.pathname);
    if (currentTab) {
      setActiveTab(currentTab.key);
    }
  }, [location.pathname, tabs]);

  const isTabActive = (tab: TabItem) => {
    return activeTab === tab.key || location.pathname === tab.path;
  };

  return (
    <div className="relative mb-6">
      <div className="flex justify-between items-center relative z-10">
        <h1 className="text-3xl font-bold text-general-primary">{title}</h1>

        {/* Opciones */}
        <div className="flex items-center gap-3 relative">
          {tabs.map((tab) => (
            <Link
              key={tab.key}
              to={tab.path}
              onClick={() => setActiveTab(tab.key)}
              className={`px-4 py-2 transition-colors relative z-10 cursor-pointer ${
                isTabActive(tab)
                  ? "border border-b-0 border-gray-300 bg-transparent text-secondary rounded-t-md"
                  : "text-primary"
              }`}
            >
              {tab.label}
            </Link>
          ))}

          {/* Línea inferior */}
          <div className="absolute bottom-0 h-px bg-gray-300 z-0 left-0 right-0"></div>
        </div>
      </div>
    </div>
  );
};