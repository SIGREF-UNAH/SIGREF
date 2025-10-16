import { useState } from "react";
import { PageContainer, ProCard } from "@ant-design/pro-components";
import {
  Input,
  Button,
  Space,
  Checkbox,
  InputNumber,
  Typography,
  Row,
  Col,
  Divider,
  message,
} from "antd";
import {
  UserOutlined,
  FileTextOutlined,
  SaveOutlined,
  ReloadOutlined,
} from "@ant-design/icons";
import { useUrlFilters } from "../../../shared/hooks";
import { IncomeSummary, ListPatient, ServiceIncome } from "../components";

const { TextArea } = Input;
const { Text } = Typography;

// Datos de ejemplo
const serviciosData = [
  {
    id: 1,
    nombre: "Consulta General",
    abreviatura: "CG",
    area: "Consulta",
    precio: 60.0,
    tipo: "servicio",
  },
  {
    id: 2,
    nombre: "Paquete de Exámenes Básicos",
    abreviatura: "PEB",
    area: "Laboratorio",
    precio: 260.0,
    tipo: "paquete",
    servicios: ["Hemograma", "Glucosa", "Urea"],
  },
  {
    id: 3,
    nombre: "Examen de la Bacteria",
    abreviatura: "EB",
    area: "Laboratorio",
    precio: 120.0,
    tipo: "servicio",
  },
  {
    id: 4,
    nombre: "Radiografía de Tórax",
    abreviatura: "RT",
    area: "Radiología",
    precio: 150.0,
    tipo: "servicio",
  },
  {
    id: 5,
    nombre: "Electrocardiograma",
    abreviatura: "ECG",
    area: "Cardiología",
    precio: 80.0,
    tipo: "servicio",
  },
  {
    id: 6,
    nombre: "Paquete de Imágenes Completo",
    abreviatura: "PIC",
    area: "Radiología",
    precio: 400.0,
    tipo: "paquete",
    servicios: ["Radiografía de Tórax", "Ultrasonido Abdominal", "ECG"],
  },
  {
    id: 7,
    nombre: "Ultrasonido Abdominal",
    abreviatura: "UA",
    area: "Radiología",
    precio: 200.0,
    tipo: "servicio",
  },
  {
    id: 8,
    nombre: "Consulta de Especialidad",
    abreviatura: "CE",
    area: "Consulta",
    precio: 100.0,
    tipo: "servicio",
  },
];

const pacientesData = [
  {
    id: 1,
    nombre: "David Enrique Lopez Garcia Prado",
    identificador: "DNI: 0232-2034-12923",
    genero: "M",
    nacionalidad: "HN",
    nacimiento: "26/07/1989",
  },
  {
    id: 2,
    nombre: "Andrea Valencia Josefina Prado",
    identificador: "DNI: 0232-2034-12923",
    genero: "F",
    nacionalidad: "HN",
    nacimiento: "26/07/1989",
  },
  {
    id: 3,
    nombre: "David Gavier Alexander Prado",
    identificador: "DNI: 0232-2034-12923",
    genero: "M",
    nacionalidad: "HN",
    nacimiento: "26/07/1989",
  },
  {
    id: 4,
    nombre: "María Fernanda Castillo",
    identificador: "DNI: 0801-1990-12345",
    genero: "F",
    nacionalidad: "HN",
    nacimiento: "15/03/1992",
  },
  {
    id: 5,
    nombre: "Carlos Eduardo Méndez",
    identificador: "DNI: 0801-1985-67890",
    genero: "M",
    nacionalidad: "GT",
    nacimiento: "22/11/1985",
  },
  {
    id: 6,
    nombre: "Lucía Rodríguez",
    identificador: "DNI: 0801-1995-54321",
    genero: "F",
    nacionalidad: "SV",
    nacimiento: "30/06/1995",
  },
];

export const CreateIncomePage = () => {
  const [messageApi, contextHolder] = message.useMessage();
  const {
    filters: servicioFilters,
    setFilter: setServicioFilter,
    resetFilters: resetServicioFilters,
    setFilters: setServicioFilters,
  } = useUrlFilters({
    defaultValues: {
      searchServicios: "",
      tipoServicio: "todos",
      pageServicio: 1,
      pageSizeServicio: 5,
    },
  });

  const {
    filters: pacienteFilters,
    setFilter: setPacienteFilter,
    resetFilters: resetPacienteFilters,
    setFilters: setPacienteFilters,
  } = useUrlFilters({
    defaultValues: {
      searchPaciente: "",
      tipoIdentificador: "DNI",
      genero: "todos",
      nacionalidad: "todos",
      identificador: "",
      pagePaciente: 1,
      pageSizePaciente: 5,
    },
  });

  const [selectedServicio, setSelectedServicio] = useState<any>(null);
  const [selectedPaciente, setSelectedPaciente] = useState<any>(null);
  const [serie, setSerie] = useState("A");
  const [numeroRecibo, setNumeroRecibo] = useState("");
  const [aPagarEfectivo, setAPagarEfectivo] = useState(0);
  const [exonerado, setExonerado] = useState(false);
  const [tramiteEmergencia, setTramiteEmergencia] = useState(false);
  const [observaciones, setObservaciones] = useState("");

  // Seleccionar servicio
  const handleSelectServicio = (servicio: any) => {
    if (selectedServicio?.id === servicio.id) {
      setSelectedServicio(null);
      setAPagarEfectivo(0);
    } else {
      setSelectedServicio(servicio);
      if (!exonerado && !tramiteEmergencia) {
        setAPagarEfectivo(servicio.precio);
      }
    }
  };

  // Manejar cambios en exonerado/emergencia
  const handleExoneradoChange = (checked: boolean) => {
    setExonerado(checked);
    if (checked) {
      setAPagarEfectivo(0);
      setTramiteEmergencia(false);
    } else if (selectedServicio) {
      setAPagarEfectivo(selectedServicio.precio);
    }
  };

  const handleEmergenciaChange = (checked: boolean) => {
    setTramiteEmergencia(checked);
    if (checked) {
      setAPagarEfectivo(0);
      setExonerado(false);
    } else if (selectedServicio) {
      setAPagarEfectivo(selectedServicio.precio);
    }
  };

  // Guardar y mostrar en consola
  const handleGuardar = () => {
    if (!selectedPaciente) {
      messageApi.warning("Por favor selecciona un paciente");
      return;
    }
    if (!selectedServicio) {
      messageApi.warning("Por favor selecciona un servicio");
      return;
    }

    const datos = {
      paciente: selectedPaciente,
      servicio: selectedServicio,
      serie,
      numeroRecibo,
      aPagarEfectivo,
      exonerado,
      tramiteEmergencia,
      observaciones,
      fecha: new Date().toISOString(),
    };

    console.log("=== DATOS DEL INGRESO ===");
    console.log(datos);
    console.log("========================");

    messageApi.success("Datos guardados. Revisa la consola.");
  };

  // Resetear todo
  const handleResetear = () => {
    setSelectedServicio(null);
    setSelectedPaciente(null);
    setSerie("A");
    setNumeroRecibo("");
    setAPagarEfectivo(0);
    setExonerado(false);
    setTramiteEmergencia(false);
    setObservaciones("");
    resetServicioFilters();
    resetPacienteFilters();
    messageApi.info("Formulario reseteado");
  };

  return (
    <>
      {contextHolder}
      <PageContainer
        title="Registro de Ingresos por Servicios"
        subTitle="Genere ingresos de los servicios del paciente"
      >
        <Row gutter={16}>
          {/* Panel Izquierdo - Servicios */}
          <Col xs={24} lg={10}>
            <ProCard
              title={
                <Space>
                  <FileTextOutlined />
                  <span>Servicios</span>
                </Space>
              }
              bordered
            >
              {/*Servicios */}
              <ServiceIncome
                serviciosData={serviciosData}
                servicioFilters={servicioFilters}
                setServicioFilter={setServicioFilter}
                setServicioFilters={setServicioFilters}
                handleSelectServicio={handleSelectServicio}
                selectedServicio={selectedServicio}
              />

              {/* Serie y Número de Recibo */}
              <Divider />

              <IncomeSummary
                serie={serie}
                setSerie={setSerie}
                numeroRecibo={numeroRecibo}
                setNumeroRecibo={setNumeroRecibo}
                selectedPaciente={selectedPaciente}
                selectedServicio={selectedServicio}
              />
            </ProCard>
          </Col>

          {/* Panel Derecho - Registro de Ingresos */}
          <Col xs={24} lg={14}>
            <ProCard
              title={
                <Space>
                  <UserOutlined />
                  <span>Registro de Ingresos</span>
                </Space>
              }
              bordered
            >
              {/* Paciente */}
              <ListPatient
                pacientesData={pacientesData}
                setSelectedPaciente={setSelectedPaciente}
                selectedPaciente={selectedPaciente}
                pacienteFilters={pacienteFilters}
                setPacienteFilter={setPacienteFilter}
                setPacienteFilters={setPacienteFilters}
              />

              {/* Pago */}
              <Divider />
              <Row gutter={16} style={{ marginBottom: 16 }}>
                <Col span={8}>
                  <Text strong>A Pagar Efectivo:</Text>
                  <InputNumber
                    prefix="L"
                    value={aPagarEfectivo}
                    onChange={(value) => setAPagarEfectivo(value || 0)}
                    style={{ width: "100%", marginTop: 8 }}
                    disabled={exonerado || tramiteEmergencia}
                  />
                </Col>
                <Col span={8}>
                  <Text strong>Ajuste</Text>
                  <div style={{ marginTop: 8 }}>
                    <Checkbox
                      checked={exonerado}
                      onChange={(e) => handleExoneradoChange(e.target.checked)}
                    >
                      Exonerado
                    </Checkbox>
                  </div>
                </Col>
                <Col span={8}>
                  <Text strong>&nbsp;</Text>
                  <div style={{ marginTop: 8 }}>
                    <Checkbox
                      checked={tramiteEmergencia}
                      onChange={(e) => handleEmergenciaChange(e.target.checked)}
                    >
                      Trámite de Emergencia
                    </Checkbox>
                  </div>
                </Col>
              </Row>

              {/* Observaciones */}
              <div style={{ marginBottom: 24 }}>
                <Text strong>Observaciones</Text>
                <TextArea
                  rows={3}
                  value={observaciones}
                  onChange={(e) => setObservaciones(e.target.value)}
                  style={{ marginTop: 8 }}
                  placeholder="Ingrese observaciones adicionales..."
                />
              </div>

              {/* Botones de acción */}
              <Space>
                <Button
                  type="primary"
                  icon={<SaveOutlined />}
                  size="large"
                  onClick={handleGuardar}
                >
                  Guardar
                </Button>
                <Button
                  icon={<ReloadOutlined />}
                  size="large"
                  onClick={handleResetear}
                >
                  Resetear
                </Button>
              </Space>
            </ProCard>
          </Col>
        </Row>
      </PageContainer>
    </>
  );
};
