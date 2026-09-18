import { useMemo } from "react";
import {
  Button,
  Select,
  DatePicker,
  Table,
  Space,
  Typography,
  Alert,
  Pagination,
  Spin,
} from "antd";
import {
  FileTextOutlined,
  FilePdfOutlined,
  FileExcelOutlined,
  PrinterOutlined,
  FilterOutlined,
  ClearOutlined,
  DownloadOutlined,
  CheckOutlined,
  ReloadOutlined,
} from "@ant-design/icons";
import dayjs from "dayjs";
import type { ReportLineDto } from "@models";
import type { ColumnsType } from "antd/es/table";
import { useReportData, useReportFilters } from "../hooks";
import useMediaFiles from "../../media-files/hooks/useMediaFiles";

const { RangePicker } = DatePicker;
const { Title, Text } = Typography;


const isAdult = (birthDate?: string | null): boolean => {
  if (!birthDate) return false;
  return dayjs().diff(dayjs(birthDate), "year") >= 18;
};
 
const buildColumns = (): ColumnsType<ReportLineDto> => [
  {
    title: "N°",
    key: "index",
    width: 52,
    align: "center",
    onHeaderCell: () => ({ style: { backgroundColor: "#bdd7ee", fontWeight: 700 } }),
    render: (_v, _r, index) => index + 1,
  },
  {
    title: "Fecha Emisión",
    dataIndex: "transactionDate",
    key: "transactionDate",
    width: 100,
    align: "center",
    onHeaderCell: () => ({ style: { backgroundColor: "#bdd7ee", fontWeight: 700 } }),
    render: (v) => (v ? dayjs(v).format("DD/MM/YYYY") : "—"),
  },
  {
    title: "Recibo",
    dataIndex: "receiptNumber",
    key: "receiptNumber",
    width: 90,
    align: "center",
    onHeaderCell: () => ({ style: { backgroundColor: "#bdd7ee", fontWeight: 700 } }),
    render: (v) => v ?? "—",
  },
  {
    title: "Serie",
    dataIndex: "seriesName",
    key: "seriesName",
    width: 90,
    align: "center",
    onHeaderCell: () => ({ style: { backgroundColor: "#bdd7ee", fontWeight: 700 } }),
    render: (v) => v ?? "—",
  },
  // Datos de Vigilante Receptoría 
  {
    title: "Datos de Vigilante Receptoría",
    key: "cashierGroup",
    onHeaderCell: () => ({
      style: {
        backgroundColor: "#bdd7ee",
        fontWeight: 700,
        textAlign: "center",
      },
    }),
    children: [
      {
        title: "Identificador",
        dataIndex: "cashierIdentity",
        key: "cashierIdentity",
        width: 120,
        align: "center",
        onHeaderCell: () => ({ style: { backgroundColor: "#ddeeff", fontWeight: 600 } }),
        render: (v) => <span className="text-xs break-all">{v ?? "—"}</span>,
      },
      {
        title: "Nombre",
        dataIndex: "cashierName",
        key: "cashierName",
        width: 180,
        align: "center",
        onHeaderCell: () => ({ style: { backgroundColor: "#ddeeff", fontWeight: 600 } }),
        render: (v) => v ?? "—",
      },
    ],
  },
  // Datos del Paciente 
  {
    title: "Datos del Paciente",
    key: "patientGroup",
    onHeaderCell: () => ({
      style: {
        backgroundColor: "#bdd7ee",
        fontWeight: 700,
        textAlign: "center",
      },
    }),
    children: [
      {
        title: "Identificador",
        dataIndex: "patientIdentity",
        key: "patientIdentity",
        width: 80,
        align: "center",
        onHeaderCell: () => ({ style: { backgroundColor: "#ddeeff", fontWeight: 600 } }),
        render: (v) => v ?? "—",
      },
      {
        title: "Nombre",
        dataIndex: "patientName",
        key: "patientName",
        width: 180,
        align: "center",
        onHeaderCell: () => ({ style: { backgroundColor: "#ddeeff", fontWeight: 600 } }),
        render: (v) => v ?? "—",
      },
      {
        title: "Mayor de Edad",
        dataIndex: "patientBirthDate",
        key: "mayorEdad",
        width: 80,
        align: "center",
        onHeaderCell: () => ({ style: { backgroundColor: "#ddeeff", fontWeight: 600 } }),
        render: (v) =>
          v ? (
            isAdult(v) ? (
              <span className="text-green-600 font-semibold">✓</span>
            ) : (
              <span className="text-red-500 font-semibold">✗</span>
            )
          ) : (
            "—"
          ),
      },
    ],
  },
  // Servicio 
  {
    title: "Servicio",
    key: "serviceGroup",
    onHeaderCell: () => ({
      style: {
        backgroundColor: "#bdd7ee",
        fontWeight: 700,
        textAlign: "center",
      },
    }),
    children: [
      {
        title: "ID",
        dataIndex: "serviceId",
        key: "serviceId",
        width: 90,
        align: "center",
        onHeaderCell: () => ({ style: { backgroundColor: "#ddeeff", fontWeight: 600 } }),
        render: (v) => v ?? "—",
      },
      {
        title: "Nombre",
        dataIndex: "serviceName",
        key: "serviceName",
        width: 120,
        align: "center",
        onHeaderCell: () => ({ style: { backgroundColor: "#ddeeff", fontWeight: 600 } }),
        render: (v) => v ?? "—",
      },
    ],
  },
  // Monto
  {
    title: "Monto",
    dataIndex: "amountPaid",
    key: "amountPaid",
    width: 80,
    align: "center",
    onHeaderCell: () => ({ style: { backgroundColor: "#bdd7ee", fontWeight: 700 } }),
    render: (v) =>
      typeof v === "number"
        ? v.toLocaleString("es-HN", { minimumFractionDigits: 2 })
        : "—",
  },
];

// TODO: Implementar useExport
// TODO: filtros para series, usuarios y ubicaciones

export const ReportGenerator = () => {
  const {
    filters,
    updateDateRange,
    updateLocations,
    updateUsers,
    updateServices,
    updatePage,
    clearFilters,
    toQueryParams,
  } = useReportFilters();

  const {
    isLoading,
    isError,
    hasData,
    isGenerated,
    summaryStats,
    hospitalInfo,
    metadata,
    items,
    pagination,
    generateReport,
    changePage,
    resetReport,
    summaryQuery,
    detailQuery,
  } = useReportData();

  const columns = useMemo(() => buildColumns(), []);

  const handleGenerate = () => {
    generateReport(toQueryParams());
  };

  const handleClear = () => {
    clearFilters();
    resetReport();
  };

  const handlePageChange = (page: number, pageSize?: number) => {
    updatePage(page, pageSize);
    changePage(page, pageSize ?? filters.pageSize);
  };

  // Valores controlados
  const executedSeriesLabel = summaryStats?.executedSeries?.join(", ") ?? "N/A";

  const doctorLabel = hospitalInfo
    ? `${hospitalInfo.directorName ?? "—"} | Tel: ${hospitalInfo.contact?.phoneNumber ?? "—"}`
    : "—";

  const hospitalName = hospitalInfo?.hospitalName ?? "Hospital";

  const generatedAt = metadata?.generatedAt
    ? dayjs(metadata.generatedAt).format("DD/MM/YYYY, HH:mm")
    : dayjs().format("DD/MM/YYYY, HH:mm");

  const generatedByUser = metadata?.generatedByUserName
    ? "Usuario | " + metadata.generatedByUserName
    : "Usuario | Desconocido";
  const generatedByRole = metadata?.generatedByRoleName
    ? " | " + metadata.generatedByRoleName
    : " | Rol desconocido";

  // Mensaje de estado
  const errorMessages: string[] = [];
  if (summaryQuery.error) {
    errorMessages.push(
      `${(summaryQuery.error as any)?.message ?? "Error al obtener resumen"}`,
    );
  }
  if (detailQuery.error) {
    errorMessages.push(
      `${(detailQuery.error as any)?.message ?? "Error al obtener detalles"}`,
    );
  }
 const { getMediaUrl } = useMediaFiles();

  console.log("Filters:", items);
  console.log(hospitalInfo?.urlLogo);
  console.log(hospitalInfo?.urlLogoHealth);
  return (
    <div className="flex flex-col gap-4">
      {isError && errorMessages.length > 0 && (
        <Alert
          type="error"
          showIcon
          message="Errores al cargar el reporte"
          description={
            <ul className="m-0 pl-4">
              {errorMessages.map((msg) => (
                <li key={msg}>{msg}</li>
              ))}
            </ul>
          }
          action={
            <Button
              size="small"
              icon={<ReloadOutlined />}
              onClick={handleGenerate}
            >
              Reintentar
            </Button>
          }
        />
      )}

      <div className="flex gap-4">
        {/* Filtros */}
        <div className="primary-card w-full">
          {/* Título */}
          <div className="flex items-center gap-2 mb-4">
            <FilterOutlined className="text-lg" />
            <span className="text-lg">Seleccionar parámetros</span>
          </div>

          {/* Selectores */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
            {/* Ubicaciones */}
            <div>
              <Text className="block mb-2">Ubicaciones</Text>
              <Select
                mode="multiple"
                placeholder="Seleccionar"
                className="w-full"
                onChange={updateLocations}
              />
            </div>

            {/* Usuarios */}
            <div>
              <Text className="block mb-2">Usuarios</Text>
              <Select
                mode="multiple"
                placeholder="Seleccionar"
                className="w-full"
                onChange={updateUsers}
              />
            </div>

            {/* Servicios */}
            <div>
              <Text className="block mb-2">Servicios</Text>
              <Select
                mode="multiple"
                placeholder="Seleccionar"
                className="w-full"
                onChange={updateServices}
              />
            </div>
          </div>

          {/* Rango de Fecha */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
            <div>
              <Text className="block mb-2">Rango de fechas</Text>
              <RangePicker
                className="w-full"
                value={filters.dateRange as any}
                onChange={updateDateRange as any}
                format="DD/MM/YYYY"
                allowClear={false}
              />
            </div>
          </div>

          {/* Botones */}
          <div className="flex flex-wrap items-center gap-4">
            <Button
              type="primary"
              icon={<CheckOutlined />}
              onClick={handleGenerate}
              loading={isLoading}
            >
              Generar Reporte
            </Button>
            <Button icon={<ClearOutlined />} onClick={handleClear}>
              Limpiar
            </Button>
          </div>
        </div>

        {/* Opciones de Exportación */}
        <div className="primary-card w-1/2">
          {/* Título */}
          <div className="flex items-center gap-2 mb-6">
            <DownloadOutlined className="text-lg" />
            <span className="text-lg">Opciones de exportación</span>
          </div>

          {/* Botones */}
          <Space direction="vertical" className="w-full" size="middle">
            <Button
              type="primary"
              danger
              icon={<FilePdfOutlined />}
              className="w-full"
              size="large"
            >
              Exportar a PDF
            </Button>
            <Button
              type="primary"
              className="w-full bg-green-600! hover:bg-green-400! text-white"
              icon={<FileExcelOutlined />}
              size="large"
            >
              Exportar a Excel
            </Button>
            <Button icon={<PrinterOutlined />} className="w-full" size="large">
              Imprimir Reporte
            </Button>
          </Space>
        </div>
      </div>

      {/* Vista previa del reporte */}
      <div className="primary-card">
        {/* Título */}
        <div className="flex items-center gap-2 mb-6">
          <FileTextOutlined className="text-lg" />
          <span className="text-lg">Vista previa del reporte</span>
        </div>

        {/* Documento */}
        <Spin spinning={isLoading} tip="Cargando reporte...">
          <div className="border border-gray-300 rounded p-6 bg-white">
            {/* Encabezado */}
            <div className="flex justify-between items-start mb-6">
              {/* Logos */}
              <div className="flex items-center gap-4">
                <div>
                  <img
                    src={
                     getMediaUrl(hospitalInfo?.urlLogoHealth) ??
                      "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f1/Logo_de_SESAL.svg/1200px-Logo_de_SESAL.svg.png"
                    }
                    alt="Gobierno de Honduras"
                    className="h-12 object-contain"
                  />
                </div>
                <div>
                  <img
                    src={
                      getMediaUrl(hospitalInfo?.urlLogo) ??
                      "https://krti.cl/wp-content/uploads/2021/04/Logo-Hospital-Final.png"
                    }
                    alt={hospitalName ?? "Hospital"}
                    className="h-12 object-contain"
                  />
                </div>
              </div>
              {/* Titulo */}
              <div className="text-right">
                <Title level={5} className="m-0">
                  Sistema de Gestión de Receptoría de Fondos (SIGREF)
                </Title>
                <div className="text-right">
                  <div>
                    <Text strong>Encargado:</Text> {doctorLabel}
                  </div>
                  <div>
                    <Text strong>Rango de fechas:</Text>{" "}
                    <Text className="ml-2">
                      {filters.dateRange[0]?.format("DD-MM-YYYY")} a{" "}
                      {filters.dateRange[1]?.format("DD-MM-YYYY")}
                    </Text>
                  </div>
                </div>
              </div>
            </div>

            {/* Tabla de datos */}
            <div className="flex flex-col gap-2 mb-6">
              <Title level={5} className="text-center">
                Detalles del Reporte
              </Title>
              {!isGenerated && !isLoading ? (
                <div className="text-center py-10 text-gray-400">
                  Configure los filtros y haga clic en{" "}
                  <strong>Generar Reporte</strong> para visualizar los datos.
                </div>
              ) : (
                <Table<ReportLineDto>
                  columns={columns}
                  dataSource={items as ReportLineDto[]}
                  rowKey={(r) =>
                    `${r.transactionDate}-${r.receiptNumber}-${r.cashierIdentity}`
                  }
                  pagination={false}
                  size="small"
                  bordered
                  scroll={{ x: "max-content" }}
                />
              )}
            </div>
            {/* Paginación */}
            {hasData && pagination && (
              <div className="flex justify-end mb-4">
                <Pagination
                  current={pagination.currentPage}
                  pageSize={pagination.pageSize}
                  total={pagination.totalItems ?? 0}
                  showSizeChanger
                  pageSizeOptions={["10", "20", "50", "100"]}
                  showTotal={(total, range) =>
                    `${range[0]}-${range[1]} de ${total} registros`
                  }
                  onChange={handlePageChange}
                  disabled={isLoading}
                />
              </div>
            )}

            {/* Estadísticas */}
            <div className="flex items-center justify-evenly py-2 border-t border-gray-200 mt-2">
              <div>
                <div>
                  <Text strong>Total de Transacciones:</Text>{" "}
                  {summaryStats?.totalTransactions ?? "—"}
                </div>
                <div>
                  <Text strong>Total de Ingresos:</Text>{" "}
                  {typeof summaryStats?.totalCollected === "number"
                    ? `L ${summaryStats.totalCollected.toLocaleString("es-HN", {
                        minimumFractionDigits: 2,
                      })}`
                    : "—"}
                </div>
              </div>
              <div>
                <div>
                  <Text strong>Servicios Exonerados:</Text>{" "}
                  {summaryStats?.exoneratedServicesCount ?? "—"}
                </div>
                <div>
                  <Text strong>Servicios Pagados:</Text>{" "}
                  {summaryStats?.paidServicesCount ?? "—"}
                </div>
              </div>
              <div>
                <div>
                  <Text strong>Series Ejecutadas:</Text> {executedSeriesLabel}
                </div>
                <div>
                  <Text strong>Recibos Cancelados:</Text>{" "}
                  {summaryStats?.canceledServicesCount ?? "—"}
                </div>
              </div>
            </div>

            {/* Pie de página */}
            <div className="mt-6">
              <div className="text-center mb-1 text-gray-600 text-sm">
                Este reporte fue generado el {generatedAt}
              </div>
              <div className="text-center text-sm text-gray-600">
                {hospitalName} - SIGREF
              </div>
              <div className="text-center text-xs text-gray-500">
                Usuario | {generatedByUser} | {generatedByRole}
              </div>

              {/* Debug info — remove before production */}
              {hasData && pagination && (
                <div className="text-center text-xs text-gray-400 mt-2">
                  Página {pagination.currentPage} de {pagination.totalPages}{" "}
                  &nbsp;|&nbsp;
                  {pagination.hasPrevious ? "← Anterior" : ""}{" "}
                  {pagination.hasNext ? "Siguiente →" : ""}
                </div>
              )}
            </div>
          </div>
        </Spin>
      </div>
    </div>
  );
};
