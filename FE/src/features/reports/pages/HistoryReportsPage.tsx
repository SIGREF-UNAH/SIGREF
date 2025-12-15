import { useAbility } from "../../../config";
import { PageHeaderTabs } from "../../../shared/components";
import { UnderConstructionPage } from "../../../shared/pages";

export const HistoryReportsPage = () => {
  const ability = useAbility();

  return (
    <div>
      {/* Encabezado */}
      <PageHeaderTabs
        title="Gestión de Reportes"
        tabs={[
          ...(ability.can("read", "reports") ? [{
            key: "listar",
            label: "Control de Reportes",
            path: "/reports/list",
          }] : []),
          ...(ability.can("create", "reports") ? [{
            key: "crear",
            label: "Generar Reporte",
            path: "/reports/create",
          }] : []),
          ...(ability.can("read", "reports") ? [{
            key: "historial",
            label: "Historial de Reportes",
            path: "/reports/history",
          }] : []),
        ]}
        defaultActive="listar"
      />
      
      {/* Contenido  */}
      <div className="primary-card">
        <UnderConstructionPage />
      </div>
    </div>
  );
};
