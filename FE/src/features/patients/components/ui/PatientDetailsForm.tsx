import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProFormDatePicker,
} from "@ant-design/pro-components";
import {
  UserOutlined,
  IdcardOutlined,
  PhoneOutlined,
  EnvironmentOutlined,
  FilterOutlined,
  CopyOutlined,
  EditOutlined,
} from "@ant-design/icons";
import { Badge, Button, Pagination, Spin, Table, Tag, Typography } from "antd";
import type { ProColumns } from "@ant-design/pro-components";
import { useGetApiPatientsId } from "../../../../api/patients/patients";
import { useGetApiPatients } from "../../../../api/patients/patients";
import { Link, useParams } from "react-router-dom";
import { useState } from "react";

interface PatientData {
  id: string;
  key: string;
  nombre: string;
  tipoId?: string;
  identificador?: string;
  contacto?: string;
  nacimiento?: string;
  nacionalidad?: string;
  genero?: string;
  estadoVital?: string;
}

export default function PatientDetailsForm() {
  const params = useParams();
  const id = params?.id as string;
  const { data, isLoading, error } = useGetApiPatientsId(id);
  const { data: apiPatients } = useGetApiPatients<PatientApi[]>();
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  if (isLoading)
    return (
      <div className="flex justify-center items-center h-screen">
        <Spin size="large" tip="Cargando paciente..." />
      </div>
    );

  if (error)
    return (
      <div className="p-8">
        <Typography.Text type="danger">
          Error al cargar los datos del paciente.
        </Typography.Text>
      </div>
    );

  const phone =
    data?.telecom?.find((t) => t.system?.toLowerCase() === "phone")?.value ??
    "No registrado";
  const email =
    data?.telecom?.find((t) => t.system?.toLowerCase() === "email")?.value ??
    "No registrado";

  const selectedPatient = {
    nombre: data?.name?.[0]?.given?.join(" ") ?? "Desconocido",
    apellidos: data?.name?.[0]?.family ?? "Desconocido",
    fechaNacimiento: data?.birthDate
      ? new Date(data.birthDate).toLocaleDateString("es-HN", {
          day: "2-digit",
          month: "long",
          year: "numeric",
        })
      : "No especificada",
    edad: data?.birthDate
      ? `${Math.floor((Date.now() - new Date(data.birthDate).getTime()) / (365.25 * 24 * 60 * 60 * 1000))} años`
      : "No especificada",
    genero:
      data?.gender === 1
        ? "Masculino"
        : data?.gender === 2
          ? "Femenino"
          : "No especificado",
    nacionalidad: data?.address?.[0]?.country ?? "No registrada",
    estadoVital: data?.active ? "Con Vida" : "Sin Vida",
    dni: data?.identifier?.[0]?.value ?? "No disponible",
    dniEmisor: data?.identifier?.[0]?.system ?? "Desconocido",
    pasaporte: data?.identifier?.[1]?.value ?? "No disponible",
    pasaporteEmisor: data?.identifier?.[1]?.system ?? "Desconocido",
    movil: phone,
    preferido: "Preferido",
    email: email,
    casaDireccion: data?.address?.[0]?.text ?? "No disponible",
    casaDetalles: `${data?.address?.[0]?.city ?? ""}, ${data?.address?.[0]?.country ?? ""}`,
    trabajoDireccion: data?.address?.[1]?.text ?? "No registrada",
    trabajoDetalles: `${data?.address?.[1]?.city ?? ""}, ${data?.address?.[1]?.country ?? ""}`,
  };

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

  const columns: ProColumns<PatientData>[] = [
    {
      title: "Nombre",
      dataIndex: "nombre",
      key: "nombre",
      width: 200,
      fixed: "left",
      render: (_, record) => (
        <Link to={`/patients/details/${record.id}`}>{record.nombre}</Link>
      ),
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
          className={
            record.estadoVital === "vivo" ? "text-green-600" : "text-red-600"
          }
        />
      ),
    },
  ];

  const handleCopyData = () => {
    if (!data) {
      navigator.clipboard.writeText("No hay datos del paciente para copiar.");
      return;
    }

    const info = `
   Nombre: ${selectedPatient.nombre} ${selectedPatient.apellidos}
   Fecha de Nacimiento: ${selectedPatient.fechaNacimiento}
   Edad: ${selectedPatient.edad}
   Género: ${selectedPatient.genero}
   Nacionalidad: ${selectedPatient.nacionalidad}
   Estado Vital: ${selectedPatient.estadoVital}

   DNI: ${selectedPatient.dni} (${selectedPatient.dniEmisor})
   Pasaporte: ${selectedPatient.pasaporte} (${selectedPatient.pasaporteEmisor})

   Móvil: ${selectedPatient.movil}
   Email: ${selectedPatient.email}

   Dirección Casa: ${selectedPatient.casaDireccion}
   Dirección Trabajo: ${selectedPatient.trabajoDireccion}
  `.trim();

    navigator.clipboard.writeText(info);
    alert("Datos del paciente copiados al portapapeles.");
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between"></div>

      {/* Patient Information Card */}
      <div className="bg-card border border-border rounded-lg p-6 shadow-sm">
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-2 text-primary">
            <UserOutlined className="text-lg" />
            <h2 className="text-base font-medium">
              Información del Paciente Seleccionado
            </h2>
          </div>
          <div className="flex gap-2">
            <Button
              type="primary"
              icon={<CopyOutlined />}
              className="bg-primary"
              onClick={handleCopyData}
            >
              Copiar Datos
            </Button>
            <Link to={`/patients/update/${id}`}>
              <Button
                type="primary"
                icon={<EditOutlined />}
                className="bg-green-600 hover:bg-green-700"
              >
                Editar Datos
              </Button>
            </Link>
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
                <span className="text-muted-foreground">
                  {selectedPatient.nombre}
                </span>
              </div>
              <div className="flex">
                <span className="font-medium w-32">Apellidos:</span>
                <span className="text-muted-foreground">
                  {selectedPatient.apellidos}
                </span>
              </div>
              <div className="flex">
                <span className="font-medium w-32">Fecha de Nacimiento:</span>
                <span className="text-muted-foreground">
                  {selectedPatient.fechaNacimiento}
                </span>
              </div>
              <div className="flex">
                <span className="font-medium w-32">Edad:</span>
                <span className="text-muted-foreground">
                  {selectedPatient.edad}
                </span>
              </div>
              <div className="flex">
                <span className="font-medium w-32">Género:</span>
                <span className="text-muted-foreground">
                  {selectedPatient.genero}
                </span>
              </div>
              <div className="flex">
                <span className="font-medium w-32">Nacionalidad:</span>
                <span className="text-muted-foreground">
                  {selectedPatient.nacionalidad}
                </span>
              </div>
              <div className="flex">
                <span className="font-medium w-32">Estado Vital:</span>
                <span className="text-muted-foreground">
                  {selectedPatient.estadoVital}
                </span>
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
                  <div className="text-muted-foreground text-xs">
                    {selectedPatient.casaDireccion}
                  </div>
                  <div className="font-medium mt-1">Detalles:</div>
                  <div className="text-muted-foreground text-xs">
                    {selectedPatient.casaDetalles}
                  </div>
                </div>
                <div>
                  <div className="font-medium mb-1">Trabajo:</div>
                  <div className="text-muted-foreground text-xs">
                    {selectedPatient.trabajoDireccion}
                  </div>
                  <div className="font-medium mt-1">Detalles:</div>
                  <div className="text-muted-foreground text-xs">
                    {selectedPatient.trabajoDetalles}
                  </div>
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
                <div className="text-muted-foreground">
                  {selectedPatient.dni}
                </div>
                <div className="font-medium mt-1">Emisor:</div>
                <div className="text-muted-foreground">
                  {selectedPatient.dniEmisor}
                </div>
              </div>
              <div className="pt-3">
                <div className="font-medium mb-1">Pasaporte:</div>
                <div className="text-muted-foreground">
                  {selectedPatient.pasaporte}
                </div>
                <div className="font-medium mt-1">Emisor:</div>
                <div className="text-muted-foreground">
                  {selectedPatient.pasaporteEmisor}
                </div>
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
                <div className="text-muted-foreground">
                  {selectedPatient.movil}
                </div>
                <div className="text-xs text-muted-foreground mt-1">
                  {selectedPatient.preferido}
                </div>
              </div>
              <div className="pt-3">
                <div className="font-medium mb-1">Email:</div>
                <div className="text-muted-foreground">
                  {selectedPatient.email}
                </div>
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
            <ProFormText
              name="nombrePaciente"
              label="Nombre del Paciente"
              placeholder="Nombre Completo"
            />
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
            <ProFormText
              name="identificador"
              label="Identificador"
              placeholder="---"
            />
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
            <ProFormText
              name="nacionalidad"
              label="Nacionalidad"
              placeholder="Todos"
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
            <ProFormText
              name="contacto"
              label="Contacto"
              placeholder="504999929329"
            />
          </div>
        </ProForm>
      </div>

      {/* Patient Registry Table */}
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
            <span className="text-sm text-gray-600">Registros por Página</span>
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
  );
}
