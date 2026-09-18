import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useMessage } from "../../../shared/hooks";
import type { UpdateHospitalPropertiesDto } from "@models/hospital-properties/updateHospitalPropertiesDto";
import {
  getGetHospitalPropertiesDetailsQueryKey,
  useGetHospitalPropertiesDetails,
  useUpdateHospitalProperties,
} from "@endpoints/hospital-properties/hospital-properties";

function getErrorMessage(error: unknown): string {
  if (typeof error !== "object" || error === null) {
    return "Error al actualizar la información del hospital";
  }

  const response = (error as {
    response?: { data?: { detail?: unknown; title?: unknown } };
  }).response;
  const detail = response?.data?.detail;
  const title = response?.data?.title;

  if (typeof detail === "string") return detail;
  if (typeof title === "string") return title;
  return "Error al actualizar la información del hospital";
}

export default function useUpdateHospital() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const msg = useMessage();

  // Obtener datos del hospital
  const {
    data: hospital,
    isLoading,
    isError,
  } = useGetHospitalPropertiesDetails();

  // Mutación para actualizar
  const { mutateAsync: updateHospital, isPending } = useUpdateHospitalProperties({
    mutation: {
      onSuccess: () => {
        // Invalidar la query de detalles para que se recargue
        void queryClient.invalidateQueries({
          queryKey: getGetHospitalPropertiesDetailsQueryKey(),
        });
        
        msg.success("Información del hospital actualizada exitosamente");
        void navigate("/hospital/details");
      },
      onError: (error: unknown) => {
        // Extraer mensaje según ProblemDetails (RFC 7807)
        msg.error(getErrorMessage(error));
      },
    },
  });

  // Redirigir si hay error o no hay datos
  useEffect(() => {
    if (isError || (!isLoading && !hospital)) {
      msg.warning("No hay información del hospital para actualizar");
      void navigate("/hospital/details");
    }
  }, [isError, isLoading, hospital, navigate, msg]);

  // Manejar submit del formulario
  const handleUpdate = async (values: UpdateHospitalPropertiesDto) => {
    await updateHospital({ data: values });
  };

  // Manejar cancelación
  const handleCancel = () => {
    void navigate("/hospital/details");
  };

  return {
    hospital,
    isPending,
    isLoading,
    isError,
    handleUpdate,
    handleCancel,
  };
}
