import { PageHeaderTabs } from "../../../shared/components";
import { ReportGenerator } from "../components/ui";

export const CreateReportPage = () => {
  return (
    <div>
      {/* Encabezado */}
      <PageHeaderTabs
        title="Gestión de Reportes"
        tabs={[
          {
            key: "listar",
            label: "Control de Reportes",
            path: "/reports/list",
          },
          {
            key: "crear",
            label: "Generar Reporte",
            path: "/reports/create",
          },
          {
            key: "historial",
            label: "Historial de Reportes",
            path: "/reports/history",
          },
        ]}
        defaultActive="listar"
      />

      {/* Contenido  */}
      <ReportGenerator />
    </div>
  );
};
