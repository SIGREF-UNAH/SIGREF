import React, { useState, useMemo } from "react";
import {
  Table,
  Input,
  Select,
  Button,
  Card,
  Space,
  Tag,
  Dropdown,
  message,
} from "antd";
import {
  SearchOutlined,
  FilterOutlined,
  ClearOutlined,
  ExportOutlined,
  EyeOutlined,
  WarningOutlined,
  DownloadOutlined,
  FileExcelOutlined,
  FilePdfOutlined,
} from "@ant-design/icons";
import { useUrlFilters } from "../../../shared/hooks";
import type { ColumnsType } from "antd/es/table";
import jsPDF from "jspdf";
import * as XLSX from "xlsx";
import autoTable from "jspdf-autotable";
import { EmployeeDetailsModal, ErrorClosingModal } from "../components/modals";

const { Option } = Select;

export interface CierreCaja {
  key: string;
  fecha: string;
  turno: string;
  ubicacion: string;
  responsable: string;
  cargo: string;
  apertura: string;
  cierre: string;
  monto: string;
  cuadro: string;
  estado: string;
  confiabilidad: number;
  montoSistema: number;
  montoFisico: number;
  diferencia: number;
  statusColor: "success" | "error";
  motivoError?: string;
}

export const HistoryClosingPage = () => {
  const { filters, setFilter, resetFilters } = useUrlFilters({
    defaultValues: {
      nombre: "",
      turno: "todos",
      modulo: "todos",
      page: 1,
      pageSize: 10,
    },
  });

  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);
  const [modalDetalleVisible, setModalDetalleVisible] = useState(false);
  const [modalAdvertenciaVisible, setModalAdvertenciaVisible] = useState(false);
  const [empleadoSeleccionado, setEmpleadoSeleccionado] =
    useState<CierreCaja | null>(null);

  // Datos de ejemplo con información extendida
  const dataCierres = [
    {
      key: "1",
      fecha: "27/11/2025",
      turno: "Turno A",
      ubicacion: "Consulta Externa",
      responsable: "María González",
      cargo: "Auxiliar de Caja Senior",
      apertura: "07:00 AM",
      cierre: "03:00 PM",
      monto: "L. 5,815.00",
      cuadro: "SI",
      estado: "Activo",
      confiabilidad: 98,
      montoSistema: 5815.0,
      montoFisico: 5815.0,
      diferencia: 0,
      statusColor: "success",
      motivoError: "",
    },
    {
      key: "2",
      fecha: "27/11/2025",
      turno: "Turno A",
      ubicacion: "Consulta Externa",
      responsable: "María González",
      cargo: "Auxiliar de Caja Senior",
      apertura: "07:00 AM",
      cierre: "03:00 PM",
      monto: "L. 5,815.00",
      cuadro: "SI",
      estado: "Activo",
      confiabilidad: 98,
      montoSistema: 5815.0,
      montoFisico: 5815.0,
      diferencia: 0,
      statusColor: "success",
      motivoError: "",
    },
    {
      key: "3",
      fecha: "27/11/2025",
      turno: "Turno A",
      ubicacion: "Consulta Externa",
      responsable: "María González",
      cargo: "Auxiliar de Caja Senior",
      apertura: "07:00 AM",
      cierre: "03:00 PM",
      monto: "L. 5,815.00",
      cuadro: "SI",
      estado: "Activo",
      confiabilidad: 98,
      montoSistema: 5815.0,
      montoFisico: 5815.0,
      diferencia: 0,
      statusColor: "success",
      motivoError: "",
    },
    {
      key: "4",
      fecha: "27/11/2025",
      turno: "Turno A",
      ubicacion: "Consulta Externa",
      responsable: "María González",
      cargo: "Auxiliar de Caja Senior",
      apertura: "07:00 AM",
      cierre: "03:00 PM",
      monto: "L. 4,815.00",
      cuadro: "No",
      estado: "Activo",
      confiabilidad: 95,
      montoSistema: 4315.0,
      montoFisico: 5815.0,
      diferencia: 500.0,
      statusColor: "error",
      motivoError: "Faltante en efectivo",
    },
    {
      key: "5",
      fecha: "27/11/2025",
      turno: "Turno B",
      ubicacion: "Emergencia",
      responsable: "Juan Pérez",
      cargo: "Auxiliar de Caja",
      apertura: "03:00 PM",
      cierre: "11:00 PM",
      monto: "L. 6,200.00",
      cuadro: "SI",
      estado: "Activo",
      confiabilidad: 100,
      montoSistema: 6200.0,
      montoFisico: 6200.0,
      diferencia: 0,
      statusColor: "success",
      motivoError: "",
    },
    {
      key: "6",
      fecha: "27/11/2025",
      turno: "Turno C",
      ubicacion: "Consulta Externa",
      responsable: "Ana Martínez",
      cargo: "Auxiliar de Caja Junior",
      apertura: "11:00 PM",
      cierre: "07:00 AM",
      monto: "L. 5,500.00",
      cuadro: "No",
      estado: "Activo",
      confiabilidad: 88,
      montoSistema: 5750.0,
      montoFisico: 5500.0,
      diferencia: -250.0,
      statusColor: "error",
      motivoError: "Diferencia en arqueo de caja",
    },
  ];

  // Filtrar datos basados en los filtros de URL
  const dataFiltrada = useMemo(() => {
    return dataCierres.filter((item) => {
      const matchNombre =
        !filters.nombre ||
        item.responsable.toLowerCase().includes(filters.nombre.toLowerCase());

      const matchTurno =
        filters.turno === "todos" ||
        item.turno.toLowerCase().includes(filters.turno.replace("turno-", ""));

      const matchModulo =
        filters.modulo === "todos" ||
        item.ubicacion.toLowerCase().includes(filters.modulo);

      return matchNombre && matchTurno && matchModulo;
    });
  }, [dataCierres, filters]);

  const handleVerDetalle = (record: CierreCaja) => {
    setEmpleadoSeleccionado(record);
    setModalDetalleVisible(true);
  };

  const handleVerAdvertencia = (record: CierreCaja) => {
    setEmpleadoSeleccionado(record);
    setModalAdvertenciaVisible(true);
  };

  const copiarNombre = (nombre: string) => {
    navigator.clipboard.writeText(nombre);
    message.success(`Nombre "${nombre}" copiado al portapapeles`);
  };

  const columns: ColumnsType = [
    { title: "Fecha", dataIndex: "fecha", key: "fecha", width: 120 },
    { title: "Turno", dataIndex: "turno", key: "turno", width: 100 },
    {
      title: "Ubicación",
      dataIndex: "ubicacion",
      key: "ubicacion",
      width: 150,
    },
    {
      title: "Responsable",
      dataIndex: "responsable",
      key: "responsable",
      width: 150,
    },
    { title: "Apertura", dataIndex: "apertura", key: "apertura", width: 100 },
    { title: "Cierre", dataIndex: "cierre", key: "cierre", width: 100 },
    {
      title: "Monto Cierre",
      dataIndex: "monto",
      key: "monto",
      width: 130,
      render: (monto) => <span className="font-semibold">{monto}</span>,
    },
    {
      title: "Cuadro",
      dataIndex: "cuadro",
      key: "cuadro",
      width: 80,
      align: "center",
      render: (cuadro, record) => (
        <Tag color={record.statusColor === "success" ? "success" : "error"}>
          {cuadro}
        </Tag>
      ),
    },
    {
      title: "Acciones",
      key: "acciones",
      align: "center",
      width: 120,
      render: (_, record) => (
        <Space size="small">
          <Button
            type="text"
            icon={<EyeOutlined />}
            onClick={() => handleVerDetalle(record as any)}
          />
          {record.statusColor === "error" && (
            <Button
              danger
              type="default"
              icon={<WarningOutlined />}
              onClick={() => handleVerAdvertencia(record as any)}
            />
          )}
        </Space>
      ),
    },
  ];

  const handleLimpiar = () => {
    resetFilters();
  };

  const handlePageChange = (page: number, pageSize: number) => {
    setFilter("page", page);
    setFilter("pageSize", pageSize);
  };

  // Exportar a Excel
  const exportarExcel = () => {
    const ws = XLSX.utils.json_to_sheet(dataFiltrada);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Cierres de Caja");

    XLSX.writeFile(wb, "Historial_Cierres.xlsx");
    message.success("Reporte exportado a Excel correctamente");
  };

  // Exportar a PDF
  const exportarPDF = () => {
    const doc = new jsPDF("landscape");
    doc.setFontSize(14);
    doc.text("Historial de Cierres de Caja", 14, 15);

    autoTable(doc, {
      startY: 25,
      head: [
        [
          "Fecha",
          "Turno",
          "Ubicación",
          "Responsable",
          "Apertura",
          "Cierre",
          "Monto",
          "Cuadro",
        ],
      ],
      body: dataFiltrada.map((row) => [
        row.fecha,
        row.turno,
        row.ubicacion,
        row.responsable,
        row.apertura,
        row.cierre,
        row.monto,
        row.cuadro,
      ]),
    });

    doc.save("Historial_Cierres.pdf");
    message.success("Reporte exportado a PDF");
  };

  const menuExportar = {
    items: [
      {
        key: "excel",
        label: "Exportar a Excel",
        icon: <FileExcelOutlined />,
        onClick: () => {
          exportarExcel();
        },
      },
      {
        key: "pdf",
        label: "Exportar a PDF",
        icon: <FilePdfOutlined />,
        onClick: () => {
          exportarPDF();
        },
      },
    ],
  };

  const rowSelection = {
    selectedRowKeys,
    onChange: (selectedKeys: React.SetStateAction<React.Key[]>) => {
      setSelectedRowKeys(selectedKeys);
    },
  };

  const getConfiabilidadColor = (valor: number) => {
    if (valor >= 95) return "#52c41a";
    if (valor >= 85) return "#faad14";
    return "#ff4d4f";
  };

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      {/* Título de la sección */}
      <h2 className="text-xl font-bold text-gray-800 mb-4">
        Historial de Cierres de Caja
      </h2>

      {/* Filtros */}
      <Card className="mb-6 shadow-sm">
        <div className="flex items-center gap-2 mb-4">
          <FilterOutlined className="text-blue-500" />
          <span className="font-semibold text-gray-700">
            Filtros de Búsqueda
          </span>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Buscar por nombre
            </label>
            <Input
              placeholder="Nombre de empleado"
              prefix={<SearchOutlined />}
              value={filters.nombre}
              onChange={(e) => setFilter("nombre", e.target.value)}
              size="large"
              allowClear
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Filtrar por turno
            </label>
            <Select
              value={filters.turno}
              onChange={(value) => setFilter("turno", value)}
              size="large"
              className="w-full"
            >
              <Option value="todos">Todos los turnos</Option>
              <Option value="turno-a">Turno A</Option>
              <Option value="turno-b">Turno B</Option>
              <Option value="turno-c">Turno C</Option>
            </Select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Filtrar por módulo
            </label>
            <Select
              value={filters.modulo}
              onChange={(value) => setFilter("modulo", value)}
              size="large"
              className="w-full"
            >
              <Option value="todos">Todos los módulos</Option>
              <Option value="consulta">Consulta Externa</Option>
              <Option value="emergencia">Emergencia</Option>
            </Select>
          </div>
        </div>
        <div className="mt-4 flex justify-end">
          <Button icon={<ClearOutlined />} onClick={handleLimpiar} size="large">
            Limpiar
          </Button>
        </div>
      </Card>

      {/* Opciones de Exportación */}
      <Card className="mb-6 shadow-sm">
        <div className="flex items-center justify-between">
          <div>
            <div className="flex items-center gap-2 mb-1">
              <ExportOutlined className="text-blue-500" />
              <span className="font-semibold text-gray-700">
                Opciones de Exportación
              </span>
            </div>
            <p className="text-sm text-gray-500">
              Fecha de Impresión del reporte:{" "}
              {new Date().toLocaleString("es-HN")}
            </p>
          </div>
          <Dropdown menu={menuExportar} placement="bottomRight">
            <Button
              type="primary"
              icon={<DownloadOutlined />}
              size="large"
              className="bg-blue-500 hover:bg-blue-600"
            >
              Exportar tabla
            </Button>
          </Dropdown>
        </div>
      </Card>

      {/* Tabla de Cierres */}
      <Card className="shadow-sm">
        <div className="mb-4 flex items-center gap-2">
          <div className="bg-gray-100 p-2 rounded">
            <SearchOutlined className="text-gray-600" />
          </div>
          <span className="font-semibold text-gray-700">Detalle de Cierre</span>
        </div>
        <Table
          columns={columns}
          dataSource={dataFiltrada}
          rowSelection={rowSelection}
          pagination={{
            current: filters.page,
            pageSize: filters.pageSize,
            total: dataFiltrada.length,
            showSizeChanger: true,
            showTotal: (total, range) => `${range[0]}-${range[1]} de ${total}`,
            locale: { items_per_page: "/ página" },
            pageSizeOptions: ["10", "20", "50", "100"],
            onChange: handlePageChange,
          }}
          scroll={{ x: 1200 }}
          className="custom-table"
          bordered
        />
      </Card>

      {/* Modal de Detalle del Empleado */}
      <EmployeeDetailsModal
        copiarNombre={copiarNombre}
        empleadoSeleccionado={empleadoSeleccionado}
        getConfiabilidadColor={getConfiabilidadColor}
        modalDetalleVisible={modalDetalleVisible}
        setModalDetalleVisible={setModalDetalleVisible}
      />

      {/* Modal de Advertencia - Error en Cierre */}
      <ErrorClosingModal
        setModalAdvertenciaVisible={setModalAdvertenciaVisible}
        modalAdvertenciaVisible={modalAdvertenciaVisible}
        empleadoSeleccionado={empleadoSeleccionado}
        copiarNombre={copiarNombre}
      />
    </div>
  );
};
