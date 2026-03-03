import { useState } from "react";
import {
  DollarOutlined,
  BoxPlotOutlined,
  TeamOutlined,
  WarningOutlined,
  RiseOutlined,
  FallOutlined,
  TrophyOutlined,
  MedicineBoxOutlined,
  FileProtectOutlined,
  FundOutlined,
  CalendarOutlined,
} from "@ant-design/icons";
import {
  BarChart,
  Bar,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
} from "recharts";
import { Select, DatePicker, Alert } from "antd";
import dayjs from "dayjs";
import { EmptyState } from "./EmpityState";
import { DashboardSkeleton } from "./DashboardSkeleton";
import { useDashboardData } from "../hooks";

const { RangePicker } = DatePicker;

const COLORS = ["#10b981", "#3b82f6", "#8b5cf6", "#f59e0b", "#ec4899",  "#e11d48", "#f97316", "#6366f1", "#22d3ee"];

export const ControlReport = () => {
  const [periodo, setPeriodo] = useState<"semana" | "mes" | "personalizado">(
    "semana",
  );
  const [fechaInicio, setFechaInicio] = useState<dayjs.Dayjs | null>(null);
  const [fechaFin, setFechaFin] = useState<dayjs.Dayjs | null>(null);

  const {
    isLoading,
    error,
    totalIncome,
    totalServices,
    totalPatients,
    totalCashierClosuresWithErrors,
    serviciosMasSolicitados,
    serviciosMenosSolicitados,
    paquetesMasUtilizados,
    paquetesMenosUtilizados,
    ingresosDiarios,
    totalIngresosSemanalMensual,
    ingresosPorModulo,
    ingresosPorTurno,
  } = useDashboardData(periodo, fechaInicio, fechaFin);

  const handlePeriodoChange = (value: "semana" | "mes" | "personalizado") => {
    setPeriodo(value);
    if (value !== "personalizado") {
      setFechaInicio(null);
      setFechaFin(null);
    }
  };

  const handleDateChange = (
    dates: [dayjs.Dayjs | null, dayjs.Dayjs | null] | null,
  ) => {
    if (dates) {
      setFechaInicio(dates[0]);
      setFechaFin(dates[1]);
    } else {
      setFechaInicio(null);
      setFechaFin(null);
    }
  };

  const formatearFecha = (fecha: dayjs.Dayjs | null) =>
    fecha ? fecha.format("DD/MM/YYYY") : "";

  const getPeriodoLabel = () => {
    if (periodo === "semana") return "Esta semana";
    if (periodo === "mes") return "Este mes";
    if (periodo === "personalizado" && fechaInicio && fechaFin) {
      return `${formatearFecha(fechaInicio)} - ${formatearFecha(fechaFin)}`;
    }
    return "Seleccione fechas";
  };

  if (isLoading) {
    return <DashboardSkeleton />;
  }

  if (error) {
    return (
      <div className="min-h-screen p-6">
        <Alert
          message="Error al cargar datos"
          description={error}
          type="error"
          showIcon
        />
      </div>
    );
  }

  return (
    <div className="min-h-screen p-3">
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-2xl font-bold text-general">Control de Reportes</h2>
        <div className="flex gap-3 items-center flex-wrap">
          <Select
            value={periodo}
            onChange={handlePeriodoChange}
            size="large"
            className="min-w-[180px]"
            options={[
              { label: "Esta Semana", value: "semana" },
              { label: "Este Mes", value: "mes" },
              { label: "Fecha Personalizada", value: "personalizado" },
            ]}
          />
          {periodo === "personalizado" && (
            <RangePicker
              size="large"
              placeholder={["Fecha Inicio", "Fecha Fin"]}
              format="DD/MM/YYYY"
              onChange={handleDateChange}
              value={fechaInicio && fechaFin ? [fechaInicio, fechaFin] : null}
              suffixIcon={<CalendarOutlined />}
            />
          )}
        </div>
      </div>

      <div>
        {/* Indicador de período personalizado */}
        {periodo === "personalizado" && fechaInicio && fechaFin && (
          <div className="mb-4 p-4 bg-blue-50 border border-blue-200 rounded-lg">
            <div className="flex items-center gap-2">
              <CalendarOutlined
                style={{ color: "#3b82f6", fontSize: "18px" }}
              />
              <span className="text-sm font-medium text-blue-800">
                Mostrando datos del {formatearFecha(fechaInicio)} al{" "}
                {formatearFecha(fechaFin)}
              </span>
            </div>
          </div>
        )}

        {/* Tarjetas superiores */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
          <div className="bg-white p-6 rounded-lg shadow-sm hover:shadow-md transition-shadow">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-general text-sm">Ingresos Totales</p>
                <p className="text-2xl font-bold">
                  L. {totalIncome?.toLocaleString() || "0.00"}
                </p>
                <p className="text-xs text-general-secondary mt-1">
                  {getPeriodoLabel()}
                </p>
              </div>
              <div className="bg-blue-100 p-3 rounded-lg">
                <DollarOutlined
                  style={{ fontSize: "24px", color: "#3b82f6" }}
                />
              </div>
            </div>
          </div>

          <div className="bg-white p-6 rounded-lg shadow-sm hover:shadow-md transition-shadow">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-general text-sm">Servicios Realizados</p>
                <p className="text-2xl font-bold">
                  {totalServices?.toLocaleString() || "0"}
                </p>
                <p className="text-xs text-general-secondary mt-1">
                  {getPeriodoLabel()}
                </p>
              </div>
              <div className="bg-green-100 p-3 rounded-lg">
                <BoxPlotOutlined
                  style={{ fontSize: "24px", color: "#10b981" }}
                />
              </div>
            </div>
          </div>

          <div className="bg-white p-6 rounded-lg shadow-sm hover:shadow-md transition-shadow">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-general text-sm">Pacientes Atendidos</p>
                <p className="text-2xl font-bold">
                  {totalPatients?.toLocaleString() || "0"}
                </p>
                <p className="text-xs text-general-secondary mt-1">
                  {getPeriodoLabel()}
                </p>
              </div>
              <div className="bg-purple-100 p-3 rounded-lg">
                <TeamOutlined style={{ fontSize: "24px", color: "#8b5cf6" }} />
              </div>
            </div>
          </div>

          <div className="bg-white p-6 rounded-lg shadow-sm hover:shadow-md transition-shadow">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-general text-sm">Cierres con Errores</p>
                <p className="text-2xl font-bold text-red-600">
                  {totalCashierClosuresWithErrors || "0"}
                </p>
                <p className="text-xs text-general-secondary mt-1">
                  {getPeriodoLabel()}
                </p>
              </div>
              <div className="bg-red-100 p-3 rounded-lg">
                <WarningOutlined
                  style={{ fontSize: "24px", color: "#ef4444" }}
                />
              </div>
            </div>
          </div>
        </div>

        {/* Gráficos secundarios */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
          {/* Servicios Más Solicitados - CON TOOLTIP MEJORADO */}
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h3 className="text-lg text-general font-semibold mb-4 flex items-center gap-2">
              <TrophyOutlined style={{ color: "#3b82f6" }} />
              Servicios Más Solicitados
            </h3>
            {serviciosMasSolicitados && serviciosMasSolicitados.length > 0 ? (
              <div className="space-y-3 max-h-[450px]">
                {serviciosMasSolicitados?.map((servicio, index) => (
                  <div
                    key={servicio.serviceId || index}
                    className="group relative flex items-center justify-between p-4 bg-gradient-to-r from-blue-50 to-transparent rounded hover:from-blue-100 transition-colors"
                  >
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center font-bold text-white">
                        {index + 1}
                      </div>
                      <span className="text-sm font-medium">
                        {servicio.serviceName}
                      </span>
                    </div>
                    <span className="font-bold text-blue-600 text-lg">
                      {servicio.count}
                    </span>

                    {/* Tooltip personalizado */}
                    <div
                      className="absolute left-0 top-full mt-1 text-xs rounded-lg p-3 shadow-lg
                          opacity-0 invisible group-hover:opacity-100 group-hover:visible
                          transition-all duration-200 z-50 whitespace-nowrap
                          bg-white/95 backdrop-blur-md border border-gray-200"
                    >
                      <div className="space-y-1">
                        <div>
                          Ingresos:{" "}
                          <span className="font-semibold">
                            L.{" "}
                            {servicio.totalGenerated?.toLocaleString() || "0"}
                          </span>
                        </div>
                        <div>
                          Porcentaje:{" "}
                          <span className="font-semibold">
                            {servicio.percentage?.toFixed(1) || "0"}%
                          </span>
                        </div>
                      </div>
                      <div className="absolute -top-1 left-8 w-3 h-3 bg-white border-l border-t border-gray-200 rotate-45"></div>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState
                title="No hay servicios solicitados"
                description="No se encontraron servicios solicitados para el período seleccionado."
                icon={TrophyOutlined}
                iconColor="#3b82f6"
              />
            )}

            <p className="text-xs text-general-secondary mt-2">
              {getPeriodoLabel()}
            </p>
          </div>

          {/* Servicios Menos Solicitados - CON TOOLTIP MEJORADO */}
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h3 className="text-lg text-general font-semibold mb-4 flex items-center gap-2">
              <FallOutlined style={{ color: "#f59e0b" }} />
              Servicios Menos Solicitados
            </h3>
            {serviciosMenosSolicitados &&
            serviciosMenosSolicitados.length > 0 ? (
              <div className="space-y-3 max-h-[450px]">
                {serviciosMenosSolicitados?.map((servicio, index) => (
                  <div
                    key={servicio.serviceId || index}
                    className="group relative flex items-center justify-between p-4 bg-gradient-to-r from-orange-50 to-transparent rounded hover:from-orange-100 transition-colors"
                  >
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-8 bg-orange-500 rounded-full flex items-center justify-center font-bold text-white">
                        {index + 1}
                      </div>
                      <span className="text-sm font-medium">
                        {servicio.serviceName}
                      </span>
                    </div>
                    <span className="font-bold text-orange-600 text-lg">
                      {servicio.count}
                    </span>

                    {/* Tooltip personalizado */}
                          <div
                      className="absolute left-0 top-full mt-1 text-xs rounded-lg p-3 shadow-lg
                          opacity-0 invisible group-hover:opacity-100 group-hover:visible
                          transition-all duration-200 z-50 whitespace-nowrap
                          bg-white/95 backdrop-blur-md border border-gray-200"
                    >
                      <div className="space-y-1">
                        <div>
                          Ingresos:{" "}
                          <span className="font-semibold">
                            L.{" "}
                            {servicio.totalGenerated?.toLocaleString() || "0"}
                          </span>
                        </div>
                        <div>
                          Porcentaje:{" "}
                          <span className="font-semibold">
                            {servicio.percentage?.toFixed(1) || "0"}%
                          </span>
                        </div>
                      </div>
                <div className="absolute -top-1 left-8 w-3 h-3 bg-white border-l border-t border-gray-200 rotate-45"></div>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState
                title="No hay servicios solicitados"
                description="No se encontraron servicios solicitados para el período seleccionado."
                icon={FallOutlined}
                iconColor="#f59e0b"
              />
            )}
            <p className="text-xs text-general-secondary mt-2">
              {getPeriodoLabel()}
            </p>
          </div>

          {/* Paquetes Más Utilizados - CON TOOLTIP */}
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
              <BoxPlotOutlined style={{ color: "#8b5cf6" }} />
              Paquetes Más Utilizados
            </h3>
            {(() => {
              const allPackages = [
                ...(paquetesMasUtilizados || []),
                ...(paquetesMenosUtilizados || []),
              ];

              return allPackages.length > 0 ? (
                <>
                  <ResponsiveContainer width="100%" height={200}>
                    <PieChart>
                      <Pie
                        data={allPackages}
                        cx="50%"
                        cy="50%"
                        labelLine={false}
                        label={({ percent }) =>
                          percent !== undefined
                            ? `${(percent * 100).toFixed(0)}%`
                            : ""
                        }
                        outerRadius={80}
                        fill="#8884d8"
                        dataKey="count"
                        nameKey="packageName"
                      >
                        {allPackages.map((_entry, index) => (
                          <Cell
                            key={`cell-${index}`}
                            fill={COLORS[index % COLORS.length]}
                          />
                        ))}
                      </Pie>
                      <Tooltip
                        contentStyle={{
                          backgroundColor: "rgba(255, 255, 255, 0.95)",
                          backdropFilter: "blur(8px)",
                          borderRadius: "8px",
                          border: "1px solid #e5e7eb",
                          boxShadow: "0 4px 6px -1px rgb(0 0 0 / 0.1)",
                          padding: "12px",
                        }}
                        formatter={(_value, _name, props) => {
                          const pkg = props.payload;
                          return [
                            <div key="tooltip" className="space-y-1">
                              <div className="font-semibold text-sm">
                                {pkg.packageName}
                              </div>
                              <div className="text-xs">
                                Cantidad:{" "}
                                <span className="font-semibold">
                                  {pkg.count}
                                </span>
                              </div>
                              <div className="text-xs text-gray-600">
                                Ingresos:{" "}
                                <span className="font-semibold text-gray-600">
                                  L.{" "}
                                  {pkg.totalGenerated?.toLocaleString() || "0"}
                                </span>
                              </div>
                              <div className="text-xs text-gray-600">
                                Porcentaje:{" "}
                                <span className="font-semibold text-gray-600">
                                  {pkg.percentage?.toFixed(1) || "0"}%
                                </span>
                              </div>
                            </div>,
                          ];
                        }}
                        labelFormatter={() => ""}
                      />
                    </PieChart>
                  </ResponsiveContainer>
                  <div className="mt-1 max-h-[150px]">
                    {allPackages.map((paquete, index) => (
                      <div
                        key={index}
                        className="group relative flex items-center justify-between text-xs px-2 py-1 rounded"
                      >
                        <div className="flex items-center gap-2">
                          <div
                            className="w-3 h-3 rounded-full shrink-0"
                            style={{
                              backgroundColor: COLORS[index % COLORS.length],
                            }}
                          ></div>
                          <span className="text-xs font-medium">
                            {paquete.packageName}
                          </span>
                        </div>
                        <span className="font-bold text-sm">
                          {paquete.count}
                        </span>
                      </div>
                    ))}
                  </div>
                </>
              ) : (
                <EmptyState
                  title="No hay paquetes utilizados"
                  description="No se encontraron paquetes utilizados para el período seleccionado."
                  icon={BoxPlotOutlined}
                  iconColor="#8b5cf6"
                />
              );
            })()}
            <p className="text-xs text-general-secondary mt-4">
              {getPeriodoLabel()}
            </p>
          </div>
        </div>

        {/* Gráficos principales */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
          {/* Ingresos Diarios - MOSTRANDO TODOS LOS DATOS */}
          <div className="bg-white p-6 rounded-lg shadow-md">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg text-general font-semibold flex items-center gap-2">
                <RiseOutlined style={{ color: "#3b82f6" }} />
                Ingresos Diarios
              </h3>
            </div>
            {ingresosDiarios && ingresosDiarios.length > 0 ? (
              <>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={ingresosDiarios}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis
                      dataKey="weekStart"
                      tickFormatter={(value) => dayjs(value).format("DD/MM")}
                    />
                    <YAxis />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "rgba(255, 255, 255, 0.95)",
                        backdropFilter: "blur(8px)",
                        borderRadius: "8px",
                        border: "1px solid #e5e7eb",
                        boxShadow: "0 4px 6px -1px rgb(0 0 0 / 0.1)",
                      }}
                      formatter={(value, _name, props) => {
                        return [
                          <div key="tooltip" className="space-y-1">
                            <div>
                              Ingresos: L. {Number(value).toLocaleString()}
                            </div>
                            <div className="text-xs text-gray-600">
                              Facturas: {props.payload.invoiceCount}
                            </div>
                          </div>,
                        ];
                      }}
                      labelFormatter={(value) =>
                        dayjs(value).format("DD/MM/YYYY")
                      }
                    />
                    <defs>
                      <linearGradient
                        id="colorIngreso"
                        x1="0"
                        y1="0"
                        x2="0"
                        y2="1"
                      >
                        <stop
                          offset="5%"
                          stopColor="#3b82f6"
                          stopOpacity={0.8}
                        />
                        <stop
                          offset="95%"
                          stopColor="#3b82f6"
                          stopOpacity={0.2}
                        />
                      </linearGradient>
                    </defs>
                    <Bar
                      dataKey="totalIncome"
                      fill="url(#colorIngreso)"
                      radius={[6, 6, 0, 0]}
                    />
                  </BarChart>
                </ResponsiveContainer>
                <div className="mt-4 p-3 bg-blue-50 rounded space-y-2">
                  <div className="flex justify-between">
                    <span className="text-sm text-general">
                      Total de Ingresos:
                    </span>
                    <span className="text-xl font-bold text-blue-600">
                      L.{" "}
                      {ingresosDiarios
                        ?.reduce((sum, item) => sum + item.totalIncome, 0)
                        .toLocaleString() || "0.00"}
                    </span>
                  </div>
                  <div className="flex justify-between text-xs">
                    <span className="text-gray-600">Total Facturas:</span>
                    <span className="font-semibold text-gray-700">
                      {ingresosDiarios?.reduce(
                        (sum, item) => sum + item.invoiceCount,
                        0,
                      ) || 0}
                    </span>
                  </div>
                </div>
              </>
            ) : (
              <EmptyState
                title="No hay ingresos diarios"
                description="No se encontraron ingresos diarios para el período seleccionado."
                icon={RiseOutlined}
                iconColor="#3b82f6"
              />
            )}
            <p className="text-xs text-general-secondary mt-2">
              {getPeriodoLabel()}
            </p>
          </div>

          {/* Ingreso por Módulo - MOSTRANDO TODOS LOS DATOS */}
          <div className="bg-white p-6 rounded-lg shadow-md">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg text-general font-semibold flex items-center gap-2">
                <FileProtectOutlined style={{ color: "#10b981" }} />
                Ingreso por Módulo
              </h3>
            </div>
            {ingresosPorModulo && ingresosPorModulo.length > 0 ? (
              <>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={ingresosPorModulo}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="locationName" />
                    <YAxis />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "rgba(255, 255, 255, 0.95)",
                        backdropFilter: "blur(8px)",
                        borderRadius: "8px",
                        border: "1px solid #e5e7eb",
                        boxShadow: "0 4px 6px -1px rgb(0 0 0 / 0.1)",
                      }}
                      formatter={(value, _name, props) => {
                        return [
                          <div key="tooltip" className="space-y-1">
                            <div>
                              Ingresos: L. {Number(value).toLocaleString()}
                            </div>
                            <div className="text-xs text-gray-600">
                              Facturas: {props.payload.totalInvoices}
                            </div>
                          </div>,
                        ];
                      }}
                    />
                    <defs>
                      <linearGradient
                        id="colorIngresoModulo"
                        x1="0"
                        y1="0"
                        x2="0"
                        y2="1"
                      >
                        <stop
                          offset="5%"
                          stopColor="#10b981"
                          stopOpacity={0.8}
                        />
                        <stop
                          offset="95%"
                          stopColor="#10b981"
                          stopOpacity={0.2}
                        />
                      </linearGradient>
                    </defs>
                    <Bar
                      dataKey="totalIncome"
                      fill="url(#colorIngresoModulo)"
                      radius={[6, 6, 0, 0]}
                    />
                  </BarChart>
                </ResponsiveContainer>
                <div className="mt-4 p-3 bg-green-50 rounded space-y-2">
                  <div className="flex justify-between">
                    <span className="text-sm text-general">
                      Total de Ingresos:
                    </span>
                    <span className="text-xl font-bold text-green-600">
                      L.{" "}
                      {ingresosPorModulo
                        ?.reduce((sum, item) => sum + item.totalIncome, 0)
                        .toLocaleString() || "0.00"}
                    </span>
                  </div>
                  <div className="flex justify-between text-xs">
                    <span className="text-gray-600">Total Facturas:</span>
                    <span className="font-semibold text-gray-700">
                      {ingresosPorModulo?.reduce(
                        (sum, item) => sum + item.totalInvoices,
                        0,
                      ) || 0}
                    </span>
                  </div>
                </div>
              </>
            ) : (
              <EmptyState
                title="No hay ingresos por módulo"
                description="No se encontraron ingresos por módulo para el período seleccionado."
                icon={FileProtectOutlined}
                iconColor="#10b981"
              />
            )}
            <p className="text-xs text-general-secondary mt-2">
              {getPeriodoLabel()}
            </p>
          </div>
        </div>

        {/* Gráficos inferiores */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {/* Ingresos Por Turno - MOSTRANDO TODOS LOS DATOS */}
          <div className="bg-white p-6 rounded-lg shadow-md">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg text-general font-semibold flex items-center gap-2">
                <MedicineBoxOutlined style={{ color: "#8b5cf6" }} />
                Ingresos Por Turno
              </h3>
            </div>
            {ingresosPorTurno && ingresosPorTurno.length > 0 ? (
              <>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={ingresosPorTurno}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="shiftName" />
                    <YAxis />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "rgba(255, 255, 255, 0.95)",
                        backdropFilter: "blur(8px)",
                        borderRadius: "8px",
                        border: "1px solid #e5e7eb",
                        boxShadow: "0 4px 6px -1px rgb(0 0 0 / 0.1)",
                      }}
                      formatter={(value, _name, props) => {
                        return [
                          <div key="tooltip" className="space-y-1">
                            <div>
                              Ingresos: L. {Number(value).toLocaleString()}
                            </div>
                            <div className="text-xs text-gray-600">
                              Facturas: {props.payload.totalInvoices}
                            </div>
                            <div className="text-xs text-gray-600">
                              Ubicación: {props.payload.locationName}
                            </div>
                          </div>,
                        ];
                      }}
                    />
                    <Legend />
                    <defs>
                      <linearGradient
                        id="colorIngresoTurno"
                        x1="0"
                        y1="0"
                        x2="0"
                        y2="1"
                      >
                        <stop
                          offset="5%"
                          stopColor="#8b5cf6"
                          stopOpacity={0.8}
                        />
                        <stop
                          offset="95%"
                          stopColor="#8b5cf6"
                          stopOpacity={0.2}
                        />
                      </linearGradient>
                    </defs>
                    <Bar
                      dataKey="totalIncome"
                      name="Total"
                      fill="url(#colorIngresoTurno)"
                      radius={[8, 8, 0, 0]}
                    />
                  </BarChart>
                </ResponsiveContainer>
                <div className="mt-4 p-3 bg-purple-50 rounded space-y-2">
                  <div className="flex justify-between">
                    <span className="text-sm text-general">
                      Total de Ingresos:
                    </span>
                    <span className="text-xl font-bold text-purple-600">
                      L.{" "}
                      {ingresosPorTurno
                        ?.reduce((sum, item) => sum + item.totalIncome, 0)
                        .toLocaleString() || "0.00"}
                    </span>
                  </div>
                  <div className="flex justify-between text-xs">
                    <span className="text-gray-600">Total Facturas:</span>
                    <span className="font-semibold text-gray-700">
                      {ingresosPorTurno?.reduce(
                        (sum, item) => sum + item.totalInvoices,
                        0,
                      ) || 0}
                    </span>
                  </div>
                </div>
              </>
            ) : (
              <EmptyState
                title="No hay ingresos por turno"
                description="No se encontraron ingresos por turno para el período seleccionado."
                icon={MedicineBoxOutlined}
                iconColor="#8b5cf6"
              />
            )}
            <p className="text-xs text-general-secondary mt-2">
              {getPeriodoLabel()}
            </p>
          </div>

          {/* Comparativo Semanal/Mensual */}
          <div className="bg-white p-6 rounded-lg shadow-md">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg text-general font-semibold flex items-center gap-2">
                <FundOutlined style={{ color: "#ff8000" }} />
                Comparativo de Ingresos{" "}
                {periodo === "semana" ? "Semanal" : "Mensual"}
              </h3>
            </div>
            {totalIngresosSemanalMensual &&
            totalIngresosSemanalMensual.length > 0 ? (
              <>
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={totalIngresosSemanalMensual}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="dia" />
                    <YAxis />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: "rgba(255, 255, 255, 0.95)",
                        backdropFilter: "blur(8px)",
                        borderRadius: "8px",
                        border: "1px solid #e5e7eb",
                        boxShadow: "0 4px 6px -1px rgb(0 0 0 / 0.1)",
                      }}
                      formatter={(value) =>
                      {return [
                        <div key="tooltip" className="space-y-1 text-gray-600">
                          <div>
                            Ingresos: L. {Number(value).toLocaleString()}
                          </div>
                        </div>,
                      ]}
                        
                      }
                    />
                    <Legend />
                    <Line
                      type="monotone"
                      dataKey="totalIncome"
                      stroke="#ff8000"
                      name="Ingresos"
                      strokeWidth={4}
                      dot={{
                        r: 4,
                        fill: "#ff8000",
                        strokeWidth: 2,
                        stroke: "#fff",
                      }}
                      activeDot={{ r: 8, strokeWidth: 0 }}
                    />
                  </LineChart>
                </ResponsiveContainer>
                <div className="mt-4 p-6 bg-orange-50 rounded">
                  <div className="flex justify-between">
                    <span className="text-sm text-general">
                      Total de Ingresos:
                    </span>
                    <span className="text-xl font-bold text-orange-600">
                      L.{" "}
                      {totalIngresosSemanalMensual
                        ?.reduce((sum, item) => sum + item.totalIncome, 0)
                        .toLocaleString() || "0.00"}
                    </span>
                  </div>
                </div>
              </>
            ) : (
              <EmptyState
                title="No hay datos para comparar"
                description="No se encontraron datos de ingresos para el período seleccionado."
                icon={FundOutlined}
                iconColor="#ff8000"
              />
            )}
            <p className="text-xs text-general-secondary mt-2">
              {getPeriodoLabel()}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};
