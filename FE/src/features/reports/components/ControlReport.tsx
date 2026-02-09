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
import { Select, DatePicker,  Alert } from "antd";
import dayjs from "dayjs";
import { useDashboardData } from "../hooks/useDashboardData";
import { EmptyState } from "./EmpityState";
import { DashboardSkeleton } from "./DashboardSkeleton";

const { RangePicker } = DatePicker;

const COLORS = ["#10b981", "#3b82f6", "#8b5cf6", "#f59e0b", "#ec4899"];

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
    totalErrors,
    serviciosMasSolicitados,
    serviciosMenosSolicitados,
    paquetesMasUtilizados,
    ingresosDiarios,
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
    dates: [dayjs.Dayjs | null, dayjs.Dayjs | null],
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
    return (
    <DashboardSkeleton/>
    );
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
              onChange={() => handleDateChange}
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
                <p className="text-2xl font-bold">L. {totalIncome || "0.00"}</p>
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
                <p className="text-2xl font-bold">{totalServices || "0"}</p>
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
                <p className="text-2xl font-bold">{totalPatients || "0"}</p>
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
                  {totalErrors || "0"}
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
          {/* Servicios Más Solicitados */}
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h3 className="text-lg text-general font-semibold mb-4 flex items-center gap-2">
              <TrophyOutlined style={{ color: "#f59e0b" }} />
              Servicios Más Solicitados
            </h3>
            {serviciosMasSolicitados && serviciosMasSolicitados.length > 0 ? (
              <div className="space-y-3">
                {serviciosMasSolicitados?.slice(0, 5).map((servicio, index) => (
                  <div
                    key={index}
                    className="flex items-center justify-between p-3 bg-linear-to-r from-blue-50 to-transparent rounded hover:from-blue-100 transition-colors"
                  >
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center font-bold text-white">
                        {index + 1}
                      </div>
                      <span className="text-sm font-medium">
                        {servicio.nombre}
                      </span>
                    </div>
                    <span className="font-bold text-blue-600 text-lg">
                      {servicio.cantidad}
                    </span>
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

          {/* Servicios Menos Solicitados */}
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h3 className="text-lg text-general font-semibold mb-4 flex items-center gap-2">
              <FallOutlined style={{ color: "#f59e0b" }} />
              Servicios Menos Solicitados
            </h3>
            {serviciosMenosSolicitados &&
            serviciosMenosSolicitados.length > 0 ? (
              <div className="space-y-3">
                {serviciosMenosSolicitados
                  ?.slice(0, 5)
                  .map((servicio, index) => (
                    <div
                      key={index}
                      className="flex items-center justify-between p-3 bg-linear-to-r from-orange-50 to-transparent rounded hover:from-orange-100 transition-colors"
                    >
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 bg-orange-500 rounded-full flex items-center justify-center font-bold text-white">
                          {index + 1}
                        </div>
                        <span className="text-sm font-medium">
                          {servicio.nombre}
                        </span>
                      </div>
                      <span className="font-bold text-orange-600 text-lg">
                        {servicio.cantidad}
                      </span>
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

          {/* Paquetes Más Utilizados */}
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
              <BoxPlotOutlined style={{ color: "#8b5cf6" }} />
              Paquetes Más Utilizados
            </h3>
            {paquetesMasUtilizados && paquetesMasUtilizados.length > 0 ? (
              <>
                <ResponsiveContainer width="100%" height={200}>
                  <PieChart>
                    <Pie
                      data={paquetesMasUtilizados}
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
                      dataKey="cantidad"
                    >
                      {paquetesMasUtilizados?.map((_entry, index) => (
                        <Cell
                          key={`cell-${index}`}
                          fill={COLORS[index % COLORS.length]}
                        />
                      ))}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
                <div className="mt-4 space-y-2">
                  {paquetesMasUtilizados?.map((paquete, index) => (
                    <div
                      key={index}
                      className="flex items-center justify-between text-xs"
                    >
                      <div className="flex items-center gap-2">
                        <div
                          className="w-3 h-3 rounded-full"
                          style={{
                            backgroundColor: COLORS[index % COLORS.length],
                          }}
                        ></div>
                        <span>{paquete.nombre}</span>
                      </div>
                      <span className="font-bold">{paquete.cantidad}</span>
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
            )}
            <p className="text-xs text-general-secondary mt-2">
              {getPeriodoLabel()}
            </p>
          </div>
        </div>

        {/* Gráficos principales */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
          {/* Ingresos Diarios */}
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
                    <XAxis dataKey="dia" />
                    <YAxis />
                    <Tooltip
                      formatter={(value) => `L. ${value.toLocaleString()}`}
                    />
                    <Bar
                      dataKey="ingreso"
                      fill="#3b82f6"
                      radius={[8, 8, 0, 0]}
                    />
                  </BarChart>
                </ResponsiveContainer>
                <div className="mt-4 p-3 bg-blue-50 rounded">
                  <p className="text-sm text-general">Total de Ingreso:</p>
                  <p className="text-xl font-bold text-blue-600">
                    L. {totalIncome || "0.00"}
                  </p>
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

          {/* Ingreso por Módulo */}
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
                    <XAxis dataKey="modulo" />
                    <YAxis />
                    <Tooltip />
                    <Bar
                      dataKey="ingreso"
                      fill="#10b981"
                      radius={[8, 8, 0, 0]}
                    />
                  </BarChart>
                </ResponsiveContainer>
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
          {/* Ingresos Por Turno */}
          <div className="bg-white p-6 rounded-lg shadow-md">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg text-general font-semibold flex items-center gap-2">
                <MedicineBoxOutlined style={{ color: "#8b5cf6" }} />
                Ingresos Por turno
              </h3>
            </div>
            {ingresosPorTurno && ingresosPorTurno.length > 0 ? (
              <>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={ingresosPorTurno} layout="vertical">
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis type="number" />
                    <YAxis dataKey="turno" type="category" />
                    <Tooltip />
                    <Legend />
                    <Bar
                      dataKey="consultaExterna"
                      name="Consulta Externa"
                      fill="#10b981"
                      radius={[0, 8, 8, 0]}
                    />
                    <Bar
                      dataKey="emergencia"
                      name="Emergencia"
                      fill="#3b82f6"
                      radius={[0, 8, 8, 0]}
                    />
                  </BarChart>
                </ResponsiveContainer>
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

          {/* Comparativo Semanal */}
          <div className="bg-white p-6 rounded-lg shadow-md">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg text-general font-semibold flex items-center gap-2">
                <FundOutlined style={{ color: "#ff8000" }} />
                Comparativo de Ingresos{" "}
                {periodo === "semana" ? "Semanal" : "Mensual"}
              </h3>
            </div>
            {ingresosDiarios && ingresosDiarios.length > 0 ? (
              <>
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={ingresosDiarios}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="periodo" />
                    <YAxis />
                    <Tooltip
                      formatter={(value) => `L. ${value.toLocaleString()}`}
                    />
                    <Legend />
                    <Line
                      type="monotone"
                      dataKey="ingresos"
                      stroke="#ff8000"
                      strokeWidth={3}
                      name="Ingresos"
                      dot={{ fill: "#ff8000", r: 6 }}
                    />
                  </LineChart>
                </ResponsiveContainer>
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
