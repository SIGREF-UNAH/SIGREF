import { useState } from "react";
import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormDatePicker,
} from "@ant-design/pro-components";
import { Button, Table, Tag, Pagination } from "antd";
import { FilterOutlined, UserOutlined, CopyOutlined } from "@ant-design/icons";
import type { ColumnsType } from "antd/es/table";
import { useGetApiPatients } from "../../../../api/patients/patients";
import { Link } from "react-router";

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
    data: apiPatients,
    isLoading,
    isError,
    error,
  } = useGetApiPatients<PatientApi[]>();
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const patients: Patient[] =
    apiPatients?.map((p: PatientApi, index: number) => ({
      id: p.id || String(index),
      key: p.id || String(index),
      nombre:
        p.name?.[0]?.text ??
        p.name?.[0]?.given?.join(" ") ??
        "Nombre no disponible",
      identificadorTipo: p.identifier?.[0]?.type?.text ?? "DNI",
      identificador: p.identifier?.[0]?.value || "-",
      contacto: p.telecom?.[0]?.value || "-",
      nacimiento: p.birthDate || "-",
      nacionalidad: p.nationality || "-",
      genero: p.gender || "-",
      estadoVital: p.active ? "vivo" : "sin vida",
    })) || [];

  const getIdentificadorColor = (tipo: string) => {
    switch (tipo) {
      case "DNI":
        return "blue";
      case "PST":
        return "purple";
      case "ID":
        return "red";
      default:
        return "default";
    }
  };

  const columns: ColumnsType<Patient> = [
    {
      title: "Nombre",
      dataIndex: "nombre",
      key: "nombre",
      width: 250,
      render: (_, record) => (
        <Link to={`/patients/getby/${record.id}`}>{record.nombre}</Link>
      ),
    },
    {
      title: "Identificador",
      key: "identificador",
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
      width: 150,
    },
    {
      title: "Nacimiento",
      dataIndex: "nacimiento",
      key: "nacimiento",
      width: 120,
    },
    {
      title: "Nacionalidad",
      dataIndex: "nacionalidad",
      key: "nacionalidad",
      width: 120,
    },
    {
      title: "Género",
      dataIndex: "genero",
      key: "genero",
      width: 80,
    },
    {
      title: "Estado Vital",
      key: "estadoVital",
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
        {/* Información del Paciente Seleccionado */}
        <div className="mb-6 rounded-lg border-2 border-blue-400 bg-white p-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <UserOutlined className="text-blue-600" />
              <span className="text-lg font-medium text-blue-600">
                Informacion del Paciente Seleccionado
              </span>
            </div>
            <Button icon={<CopyOutlined />}>Copiar Datos</Button>
          </div>
        </div>

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
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
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

              <div>
                <label className="mb-2 block text-sm text-gray-700">
                  Identificador
                </label>
                <div className="flex gap-2">
                  <input
                    type="text"
                    placeholder="—"
                    className="flex-1 rounded border border-gray-300 px-3 py-1.5"
                  />
                </div>
              </div>

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

              <ProFormSelect
                name="nacionalidad"
                label="Nacionalidad"
                options={[
                  { label: "Todos", value: "todos" },
                  { label: "HN", value: "HN" },
                  { label: "GUA", value: "GUA" },
                  { label: "GER", value: "GER" },
                  { label: "NIC", value: "NIC" },
                  { label: "COL", value: "COL" },
                ]}
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

          <Table
            columns={columns}
            dataSource={patients}
            pagination={false}
            scroll={{ x: 1200 }}
            className="patient-table"
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
