import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormDatePicker,
  ProTable,
  type ProColumns,
} from "@ant-design/pro-components";
import { Tag, Pagination } from "antd";
import { FilterOutlined } from "@ant-design/icons";
import { Link } from "react-router";
import { useListPatients } from "../../hooks";

interface Patient {
  id: string;
  key: string;
  nombre: string;
  identificadorTipo: "DNI" | "PST" | "ID";
  identificador: string;
  contacto: string;
  nacimiento: string;
  nacionalidad: string;
  genero: string;
  estadoVital: "vivo" | "sin vida";
}

export default function ListFormPatients() {
  const {
    patients,
    currentPage,
    setCurrentPage,
    pageSize,
    setPageSize,
    getIdentificadorColor,
  } = useListPatients();

  const columns: ProColumns<Patient>[] = [
    {
      title: "Nombre",
      dataIndex: "nombre",
      key: "nombre",
      valueType: "text",
      width: 250,
      render: (_, record) => (
        <Link to={`/patients/details/${record.id}`}>{record.nombre}</Link>
      ),
    },
    {
      title: "Identificador",
      dataIndex: "identificador",
      key: "identificador",
      valueType: "text",
      width: 200,
      render: (_, record) => (
        <span>
          <Tag color={getIdentificadorColor(record.identificadorTipo)}>
            {record.identificadorTipo}:
          </Tag>
          {record.identificador}
        </span>
      ),
    },
    {
      title: "Contacto",
      dataIndex: "contacto",
      key: "contacto",
      valueType: "text",
      width: 150,
    },
    {
      title: "Nacimiento",
      dataIndex: "nacimiento",
      key: "nacimiento",
      valueType: "date",
      width: 120,
    },
    {
      title: "Nacionalidad",
      dataIndex: "nacionalidad",
      key: "nacionalidad",
      valueType: "text",
      width: 120,
    },
    {
      title: "Género",
      dataIndex: "genero",
      key: "genero",
      valueType: "text",
      width: 80,
    },
    {
      title: "Estado Vital",
      dataIndex: "estadoVital",
      key: "estadoVital",
      valueType: "text",
      width: 120,
      render: (_, record) => (
        <Tag color={record.estadoVital === "vivo" ? "green" : "red"}>
          {record.estadoVital}
        </Tag>
      ),
    },
  ];

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="mx-auto max-w-7xl">
        {/* Filtros de Búsqueda */}
        <div className="mb-6 rounded-lg border border-gray-300 bg-white p-6">
          <div className="mb-4 flex items-center gap-2">
            <FilterOutlined className="text-gray-600" />
            <span className="text-lg font-medium text-blue-600">
              Filtros de Búsqueda
            </span>
          </div>

          <ProForm
            submitter={false}
            layout="horizontal"
            className="patient-filters"
          >
            {/* Contenedor de los filtros */}
            <div className="grid grid-cols-1 gap-x-6 gap-y-4 md:grid-cols-2 lg:grid-cols-4">
              <ProFormText
                name="nombreCompleto"
                label="Nombres del Paciente"
                placeholder="Nombre Completo"
              />

              <ProFormSelect
                name="tipoIdentificador"
                label="Tipo de Identificador"
                options={[
                  { label: "DNI", value: "DNI" },
                  { label: "PST", value: "PST" },
                  { label: "ID", value: "ID" },
                ]}
                placeholder="DNI"
              />

              <ProFormText
                name="identificador"
                label="Identificador"
                placeholder="—"
              />

              <ProFormSelect
                name="genero"
                label="Género"
                options={[
                  { label: "Todos", value: "todos" },
                  { label: "H", value: "H" },
                  { label: "M", value: "M" },
                ]}
                placeholder="Todos"
              />

              <ProFormText
                name="nacionalidad"
                label="Nacionalidad"
                placeholder="Todos"
              />

              <ProFormSelect
                name="estadoVital"
                label="Estado Vital"
                options={[
                  { label: "Todos", value: "todos" },
                  { label: "Vivo", value: "vivo" },
                  { label: "Sin vida", value: "sin vida" },
                ]}
                placeholder="Todos"
              />

              <ProFormDatePicker
                name="fechaNacimiento"
                label="Fecha Nacimiento"
                placeholder="DD / MM / YYYY"
                fieldProps={{
                  format: "DD/MM/YYYY",
                }}
              />

              <ProFormSelect
                name="tipoContacto"
                label="Tipo Contacto"
                options={[
                  { label: "Todos", value: "todos" },
                  { label: "Teléfono", value: "telefono" },
                  { label: "Email", value: "email" },
                ]}
                placeholder="Todos"
              />

              <ProFormText
                name="contacto"
                label="Contacto"
                placeholder="50499919292329"
              />
            </div>
          </ProForm>
        </div>

        {/* Registro de Pacientes */}
        <div className="rounded-lg border border-gray-300 bg-white p-6">
          <div className="mb-4 flex items-center gap-2">
            <span className="text-lg font-medium text-blue-600">
              📋 Registro de Pacientes
            </span>
          </div>

          <ProTable
            columns={columns}
            dataSource={patients}
            search={false}
            options={false}
            pagination={false}
            scroll={{ x: 1200 }}
          />

          <div className="mt-4 flex items-center justify-between">
            <span className="text-sm text-gray-600">1-50 of 1,250</span>
            <div className="flex items-center gap-4">
              <span className="text-sm text-gray-600">
                Registros por Página
              </span>
              <select
                value={pageSize}
                onChange={(e) => setPageSize(Number(e.target.value))}
                className="rounded border border-gray-300 px-3 py-1"
              >
                <option value={10}>10</option>
                <option value={20}>20</option>
                <option value={50}>50</option>
              </select>
              <Pagination
                current={currentPage}
                total={1250}
                pageSize={pageSize}
                onChange={setCurrentPage}
                showSizeChanger={false}
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
