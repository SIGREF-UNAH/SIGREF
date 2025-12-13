import { useState } from 'react';
import { Card, Select, Button, Steps, Spin, Empty } from 'antd';
import { EnvironmentOutlined, ClockCircleOutlined, CheckCircleOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router';
import { useCashierSessionStore } from '../store';
import { useGetApiLocations } from '../../../api/locations/locations';
import { useGetApiShifts } from '../../../api/shifts/shifts';
import { usePostApiCashierSessionsOpen } from '../../../api/cashier-sessions/cashier-sessions';
import { useMessage } from '../../../shared/hooks';

const { Option } = Select;

export default function OpenCashierSessionPage() {
  const [currentStep, setCurrentStep] = useState(0);
  const [selectedLocationId, setSelectedLocationId] = useState<string | null>(null);
  const [selectedShiftId, setSelectedShiftId] = useState<string | null>(null);
  const navigate = useNavigate();
  const message = useMessage();
  const setSession = useCashierSessionStore((state) => state.setSession);

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
  const { mutate: openSession, isPending: isOpeningSession } = usePostApiCashierSessionsOpen({
    mutation: {
      onSuccess: (response: any) => {
        if (response?.data) {
          setSession({
            id: response.data.id,
            openAt: response.data.openAt,
          });
          message.success(response.message || 'Sesión abierta exitosamente');
          // Redirigir a Home
          navigate('/');
        }
      },
      onError: (error: any) => {
        message.error(
          error?.response?.data?.message || 'Error al abrir la sesión'
        );
      },
    },
  });

  const handleLocationChange = (value: string) => {
    setSelectedLocationId(value);
    setSelectedShiftId(null);
    setCurrentStep(1);
  };

  const handleShiftChange = (value: string) => {
    setSelectedShiftId(value);
    setCurrentStep(2);
  };

  const handleOpenSession = () => {
    if (!selectedShiftId) {
      message.warning('Por favor seleccione un turno');
      return;
    }

    openSession({
      data: {
        shiftId: selectedShiftId,
      },
    });
  };

  const locations = locationsData?.items || [];
  const shifts = (shiftsData as any)?.data?.items || [];

  const steps = [
    {
      title: 'Ubicación',
      icon: <EnvironmentOutlined />,
    },
    {
      title: 'Turno',
      icon: <ClockCircleOutlined />,
    },
    {
      title: 'Confirmar',
      icon: <CheckCircleOutlined />,
    },
  ];

  return (
    <div className="min-h-screen bg-primary flex items-center justify-center p-4">
      <Card className="w-full max-w-2xl shadow-lg">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-800 mb-2">
            Iniciar Sesión de Caja
          </h1>
          <p className="text-gray-600">
            Seleccione su ubicación y turno para comenzar
          </p>
        </div>

        <Steps current={currentStep} items={steps} className="mb-8" />

        <div className="space-y-6">
          {/* Step 1: Select Location */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Ubicación del Hospital
            </label>
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
                optionFilterProp="children"
              >
                {locations.map((location: any) => (
                  <Option key={location.id} value={location.id}>
                    <div className="flex items-center">
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

          {/* Step 2: Select Shift */}
          {selectedLocationId && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Turno
              </label>
              {isLoadingShifts ? (
                <div className="text-center py-8">
                  <Spin size="large" />
                </div>
              ) : shifts.length === 0 ? (
                <Empty description="No hay turnos disponibles para esta ubicación" />
              ) : (
                <Select
                  size="large"
                  placeholder="Seleccione un turno"
                  onChange={handleShiftChange}
                  value={selectedShiftId}
                  className="w-full"
                >
                  {shifts.map((shift: any) => (
                    <Option key={shift.id} value={shift.id}>
                      <div className="flex items-center justify-between">
                        <div>
                          <ClockCircleOutlined className="mr-2 text-green-500" />
                          <span className="font-medium">{shift.name}</span>
                        </div>
                        <div className="text-sm text-gray-500">
                          {shift.startTime} - {shift.endTime}
                        </div>
                      </div>
                    </Option>
                  ))}
                </Select>
              )}
            </div>
          )}

          {/* Step 3: Confirmation */}
          {selectedLocationId && selectedShiftId && (
            <Card className="bg-blue-50 border-blue-200">
              <div className="space-y-2">
                <div className="flex justify-between items-center">
                  <span className="font-medium text-gray-700">Ubicación:</span>
                  <span className="text-gray-900">
                    {locations.find((l: any) => l.id === selectedLocationId)?.name}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="font-medium text-gray-700">Turno:</span>
                  <span className="text-gray-900">
                    {shifts.find((s: any) => s.id === selectedShiftId)?.name}
                  </span>
                </div>
              </div>
            </Card>
          )}

          {/* Action Button */}
          <Button
            type="primary"
            size="large"
            block
            onClick={handleOpenSession}
            disabled={!selectedShiftId || isOpeningSession}
            loading={isOpeningSession}
            className="mt-6"
          >
            {isOpeningSession ? 'Abriendo sesión...' : 'Iniciar Sesión de Caja'}
          </Button>
        </div>
      </Card>
    </div>
  );
}