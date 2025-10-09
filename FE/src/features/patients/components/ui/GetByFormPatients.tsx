"use client"

import { useState } from "react"
import { ProForm, ProFormText, ProFormSelect, ProFormDatePicker } from "@ant-design/pro-components"
import { ProTable } from "@ant-design/pro-components"
import {
  UserOutlined,
  IdcardOutlined,
  PhoneOutlined,
  EnvironmentOutlined,
  FilterOutlined,
  BookOutlined,
  CopyOutlined,
  EditOutlined,
} from "@ant-design/icons"
import { Badge, Button, Tag } from "antd"
import type { ProColumns } from "@ant-design/pro-components"

interface PatientData {
  key: string
  nombre: string
  identificador: string
  tipoId: string
  contacto: string
  nacimiento: string
  nacionalidad: string
  genero: string
  estadoVital: string
}

const mockData: PatientData[] = [
  {
    key: "1",
    nombre: "Yuvini Perez Andrade Menjivar",
    identificador: "0001-2000-12345",
    tipoId: "DNI",
    contacto: "(+504) 9999-9999",
    nacimiento: "01/09/2025",
    nacionalidad: "HN",
    genero: "H",
    estadoVital: "vivo",
  },
  {
    key: "2",
    nombre: "Yuvini Perez Andrade Menjivar",
    identificador: "0001-2000-12345",
    tipoId: "DNI",
    contacto: "(+504) 9999-9999",
    nacimiento: "01/09/2025",
    nacionalidad: "GUA",
    genero: "H",
    estadoVital: "vivo",
  },
  {
    key: "3",
    nombre: "Yuvini Perez Andrade Menjivar",
    identificador: "0001-2000-00048",
    tipoId: "DNI",
    contacto: "(+504) 9999-9999",
    nacimiento: "01/09/2025",
    nacionalidad: "GER",
    genero: "H",
    estadoVital: "vivo",
  },
  {
    key: "4",
    nombre: "Yuvini Perez Andrade Menjivar",
    identificador: "AE56718K900088",
    tipoId: "PST",
    contacto: "(+504) 9999-9999",
    nacimiento: "01/09/2025",
    nacionalidad: "GER",
    genero: "H",
    estadoVital: "vivo",
  },
  {
    key: "5",
    nombre: "Yuvini Perez Andrade Menjivar",
    identificador: "AE56718K900088",
    tipoId: "PST",
    contacto: "(+504) 9999-9999",
    nacimiento: "01/09/2025",
    nacionalidad: "NIC",
    genero: "H",
    estadoVital: "vivo",
  },
  {
    key: "6",
    nombre: "Yuvini Perez Andrade Menjivar",
    identificador: "AE56718K900088",
    tipoId: "ID",
    contacto: "(+504) 9999-9999",
    nacimiento: "01/09/2025",
    nacionalidad: "COL",
    genero: "H",
    estadoVital: "sin vida",
  },
  {
    key: "7",
    nombre: "Yuvini Perez Andrade Menjivar",
    identificador: "AE56718K900088",
    tipoId: "ID",
    contacto: "yuvini@gm.ad",
    nacimiento: "01/09/2025",
    nacionalidad: "COL",
    genero: "H",
    estadoVital: "sin vida",
  },
  {
    key: "8",
    nombre: "Yuvini Perez Andrade Menjivar",
    identificador: "0001-2000-12345",
    tipoId: "DNI",
    contacto: "juanperez@juaaa",
    nacimiento: "01/09/2025",
    nacionalidad: "HN",
    genero: "H",
    estadoVital: "vivo",
  },
  {
    key: "9",
    nombre: "Yuvini Perez Andrade Menjivar",
    identificador: "0001-2000-12345",
    tipoId: "DNI",
    contacto: "(+504) 9999-9999",
    nacimiento: "01/09/2025",
    nacionalidad: "GUA",
    genero: "H",
    estadoVital: "vivo",
  },
]

export default function GetByPatientsForm() {
  const [selectedPatient] = useState({
    nombre: "Juan Jose",
    apellidos: "Pineda Gutierrez",
    fechaNacimiento: "03 de Agosto de 1999",
    edad: "26 años",
    genero: "Masculino",
    nacionalidad: "Honduras",
    estadoVital: "Con Vida",
    dni: "0001-2000-12345",
    dniEmisor: "RNP",
    pasaporte: "HN-z82jq33",
    pasaporteEmisor: "PSST-(HN)",
    movil: "+504 9999-9999",
    preferido: "Preferido",
    email: "juanp@per.me",
    casaDireccion: "Fisico | Santa Rosa de Copan | Copan | Honduras",
    casaDetalles: "Bo, Santa Teresa, Av13 Calle 12 , Casa verde",
    trabajoDireccion: "Fisico | Santa Rosa de Copan | Copan | Honduras",
    trabajoDetalles: "Bo, Santa Teresa, Av13 Calle 12 , Casa verde",
  })

  const columns: ProColumns<PatientData>[] = [
    {
      title: "Nombre",
      dataIndex: "nombre",
      key: "nombre",
      width: 200,
      fixed: "left",
    },
    {
      title: "Identificador",
      dataIndex: "identificador",
      key: "identificador",
      width: 180,
      render: (_, record) => (
        <div className="flex items-center gap-2">
          <Tag color="blue">{record.tipoId}</Tag>
          <span className="text-sm">{record.identificador}</span>
        </div>
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
      dataIndex: "estadoVital",
      key: "estadoVital",
      width: 120,
      render: (_, record) => (
        <Badge
          status={record.estadoVital === "vivo" ? "success" : "error"}
          text={record.estadoVital}
          className={record.estadoVital === "vivo" ? "text-green-600" : "text-red-600"}
        />
      ),
    },
  ]

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-foreground">Gestión de Pacientes</h1>
      </div>

      {/* Patient Information Card */}
      <div className="bg-card border border-border rounded-lg p-6 shadow-sm">
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-2 text-primary">
            <UserOutlined className="text-lg" />
            <h2 className="text-base font-medium">Información del Paciente Seleccionado</h2>
          </div>
          <div className="flex gap-2">
            <Button type="primary" icon={<CopyOutlined />} className="bg-primary">
              Copiar Datos
            </Button>
            <Button type="primary" icon={<EditOutlined />} className="bg-green-600 hover:bg-green-700">
              Editar Datos
            </Button>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Personal Information */}
          <div className="space-y-4">
            <div className="flex items-center gap-2 text-foreground mb-3">
              <UserOutlined />
              <h3 className="font-medium">Información Personal</h3>
            </div>
            <div className="space-y-2 text-sm">
              <div className="flex">
                <span className="font-medium w-32">Nombre:</span>
                <span className="text-muted-foreground">{selectedPatient.nombre}</span>
              </div>
              <div className="flex">
                <span className="font-medium w-32">Apellidos:</span>
                <span className="text-muted-foreground">{selectedPatient.apellidos}</span>
              </div>
              <div className="flex">
                <span className="font-medium w-32">Fecha de Nacimiento:</span>
                <span className="text-muted-foreground">{selectedPatient.fechaNacimiento}</span>
              </div>
              <div className="flex">
                <span className="font-medium w-32">Edad:</span>
                <span className="text-muted-foreground">{selectedPatient.edad}</span>
              </div>
              <div className="flex">
                <span className="font-medium w-32">Género:</span>
                <span className="text-muted-foreground">{selectedPatient.genero}</span>
              </div>
              <div className="flex">
                <span className="font-medium w-32">Nacionalidad:</span>
                <span className="text-muted-foreground">{selectedPatient.nacionalidad}</span>
              </div>
              <div className="flex">
                <span className="font-medium w-32">Estado Vital:</span>
                <span className="text-muted-foreground">{selectedPatient.estadoVital}</span>
              </div>
            </div>

            {/* Direcciones */}
            <div className="pt-4">
              <div className="flex items-center gap-2 text-foreground mb-3">
                <EnvironmentOutlined />
                <h3 className="font-medium">Direcciones</h3>
              </div>
              <div className="space-y-3 text-sm">
                <div>
                  <div className="font-medium mb-1">Casa:</div>
                  <div className="text-muted-foreground text-xs">{selectedPatient.casaDireccion}</div>
                  <div className="font-medium mt-1">Detalles:</div>
                  <div className="text-muted-foreground text-xs">{selectedPatient.casaDetalles}</div>
                </div>
                <div>
                  <div className="font-medium mb-1">Trabajo:</div>
                  <div className="text-muted-foreground text-xs">{selectedPatient.trabajoDireccion}</div>
                  <div className="font-medium mt-1">Detalles:</div>
                  <div className="text-muted-foreground text-xs">{selectedPatient.trabajoDetalles}</div>
                </div>
              </div>
            </div>
          </div>

          {/* Identificadores */}
          <div className="space-y-4">
            <div className="flex items-center gap-2 text-foreground mb-3">
              <IdcardOutlined />
              <h3 className="font-medium">Identificadores</h3>
            </div>
            <div className="space-y-3 text-sm">
              <div>
                <div className="font-medium mb-1">DNI:</div>
                <div className="text-muted-foreground">{selectedPatient.dni}</div>
                <div className="font-medium mt-1">Emisor:</div>
                <div className="text-muted-foreground">{selectedPatient.dniEmisor}</div>
              </div>
              <div className="pt-3">
                <div className="font-medium mb-1">Pasaporte:</div>
                <div className="text-muted-foreground">{selectedPatient.pasaporte}</div>
                <div className="font-medium mt-1">Emisor:</div>
                <div className="text-muted-foreground">{selectedPatient.pasaporteEmisor}</div>
              </div>
            </div>
          </div>

          {/* Contacto */}
          <div className="space-y-4">
            <div className="flex items-center gap-2 text-foreground mb-3">
              <PhoneOutlined />
              <h3 className="font-medium">Contacto</h3>
            </div>
            <div className="space-y-3 text-sm">
              <div>
                <div className="font-medium mb-1">Móvil:</div>
                <div className="text-muted-foreground">{selectedPatient.movil}</div>
                <div className="text-xs text-muted-foreground mt-1">{selectedPatient.preferido}</div>
              </div>
              <div className="pt-3">
                <div className="font-medium mb-1">Email:</div>
                <div className="text-muted-foreground">{selectedPatient.email}</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Search Filters */}
      <div className="bg-card border border-border rounded-lg p-6 shadow-sm">
        <div className="flex items-center gap-2 text-primary mb-4">
          <FilterOutlined className="text-lg" />
          <h2 className="text-base font-medium">Filtros de Búsqueda</h2>
        </div>

        <ProForm
          submitter={{
            render: () => null,
          }}
          layout="horizontal"
        >
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <ProFormText name="nombrePaciente" label="Nombre del Paciente" placeholder="Nombre Completo" />
            <ProFormSelect
              name="tipoIdentificador"
              label="Tipo de Identificador"
              placeholder="DNI"
              options={[
                { label: "DNI", value: "dni" },
                { label: "Pasaporte", value: "pasaporte" },
                { label: "ID", value: "id" },
              ]}
            />
            <ProFormText name="identificador" label="Identificador" placeholder="---" />
            <ProFormSelect
              name="genero"
              label="Género"
              placeholder="Todos"
              options={[
                { label: "Todos", value: "todos" },
                { label: "Masculino", value: "M" },
                { label: "Femenino", value: "F" },
              ]}
            />
            <ProFormSelect
              name="nacionalidad"
              label="Nacionalidad"
              placeholder="Todos"
              options={[
                { label: "Todos", value: "todos" },
                { label: "Honduras", value: "HN" },
                { label: "Guatemala", value: "GUA" },
                { label: "Nicaragua", value: "NIC" },
              ]}
            />
            <ProFormSelect
              name="estadoVital"
              label="Estado Vital"
              placeholder="Todos"
              options={[
                { label: "Todos", value: "todos" },
                { label: "Vivo", value: "vivo" },
                { label: "Sin Vida", value: "sin_vida" },
              ]}
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
              placeholder="Todos"
              options={[
                { label: "Todos", value: "todos" },
                { label: "Teléfono", value: "telefono" },
                { label: "Email", value: "email" },
              ]}
            />
            <ProFormText name="contacto" label="Contacto" placeholder="504999929329" />
          </div>
        </ProForm>
      </div>

      {/* Patient Registry Table */}
      <div className="bg-card border border-border rounded-lg shadow-sm">
        <div className="p-6 border-b border-border">
          <div className="flex items-center gap-2 text-primary">
            <BookOutlined className="text-lg" />
            <h2 className="text-base font-medium">Registro de Pacientes</h2>
          </div>
        </div>

        <ProTable<PatientData>
          columns={columns}
          dataSource={mockData}
          rowKey="key"
          search={false}
          options={false}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showTotal: (total, range) => `${range[0]}-${range[1]} de ${total}`,
            pageSizeOptions: ["10", "20", "50"],
          }}
          scroll={{ x: 1200 }}
          className="patient-table"
        />
      </div>
    </div>
  )
}
