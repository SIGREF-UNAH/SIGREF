import { useState } from "react";
import {
  Card,
  Button,
  Select,
  Checkbox,
  DatePicker,
  Table,
  Space,
  Typography,
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
} from "@ant-design/icons";
import dayjs from "dayjs";
import { healthcares, locations, reportData, users } from "../store";

const { RangePicker } = DatePicker;
const { Title, Text } = Typography;

// TODO: Revisar bien los datos de la tabla y cambiar color de botones

export const ReportGenerator = () => {
  const [selectedArea, setSelectedArea] = useState(null);
  const [selectedUsers, setSelectedUsers] = useState([]);
  const [selectedServices, setSelectedServices] = useState([]);
  const [dateRange, setDateRange] = useState([
    dayjs().startOf("month"),
    dayjs(),
  ]);
  const [includeCharts, setIncludeCharts] = useState(false);
  const [includeSummary, setIncludeSummary] = useState(false);

  const columns = [
    { title: "N°", dataIndex: "key", key: "key", width: 50 },
    { title: "Fecha", dataIndex: "fecha", key: "fecha", width: 100 },
    { title: "Recibo", dataIndex: "recibo", key: "recibo", width: 100 },
    { title: "Serie", dataIndex: "serie", key: "serie", width: 80 },
    {
      title: "Auxiliar de Caja",
      dataIndex: "auxiliar",
      key: "auxiliar",
      width: 150,
    },
    { title: "Paciente", dataIndex: "paciente", key: "paciente", width: 180 },
    { title: "Servicio", dataIndex: "servicio", key: "servicio", width: 100 },
    {
      title: "Mayor de Edad",
      dataIndex: "mayorEdad",
      key: "mayorEdad",
      width: 100,
    },
    { title: "Monto", dataIndex: "monto", key: "monto", width: 100 },
  ];

  return (
    <div className="flex flex-col gap-4">
      <div className="flex gap-4">
        {/* Filtros */}
        <Card className="primary-card w-full">
          {/* Título */}
          <div className="flex items-center gap-2 mb-4">
            <FilterOutlined 
              className="text-lg"
            />
            <span className="text-lg">
              Seleccionar parámetros
            </span>
          </div>

          {/* Selectores */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
            {/* Ubicaciones */}
            <div>
              <Text className="block mb-2">
                Ubicaciones
              </Text>
              <Select
                mode="multiple"
                placeholder="Seleccionar"
                className="w-full"
                value={selectedArea}
                onChange={setSelectedArea}
                options={locations.map((location) => ({
                  label: location,
                  value: location,
                }))}
              />
            </div>

            {/* Usuarios */}
            <div>
              <Text className="block mb-2">
                Usuarios
              </Text>
              <Select
                mode="multiple"
                placeholder="Seleccionar"
                className="w-full"
                value={selectedUsers}
                onChange={setSelectedUsers}
                options={users.map((user) => ({ label: user, value: user }))}
              />
            </div>

            {/* Servicios */}
            <div>
              <Text className="block mb-2">
                Servicios
              </Text>
              <Select
                mode="multiple"
                placeholder="Seleccionar"
                className="w-full"
                value={selectedServices}
                onChange={setSelectedServices}
                options={healthcares.map((healthcare) => ({
                  label: healthcare,
                  value: healthcare,
                }))}
              />
            </div>
          </div>

          {/* Rango de Fecha */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
            <div>
              <Text className="block mb-2">
                Rango de fechas
              </Text>
              <RangePicker
                className="w-full"
                value={dateRange as any}
                onChange={setDateRange as any}
                format="DD/MM/YYYY"
              />
            </div>
          </div>

          {/* Botones */}
          <div className="flex flex-wrap items-center gap-4">
            <Button type="primary" icon={<CheckOutlined />}>
              Generar Reporte
            </Button>
            <Button icon={<ClearOutlined />}>Limpiar</Button>
            <Checkbox
              checked={includeCharts}
              onChange={(e) => setIncludeCharts(e.target.checked)}
            >
              Agregar Gráficas Generales
            </Checkbox>
            <Checkbox
              checked={includeSummary}
              onChange={(e) => setIncludeSummary(e.target.checked)}
            >
              Resumen Ejecutivo
            </Checkbox>
          </div>
        </Card>

        {/* Opciones de Exportación */}
        <Card className="primary-card w-1/2">
          {/* Título */}
          <div className="flex items-center gap-2 mb-6">
            <DownloadOutlined 
              className="text-lg"
            />
            <span className="text-lg">
              Opciones de exportación
            </span>
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
        </Card>
      </div>

      {/* Vista previa del reporte */}
      <Card className="primary-card">
        {/* Título */}
        <div className="flex items-center gap-2 mb-6">
          <FileTextOutlined 
            className="text-lg"
          />
          <span className="text-lg">
            Vista previa del reporte
          </span>
        </div>

        {/* Documento */}
        <div className="border border-gray-300 rounded p-6 bg-white">
          {/* Encabezado */}
          <div className="flex justify-between items-start mb-6">
            {/* Logos */}
            <div className="flex items-center gap-4">
              <div>
                <img src="https://upload.wikimedia.org/wikipedia/commons/thumb/f/f1/Logo_de_SESAL.svg/1200px-Logo_de_SESAL.svg.png"
                  alt="Gobierno de Honduras"
                  className="h-12"
                />
              </div>
              <div>
                <img
                  src="https://krti.cl/wp-content/uploads/2021/04/Logo-Hospital-Final.png"
                  alt="Hospital de Occidente"
                  className="h-12"
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
                  <Text strong>Encargado:</Text> Dr. Juan Pérez | Tel: (504)
                  2234-5678 Ext. 101
                </div>
                <div>
                  <Text strong>Rango de fechas:</Text>
                  <Text className="ml-2">
                    {dateRange[0]?.format("DD-MM-YYYY")} a{" "}
                    {dateRange[1]?.format("DD-MM-YYYY")}
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
            <Table
              columns={columns}
              dataSource={reportData}
              pagination={false}
              size="small"
              bordered
            />
          </div>

          {/* Estadísticas */}
          <div className="flex items-center justify-evenly">
            <div>
              <div>
                <Text strong>Total de Transacciones:</Text> 2000
              </div>
              <div>
                <Text strong>Total de Ingresos:</Text> 3960.00
              </div>
            </div>
            <div>
              <div>
                <Text strong>Servicios Exonerados:</Text> 504
              </div>
              <div>
                <Text strong>Servicios Pagados:</Text> 10
              </div>
            </div>
            <div>
              <div>
                <Text strong>Series Ejecutadas:</Text> C/2
              </div>
              <div>
                <Text strong>Recibos Cancelados:</Text> 0
              </div>
            </div>
          </div>

          {/* Pie de página */}
          <div>
            <div className="text-center mt-6 mb-2 text-gray-600">
              Este reporte fue generado el{" "}
              {dayjs().format("DD/MM/YYYY, HH:mm")}
            </div>
            <div className="text-center text-sm text-gray-600">
              Hospital de Occidente - SIGREF
            </div>
            <div className="text-center text-xs text-gray-500">
              Usuario | Maria Magdalena | Administrador
            </div>
          </div>
        </div>
      </Card>
    </div>
  );
};
