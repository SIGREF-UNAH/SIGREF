import { useEffect, useState } from "react";
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
  Tabs,
} from "antd";
import {
  UserOutlined,
  FileTextOutlined,
  SaveOutlined,
  ReloadOutlined,
  MedicineBoxOutlined,
  AppstoreOutlined,
} from "@ant-design/icons";
import { useUrlFilters } from "../../../shared/hooks";
import { IncomeSummary, ListPatient, ServiceIncome } from "../components";
import { ServiceGroupIncome } from "../components/ServiceGroupIncome";
import { useHealthcaresList } from "../../healthcares/hooks";
import { useServiceGroupsList } from "../../service-groups/hooks";
import { usePatientsInformation } from "../../patients/hooks";
import { useCreateIncome } from "../hooks/useCreateIncome";
import { PageHeaderTabs } from "../../../shared/components";
import { useAbility } from "../../../config";
import { useInvoiceSeriesManager } from "../hooks";

const { TextArea } = Input;
const { Text } = Typography;

export const CreateIncomePage = () => {
  const ability = useAbility();
  const [tipoSeleccion, setTipoSeleccion] = useState<"servicio" | "paquete">(
    "servicio",
  );

  const {
    createIncome,
    isLoading: isCreatingIncome,
    contextHolder: incomeContextHolder,
    messageApi,
  } = useCreateIncome({
    onSuccess: () => {
      handleResetear(false);
    },
  });

  // Hook para servicios
  const {
    healthcares,
    isLoading: isLoadingHealthcares,
    setFilter: setHealthcareFilter,
  } = useHealthcaresList();

  useEffect(() => {
    setHealthcareFilter("includeCost", true);
  }, []);

  // Hook para paquetes
  const {
    serviceGroups,
    isLoading: isLoadingServiceGroups,
    setFilter: setServiceGroupFilter,
  } = useServiceGroupsList();

  // Hook para pacientes
  const {
    patients,
    isLoading: isLoadingPatients,
    filters: patientsFilters,
    setFilter: setPatientsFilter,
    setFilters: setPatientsFilters,
  } = usePatientsInformation();

  // Filtros UI para servicios
  const {
    filters: servicioUIFilters,
    setFilter: setServicioUIFilter,
    setFilters: setServicioUIFilters,
  } = useUrlFilters({
    defaultValues: {
      searchServicios: "",
      pageServicio: 1,
      pageSizeServicio: 10,
    },
  });

  // Filtros UI para paquetes
  const {
    filters: serviceGroupUIFilters,
    setFilter: setServiceGroupUIFilter,
    setFilters: setServiceGroupUIFilters,
  } = useUrlFilters({
    defaultValues: {
      searchServiceGroups: "",
      pageServiceGroup: 1,
      pageSizeServiceGroup: 10,
    },
  });

  // Filtros UI para pacientes
  const {
    filters: pacienteUIFilters,
    setFilter: setPacienteUIFilter,
    setFilters: setPacienteUIFilters,
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

  // Estados del formulario
  const [selectedServicio, setSelectedServicio] = useState<any>(null);
  const [selectedServiceGroup, setSelectedServiceGroup] = useState<any>(null);
  const [selectedPaciente, setSelectedPaciente] = useState<any>(null);
  const seriesManager = useInvoiceSeriesManager();
  const [exonerado, setExonerado] = useState(false);
  const [tramiteEmergencia, setTramiteEmergencia] = useState(false);
  const [observaciones, setObservaciones] = useState("");

  // Solo guardar el ajuste manual del cajero
  const [ajusteManual, setAjusteManual] = useState<number | null>(null);

  const precioBase =
    selectedServicio?.cost ||
    selectedServicio?.precio ||
    selectedServiceGroup?.totalPrice ||
    selectedServiceGroup?.precio ||
    0;

  // aPagarEfectivo se calcula, nunca se guarda en estado
  const aPagarEfectivo =
    exonerado || tramiteEmergencia ? 0 : (ajusteManual ?? precioBase);

  const handleSelectServicio = (servicio: any) => {
    setAjusteManual(null);
    if (selectedServicio?.id === servicio.id) {
      setSelectedServicio(null);
    } else {
      const servicioNormalizado = {
        id: servicio.id,
        nombre: servicio.name || "Servicio sin nombre",
        precio: servicio.cost || 0,
        cost: servicio.cost || 0,
        tipo: "servicio",
        abbreviation: servicio.abbreviation,
        ...servicio,
      };
      setSelectedServicio(servicioNormalizado);
      setSelectedServiceGroup(null);
      setTipoSeleccion("servicio");
    }
  };

  const handleSelectServiceGroup = (serviceGroup: any) => {
    setAjusteManual(null);
    if (selectedServiceGroup?.id === serviceGroup.id) {
      setSelectedServiceGroup(null);
    } else {
      const paqueteNormalizado = {
        id: serviceGroup.id,
        nombre: serviceGroup.title || "Paquete sin título",
        precio: serviceGroup.totalPrice || 0,
        totalPrice: serviceGroup.totalPrice || 0,
        tipo: "paquete",
        items: serviceGroup.items || [],
        code: serviceGroup.code?.coding?.[0]?.code,
        description: serviceGroup.description || "",
      };
      setSelectedServiceGroup(paqueteNormalizado);
      setSelectedServicio(null);
      setTipoSeleccion("paquete");
    }
  };
  // Sincronizar filtros de pacientes
  const handleSetPacienteFilter = (key: string | number, value: any) => {
    const typedKey = key as "searchPaciente" | "tipoIdentificador" | "genero" | "nacionalidad" | "identificador" | "pagePaciente" | "pageSizePaciente";
    setPacienteUIFilter(typedKey, value);

    const filterMap: Record<string, string> = {
      searchPaciente: "search",
      tipoIdentificador: "tipoIdentificador",
      genero: "genero",
      nacionalidad: "nacionalidad",
      identificador: "identificador",
    };

    if (filterMap[key]) {
      setPatientsFilter(filterMap[key] as "tipoIdentificador" | "genero" | "identificador" | "search" | "pageNumber" | "pageSize" | "nombreCompleto" | "estadoVital" | "fechaNacimiento", value);
    }
  };

  const handleSetPacienteFilters = (newFilters: any) => {
    setPacienteUIFilters(newFilters);

    if (newFilters.pagePaciente || newFilters.pageSizePaciente) {
      setPatientsFilters({
        pageNumber: newFilters.pagePaciente || patientsFilters.pageNumber,
        pageSize: newFilters.pageSizePaciente || patientsFilters.pageSize,
      });
    }
  };

  const handleExoneradoChange = (checked: boolean) => {
    setExonerado(checked);
    if (checked) {
      setTramiteEmergencia(false); // Desmarcar tramite de emergencia
    }
  };

  const handleEmergenciaChange = (checked: boolean) => {
    setTramiteEmergencia(checked);
    if (checked) {
      setExonerado(false); // Desmarcar exonerado
    }
  };

  const handleGuardar = () => {
    if (!selectedPaciente) {
      messageApi.warning("Por favor selecciona un paciente");
      return;
    }
    if (!selectedServicio && !selectedServiceGroup) {
      messageApi.warning("Por favor selecciona un servicio o paquete");
      return;
    }
    if (!numeroRecibo.trim()) {
      messageApi.warning("Por favor ingresa un número de recibo");
      return;
    }
    if (!serieId || serieId === "") {
      messageApi.error("Por favor selecciona una serie válida");
      return;
    }
    if (!seriesManager.isNumberInRange) {
      message.error(
        `El número de recibo está fuera del rango permitido 
     (${seriesManager.currentSerie?.startNumber} - ${seriesManager.currentSerie?.endNumber})`,
      );
      return;
    }

    const itemSeleccionado = selectedServicio || selectedServiceGroup;

    const pacienteData = {
      id: selectedPaciente.id,
      nombre: selectedPaciente.nombre,
      identificador: selectedPaciente.identificador,
    };

    createIncome({
      selectedPaciente: pacienteData,
      selectedServicio: itemSeleccionado,
      numeroRecibo: seriesManager.numeroRecibo,
      aPagarEfectivo,
      exonerado,
      tramiteEmergencia,
      serieId: seriesManager.serieId,
    });
  };

  const handleResetear = (showNotification = true) => {
    setSelectedServicio(null);
    setSelectedServiceGroup(null);
    setSelectedPaciente(null);
    seriesManager.resetSerie();
    setAjusteManual(null);
    setExonerado(false);
    setTramiteEmergencia(false);
    setObservaciones("");
    setTipoSeleccion("servicio");

    setServicioUIFilters({
      searchServicios: "",
      pageServicio: 1,
      pageSizeServicio: 10,
    });

    setServiceGroupUIFilters({
      searchServiceGroups: "",
      pageServiceGroup: 1,
      pageSizeServiceGroup: 10,
    });

    setHealthcareFilter("search", "");
    setHealthcareFilter("pageNumber", 1);
    setHealthcareFilter("pageSize", 10);
    setHealthcareFilter("location", undefined);
    setHealthcareFilter("status", undefined);
    setHealthcareFilter("scope", undefined);
    setHealthcareFilter("includeCost", true);

    setServiceGroupFilter("search", "");
    setServiceGroupFilter("pageNumber", 1);
    setServiceGroupFilter("pageSize", 10);
    setServiceGroupFilter("location", undefined);
    setServiceGroupFilter("status", undefined);

    setPacienteUIFilters({
      searchPaciente: "",
      tipoIdentificador: "DNI",
      genero: "todos",
      nacionalidad: "todos",
      identificador: "",
      pagePaciente: 1,
      pageSizePaciente: 5,
    });

    setPatientsFilters({
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

    if (showNotification) {
      messageApi.info("Formulario reseteado");
    }
  };

  const itemSeleccionado = selectedServicio || selectedServiceGroup;

  return (
    <>
      {incomeContextHolder}
      <div>
        {/* Header */}
        <PageHeaderTabs
          title="Gestión de Ingresos"
          tabs={[
            ...(ability.can("read", "incomes")
              ? [
                  {
                    key: "read",
                    label: "Lista de Ingresos",
                    path: "/incomes/list",
                  },
                ]
              : []),
            ...(ability.can("create", "incomes")
              ? [
                  {
                    key: "create",
                    label: "Generar Ingreso",
                    path: "/incomes/create",
                  },
                ]
              : []),
            ...(ability.can("update", "incomes")
              ? [
                  {
                    key: "update",
                    label: "Cerrar Caja",
                    path: "/incomes/close",
                  },
                ]
              : []),
            ...(ability.can("read", "incomes")
              ? [
                  {
                    key: "history",
                    label: "Historial de Cierres de Caja",
                    path: "/incomes/history",
                  },
                ]
              : []),
          ]}
          defaultActive="create"
        />
        {/* Content */}
        <div className="primary-card">
          {/* Resumen de la factura */}
          <div className="secondary-card mb-4">
            <IncomeSummary
              seriesManager={seriesManager}
              selectedPaciente={selectedPaciente}
              selectedServicio={itemSeleccionado}
              aPagarEfectivo={aPagarEfectivo}
              exonerado={exonerado}
              tramiteEmergencia={tramiteEmergencia}
            />
            <Divider />
            <div>
              <Row gutter={16} style={{ marginBottom: 16 }}>
                <Col span={8}>
                  <Text strong>A Pagar Efectivo:</Text>
                  <InputNumber
                    prefix="L"
                    value={aPagarEfectivo}
                    onChange={(value) => setAjusteManual(value || 0)}
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
              <Space className="flex! justify-end!">
                <Button
                  icon={<ReloadOutlined />}
                  size="large"
                  onClick={handleResetear}
                  disabled={isCreatingIncome}
                >
                  Cancelar
                </Button>
                <Button
                  type="primary"
                  icon={<SaveOutlined />}
                  size="large"
                  onClick={handleGuardar}
                  loading={isCreatingIncome}
                  disabled={
                    !selectedPaciente ||
                    (!selectedServicio && !selectedServiceGroup)
                  }
                >
                  Guardar
                </Button>
              </Space>
            </div>
          </div>

          <Row gutter={16}>
            {/* Servicios y Paquetes */}
            <Col xs={24} lg={10}>
              <div className="secondary-card">
                <Space className="text-lg!">
                  <FileTextOutlined />
                  <span>Servicios y Paquetes</span>
                </Space>
                <Tabs
                  activeKey={tipoSeleccion}
                  onChange={(key) =>
                    setTipoSeleccion(key as "servicio" | "paquete")
                  }
                  items={[
                    {
                      key: "servicio",
                      label: (
                        <Space>
                          <MedicineBoxOutlined />
                          Servicios
                        </Space>
                      ),
                      children: (
                        <ServiceIncome
                          serviciosData={healthcares}
                          servicioFilters={servicioUIFilters}
                          setServicioFilter={setServicioUIFilter}
                          setServicioFilters={setServicioUIFilters}
                          handleSelectServicio={handleSelectServicio}
                          selectedServicio={selectedServicio}
                          isLoading={isLoadingHealthcares}
                        />
                      ),
                    },
                    {
                      key: "paquete",
                      label: (
                        <Space>
                          <AppstoreOutlined />
                          Paquetes
                        </Space>
                      ),
                      children: (
                        <ServiceGroupIncome
                          serviceGroupsData={serviceGroups}
                          serviceGroupFilters={serviceGroupUIFilters}
                          setServiceGroupFilter={setServiceGroupUIFilter}
                          setServiceGroupFilters={setServiceGroupUIFilters}
                          handleSelectServiceGroup={handleSelectServiceGroup}
                          selectedServiceGroup={selectedServiceGroup}
                          isLoading={isLoadingServiceGroups}
                        />
                      ),
                    },
                  ]}
                />
              </div>
            </Col>

            {/* Pacientes */}
            <Col xs={24} lg={14}>
              <div className="secondary-card">
                <Space className="mb-2! text-lg!">
                  <UserOutlined />
                  <span>Pacientes</span>
                </Space>
                <ListPatient
                  pacientesData={patients}
                  setSelectedPaciente={setSelectedPaciente}
                  selectedPaciente={selectedPaciente}
                  pacienteFilters={pacienteUIFilters}
                  setPacienteFilter={handleSetPacienteFilter}
                  setPacienteFilters={handleSetPacienteFilters}
                  isLoading={isLoadingPatients}
                />
              </div>
            </Col>
          </Row>
        </div>
      </div>
    </>
  );
};
