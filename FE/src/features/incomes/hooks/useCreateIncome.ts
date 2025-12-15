import { message } from "antd";
import { usePostApiInvoices } from "../../../api/invoice/invoice";
import { InvoiceType, PaymentMethodType, type InvoiceCreateDto, type InvoiceItemCreateDto } from "../../../api/models";

interface SelectedService {
  id: string;
  nombre: string;
  precio: number;
  tipo: string;
  servicios?: string[]; // Para paquetes
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

export const useCreateIncome = ({ onSuccess, onError }: UseCreateIncomeProps = {}) => {
  const [messageApi, contextHolder] = message.useMessage();

  const createInvoice = usePostApiInvoices({
    mutation: {
      onSuccess: () => {
        messageApi.success("Ingreso creado exitosamente");
        onSuccess?.();
      },
      onError: (error) => {
        console.error("Error al crear el ingreso:", error);
        messageApi.error("Error al crear el ingreso. Por favor revisa los datos.");
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
      messageApi.warning("Por favor selecciona un paciente");
      return;
    }
    if (!selectedServicio) {
      messageApi.warning("Por favor selecciona un servicio");
      return;
    }
    if (!serieId || serieId === "") {
      messageApi.warning("Por favor selecciona una serie válida");
      return;
    }
    if (!numeroRecibo || numeroRecibo.trim() === "") {
      messageApi.warning("Por favor ingresa un número de recibo");
      return;
    }

    // Calcular descuento
    const precioOriginal = selectedServicio.precio;
    const discount = exonerado ? precioOriginal : 0;
    const totalAmount = exonerado ? 0 : aPagarEfectivo;

    // Crear el item del servicio
    const item: InvoiceItemCreateDto = {
      serviceId: selectedServicio.id,
      nameService: selectedServicio.nombre,
      quantity: 1,
      unitPrice: precioOriginal,
      discount: discount,
      totalAmount: totalAmount,
    };

    // Determinar el tipo de factura
    let invoiceType: InvoiceType;
    if (tramiteEmergencia) {
      invoiceType = InvoiceType.NUMBER_1; // Emergencia
    } else {
      invoiceType = InvoiceType.NUMBER_0; // Normal
    }

    // Determinar el método de pago
    let paymentType: PaymentMethodType;
    if (exonerado) {
      paymentType = PaymentMethodType.NUMBER_2; // Exonerado
    } else if (tramiteEmergencia) {
      paymentType = PaymentMethodType.NUMBER_1; // Emergencia
    } else {
      paymentType = PaymentMethodType.NUMBER_0; // Efectivo
    }

    // Construir el objeto según la estructura de la API
    const invoiceData: InvoiceCreateDto = {
      // Datos del paciente (requeridos)
      patientIdFhir: selectedPaciente.id,
      patientDisplay: selectedPaciente.nombre,
      patientSystem: "DNI", // Puedes ajustar según el tipo de identificador
      patientValue: selectedPaciente.identificador,

      // Datos del servicio
      singleServiceId: selectedServicio.tipo === "servicio" ? selectedServicio.id : null,
      serviceGroupFhirId: selectedServicio.tipo === "paquete" ? selectedServicio.id : null,

      // Items del servicio (requerido)
      items: [item],

      // Tipo de factura y pago (requeridos)
      invoice_type: invoiceType,
      payment_type: paymentType,

      // Serie y número
      serieId: serieId,
      serieNumber: parseInt(numeroRecibo) || 0,

      // Pago inicial
      initialPayment: totalAmount,

      // Parent invoice (null para facturas nuevas)
      parentInvoiceId: null,
    };

    // Log para debugging (puedes comentarlo en producción)
    console.log("=== DATOS DEL INVOICE A ENVIAR ===");
    console.log(JSON.stringify(invoiceData, null, 2));
    console.log("===================================");

    // Ejecutar la mutación
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