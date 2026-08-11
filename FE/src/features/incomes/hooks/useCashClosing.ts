import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useKeycloak } from "@react-keycloak/web";
import { useGetHospitalPropertiesDetails } from "../../../api/hospital-properties/hospital-properties";
import { useCashierSessionStore } from "../../cashier-sessions/store";
import type { CashierSession } from "../../cashier-sessions/store";
import { useCreateSessionCloseById } from "../../../api/cashier-sessions/cashier-sessions";
import useMediaFiles from "../../media-files/hooks/useMediaFiles";
import { useExport } from "../../../shared/utils";
import { useMessage } from "../../../shared/hooks";
import dayjs from "dayjs";
import { completeCashierSessionClose } from "./cashClosingState";

export default function useCashClosing() {
  const msg = useMessage();
  const navigate = useNavigate();
  const { keycloak } = useKeycloak();
  const { getMediaUrl } = useMediaFiles();

  // Estados locales
  const [amount, setAmount] = useState<number | null>(null);
  const [isLocked, setIsLocked] = useState(false);
  const [showConfirmation, setShowConfirmation] = useState(false);
  const [showResult, setShowResult] = useState(false);
  const [closedSessionId, setClosedSessionId] = useState<string>("");
  const [systemAmount, setSystemAmount] = useState(0);
  const [closedSession, setClosedSession] = useState<CashierSession | null>(null);

  // Store de sesión de caja
  const { session, clearSession } = useCashierSessionStore();

  // Información del hospital
  const { data: hospitalData, isLoading: isLoadingHospital } =
    useGetHospitalPropertiesDetails();

  // Mutación para cerrar sesión
  const { mutateAsync: closeSession, isPending: isClosingSession } =
    useCreateSessionCloseById({
      mutation: {
        onSuccess: (response) => {
          // CashierSessionDtoResponseDto tiene wrapper .data
          // TODO Cuando BE deje de usar wrapper, eliminar .data
          const sessionData = response;
          if (session) {
            const closeState = completeCashierSessionClose(
              session,
              sessionData,
              clearSession,
            );
            setClosedSession(closeState.closedSession);
            setClosedSessionId(closeState.closedSessionId);
            setSystemAmount(closeState.systemAmount);
          }
          msg.success("Sesión cerrada exitosamente");
          setShowConfirmation(false);
          setShowResult(true);
        },
        onError: (error: any) => {
          const errorMessage =
            error?.response?.data?.detail ||
            error?.response?.data?.title ||
            "Error al cerrar caja";
          msg.error(errorMessage);
          setIsLocked(false);
        },
      },
    });

  // Datos del hospital (HospitalDetailsDto no tiene wrapper)
  const logoHealthUrl = hospitalData?.urlLogoHealth
    ? getMediaUrl(hospitalData.urlLogoHealth)
    : "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f1/Logo_de_SESAL.svg/1200px-Logo_de_SESAL.svg.png";

  const logoHospitalUrl = hospitalData?.urlLogo
    ? getMediaUrl(hospitalData.urlLogo)
    : "https://krti.cl/wp-content/uploads/2021/04/Logo-Hospital-Final.png";

  const hospitalName = hospitalData?.name || "Hospital";
  const hospitalAddress = hospitalData?.location || "";
  const hospitalCurrency = hospitalData?.currency || "LPS";

  const userName = keycloak.tokenParsed?.name || "Usuario";

  // Fecha y hora actual
  const currentDateTime = dayjs().format("DD [de] MMMM [de] YYYY hh:mm:ss A");
  const currentDate = dayjs().format("DD [de] MMMM [de] YYYY");
  const currentTime = dayjs().format("hh:mm:ss A");

  // Diferencia entre monto declarado y del sistema
  const difference = amount !== null ? amount - systemAmount : 0;
  const isMatch = difference === 0;

  const handleSave = () => {
    if (!session?.id) {
      msg.error("No hay sesión activa para cerrar");
      return;
    }
    setShowConfirmation(true);
    setIsLocked(true);
  };

  const handleConfirm = async () => {
    if (!session?.id || amount === null) {
      msg.error("Datos incompletos para cerrar la sesión");
      return;
    }

    await closeSession({
      sessionId: session.id,
      data: {
        declaredAmount: amount,
      },
    });
  };

  const handleCancel = () => {
    setShowConfirmation(false);
    setIsLocked(false);
  };

  const handleFinish = () => {
    clearSession();
    navigate("/");
  };

  // Ref para exportación
  const printRef = useRef<HTMLDivElement>(null);
  const { exportData } = useExport();

  // Limpiar sesión al desmontar si se mostró resultado
  useEffect(() => {
    return () => {
      if (showResult) {
        clearSession();
      }
    };
  }, [showResult, clearSession]);

  return {
    session: session ?? closedSession,
    amount,
    printRef,
    isLoadingHospital,
    logoHealthUrl,
    logoHospitalUrl,
    hospitalName,
    hospitalAddress,
    hospitalCurrency,
    userName,
    currentDateTime,
    currentDate,
    currentTime,
    difference,
    isMatch,
    isLocked,
    isClosingSession,
    showConfirmation,
    showResult,
    closedSessionId,
    systemAmount,
    handleSave,
    handleConfirm,
    handleCancel,
    handleFinish,
    exportData,
    setAmount,
  };
}
