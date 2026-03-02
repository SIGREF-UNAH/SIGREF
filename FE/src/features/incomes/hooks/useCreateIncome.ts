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
  items?: any[]; // Para paquetes
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

  const createInvoice = usePostApiInvoices({
    mutation: {
      onSuccess: () => {
        messageApi.success("Ingreso creado exitosamente");
        onSuccess?.();
      },
      onError: (error: any) => {
        messageApi.error(
          error?.response?.data?.message ||
            error?.response?.data?.title ||
            "Error desconocido al crear el ingreso",
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
    if (!selectedPaciente) {
      return messageApi.warning("Por favor selecciona un paciente");
    }
    if (!selectedServicio) {
      return messageApi.warning("Por favor selecciona un servicio o paquete");
    }
    if (!serieId) {
      return messageApi.warning("Por favor selecciona una serie válida");
    }
    if (!numeroRecibo.trim()) {
      return messageApi.warning("Por favor ingresa un número de recibo");
    }

    const precioOriginal = selectedServicio.precio;

    // Calcular descuento: 100% si esta exonerado O si es tramite de emergencia
    const discount = exonerado || tramiteEmergencia ? precioOriginal : 0;
    const totalAmount = precioOriginal - discount;

    // Crear items del invoice
    const items: InvoiceItemCreateDto[] = [];

    if (selectedServicio.tipo === "servicio") {
      // Si es un servicio individual
      items.push({
        serviceId: selectedServicio.id,
        nameService: selectedServicio.nombre,
        quantity: 1,
        unitPrice: precioOriginal,
        discount: discount,
        totalAmount: totalAmount,
      });
    } else if (selectedServicio.tipo === "paquete") {
      // Si es un paquete, agregar todos los items del paquete
      if (selectedServicio.items && Array.isArray(selectedServicio.items)) {
        selectedServicio.items.forEach((item: any) => {
          const itemUnitPrice = item.unitPrice || item.precio || 0;
          const itemQuantity = item.quantity || 1;

          // Descuento: 100% si esta exonerado O si es tramite de emergencia
          const itemDiscount =
            exonerado || tramiteEmergencia ? itemUnitPrice : 0;
          const itemTotal =
            exonerado || tramiteEmergencia ? 0 : itemUnitPrice * itemQuantity;

          items.push({
            serviceId: item.id || item.serviceId,
            nameService: item.name || item.nameService || item.nombre,
            quantity: itemQuantity,
            unitPrice: itemUnitPrice,
            discount: itemDiscount,
            totalAmount: itemTotal,
          });
        });
      } else {
        // Si el paquete no tiene items definidos, crear uno generico
        items.push({
          serviceId: selectedServicio.id,
          nameService: selectedServicio.nombre,
          quantity: 1,
          unitPrice: precioOriginal,
          discount: discount,
          totalAmount: totalAmount,
        });
      }
    }

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
      singleServiceFhirId:
        selectedServicio.tipo === "servicio" ? selectedServicio.id : null,
      serviceGroupFhirId:
        selectedServicio.tipo === "paquete" ? selectedServicio.id : null,
      items,
      invoice_type: invoiceType,
      payment_type: paymentType,
      serieId,
      serieNumber: parseInt(numeroRecibo) || 0,
      initialPayment: exonerado || tramiteEmergencia ? 0 : aPagarEfectivo,
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
    messageApi,
  };
};
