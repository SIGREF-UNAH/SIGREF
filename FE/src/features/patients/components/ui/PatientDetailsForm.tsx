import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProTable,
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
import { Badge, Button, Spin, Tag, Typography } from "antd";
import type { ProColumns } from "@ant-design/pro-components";
import { Link } from "react-router-dom";
import { useDetailsPatient } from "../../hooks";
import { BiTrash } from "react-icons/bi";

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
  const {
    id,
    isLoading,
    error,
    selectedPatient,
    patients,
    loadingPatients,
    paginationConfig,
    contextHolder,
    handleCopyData,
    deletePatient,
    filters,
    setFilter,
  } = useDetailsPatient();

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

  console.log(selectedPatient);

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

  return (
    <div className="space-y-6">
      {contextHolder}
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
          <div className="flex gap-2 ">
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
            <Button
              type="primary"
              icon={<BiTrash />}
              className="!bg-red-600 !hover:bg-red-400"
              danger
              onClick={() => deletePatient({ id: (selectedPatient as any).id })}
            >
              Eliminar
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

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                {/* Dirección principal (Casa) */}
                <div>
                  <div className="font-medium mb-1">Casa:</div>
                  <div className="text-muted-foreground text-xs">
                    {selectedPatient.casaDireccion || "No registrada"}
                  </div>
                  <div className="font-medium mt-1">Detalles:</div>
                  <div className="text-muted-foreground text-xs">
                    {selectedPatient.casaDetalles || "No registrados"}
                  </div>
                </div>

                {/* Segunda dirección (Trabajo) */}
                <div>
                  <div className="font-medium mb-1">
                    Segunda dirección (Lugar):
                  </div>
                  <div className="text-muted-foreground text-xs">
                    {selectedPatient.trabajoDireccion || "No registrada"}
                  </div>
                  <div className="font-medium mt-1">Detalles:</div>
                  <div className="text-muted-foreground text-xs">
                    {selectedPatient.trabajoDetalles || "No registrados"}
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

      {/* Contenedor de los filtros */}
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
                { label: "DNI", value: "dni_hn" },
                { label: "PST", value: "pasaporte" },
                { label: "CDL", value: "cedula" },
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
                { label: "Sin vida", value: "Fallecido" },
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
          loading={isLoading || loadingPatients}
          pagination={paginationConfig}
          scroll={{ x: 1200 }}
        />
      </div>
    </div>
  );
}
