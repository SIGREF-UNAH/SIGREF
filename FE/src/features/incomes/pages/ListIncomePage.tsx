import { PageContainer, ProCard, ProTable, type ProColumns } from "@ant-design/pro-components";
import { Typography, Button, Input } from "antd";
import { useState } from "react";
import { useUrlFilters } from "../../../shared/hooks";
import { EditIncomeModal } from "../components/modals";
interface Income {
  id: number;
  fecha: string;
  recibo: string;
  serie: string;
  modulo: string;
  servicio: string;
  monto: number;
  paciente: string;
  identificador: string;
  auxiliar: string;
}
const listIncomeData = [
  {
    id: 1,
    fecha: "2023-10-01",
    recibo: "A-001",
    serie: "A",
    modulo: "Consultas Externas",
    servicio: "Consulta General",
    monto: 50.0,
    paciente: "Juan Perez",
    identificador: "0801-1990-12345",
    auxiliar: "Maria Lopez",
  },
  {
    id: 2,
    fecha: "2023-10-02",
    recibo: "A-002",
    serie: "A",
    modulo: "Emergencias",
    servicio: "Hemograma Completo",
    monto: 75.0,
    paciente: "Ana Gomez",
    identificador: "0801-1985-54321",
    auxiliar: "Carlos Ruiz",
  },
  {
    id: 3,
    fecha: "2023-10-03",
    recibo: "B-001",
    serie: "B",
    modulo: "Emergencias",
    servicio: "Rayos X",
    monto: 120.0,
    paciente: "Luis Martinez",
    identificador: "0801-1978-67890",
    auxiliar: "Sofia Torres",
  },
  {
    id: 4,
    fecha: "2023-10-04",
    recibo: "C-001",
    serie: "C",
    modulo: "Consultas Externas",
    servicio: "Consulta Pediátrica",
    monto: 60.0,
    paciente: "Carlos Sanchez",
    identificador: "0801-2015-11223",
    auxiliar: "Elena Ramirez",
  },
  {
    id: 5,
    fecha: "2023-10-05",
    recibo: "A-003",
    serie: "A",
    modulo: "Consultas Externas",
    servicio: "Perfil Lipídico",
    monto: 90.0,
    paciente: "Marta Fernandez",
    identificador: "0801-1992-33445",
    auxiliar: "Jorge Castillo",
  },
  {
    id: 6,
    fecha: "2023-10-06",
    recibo: "B-002",
    serie: "B",
    modulo: "Emergencias",
    servicio: "Ultrasonido Abdominal",
    monto: 150.0,
    paciente: "Pedro Alvarez",
    identificador: "0801-1980-55667",
    auxiliar: "Lucia Moreno",
  },
];

export const ListIncomePage = () => {
  const { filters, setFilter, setFilters } = useUrlFilters({
    defaultValues: {
      search: "",
      modulo: "",
      recibo: "",
      auxiliar: "",
      page: 1,
      pageSize: 5,
    },
  });
  const filteredData = listIncomeData.filter((item) => {
    const s = filters.search.toLowerCase();
    return (
      item.paciente.toLowerCase().includes(s) ||
      item.servicio.toLowerCase().includes(s) ||
      item.recibo.toLowerCase().includes(s) ||
      item.auxiliar.toLowerCase().includes(s)
    );
  });

  const [modalVisible, setModalVisible] = useState(false);
  const [selectedIncome, setSelectedIncome] = useState<any>(null);

  const columns: ProColumns<Income>[] = [
    {
      title: "Fecha",
      dataIndex: "fecha",
      key: "fecha",
    },
    {
      title: "Recibo",
      dataIndex: "recibo",
      key: "recibo",
    },
    {
      title: "Serie",
      dataIndex: "serie",
      key: "serie",
    },
    {
      title: "Módulo",
      dataIndex: "modulo",
      key: "modulo",
    },
    {
      title: "Servicio",
      dataIndex: "servicio",
      key: "servicio",
    },
    {
      title: "Monto (L)",
      dataIndex: "monto",
      key: "monto",
      render: (value: any) => `L. ${(value.toFixed(2))}`
    },
    {
      title: "Paciente",
      dataIndex: "paciente",
      key: "paciente",
    },
    {
      title: "Identificador",
      dataIndex: "identificador",
      key: "identificador",
    },
    {
      title: "Auxiliar",
      dataIndex: "auxiliar",
      key: "auxiliar",
    },
    {
      title: "Acciones",
      key: "acciones",
      render: (_: any, record: any) => (
        <Button
          type="link"
          color="red"
          onClick={() => {
            setSelectedIncome(record);
            setModalVisible(true);
          }}
        >
          Invalidar
        </Button>
      ),
    },
  ];

  return (
    <PageContainer
      title={
        <Typography.Title level={3} style={{ margin: 0 }}>
          Gestión de Fondos
        </Typography.Title>
      }
      subTitle={
        <Typography.Text>
          Gestión de contrapartidas, libro contable y egresos
        </Typography.Text>
      }
    >
      <ProCard
        title={<Typography.Title level={4}>Lista de Ingresos</Typography.Title>}
        extra={
          <Input.Search
            placeholder="Buscar por paciente, servicio o recibo..."
            allowClear
            style={{ width: 400 }}
            value={filters.search}
            onChange={(e) => setFilter("search", e.target.value)}
          />
        }
      >
        {/* Lista de Ingresos */}
        <ProTable
          rowKey="id"
          columns={columns}
          dataSource={filteredData}
          search={false}
          pagination={{
            current: filters.page,
            pageSize: filters.pageSize,
            onChange: (page, pageSize) => setFilters({ page, pageSize }),
            showSizeChanger: true,
            pageSizeOptions: ["5", "10", "20"],
          }}
          toolBarRender={false}
        />
      </ProCard>

      {/* Editar */}
      <EditIncomeModal
        modalVisible={modalVisible}
        setModalVisible={setModalVisible}
        selectedIncome={selectedIncome}
      />
    </PageContainer>
  );
};
