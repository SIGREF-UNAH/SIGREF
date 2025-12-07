import { useAbility } from "../../../config";
import { PageHeaderTabs } from "../../../shared/components";
import { ReportGenerator } from "../components";

export const CreateReportPage = () => {
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
      <ReportGenerator />
    </div>
  );
};
