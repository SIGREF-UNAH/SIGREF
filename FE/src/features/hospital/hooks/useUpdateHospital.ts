import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useMessage } from "../../../shared/hooks";
import type { UpdateHospitalPropertiesDto } from "../../../api/models";
import {
  getGetHospitalPropertiesDetailsQueryKey,
  useGetHospitalPropertiesDetails,
  useUpdateHospitalProperties,
} from "../../../api/hospital-properties/hospital-properties";

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
        queryClient.invalidateQueries({
          queryKey: getGetHospitalPropertiesDetailsQueryKey(),
        });
        
        msg.success("Información del hospital actualizada exitosamente");
        navigate("/hospital/details");
      },
      onError: (error: any) => {
        // Extraer mensaje según ProblemDetails (RFC 7807)
        const errorMessage =
          error?.response?.data?.detail ||
          error?.response?.data?.title ||
          "Error al actualizar la información del hospital";

        msg.error(errorMessage);
      },
    },
  });

  // Redirigir si hay error o no hay datos
  useEffect(() => {
    if (isError || (!isLoading && !hospital)) {
      msg.warning("No hay información del hospital para actualizar");
      navigate("/hospital/details");
    }
  }, [isError, isLoading, hospital, navigate, msg]);

  // Manejar submit del formulario
  const handleUpdate = async (values: UpdateHospitalPropertiesDto) => {
    await updateHospital({ data: values });
  };

  // Manejar cancelación
  const handleCancel = () => {
    navigate("/hospital/details");
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