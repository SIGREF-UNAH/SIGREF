import { useMessage } from "../../../shared/hooks";
import { useCreateInvoice } from "../../../api/invoice/invoice";
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
  items?: any[];
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
  const msg = useMessage();

  const { mutateAsync: createInvoice, isPending } = useCreateInvoice({
    mutation: {
      onSuccess: () => {
        msg.success("Ingreso creado exitosamente");
        onSuccess?.();
      },
      onError: (error: any) => {
        const errorMessage =
          error?.response?.data?.detail ||
          error?.response?.data?.title ||
          "Error al crear el ingreso. Verifique los datos e intente nuevamente.";

        msg.error(errorMessage);
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
    // Validaciones
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

    const precioOriginal = selectedServicio.precio;

    // Calcular descuento: 100% si está exonerado O si es trámite de emergencia
    const discount = exonerado || tramiteEmergencia ? precioOriginal : 0;
    const totalAmount = precioOriginal - discount;

    // Crear items del invoice
    const items: InvoiceItemCreateDto[] = [];

    if (selectedServicio.tipo === "servicio") {
      items.push({
        serviceId: selectedServicio.id,
        nameService: selectedServicio.nombre,
        quantity: 1,
        unitPrice: precioOriginal,
      });
    } else if (selectedServicio.tipo === "paquete") {
      if (selectedServicio.items && Array.isArray(selectedServicio.items)) {
        selectedServicio.items.forEach((item: any) => {
          const itemUnitPrice = item.unitPrice || item.precio || 0;
          const itemQuantity = item.quantity || 1;

          items.push({
            serviceId: item.id || item.serviceId,
            nameService: item.name || item.nameService || item.nombre,
            quantity: itemQuantity,
            unitPrice: itemUnitPrice,
          });
        });
      } else {
        items.push({
          serviceId: selectedServicio.id,
          nameService: selectedServicio.nombre,
          quantity: 1,
          unitPrice: precioOriginal,
        });
      }
    }

    // Determinar tipo de factura
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
      serieNumber: parseInt(numeroRecibo) || 0,
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