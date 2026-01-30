import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router";
import { useKeycloak } from "@react-keycloak/web";
import { useGetApiHospitalPropertiesDetails } from "../../../api/hospital-properties/hospital-properties";
import { useCashierSessionStore } from "../../cashier-sessions/store";
import { usePostApiCashierSessionsSessionIdClose } from "../../../api/cashier-sessions/cashier-sessions";
import useMediaFiles from "../../media-files/hooks/useMediaFiles";
import { useExport } from "../../../shared/utils";
import { useMessage } from "../../../shared/hooks";
import dayjs from "dayjs";

export default function useCashClosing() {
  const message = useMessage();
  const navigate = useNavigate();
  const { keycloak } = useKeycloak();
  const { getMediaUrl } = useMediaFiles();
  const [amount, setAmount] = useState<number | null>(null);
  const [isLocked, setIsLocked] = useState(false);
  const [showConfirmation, setShowConfirmation] = useState(false);
  const [showResult, setShowResult] = useState(false);
  const [closedSessionId, setClosedSessionId] = useState<string>("");
  const [systemAmount, setSystemAmount] = useState(0);

  // Store de sesión de caja
  const { session, clearSession } = useCashierSessionStore();

  // Información del hospital
  const { data: hospitalResponse, isLoading: isLoadingHospital } =
    useGetApiHospitalPropertiesDetails();

  // Mutation para cerrar sesión
  const { mutate: closeSession, isPending: isClosingSession } =
    usePostApiCashierSessionsSessionIdClose({
      mutation: {
        onSuccess: (response: any) => {
          setClosedSessionId(response?.data?.id || session?.id || "");
          message.success("Sesión cerrada exitosamente");
          setSystemAmount(response?.data?.systemAmount);
          setShowConfirmation(false);
          setShowResult(true);
        },
        onError: (error: any) => {
          message.error(
            error?.response?.data?.message || "Error al cerrar caja",
          );
          setIsLocked(false);
        },
      },
    });

  // Datos del hospital
  const hospitalResponseData = hospitalResponse as any;
  const hospitalData = hospitalResponseData?.data;

  const logoHealthUrl = hospitalData?.urlLogoHealth
    ? getMediaUrl(hospitalData.urlLogoHealth)
    : "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f1/Logo_de_SESAL.svg/1200px-Logo_de_SESAL.svg.png";

  const logoHospitalUrl = hospitalData?.urlLogo
    ? getMediaUrl(hospitalData.urlLogo)
    : "https://krti.cl/wp-content/uploads/2021/04/Logo-Hospital-Final.png";

  const hospitalName = hospitalData?.name || "Hospital";
  const hospitalAddress = hospitalData?.ubication || "";
  const hospitalCurrency = hospitalData?.currency || "LPS";

  const userName = keycloak.tokenParsed?.name || "Usuario";

  // Fecha y hora actual
  const currentDateTime = dayjs().format("DD [de] MMMM [de] YYYY hh:mm:ss A");
  const currentDate = dayjs().format("DD [de] MMMM [de] YYYY");
  const currentTime = dayjs().format("hh:mm:ss A");

  const handleSave = () => {
    if (!session?.id) {
      message.error("No hay sesión activa para cerrar");
      return;
    }
    setShowConfirmation(true);
    setIsLocked(true);
  };

  const handleConfirm = () => {
    if (!session?.id || amount === null) {
      message.error("Datos incompletos para cerrar la sesión");
      return;
    }

    closeSession({
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
    navigate("/");
    clearSession();
  };

  const difference = amount !== null ? amount - systemAmount : 0;
  const isMatch = difference === 0;

  // Ref para exportación
  const printRef = useRef<HTMLDivElement>(null);
  const { exportData } = useExport();

  useEffect(() => {
    return () => {
      // limpiar sesión si no presiona Finalizar
      if (showResult) {
        clearSession();
      }
    };
  }, [showResult, clearSession]);

  return {
    session,
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
