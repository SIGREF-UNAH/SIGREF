import { useState } from "react";
import { useNavigate } from "react-router";
import { useKeycloak } from "@react-keycloak/web";
import { useCashierSessionStore } from "../store";
import { useGetLocationList } from "../../../api/locations/locations";
import { useGetShiftList } from "../../../api/shifts/shifts";
import { useCreateSessionOpen } from "../../../api/cashier-sessions/cashier-sessions";
import { useMessage } from "../../../shared/hooks";
import { useAbility } from "../../../config";
import relativeTime from "dayjs/plugin/relativeTime";
import dayjs from "dayjs";

dayjs.extend(relativeTime);
dayjs.locale("es");

export default function useOpenCashierSession() {
  const navigate = useNavigate();
  const { keycloak } = useKeycloak();
  const ability = useAbility();
  const message = useMessage();
  const [selectedLocationId, setSelectedLocationId] = useState<string | null>(null);
  const [selectedShiftId, setSelectedShiftId] = useState<string | null>(null);
  const { session, setSessionForUser } = useCashierSessionStore();

  // Cargar Ubicaciones
  const { data: locationsData, isLoading: isLoadingLocations } = useGetLocationList();

  // Cargar los turnos cuando se selecciona una ubicación
  const { data: shiftsData, isLoading: isLoadingShifts } = useGetShiftList(
    selectedLocationId ? { LocationId: selectedLocationId } : undefined,
    {
      query: {
        enabled: !!selectedLocationId,
      },
    }
  );

  // Mutación para crear la CashierSession
  const { mutate: openSession, isPending: isOpeningSession } =
    useCreateSessionOpen({
      mutation: {
        onSuccess: (response: any) => {
          if (response?.data) {
            const selectedShift = shifts.find(
              (s: any) => s.id === selectedShiftId
            );
            const selectedLocation = locations.find(
              (l: any) => l.id === selectedLocationId
            );

            const userId = keycloak.tokenParsed?.sub;
            if (!userId) {
              message.error("No se pudo identificar al usuario autenticado");
              return;
            }

            setSessionForUser(userId, {
              id: response.data.id,
              openAt: response.data.openAt,
              shiftName: selectedShift?.name || "N/A",
              locationName: selectedLocation?.name || "N/A",
              shiftId: selectedShiftId || undefined,
              locationId: selectedLocationId || undefined,
            });
            message.success("Turno iniciado correctamente");
          }
        },
        onError: (error: any) => {
          message.error(
            error?.response?.data?.message || "Error al abrir el turno"
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

  return {
    session,
    locations,
    shifts,
    selectedLocationId,
    selectedShiftId,
    ability,
    sessionDuration,
    sessionOpenDate,
    isOpeningSession,
    isLoadingLocations,
    isLoadingShifts,
    navigate,
    handleLocationChange,
    handleShiftClick,
    handleOpenSession,
  };
}
