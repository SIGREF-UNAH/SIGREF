import {
  ProForm,
  ProFormText,
  ProFormSelect,
  ProTable,
  ProFormDatePicker,
  ProDescriptions,
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

//! Limpiar Fecha de Nacimiento no funciona
//! De lado de backend se tiene arreglar el filtro de Idetnificacion y Tipo

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
        block: "center",
      });
    }
  }, [selectedPatientId]);

  // Sincronizar formulario con filtros
  useEffect(() => {
    if (formRef.current) {
      const formValues: any = {
        fechaNacimiento: filters.fechaNacimiento
          ? dayjs(filters.fechaNacimiento)
          : undefined,
        genero: filters.genero || undefined,
        tipoIdentificador: filters.tipoIdentificador || undefined,
        estadoVital: filters.estadoVital || undefined,
      };
      formRef.current.setFieldsValue(formValues);
    }
  }, [filters]);

  // Eliminar
  const handleDeleteConfirm = () => {
    if (selectedPatient?.id) {
      deletePatient({ id: selectedPatient.id });
    }
  };

  // Limpiar todos los filtros
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
      width: 230,
      render: (_, record) => (
        <Button
          type="link"
          onClick={() => handleSelectPatient(record.id)}
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
      <div
        ref={patientInfoRef}
        className="rounded-lg border border-gray-300 bg-white p-6"
      >
        {selectedPatient.id !== "" ? (
          <div>
            {/* Encabezado y Botones */}
            <div className="flex items-center justify-between mb-6">
              <div className="flex items-center gap-2 text-primary">
                <QuestionCircleOutlined
                  className="text-lg"
                  style={{ color: "var(--color-primary)" }}
                />
                <span className="text-lg font-medium text-primary">
                  Información del Paciente Seleccionado
                </span>
              </div>
              <div className="flex gap-2">
                <Button
                  type="dashed"
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
                    Editar
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
            <div className="grid grid-cols-2 gap-6">
              {/* Primer columna */}
              <div className="flex flex-col gap-6">
                <ProDescriptions
                  column={1}
                  title={
                    <div className="flex items-center gap-2 text-foreground">
                      <UserOutlined />
                      <span className="font-medium">Información Personal</span>
                    </div>
                  }
                  dataSource={selectedPatient}
                  columns={[
                    {
                      title: "Nombre",
                      dataIndex: "nombre",
                      key: "nombre",
                    },
                    {
                      title: "Apellidos",
                      dataIndex: "apellidos",
                      key: "apellidos",
                    },
                    {
                      title: "Fecha de Nacimiento",
                      dataIndex: "fechaNacimiento",
                      key: "fechaNacimiento",
                    },
                    {
                      title: "Edad",
                      dataIndex: "edad",
                      key: "edad",
                    },
                    {
                      title: "Género",
                      dataIndex: "genero",
                      key: "genero",
                    },
                    {
                      title: "Estado Civil",
                      dataIndex: "estadoCivil",
                      key: "estadoCivil",
                    },
                    {
                      title: "Nacionalidad",
                      dataIndex: "nacionalidad",
                      key: "nacionalidad",
                    },
                    {
                      title: "Estado Vital",
                      dataIndex: "estadoVital",
                      key: "estadoVital",
                      render: (_, record) => {
                        const estado = record.estadoVital?.trim().toLowerCase();
                        return (
                          <Tag color={estado === "vivo" ? "green" : "red"}>
                            {record.estadoVital}
                          </Tag>
                        );
                      },
                    },
                  ]}
                />
              </div>
            
              {/* Segunda columna */}
              <div className="flex flex-col gap-6">
                {/* Identificadores */}
                <ProDescriptions
                  column={1}
                  title={
                    <div className="flex items-center gap-2 text-foreground">
                      <IdcardOutlined />
                      <span className="font-medium">Identificación</span>
                    </div>
                  }
                  dataSource={{
                    identificadores: selectedPatient.identificadores,
                  }}
                  columns={[
                    {
                      dataIndex: "identificadores",
                      key: "identificadores",
                      render: (identificadores) => {
                        const lista = Array.isArray(identificadores)
                          ? identificadores
                          : [];
                        return (
                          <div className="">
                            {lista.length > 0 ? (
                              lista.map((id, idx) => (
                                <div
                                  key={idx}
                                  className="border border-gray-200 rounded-lg p-3"
                                >
                                  <div className="font-medium text-sm">
                                    {id.tipo}{id.emisor !== null && ` (${id.emisor})`}: 
                                    <span className="text-primary text-muted-foreground text-sm">
                                      {" "}{id.valor}
                                    </span>
                                  </div>
                                </div>
                              ))
                            ) : (
                              <div className="text-muted-foreground text-sm">
                                No hay identificadores registrados
                              </div>
                            )}
                          </div>
                        );
                      },
                    },
                  ]}
                />
                {/* Contactos */}
                <ProDescriptions
                  column={1}
                  title={
                    <div className="flex items-center gap-2 text-foreground">
                      <PhoneOutlined />
                      <span className="font-medium">Contactos</span>
                    </div>
                  }
                  dataSource={{ contactos: selectedPatient.contactos }}
                  columns={[
                    {
                      dataIndex: "contactos",
                      key: "contactos",
                      span: 2,
                      render: (contactos) => {
                        const lista = Array.isArray(contactos) ? contactos : [];
                        return (
                          <div className="grid grid-cols-1 gap-4">
                            {lista.length > 0 ? (
                              lista.map((contacto, idx) => (
                                <div
                                  key={idx}
                                  className="border border-gray-200 rounded-lg p-3"
                                >
                                  <div className="font-medium text-sm capitalize">
                                    {contacto.tipo} ({contacto.uso}):{" "}
                                    <span className="text-primary text-muted-foreground text-sm">
                                      {contacto.valor}
                                    </span>
                                  </div>
                                </div>
                              ))
                            ) : (
                              <div className="text-muted-foreground text-sm col-span-2">
                                No hay contactos registradas
                              </div>
                            )}
                          </div>
                        );
                      },
                    },
                  ]}
                />
                {/* Direcciones */}
                <ProDescriptions
                  column={1}
                  title={
                    <div className="flex items-center gap-2 text-foreground">
                      <EnvironmentOutlined />
                      <span className="font-medium">Direcciones</span>
                    </div>
                  }
                  dataSource={{ direcciones: selectedPatient.direcciones }}
                  columns={[
                    {
                      dataIndex: "direcciones",
                      key: "direcciones",
                      span: 2,
                      render: (direcciones) => {
                        const lista = Array.isArray(direcciones) ? direcciones : [];
                        return (
                          <div className="grid grid-cols-1 gap-4">
                            {lista.length > 0 ? (
                              lista.map((direccion, idx) => (
                                <div
                                  key={idx}
                                  className="border border-gray-200 rounded-lg p-3"
                                >
                                  <div className="font-medium text-sm capitalize">
                                    {direccion.tipo}:{" "}
                                    <span className="text-primary text-muted-foreground text-sm">
                                      {direccion.valor}
                                    </span>
                                  </div>
                                </div>
                              ))
                            ) : (
                              <div className="text-muted-foreground text-sm col-span-2">
                                No hay direcciones registradas
                              </div>
                            )}
                          </div>
                        );
                      },
                    },
                  ]}
                />
              </div>
            </div>
          </div>
        ) : (
          <Typography.Text type="secondary">
            <QuestionCircleOutlined className="mr-2" />
            No hay paciente seleccionado. Por favor, seleccione un paciente de
            la lista para ver su información.
          </Typography.Text>
        )}
      </div>

      {/* Busqueda y Filtros */}
      <div className="rounded-lg border border-gray-300 bg-white p-6">
        <div className="mb-4 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <FilterOutlined
              className="text-lg"
              style={{ color: "var(--color-primary)" }}
            />
            <span className="text-lg font-medium text-primary">
              Filtros de Búsqueda
            </span>
            {hasActiveFilters && (
              <Tag color="blue">
                {Object.values(filters).filter(
                  (v) => v && v !== "" && v !== null
                ).length - 2}{" "}
                activos
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
              if (key === "fechaNacimiento" && value) {
                value = dayjs(value).format("DD-MM-YYYY");
              }

              // Si el valor es undefined, null o string vacío, limpiar el filtro
              if (value === undefined || value === null || value === "") {
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
                onClear: () => handleClearFilter("fechaNacimiento"),
              }}
            />

            <ProFormSelect
              name="genero"
              label="Género"
              options={[
                { label: "Masculino", value: "Masculino" },
                { label: "Femenino", value: "Femenino" },
                { label: "Otro", value: "Otro" },
                { label: "Desconocido", value: "Desconocido" },
              ]}
              placeholder="Seleccionar"
              allowClear
              fieldProps={{
                onClear: () => handleClearFilter("genero"),
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
              placeholder="Seleccionar"
              allowClear
              fieldProps={{
                onClear: () => handleClearFilter("tipoIdentificador"),
              }}
            />

            <ProFormSelect
              name="estadoVital"
              label="Estado Vital"
              options={[
                { label: "Vivo", value: "Vivo" },
                { label: "Fallecido", value: "Fallecido" },
              ]}
              placeholder="Seleccionar"
              allowClear
              fieldProps={{
                onClear: () => handleClearFilter("estadoVital"),
              }}
            />
          </div>
        </ProForm>
      </div>

      {/* Lista de Pacientes */}
      <div className="rounded-lg border border-gray-300 bg-white p-6">
        <div className="mb-4 flex items-center gap-2">
          <UserOutlined
            className="text-lg"
            style={{ color: "var(--color-primary)" }}
          />
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
