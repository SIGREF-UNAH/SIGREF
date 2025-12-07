import { useState, type SetStateAction } from "react";
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
import { Select, DatePicker } from "antd";
import dayjs from "dayjs";

const { RangePicker } = DatePicker;

export const ControlReport = () => {
  const [periodo, setPeriodo] = useState("semana");
  const [fechaInicio, setFechaInicio] = useState(null);
  const [fechaFin, setFechaFin] = useState(null);

  const handlePeriodoChange = (value: SetStateAction<string>) => {
    setPeriodo(value);
    if (value !== "personalizado") {
      setFechaInicio(null);
      setFechaFin(null);
    }
  };

  const handleDateChange = (dates: SetStateAction<null>[]) => {
    if (dates) {
      setFechaInicio(dates[0]);
      setFechaFin(dates[1]);
    } else {
      setFechaInicio(null);
      setFechaFin(null);
    }
  };

  const formatearFecha = (fecha: string | number | dayjs.Dayjs | Date | null | undefined) => {
    if (!fecha) return "";
    return dayjs(fecha).format("DD/MM/YYYY");
  };

  // Datos para la semana
  const ingresosDiariosSemanales = [
    { dia: "Lunes", ingreso: 7000 },
    { dia: "Martes", ingreso: 4200 },
    { dia: "Miércoles", ingreso: 4800 },
    { dia: "Jueves", ingreso: 1300 },
    { dia: "Viernes", ingreso: 3400 },
    { dia: "Sábado", ingreso: 2100 },
    { dia: "Domingo", ingreso: 5500 },
  ];

  // Datos para el mes
  const ingresosDiariosMensuales = [
    { dia: "Sem 1", ingreso: 18500 },
    { dia: "Sem 2", ingreso: 22300 },
    { dia: "Sem 3", ingreso: 19800 },
    { dia: "Sem 4", ingreso: 20000 },
  ];

  const ingresosDiarios =
    periodo === "semana" ? ingresosDiariosSemanales : ingresosDiariosMensuales;
  const totalIngresos = periodo === "semana" ? 28300 : 80600;

  // Datos por módulo
  const ingresosPorModuloSemana = [
    { modulo: "Consulta Externa", cantidad: 893, ingreso: 7093 },
    { modulo: "Emergencia", cantidad: 316, ingreso: 3800 },
  ];

  const ingresosPorModuloMes = [
    { modulo: "Consulta Externa", cantidad: 3572, ingreso: 28372 },
    { modulo: "Emergencia", cantidad: 1264, ingreso: 15200 },
  ];

  const ingresosPorModulo =
    periodo === "semana" ? ingresosPorModuloSemana : ingresosPorModuloMes;

  // Datos por turno
  const ingresosPorTurnoSemana = [
    { turno: "Turno A", consultaExterna: 4893, emergencia: 2100 },
    { turno: "Turno B", consultaExterna: 3200, emergencia: 5200 },
    { turno: "Turno C", consultaExterna: 2000, emergencia: 3500 },
  ];

  const ingresosPorTurnoMes = [
    { turno: "Turno A", consultaExterna: 19572, emergencia: 8400 },
    { turno: "Turno B", consultaExterna: 12800, emergencia: 20800 },
    { turno: "Turno C", consultaExterna: 8000, emergencia: 14000 },
  ];

  const ingresosPorTurno =
    periodo === "semana" ? ingresosPorTurnoSemana : ingresosPorTurnoMes;

  // Servicios más solicitados
  const serviciosMasSolicitadosSemana = [
    { nombre: "Consulta General", cantidad: 45 },
    { nombre: "Rayos X", cantidad: 38 },
    { nombre: "Laboratorio Completo", cantidad: 32 },
    { nombre: "Ecografía", cantidad: 28 },
    { nombre: "Electrocardiograma", cantidad: 25 },
  ];

  const serviciosMasSolicitadosMes = [
    { nombre: "Consulta General", cantidad: 180 },
    { nombre: "Rayos X", cantidad: 152 },
    { nombre: "Laboratorio Completo", cantidad: 128 },
    { nombre: "Ecografía", cantidad: 112 },
    { nombre: "Electrocardiograma", cantidad: 100 },
  ];

  const serviciosMasSolicitados =
    periodo === "semana"
      ? serviciosMasSolicitadosSemana
      : serviciosMasSolicitadosMes;

  // Servicios menos solicitados
  const serviciosMenosSolicitadosSemana = [
    { nombre: "Endoscopia", cantidad: 3 },
    { nombre: "Colonoscopia", cantidad: 4 },
    { nombre: "Biopsia", cantidad: 5 },
    { nombre: "Tomografía", cantidad: 7 },
    { nombre: "Resonancia Magnética", cantidad: 8 },
  ];

  const serviciosMenosSolicitadosMes = [
    { nombre: "Endoscopia", cantidad: 12 },
    { nombre: "Colonoscopia", cantidad: 16 },
    { nombre: "Biopsia", cantidad: 20 },
    { nombre: "Tomografía", cantidad: 28 },
    { nombre: "Resonancia Magnética", cantidad: 32 },
  ];

  const serviciosMenosSolicitados =
    periodo === "semana"
      ? serviciosMenosSolicitadosSemana
      : serviciosMenosSolicitadosMes;

  // Paquetes más utilizados
  const paquetesMasUtilizadosSemana = [
    { nombre: "Paquete Prenatal", cantidad: 42 },
    { nombre: "Chequeo Ejecutivo", cantidad: 35 },
    { nombre: "Paquete Cardiológico", cantidad: 28 },
    { nombre: "Análisis Completo", cantidad: 22 },
    { nombre: "Paquete Pediátrico", cantidad: 18 },
  ];

  const paquetesMasUtilizadosMes = [
    { nombre: "Chequeo Ejecutivo", cantidad: 140 },
    { nombre: "Paquete Cardiológico", cantidad: 112 },
    { nombre: "Paquete Prenatal", cantidad: 168 },
    { nombre: "Análisis Completo", cantidad: 88 },
    { nombre: "Paquete Pediátrico", cantidad: 72 },
  ];

  const paquetesMasUtilizados =
    periodo === "semana"
      ? paquetesMasUtilizadosSemana
      : paquetesMasUtilizadosMes;

  const comparativo4ultimossemanas = [
    { periodo: "Semana 1", ingresos: 18500 },
    { periodo: "Semana 2", ingresos: 22300 },
    { periodo: "Semana 3", ingresos: 19800 },
    { periodo: "Semana 4", ingresos: 20000 },
  ];

  const comprartivoUltimos4Meses = [
    { periodo: "Enero", ingresos: 75000 },
    { periodo: "Febrero", ingresos: 82000 },
    { periodo: "Marzo", ingresos: 79000 },
    { periodo: "Abril", ingresos: 86000 },
  ];
  const comparativoMensual =
    periodo === "semana"
      ? comparativo4ultimossemanas
      : comprartivoUltimos4Meses;

  const COLORS = ["#10b981", "#3b82f6", "#8b5cf6", "#f59e0b", "#ec4899"];

  const ingresosTotales = periodo === "semana" ? 20000 : 80600;
  const serviciosRealizados = periodo === "semana" ? 209 : 836;
  const pacientesAtendidos = periodo === "semana" ? 209 : 836;

  const getPeriodoLabel = () => {
    if (periodo === "semana") return "Esta semana";
    if (periodo === "mes") return "Este mes";
    if (periodo === "personalizado" && fechaInicio && fechaFin) {
      return `${formatearFecha(fechaInicio)} - ${formatearFecha(fechaFin)}`;
    }
    return "Seleccione fechas";
  };

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
          {/* TODO:agregar despues  el exportar */}
        </div>
      </div>
      <div>
        {/* Indicador de período personalizado */}
        {periodo === "personalizado" && fechaInicio && fechaFin && (
          <div className="mb-4 p-4 bg-blue-50 border border-blue-200 rounded-lg">
            <div className="flex items-center gap-2">
              <CalendarOutlined style={{ color: "#3b82f6", fontSize: "18px" }} />
              <span className="text-sm font-medium text-blue-800">
                Mostrando datos del {formatearFecha(fechaInicio)} al {formatearFecha(fechaFin)}
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
                  L. {ingresosTotales.toLocaleString()}.00
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
                <p className="text-2xl font-bold">{serviciosRealizados}</p>
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
                <p className="text-2xl font-bold">{pacientesAtendidos}</p>
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
                <p className="text-2xl font-bold text-red-600">5</p>
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
            <div className="space-y-3">
              {serviciosMasSolicitados.map((servicio, index) => (
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
            <div className="space-y-3">
              {serviciosMenosSolicitados.map((servicio, index) => (
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
                  {paquetesMasUtilizados.map((_entry, index) => (
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
              {paquetesMasUtilizados.map((paquete, index) => (
                <div
                  key={index}
                  className="flex items-center justify-between text-xs"
                >
                  <div className="flex items-center gap-2">
                    <div
                      className="w-3 h-3 rounded-full"
                      style={{ backgroundColor: COLORS[index] }}
                    ></div>
                    <span>{paquete.nombre}</span>
                  </div>
                  <span className="font-bold">{paquete.cantidad}</span>
                </div>
              ))}
            </div>
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
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={ingresosDiarios}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="dia" />
                <YAxis />
                <Tooltip
                  formatter={(value) => `L. ${value.toLocaleString()}`}
                />
                <Bar dataKey="ingreso" fill="#3b82f6" radius={[8, 8, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
            <div className="mt-4 p-3 bg-blue-50 rounded">
              <p className="text-sm text-general">Total de Ingreso:</p>
              <p className="text-xl font-bold text-blue-600">
                L. {totalIngresos.toLocaleString()}.00
              </p>
            </div>
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
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={ingresosPorModulo}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="modulo" />
                <YAxis />
                <Tooltip />
                <Bar dataKey="ingreso" fill="#10b981" radius={[8, 8, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
            <div className="mt-4 grid grid-cols-2 gap-4">
              <div className="p-3 bg-green-50 rounded">
                <p className="text-xs text-general">Consulta Externa</p>
                <p className="font-bold">
                  {ingresosPorModulo[0].cantidad} servicios
                </p>
                <p className="text-green-600 font-bold">
                  L. {ingresosPorModulo[0].ingreso.toLocaleString()}.00
                </p>
              </div>
              <div className="p-3 bg-blue-50 rounded">
                <p className="text-xs text-general">Emergencia</p>
                <p className="font-bold">
                  {ingresosPorModulo[1].cantidad} servicios
                </p>
                <p className="text-blue-600 font-bold">
                  L. {ingresosPorModulo[1].ingreso.toLocaleString()}.00
                </p>
              </div>
            </div>
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
            <div className="mt-4 p-3 bg-gray-50 rounded">
              <div className="grid grid-cols-2 gap-2 text-sm">
                <div>
                  <p className="text-general">Consulta Externa:</p>
                  <p className="font-bold text-green-600">
                    L. {ingresosPorTurno[2].consultaExterna.toLocaleString()}.00
                  </p>
                </div>
                <div>
                  <p className="text-general">Emergencia:</p>
                  <p className="font-bold text-blue-600">
                    L. {ingresosPorTurno[2].emergencia.toLocaleString()}.00
                  </p>
                </div>
              </div>
            </div>
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
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={comparativoMensual}>
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
            <div className="mt-4 grid grid-cols-2 gap-4">
              <div className="p-3 bg-purple-50 rounded">
                <p className="text-xs text-general">Promedio Semanal</p>
                <p className="text-xl font-bold text-purple-600">
                  L. 20,150.00
                </p>
              </div>
              <div className="p-3 bg-green-50 rounded">
                <p className="text-xs text-general">Mejor Semana</p>
                <p className="text-xl font-bold text-green-600">L. 22,300.00</p>
              </div>
            </div>
            <p className="text-xs text-general-secondary mt-2">
              {getPeriodoLabel()}
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};