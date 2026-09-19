import { useMessage } from "../../../shared/hooks";
import { useCreateInvoice } from "@endpoints/invoices/invoices";
import {
  type InvoiceCreateDto,
  type InvoiceItemCreateDto,
} from "@models";
import {
  InvoiceType,
  PaymentMethodType,
} from "../../../api/generated/schemas/types/invoices";

interface SelectedService {
  id: string;
  nombre: string;
  precio: number;
  tipo: string;
  items?: PackageItem[];
}

interface PackageItem {
  id?: string | null;
  serviceId?: string | null;
  name?: string | null;
  nameService?: string | null;
  nombre?: string | null;
  unitPrice?: number | null;
  precio?: number | null;
  price?: number | null;
  quantity?: number | null;
}

interface SelectedPatient {
  id: string;
  nombre: string;
  identificador: string;
}

interface UseCreateIncomeProps {
  onSuccess?: () => void;
  onError?: (error: unknown) => void;
}

type ApiErrorData = {
  detail?: unknown;
  title?: unknown;
};

type ApiError = {
  response?: {
    data?: ApiErrorData;
  };
};

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === "object" && value !== null;

const getErrorMessage = (error: unknown): string => {
  if (!isRecord(error)) {
    return "Error al crear el ingreso. Verifique los datos e intente nuevamente.";
  }

  const response = isRecord(error.response) ? error.response : undefined;
  const data = response && isRecord(response.data) ? response.data : undefined;
  const detail = data?.detail;
  const title = data?.title;

  if (typeof detail === "string" && detail.trim()) {
    return detail;
  }

  if (typeof title === "string" && title.trim()) {
    return title;
  }

  return "Error al crear el ingreso. Verifique los datos e intente nuevamente.";
};

/**
 * * Convierte la selección del usuario en el formato que espera la API.
 * La función es pura para mantener la construcción de la factura aislada de
 * los efectos secundarios del hook y facilitar su verificación con tests.
 */
export const buildInvoiceItems = (
  selectedServicio: SelectedService,
): InvoiceItemCreateDto[] => {
  if (selectedServicio.tipo === "servicio") {
    return [
      {
        serviceId: selectedServicio.id,
        nameService: selectedServicio.nombre,
        quantity: 1,
        unitPrice: selectedServicio.precio,
      },
    ];
  }

  if (selectedServicio.tipo !== "paquete" || !selectedServicio.items) {
    return [
      {
        serviceId: selectedServicio.id,
        nameService: selectedServicio.nombre,
        quantity: 1,
        unitPrice: selectedServicio.precio,
      },
    ];
  }

  return selectedServicio.items.map((item) => ({
    serviceId: item.id ?? item.serviceId,
    nameService: item.name ?? item.nameService ?? item.nombre,
    quantity: item.quantity ?? 1,
    unitPrice: item.unitPrice ?? item.precio ?? item.price ?? 0,
  }));
};

export const useCreateIncome = ({
  onSuccess,
  onError,
}: UseCreateIncomeProps = {}) => {
  const msg = useMessage();

  const { mutateAsync: createInvoice, isPending } = useCreateInvoice({
    mutation: {
      onSuccess: () => {
        msg.success("Ingreso creado exitosamente");
        onSuccess?.();
      },
      onError: (error: ApiError) => {
        msg.error(getErrorMessage(error));
        onError?.(error);
      },
    },
  });

  const createIncome = async ({
    selectedPaciente,
    selectedServicio,
    numeroRecibo,
    aPagarEfectivo,
    exonerado,
    tramiteEmergencia,
    serieId,
  }: {
    selectedPaciente: SelectedPatient;
    selectedServicio: SelectedService;
    numeroRecibo: string;
    aPagarEfectivo: number;
    exonerado: boolean;
    tramiteEmergencia: boolean;
    serieId: string;
  }) => {
    // * Estas validaciones evitan enviar una factura incompleta a la API.
    // La página también controla estos valores, pero el hook debe proteger su
    // propio contrato porque puede reutilizarse desde otra vista.
    if (!selectedPaciente) {
      msg.warning("Por favor selecciona un paciente");
      return;
    }
    if (!selectedServicio) {
      msg.warning("Por favor selecciona un servicio o paquete");
      return;
    }
    if (!serieId) {
      msg.warning("Por favor selecciona una serie válida");
      return;
    }
    if (!numeroRecibo.trim()) {
      msg.warning("Por favor ingresa un número de recibo");
      return;
    }

    // * Los items se normalizan antes de construir el DTO para que servicios
    // individuales y paquetes sigan exactamente el mismo contrato.
    const items = buildInvoiceItems(selectedServicio);

    // * La emergencia tiene precedencia sobre la exoneración para conservar la
    // clasificación específica del trámite en los reportes de ingresos.
    const invoiceType = tramiteEmergencia
      ? InvoiceType.emergency
      : exonerado
        ? InvoiceType.exempt
        : InvoiceType.normal;

    // TODO: [Mantenimiento] Actualmente solo se permite pago en efectivo.
    // Cuando se habiliten más métodos de pago en la UI:
    // 1. Agregar un selector de método de pago en el formulario de ingreso
    // 2. Pasar el valor seleccionado como parámetro a createIncome
    // 3. Mapear la selección al enum:
    //    - "card"     → PaymentMethodType.card
    //    - "transfer" → PaymentMethodType.transfer
    //    - "mixed"    → PaymentMethodType.mixed
    //    - default    → PaymentMethodType.cash
    // 4. Agregar validación de método de pago requerido
    const paymentType = PaymentMethodType.cash;

    const invoiceData: InvoiceCreateDto = {
      patientIdFhir: selectedPaciente.id,
      patientDisplay: selectedPaciente.nombre,
      patientSystem: "DNI",
      patientValue: selectedPaciente.identificador,
      singleServiceFhirId:
        selectedServicio.tipo === "servicio" ? selectedServicio.id : null,
      serviceGroupFhirId:
        selectedServicio.tipo === "paquete" ? selectedServicio.id : null,
      items,
      invoice_type: invoiceType,
      payment_type: paymentType,
      serieId,
      serieNumber: Number.parseInt(numeroRecibo, 10) || 0,
      initialPayment: exonerado || tramiteEmergencia ? 0 : aPagarEfectivo,
      parentInvoiceId: null,
    };

    await createInvoice({ data: invoiceData });
  };

  return {
    createIncome,
    isLoading: isPending,
  };
};
