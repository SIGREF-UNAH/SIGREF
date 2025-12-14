import { useState } from "react";
import { Card, Select, Button, Spin, Empty, Result, Alert } from "antd";
import {
  EnvironmentOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined,
  InfoCircleOutlined,
} from "@ant-design/icons";
import { useNavigate } from "react-router";
import { useCashierSessionStore } from "../store";
import { useGetApiLocations } from "../../../api/locations/locations";
import { useGetApiShifts } from "../../../api/shifts/shifts";
import { usePostApiCashierSessionsOpen } from "../../../api/cashier-sessions/cashier-sessions";
import { useMessage } from "../../../shared/hooks";
import { PageHeaderTabs } from "../../../shared/components";
import { useAbility } from "../../../config";
import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";
import "dayjs/locale/es";

dayjs.extend(relativeTime);
dayjs.locale("es");

const { Option } = Select;

export default function OpenCashierSessionPage() {
  const ability = useAbility();
  const [selectedLocationId, setSelectedLocationId] = useState<string | null>(null);
  const [selectedShiftId, setSelectedShiftId] = useState<string | null>(null);
  const navigate = useNavigate();
  const message = useMessage();
  const { session, setSession } = useCashierSessionStore();

  // Cargar Ubicaciones
  const { data: locationsData, isLoading: isLoadingLocations } = useGetApiLocations();

  // Cargar los turnos cuando se selecciona una ubicación
  const { data: shiftsData, isLoading: isLoadingShifts } = useGetApiShifts(
    selectedLocationId ? { LocationId: selectedLocationId } : undefined,
    {
      query: {
        enabled: !!selectedLocationId,
      },
    }
  );

  // Mutación para crear la CashierSession
  const { mutate: openSession, isPending: isOpeningSession } =
    usePostApiCashierSessionsOpen({
      mutation: {
        onSuccess: (response: any) => {
          if (response?.data) {
            setSession({
              id: response.data.id,
              openAt: response.data.openAt,
            });
            message.success(response.message || "Sesión abierta exitosamente");
            navigate("/");
          }
        },
        onError: (error: any) => {
          message.error(
            error?.response?.data?.message || "Error al abrir la sesión"
          );
        },
      },
    });

  const handleLocationChange = (value: string) => {
    setSelectedLocationId(value);
    setSelectedShiftId(null);
  };

  const handleShiftClick = (shiftId: string) => {
    setSelectedShiftId(shiftId);
  };

  const handleOpenSession = () => {
    if (!selectedShiftId) {
      message.warning("Por favor seleccione un turno");
      return;
    }

    openSession({
      data: {
        shiftId: selectedShiftId,
      },
    });
  };

  const locations = locationsData?.items || [];
  const shifts = ((shiftsData as any)?.data?.items || []).filter((shift: any) => shift.isActive === true);

  // Calcular duración de la sesión activa
  const sessionDuration = session ? dayjs(session.openAt).fromNow(true) : "";
  const sessionOpenDate = session ? dayjs(session.openAt).format("DD/MM/YYYY HH:mm") : "";

  // Si ya tiene una sesión activa, mostrar mensaje informativo
  if (session) {
    return (
      <div>
        {/* Encabezado */}
        <PageHeaderTabs 
          title="Gestión de Turnos" 
          tabs={[
            ...(ability.can("read", "shifts") ? [{
              key: "listar",
              label: "Lista de Turnos",
              path: "/shifts/list",
            }] : []),
            ...(ability.can("create", "cashier-sessions") ? [{
              key: "iniciar",
              label: "Iniciar Turno",
              path: "/cashier/open-session",
            }] : []),
          ]} 
          defaultActive="inciar"
        />

        {/* Contenido */}
        <div className="primary-card">
          <Result
            status="info"
            icon={<InfoCircleOutlined style={{ color: "#1890ff" }} />}
            title="Ya tienes una sesión de caja activa"
            subTitle="Debes cerrar tu sesión actual antes de poder abrir una nueva"
            extra={[
              <Card 
                key="session-info"
                className="max-w-md mx-auto mb-6 border-blue-200 bg-blue-50"
                style={{ borderRadius: '12px' }}
              >
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-medium text-gray-600">ID de Sesión:</span>
                    <span className="text-sm font-mono text-gray-800">{session.id}</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-medium text-gray-600">Fecha de Apertura:</span>
                    <span className="text-sm text-gray-800">{sessionOpenDate}</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-medium text-gray-600">Tiempo Activa:</span>
                    <span className="text-sm font-semibold text-blue-600">
                      {sessionDuration}
                    </span>
                  </div>
                </div>
              </Card>,
              <div key="actions" className="space-x-4">
                <Button 
                  type="default" 
                  size="large"
                  onClick={() => navigate("/")}
                >
                  Volver al Inicio
                </Button>
                <Button 
                  type="primary" 
                  size="large"
                  onClick={() => navigate("/incomes/cash-closing")}
                >
                  Ir a Cerrar Sesión
                </Button>
              </div>
            ]}
          >
            <Alert
              message="Nota"
              description="Para abrir una nueva sesión de caja, primero debes cerrar la sesión actual. Ve a la sección de Fondos para cerrar tu sesión."
              type="warning"
              showIcon
              className="max-w-lg mx-auto"
            />
          </Result>
        </div>
      </div>
    );
  }

  return (
    <div>
      {/* Encabezado */}
      <PageHeaderTabs 
        title="Gestión de Turnos" 
        tabs={[
          ...(ability.can("read", "shifts") ? [{
            key: "listar",
            label: "Lista de Turnos",
            path: "/shifts/list",
          }] : []),
          ...(ability.can("create", "cashier-sessions") ? [{
            key: "inciar",
            label: "Iniciar Turno",
            path: "/cashier/open-session",
          }] : []),
        ]} 
        defaultActive="iniciar"
      />

      {/* Contenido */}
      <div className="primary-card">
        {/* Título Principal */}
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-800 mb-2">
            Seleccione un Turno
          </h1>
          <p className="text-base text-gray-600">
            Elija su ubicación y turno de trabajo
          </p>
        </div>

        {/* Selector de Ubicación */}
        <div className="mb-8 max-w-2xl mx-auto">
          {isLoadingLocations ? (
            <div className="text-center py-8">
              <Spin size="large" />
            </div>
          ) : locations.length === 0 ? (
            <Empty description="No hay ubicaciones disponibles" />
          ) : (
            <Select
              size="large"
              placeholder="Seleccione una ubicación"
              onChange={handleLocationChange}
              value={selectedLocationId}
              className="w-full"
              showSearch
              allowClear
              optionFilterProp="children"
              style={{ fontSize: '16px' }}
            >
              {locations.map((location: any) => (
                <Option key={location.id} value={location.id}>
                  <div className="flex items-center py-1">
                    <EnvironmentOutlined className="mr-2 text-blue-500" />
                    <div>
                      <div className="font-medium">{location.name}</div>
                      {location.description && (
                        <div className="text-xs text-gray-500">
                          {location.description}
                        </div>
                      )}
                    </div>
                  </div>
                </Option>
              ))}
            </Select>
          )}
        </div>

        {/* Grid de Turnos */}
        {selectedLocationId && (
          <div>
            {isLoadingShifts ? (
              <div className="text-center py-12">
                <Spin size="large" />
              </div>
            ) : shifts.length === 0 ? (
              <Empty 
                description="No hay turnos disponibles para esta ubicación"
                className="py-12"
              />
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
                {shifts.map((shift: any) => (
                  <Card
                    key={shift.id}
                    className={`relative cursor-pointer transition-all duration-300 ${
                      selectedShiftId === shift.id
                        ? 'border-2 border-blue-500 shadow-2xl scale-105'
                        : 'border border-gray-200 hover:border-blue-300 hover:shadow-lg'
                    }`}
                    onClick={() => handleShiftClick(shift.id)}
                    style={{
                      borderRadius: '20px',
                      overflow: 'hidden',
                      minHeight: '300px',
                      background: selectedShiftId === shift.id 
                        ? 'linear-gradient(135deg, #ffffff 0%, #f0f7ff 100%)'
                        : '#ffffff',
                    }}
                    bodyStyle={{ padding: '24px', height: '100%' }}
                  >
                    {/* Badge de selección */}
                    {selectedShiftId === shift.id && (
                      <div className="absolute top-3 right-3 bg-blue-500 rounded-full p-2">
                        <CheckCircleOutlined className="text-white text-lg" />
                      </div>
                    )}

                    {/* Contenido de la Card */}
                    <div className="flex flex-col items-center justify-between h-full">
                      {/* Icono de Reloj con animación */}
                      <div className="mb-4">
                        <div 
                          className={`w-20 h-20 rounded-full flex items-center justify-center transition-all duration-300 ${
                            selectedShiftId === shift.id 
                              ? 'bg-blue-500 shadow-lg' 
                              : 'bg-gray-100'
                          }`}
                        >
                          <ClockCircleOutlined 
                            className={`text-4xl transition-colors duration-300 ${
                              selectedShiftId === shift.id 
                                ? 'text-white' 
                                : 'text-gray-500'
                            }`}
                          />
                        </div>
                      </div>

                      {/* Información del Turno */}
                      <div className="text-center flex-1 flex flex-col justify-center">
                        {/* Nombre del Turno */}
                        <h3 className={`text-2xl font-bold mb-3 transition-colors duration-300 ${
                          selectedShiftId === shift.id 
                            ? 'text-blue-600' 
                            : 'text-gray-800'
                        }`}>
                          {shift.name}
                        </h3>

                        {/* Horario */}
                        <div className="space-y-2">
                          <div className="flex items-center justify-center text-gray-600">
                            <span className="text-sm font-medium mr-2">Inicio:</span>
                            <span className="text-base font-semibold">{shift.startTime}</span>
                          </div>
                          <div className="flex items-center justify-center text-gray-600">
                            <span className="text-sm font-medium mr-2">Fin:</span>
                            <span className="text-base font-semibold">{shift.endTime}</span>
                          </div>
                        </div>
                      </div>

                      {/* Decoración Inferior con ondas */}
                      <div className="w-full mt-6">
                        <div 
                          className={`h-20 relative transition-all duration-300 ${
                            selectedShiftId === shift.id 
                              ? 'bg-linear-to-r from-blue-400 via-blue-500 to-blue-600' 
                              : 'bg-linear-to-r from-gray-300 via-gray-400 to-gray-500'
                          }`}
                          style={{
                            borderRadius: '0 0 16px 16px',
                            marginLeft: '-24px',
                            marginRight: '-24px',
                            marginBottom: '-24px',
                            clipPath: 'polygon(0 30%, 10% 20%, 20% 30%, 30% 20%, 40% 30%, 50% 20%, 60% 30%, 70% 20%, 80% 30%, 90% 20%, 100% 30%, 100% 100%, 0 100%)',
                          }}
                        >
                          {/* Círculos decorativos flotantes */}
                          <div className="absolute inset-0 overflow-hidden">
                            <div 
                              className="absolute rounded-full bg-white opacity-20"
                              style={{
                                width: '50px',
                                height: '50px',
                                top: '30%',
                                left: '10%',
                              }}
                            />
                            <div 
                              className="absolute rounded-full bg-white opacity-15"
                              style={{
                                width: '35px',
                                height: '35px',
                                top: '20%',
                                left: '40%',
                              }}
                            />
                            <div 
                              className="absolute rounded-full bg-white opacity-25"
                              style={{
                                width: '60px',
                                height: '60px',
                                top: '35%',
                                right: '15%',
                              }}
                            />
                            <div 
                              className="absolute rounded-full bg-white opacity-10"
                              style={{
                                width: '30px',
                                height: '30px',
                                top: '15%',
                                right: '35%',
                              }}
                            />
                          </div>
                        </div>
                      </div>
                    </div>
                  </Card>
                ))}
              </div>
            )}

            {/* Botón de Confirmar */}
            {shifts.length > 0 && (
              <div className="flex justify-center mt-8">
                <Button
                  type="primary"
                  size="large"
                  icon={<CheckCircleOutlined />}
                  onClick={handleOpenSession}
                  disabled={!selectedShiftId || isOpeningSession}
                  loading={isOpeningSession}
                  className="px-10 h-12 text-base font-semibold"
                  style={{ borderRadius: '10px' }}
                >
                  {isOpeningSession ? "Iniciando Turno..." : "Iniciar Turno"}
                </Button>
              </div>
            )}
          </div>
        )}

        {/* Mensaje inicial si no hay ubicación seleccionada */}
        {!selectedLocationId && !isLoadingLocations && locations.length > 0 && (
          <div className="text-center py-12">
            <div className="text-gray-400 mb-4">
              <EnvironmentOutlined style={{ fontSize: '56px' }} />
            </div>
            <p className="text-base text-gray-500">
              Seleccione una ubicación para ver los turnos disponibles
            </p>
          </div>
        )}
      </div>
    </div>
  );
}