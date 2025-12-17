import { message } from "antd";
import { usePostApiInvoices } from "../../../api/invoice/invoice";
import {
  InvoiceType,
  PaymentMethodType,
  type InvoiceCreateDto,
  type InvoiceItemCreateDto,
} from "../../../api/models";

interface SelectedService {
  id: string;
  nombre: string;
  precio: number;
  tipo: string;
  servicios?: string[];
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

export const useCreateIncome = ({
  onSuccess,
  onError,
}: UseCreateIncomeProps = {}) => {
  const [messageApi, contextHolder] = message.useMessage();

  //La mayoría de endpoints de Orval usan body, no data
  const createInvoice = usePostApiInvoices({
    mutation: {
      onSuccess: () => {
        messageApi.success("Ingreso creado exitosamente");
        onSuccess?.();
      },
      onError: (error: any) => {
        messageApi.error(
          error?.response?.data?.message ||
            error?.response?.data ||
            "Error desconocido al crear el ingreso"
        );

        onError?.(error);
      },
    },
  });

  const createIncome = ({
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
    // Validaciones
    if (!selectedPaciente)
      return messageApi.warning("Por favor selecciona un paciente");
    if (!selectedServicio)
      return messageApi.warning("Por favor selecciona un servicio");
    if (!serieId)
      return messageApi.warning("Por favor selecciona una serie válida");
    if (!numeroRecibo.trim())
      return messageApi.warning("Por favor ingresa un número de recibo");

    const precioOriginal = selectedServicio.precio;
    const discount = exonerado ? precioOriginal : 0;

    // totalAmount debe ser precio - descuento
    const totalAmount = precioOriginal - discount;

    const item: InvoiceItemCreateDto = {
      serviceId: selectedServicio.id,
      nameService: selectedServicio.nombre,
      quantity: 1,
      unitPrice: precioOriginal,
      discount,
      totalAmount,
    };

    const invoiceType = tramiteEmergencia
      ? InvoiceType.NUMBER_1
      : InvoiceType.NUMBER_0;

    const paymentType = exonerado
      ? PaymentMethodType.NUMBER_2
      : tramiteEmergencia
        ? PaymentMethodType.NUMBER_1
        : PaymentMethodType.NUMBER_0;

    const invoiceData: InvoiceCreateDto = {
      patientIdFhir: selectedPaciente.id,
      patientDisplay: selectedPaciente.nombre,
      patientSystem: "DNI",
      patientValue: selectedPaciente.identificador,

      singleServiceId:
        selectedServicio.tipo === "servicio" ? selectedServicio.id : null,
      serviceGroupFhirId:
        selectedServicio.tipo === "paquete" ? selectedServicio.id : null,

      items: [item],

      invoice_type: invoiceType,
      payment_type: paymentType,

      serieId,
      serieNumber: parseInt(numeroRecibo) || 0,
      initialPayment: exonerado ? 0 : aPagarEfectivo,

      parentInvoiceId: null,
    };

    createInvoice.mutate({ data: invoiceData });
  };

  return {
    createIncome,
    isLoading: createInvoice.isPending,
    isSuccess: createInvoice.isSuccess,
    isError: createInvoice.isError,
    error: createInvoice.error,
    contextHolder,
  };
};
