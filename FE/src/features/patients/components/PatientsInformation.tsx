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
  ExclamationCircleOutlined,
  QuestionCircleOutlined,
  ClearOutlined,
} from "@ant-design/icons";
import type { ProColumns } from "@ant-design/pro-components";
import type { FormInstance } from "antd";
import { Button, Tag, Typography, Popconfirm } from "antd";
import { Link } from "react-router-dom";
import { BiTrash } from "react-icons/bi";
import { useRef, useEffect } from "react";
import { usePatientsInformation } from "../hooks";
import dayjs from "dayjs";

interface PatientData {
  id: string;
  key: string;
  nombre: string;
  identificadorTipo: "DNI" | "PST" | "ID";
  identificador?: string;
  contacto?: string;
  nacimiento?: string;
  nacionalidad?: string;
  genero?: string;
  estadoVital?: string;
}

// TODO: Usar ProDescriptions para la información de los pacientes
// TODO: Corregir campos del formulario de creacion y edicion
//! En el campo de nacionalidad solo devuelve Honduras
//! Limpiar Fecha de Nacimiento no funciona

export default function PatientsInformation() {
  const patientInfoRef = useRef<HTMLDivElement>(null);
  const formRef = useRef<FormInstance>(null);
  
  const {
    selectedPatientId,
    isLoading,
    error,
    selectedPatient,
    patients,
    loadingPatients,
    paginationConfig,
    contextHolder,
    filters,
    handleCopyData,
    handleSelectPatient,
    deletePatient,
    setFilter,
    clearAllFilters,
    getIdentificadorColor,
  } = usePatientsInformation();

  // Scroll automático cuando se selecciona un paciente
  useEffect(() => {
    if (selectedPatientId && patientInfoRef.current) {
      patientInfoRef.current.scrollIntoView({ 
        behavior: "smooth", 
        block: "center" 
      });
    }
  }, [selectedPatientId]);

  // Sincronizar formulario con filtros
  useEffect(() => {
    if (formRef.current) {
      const formValues: any = {
        fechaNacimiento: filters.fechaNacimiento ? dayjs(filters.fechaNacimiento) : undefined,
        genero: filters.genero || undefined,
        tipoIdentificador: filters.tipoIdentificador || undefined,
        estadoVital: filters.estadoVital || undefined,
      };
      formRef.current.setFieldsValue(formValues);
    }
  }, [filters]);

  const handleDeleteConfirm = () => {
    if (selectedPatient?.id) {
      deletePatient({ id: selectedPatient.id });
    }
  };

  const handleClearAllFilters = () => {
    clearAllFilters();
    formRef.current?.resetFields();
  };

  // Función para limpiar un filtro individual
  const handleClearFilter = (filterName: string) => {
    setFilter(filterName as keyof typeof filters, null as any);
    formRef.current?.setFieldValue(filterName, undefined);
  };

  // Verificar si hay filtros activos
  const hasActiveFilters = 
    filters.nombreCompleto || 
    filters.identificador || 
    filters.fechaNacimiento || 
    filters.genero || 
    filters.tipoIdentificador || 
    filters.estadoVital;

  // Manejo de errores
  if (error)
    return (
      <div className="p-8">
        <Typography.Text type="danger">
          Error al cargar los datos del paciente.
        </Typography.Text>
      </div>
    );

  // Columnas de la tabla
  const columns: ProColumns<PatientData>[] = [
    {
      title: "Nombre",
      dataIndex: "nombre",
      key: "nombre",
      width: 200,
      fixed: "left",
      render: (_, record) => (
        <Button
          type="link"
          onClick={() => handleSelectPatient(record.id)}
          className="p-0"
        >
          {record.nombre}
        </Button>
      ),
    },
    {
      title: "Identificador",
      dataIndex: "identificador",
      key: "identificador",
      width: 180,
      render: (_, record) => (
        <span>
          <Tag color={getIdentificadorColor(record.identificadorTipo)}>
            {record.identificadorTipo}
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
      dataIndex: "estadoVital",
      key: "estadoVital",
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
    <div className="space-y-4">
      {contextHolder}

      {/* Información del Paciente */}
      <div ref={patientInfoRef} className="rounded-lg border border-gray-300 bg-white p-6">
        {selectedPatient.id !== "" ? (
          <div>
            {/* Encabezado y Botones */}
            <div className="flex items-center justify-between mb-6">
              <div className="flex items-center gap-2 text-primary">
                <QuestionCircleOutlined className="text-lg" style={{color: "var(--color-primary)"}}/>
                <span className="text-lg font-medium text-primary">
                  Información del Paciente Seleccionado
                </span>
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
                <Link to={`/patients/update/${selectedPatientId}`}>
                  <Button
                    type="primary"
                    icon={<EditOutlined />}
                    className="bg-green-600 hover:bg-green-700"
                  >
                    Editar Datos
                  </Button>
                </Link>
                <Popconfirm
                  title="Eliminar Paciente"
                  description={
                    <div className="max-w-xs">
                      <p className="mb-2">
                        ¿Está seguro de que desea eliminar este paciente?
                      </p>
                      <p className="text-gray-500 text-sm">
                        Esta acción no se puede deshacer.
                      </p>
                    </div>
                  }
                  onConfirm={handleDeleteConfirm}
                  okText="Sí, eliminar"
                  cancelText="Cancelar"
                  okButtonProps={{
                    danger: true,
                  }}
                  icon={<ExclamationCircleOutlined style={{ color: "red" }} />}
                >
                  <Button
                    type="primary"
                    icon={<BiTrash />}
                    className="!bg-red-600 hover:!bg-red-700"
                    danger
                  >
                    Eliminar
                  </Button>
                </Popconfirm>
              </div>
            </div>

            {/* Información */}
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
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
                  {selectedPatient.identificadores.map((id, idx) => (
                    <div key={idx}>
                      <div className="font-medium mb-1">{id.tipo}:</div>
                      <div className="text-muted-foreground">{id.valor}</div>
                      <div className="font-medium mt-1">Emisor:</div>
                      <div className="text-muted-foreground">{id.emisor}</div>
                    </div>
                  ))}
                </div>
              </div>

              {/* Contacto */}
              <div className="space-y-4">
                <div className="flex items-center gap-2 text-foreground mb-3">
                  <PhoneOutlined />
                  <h3 className="font-medium">Contacto</h3>
                </div>
                <div className="space-y-3 text-sm">
                  {[
                    { label: "Móvil", value: selectedPatient.movil },
                    { label: "Email", value: selectedPatient.email },
                    { label: "Fax", value: selectedPatient.fax },
                    { label: "Pager", value: selectedPatient.pager },
                    { label: "URL", value: selectedPatient.url },
                    { label: "SMS", value: selectedPatient.sms },
                    { label: "Otro", value: selectedPatient.other },
                  ]
                    .filter((item) => item.value && item.value !== "No registrado")
                    .map((item) => (
                      <div key={item.label}>
                        <div className="font-medium mb-1">{item.label}:</div>
                        <div className="text-muted-foreground">{String(item.value)}</div>
                      </div>
                    ))}
                </div>
              </div>
            </div>
          </div>
        ) : (
          <Typography.Text type="secondary">
            <QuestionCircleOutlined className="mr-2" />
            No hay paciente seleccionado. Por favor, seleccione un paciente de la
            lista para ver su información.
          </Typography.Text>
        )}
      </div>

      {/* Busqueda y Filtros */}
      <div className="rounded-lg border border-gray-300 bg-white p-6">
        <div className="mb-4 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <FilterOutlined className="text-lg" style={{color: "var(--color-primary)"}}/>
            <span className="text-lg font-medium text-primary">
              Filtros de Búsqueda
            </span>
            {hasActiveFilters && (
              <Tag color="blue">
                {Object.values(filters).filter(v => v && v !== '' && v !== null).length - 2} activos
              </Tag>
            )}
          </div>
          {hasActiveFilters && (
            <Button
              icon={<ClearOutlined />}
              onClick={handleClearAllFilters}
              size="small"
            >
              Limpiar Todos
            </Button>
          )}
        </div>

        <ProForm
          formRef={formRef}
          submitter={false}
          layout="horizontal"
          className="patient-filters"
          onValuesChange={(changedValues, allValues) => {
            Object.keys(changedValues).forEach((key) => {
              let value = allValues[key];
              
              // Manejo especial para fechas
              if (key === 'fechaNacimiento' && value) {
                value = dayjs(value).format('DD-MM-YYYY');
              }
              
              // Si el valor es undefined, null o string vacío, limpiar el filtro
              if (value === undefined || value === null || value === '') {
                setFilter(key as keyof typeof filters, null as any);
              } else {
                setFilter(key as keyof typeof filters, value);
              }
            });
          }}
        >
          <div className="grid gap-y-2 gap-x-4 grid-cols-1 md:grid-cols-2 lg:grid-cols-3">
            <ProFormText
              name="nombreCompleto"
              label="Nombre"
              placeholder="Ej. Juan Perez"
            />

            <ProFormText
              name="identificador"
              label="Identificación"
              placeholder="Ej. 0401202501031"
            />

            <ProFormDatePicker
              name="fechaNacimiento"
              label="Fecha de Nacimiento"
              placeholder="Ej. 23/09/2001"
              width="100%"
              allowClear
              fieldProps={{ 
                format: "DD-MM-YYYY",
                onClear: () => handleClearFilter('fechaNacimiento'),
              }}
            />

            <ProFormSelect
              name="genero"
              label="Género"
              options={[
                { label: "Masculino", value: "Masculino" },
                { label: "Femenino", value: "Femenino" },
              ]}
              placeholder="Por género"
              allowClear
              fieldProps={{
                onClear: () => handleClearFilter('genero'),
              }}
            />

            <ProFormSelect
              name="tipoIdentificador"
              label="Tipo de Identificación"
              options={[
                { label: "DNI", value: "DNI" },
                { label: "Pasaporte", value: "PPN" },
                { label: "Otro", value: "NI" },
              ]}
              placeholder="Por tipo de identificación"
              allowClear
              fieldProps={{
                onClear: () => handleClearFilter('tipoIdentificador'),
              }}
            />

            <ProFormSelect
              name="estadoVital"
              label="Estado Vital"
              options={[
                { label: "Vivo", value: "Vivo" },
                { label: "Fallecido", value: "Fallecido" },
              ]}
              placeholder="Por estado vital"
              allowClear
              fieldProps={{
                onClear: () => handleClearFilter('estadoVital'),
              }}
            />
          </div>
        </ProForm>
      </div>

      {/* Lista de Pacientes */}
      <div className="rounded-lg border border-gray-300 bg-white p-6">
        <div className="mb-4 flex items-center gap-2">
          <UserOutlined className="text-lg" style={{color: "var(--color-primary)"}}/>
          <span className="text-lg font-medium text-primary">
            Lista de Pacientes
          </span>
        </div>

        <ProTable
          columns={columns}
          dataSource={patients as PatientData[]}
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