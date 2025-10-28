import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProTable,
  type ProColumns,
  ProFormDatePicker,
} from "@ant-design/pro-components";
import { Tag } from "antd";
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
    filters,
    setFilter,
    patients,
    paginationConfig,
    isLoading,
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
      render: (_, record) => {
        const estado = record.estadoVital?.trim().toLowerCase();
        return (
          <Tag color={estado === "vivo" ? "green" : "red"}>
            {record.estadoVital}
          </Tag>
        );
      },
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
            initialValues={filters}
            onValuesChange={(changedValues, allValues) => {
              Object.keys(changedValues).forEach((key) => {
                setFilter(key as keyof typeof filters, allValues[key]);
              });
            }}
          >
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
                  { label: "PST", value: "PPT" },
                  { label: "CDL", value: "NI" },
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
                  { label: "D", value: "D" },
                ]}
                placeholder="Todos"
              />

              {/* <ProFormText
                name="nacionalidad"
                label="Nacionalidad"
                placeholder="Todos"
              /> */}

              <ProFormSelect
                name="estadoVital"
                label="Estado Vital"
                options={[
                  { label: "Todos", value: "todos" },
                  { label: "Vivo", value: "Vivo" },
                  { label: "Sin vida", value: "Sin vida" },
                ]}
                placeholder="Todos"
              />

              <ProFormDatePicker
                name="fechaNacimiento"
                label="Fecha Nacimiento"
                placeholder="DD / MM / YYYY"
                fieldProps={{ format: "DD/MM/YYYY" }}
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
            loading={isLoading}
            pagination={paginationConfig}
            scroll={{ x: 1200 }}
          />
        </div>
      </div>
    </div>
  );
}
