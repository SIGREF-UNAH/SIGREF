import { PageHeaderTabs } from "../../../shared/components";
import { ControlReport } from "../components/ui";

export const ReportsControlPage = () => {
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
      <div className="primary-card">
        <ControlReport/>
      </div>
    </div>
  );
};
