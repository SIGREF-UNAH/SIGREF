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
import { useHealthcaresList } from "../../healthcares/hooks";
import { usePatientsInformation } from "../../patients/hooks";
import { useCreateIncome } from "../hooks/useCreateIncome";

const { TextArea } = Input;
const { Text } = Typography;

export const CreateIncomePage = () => {
  // Hook para crear ingresos 
  const { 
    createIncome, 
    isLoading: isCreatingIncome,
    contextHolder: incomeContextHolder 
  } = useCreateIncome({
    onSuccess: () => {
      handleResetear();
    },
  });
  
  // Hook para obtener los servicios del backend
  const { healthcares, isLoading: isLoadingHealthcares } = useHealthcaresList();

  // Hook para obtener los pacientes del backend
  const { 
    patients, 
    isLoading: isLoadingPatients,
    filters: patientsHookFilters,
    setFilter: setPatientsHookFilter,
    setFilters: setPatientsHookFilters,
  } = usePatientsInformation();

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

  // Sincronizar filtros locales con el hook de pacientes
  const handleSetPacienteFilter = (key: string, value: any) => {
    setPacienteFilter(key, value);
    
    const filterMap: Record<string, string> = {
      searchPaciente: "nombreCompleto",
      genero: "genero",
      nacionalidad: "nacionalidad",
      tipoIdentificador: "tipoIdentificador",
      identificador: "identificador",
    };
    
    if (filterMap[key]) {
      setPatientsHookFilter(filterMap[key], value);
    }
  };

  const handleSetPacienteFilters = (newFilters: any) => {
    setPacienteFilters(newFilters);
    
    if (newFilters.pagePaciente || newFilters.pageSizePaciente) {
      setPatientsHookFilters({
        pageNumber: newFilters.pagePaciente || patientsHookFilters.pageNumber,
        pageSize: newFilters.pageSizePaciente || patientsHookFilters.pageSize,
      });
    }
  };

  const [selectedServicio, setSelectedServicio] = useState<any>(null);
  const [selectedPaciente, setSelectedPaciente] = useState<any>(null);
  const [serie, setSerie] = useState("");
  const [serieId, setSerieId] = useState("");
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
        setAPagarEfectivo(servicio.cost || 0);
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
      setAPagarEfectivo(selectedServicio.cost || 0);
    }
  };

  const handleEmergenciaChange = (checked: boolean) => {
    setTramiteEmergencia(checked);
    if (checked) {
      setAPagarEfectivo(0);
      setExonerado(false);
    } else if (selectedServicio) {
      setAPagarEfectivo(selectedServicio.cost || 0);
    }
  };

  // Guardar y crear el ingreso
  const handleGuardar = () => {
    if (!selectedPaciente) {
      message.warning("Por favor selecciona un paciente");
      return;
    }
    if (!selectedServicio) {
      message.warning("Por favor selecciona un servicio");
      return;
    }
    if (!numeroRecibo.trim()) {
      message.warning("Por favor ingresa un número de recibo");
      return;
    }
    if (!serieId || serieId === "") {
      message.error("Por favor selecciona una serie válida");
      return;
    }

    const servicioData = {
      id: selectedServicio.id,
      nombre: selectedServicio.name || "Servicio sin nombre",
      precio: selectedServicio.cost || 0,
      tipo: "servicio",
    };

    const pacienteData = {
      id: selectedPaciente.id,
      nombre: selectedPaciente.nombre,
      identificador: selectedPaciente.identificador,
    };

    createIncome({
      selectedPaciente: pacienteData,
      selectedServicio: servicioData,
      numeroRecibo,
      aPagarEfectivo,
      exonerado,
      tramiteEmergencia,
      serieId: serieId,
    });
  };

  // Resetear todo
  const handleResetear = () => {
    setSelectedServicio(null);
    setSelectedPaciente(null);
    setSerie("");
    setSerieId("");
    setNumeroRecibo("");
    setAPagarEfectivo(0);
    setExonerado(false);
    setTramiteEmergencia(false);
    setObservaciones("");
    resetServicioFilters();
    
    setPacienteFilters({
      searchPaciente: "",
      tipoIdentificador: "DNI",
      genero: "todos",
      nacionalidad: "todos",
      identificador: "",
      pagePaciente: 1,
      pageSizePaciente: 5,
    });
    
    setPatientsHookFilters({
      search: "",
      pageNumber: 1,
      pageSize: 10,
      nombreCompleto: null,
      genero: null,
      estadoVital: null,
      tipoIdentificador: null,
      identificador: null,
      fechaNacimiento: null,
    });
    
    message.info("Formulario reseteado");
  };
  console.log("=== SERVICIOS DESDE useHealthcaresList ===");
console.log(JSON.stringify(healthcares, null, 2));


  return (
    <>
      {incomeContextHolder}
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
              <ServiceIncome
                serviciosData={healthcares}
                servicioFilters={servicioFilters}
                setServicioFilter={setServicioFilter}
                setServicioFilters={setServicioFilters}
                handleSelectServicio={handleSelectServicio}
                selectedServicio={selectedServicio}
                isLoading={isLoadingHealthcares}
              />

              <Divider />

              <IncomeSummary
                serie={serie}
                setSerie={setSerie}
                serieId={serieId}
                setSerieId={setSerieId}
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
              <ListPatient
                pacientesData={patients}
                setSelectedPaciente={setSelectedPaciente}
                selectedPaciente={selectedPaciente}
                pacienteFilters={pacienteFilters}
                setPacienteFilter={handleSetPacienteFilter}
                setPacienteFilters={handleSetPacienteFilters}
                isLoading={isLoadingPatients}
              />

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

              <Space>
                <Button
                  type="primary"
                  icon={<SaveOutlined />}
                  size="large"
                  onClick={handleGuardar}
                  loading={isCreatingIncome}
                  disabled={!selectedPaciente || !selectedServicio}
                >
                  Guardar
                </Button>
                <Button
                  icon={<ReloadOutlined />}
                  size="large"
                  onClick={handleResetear}
                  disabled={isCreatingIncome}
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